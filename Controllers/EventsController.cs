using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BCIT_Social.Data;
using BCIT_Social.Models;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Controllers;

// Handles the events page and its JSON.
public class EventsController : Controller
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EventsController> _logger;
    private readonly ApplicationDbContext _context;

    public EventsController(
        IWebHostEnvironment env,
        ILogger<EventsController> logger,
        ApplicationDbContext context)
    {
        _env = env;
        _logger = logger;
        _context = context;
    }

    // GET /Events?q=...&campus=...  The URL values are bound into "query".
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] EventsQuery query)
    {
        var content = LoadContentFromJson();
        var model = new EventsViewModel(query)
        {
            // The page text always comes from the JSON file.
            Text = content.Text ?? new EventsText(),
        };
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var communities = await _context.Communities.AsNoTracking()
            .Where(c => c.DateOfDel == null && userId != null && (c.CreatorId == userId || c.Members.Any(m => m.UserId == userId)))
            .OrderBy(c => c.Name).ToListAsync();
        model.Communities = communities.Select(c => c.Name).Distinct().ToList();
        model.OwnedCommunities = communities.Where(c => c.CreatorId == userId).Select(c => (c.Id, c.Name)).ToList();
        model.MinimumEventDate = EventToday.AddDays(1);
        var events = _context.CommunityEvents.AsNoTracking().Where(e => e.DateOfDel == null);
        var rsvps = _context.EventRsvps;
        if (model.View == EventsViewModel.CALENDAR)
        {
            var monthStart = model.Month.ToDateTime(TimeOnly.MinValue);
            var monthEnd = monthStart.AddMonths(1);
            events = events.Where(e => e.EventDate >= monthStart && e.EventDate < monthEnd && userId != null && rsvps.Any(r => r.EventId == e.Id && r.UserId == userId));
        }
        if (model.Query is { } search)
            events = events.Where(e => e.Title.Contains(search) || e.Description.Contains(search));
        if (model.Community is { } community && model.Communities.Contains(community))
            events = events.Where(e => e.Community != null && e.Community.Name == community);
        if (model.Campus is { } campus)
            events = events.Where(e => e.Campus == campus);
        var today = EventToday;
        events = model.Range switch
        {
            "week" => events.Where(e => e.EventDate >= today && e.EventDate < today.AddDays(7)),
            "month" => events.Where(e => e.EventDate >= new DateTime(today.Year, today.Month, 1) && e.EventDate < new DateTime(today.Year, today.Month, 1).AddMonths(1)),
            "upcoming" => events.Where(e => e.EventDate >= today),
            "past" => events.Where(e => e.EventDate < today),
            _ => events
        };
        events = model.Status switch
        {
            "going" => events.Where(e => userId != null && rsvps.Any(r => r.EventId == e.Id && r.UserId == userId && r.Going)),
            "interested" => events.Where(e => userId != null && rsvps.Any(r => r.EventId == e.Id && r.UserId == userId && !r.Going)),
            "hosting" => events.Where(e => userId != null && e.CreatorId == userId),
            "saved" => events.Where(e => false),
            _ => events
        };
        events = model.Sort switch
        {
            "latest" => events.OrderByDescending(e => e.EventDate),
            "popular" => events.OrderByDescending(e => rsvps.Count(r => r.EventId == e.Id && r.Going)).ThenBy(e => e.EventDate),
            _ => events.OrderBy(e => e.EventDate)
        };
        model.Events = await ProjectEventCards(events, userId).ToListAsync();
        return View(model);
    }

    private IQueryable<EventCardViewModel> ProjectEventCards(IQueryable<CommunityEvent> events, string? userId)
    {
        var rsvps = _context.EventRsvps;
        return events.Select(e => new EventCardViewModel
        {
            Id = e.Id, Title = e.Title, StartsAt = e.EventDate, EndsAt = e.EndsAt,
            IsOwner = userId != null && e.CreatorId == userId,
            Campus = e.Campus, Location = e.Location, Description = e.Description,
            HostName = e.Community != null ? e.Community.Name : "",
            GoingCount = rsvps.Count(r => r.EventId == e.Id && r.Going),
            Rsvp = rsvps.Any(r => r.EventId == e.Id && r.UserId == userId && r.Going) ? RsvpStatus.Going :
                rsvps.Any(r => r.EventId == e.Id && r.UserId == userId) ? RsvpStatus.Interested :
                userId != null && e.CreatorId == userId ? RsvpStatus.Hosting : RsvpStatus.None
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (await FindActiveEventAsync(id) is null) return EventUnavailable();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var item = await ProjectEventCards(_context.CommunityEvents.AsNoTracking()
            .Where(e => e.Id == id && e.DateOfDel == null), userId).FirstOrDefaultAsync();
        if (item is null) return EventUnavailable();
        ViewData[nameof(EventsText)] = LoadContentFromJson().Text ?? new EventsText();
        ViewData["EventDetails"] = true;
        return View(item);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromIndex([FromForm] EventInput input)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (!await ValidateInputAsync(input))
        {
            TempData["EventMessage"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }
        await CreateEventAsync(input, userId);
        TempData["EventCreated"] = true;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rsvp(int id, bool going, bool fromDetails = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (await FindActiveEventAsync(id) is null) return EventUnavailable();
        var existing = await _context.EventRsvps.FindAsync(id, userId);
        if (existing is null) _context.EventRsvps.Add(new EventRsvp { EventId = id, UserId = userId, Going = going });
        else if (existing.Going == going) _context.EventRsvps.Remove(existing);
        else existing.Going = going;
        await _context.SaveChangesAsync();
        return fromDetails ? RedirectToAction(nameof(Details), new { id }) : RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize]
    public async Task<IActionResult> Manage(int id)
    {
        var item = await FindActiveEventAsync(id);
        if (item is null) return EventUnavailable();
        var accessError = CheckEventAccess(item);
        if (accessError is not null) return accessError;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var communities = await _context.Communities
            .Where(community => userId != null && community.CreatorId == userId && community.DateOfDel == null)
            .OrderBy(community => community.Name)
            .ToListAsync();

        return View(new ManageEventsViewModel
        {
            Event = item!,
            OwnedCommunities = communities,
            MinimumEventDate = EventToday.AddDays(1),
        });
    }

    [HttpGet("/api/events")]
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.CommunityEvents
            .Where(communityEvent => communityEvent.DateOfDel == null)
            .OrderBy(communityEvent => communityEvent.EventDate)
            .Select(communityEvent => ToResponse(communityEvent))
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("/api/events/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var communityEvent = await FindActiveEventAsync(id);

        if (communityEvent is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(communityEvent));
    }

    [HttpPost("/api/events")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] EventInput? input)
    {
        var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (creatorId is null)
        {
            return Unauthorized();
        }

        if (!await ValidateInputAsync(input))
        {
            return ValidationProblem(ModelState);
        }

        var communityEvent = await CreateEventAsync(input!, creatorId);
        return CreatedAtAction(nameof(GetById), new { id = communityEvent.Id }, ToResponse(communityEvent));
    }

    [HttpPut("/api/events/{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] EventInput? input)
    {
        var communityEvent = await FindActiveEventAsync(id);
        var accessError = CheckEventAccess(communityEvent);
        if (accessError is not null)
        {
            return accessError;
        }

        if (!await ValidateInputAsync(input))
        {
            return ValidationProblem(ModelState);
        }

        await UpdateEventAsync(communityEvent!, input!);
        return NoContent();
    }

    [HttpDelete("/api/events/{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var communityEvent = await FindActiveEventAsync(id);
        var accessError = CheckEventAccess(communityEvent);
        if (accessError is not null)
        {
            return accessError;
        }

        await DeleteEventAsync(communityEvent!);
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromPage([FromForm] EventInput input)
    {
        var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (creatorId is null)
        {
            return Unauthorized();
        }

        if (!await ValidateInputAsync(input))
        {
            return PageValidationError();
        }

        var item = await CreateEventAsync(input, creatorId);
        return RedirectToAction(nameof(Manage), new { id = item.Id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateFromPage([FromForm] int id, [FromForm] EventInput input)
    {
        var communityEvent = await FindActiveEventAsync(id);
        var accessError = CheckEventAccess(communityEvent);
        if (accessError is not null)
        {
            return accessError;
        }

        if (!await ValidateInputAsync(input))
        {
            return PageValidationError(id);
        }

        communityEvent!.Campus = input.Campus ?? "";
        communityEvent.Location = input.Location ?? "";
        communityEvent.Description = input.Description ?? "";
        await UpdateEventAsync(communityEvent, input);
        return RedirectToAction(nameof(Manage), new { id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFromPage([FromForm] int id)
    {
        var communityEvent = await FindActiveEventAsync(id);
        var accessError = CheckEventAccess(communityEvent);
        if (accessError is not null)
        {
            return accessError;
        }

        await DeleteEventAsync(communityEvent!);
        return RedirectToAction(nameof(Index));
    }

    private async Task<CommunityEvent?> FindActiveEventAsync(int id)
    {
        var item = await _context.CommunityEvents.FirstOrDefaultAsync(e => e.Id == id && e.DateOfDel == null);
        if (item is null) return null;

        var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/Vancouver");
        var expired = item.EndsAt.HasValue ? item.EndsAt.Value <= now : item.EventDate < now.Date;
        if (expired)
        {
            await DeleteEventAsync(item);
            return null;
        }

        return item;
    }

    private IActionResult EventUnavailable()
    {
        TempData["EventMessage"] = "This event is no longer available.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult? CheckEventAccess(CommunityEvent? communityEvent)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        if (communityEvent is null)
        {
            return NotFound();
        }

        return communityEvent.CreatorId == userId ? null : Forbid();
    }

    private async Task<CommunityEvent> CreateEventAsync(EventInput input, string creatorId)
    {
        var communityEvent = new CommunityEvent { CreatorId = creatorId,
            Campus = input.Campus ?? "", Location = input.Location ?? "", Description = input.Description ?? "" };
        ApplyInput(communityEvent, input);
        _context.CommunityEvents.Add(communityEvent);
        await _context.SaveChangesAsync();
        return communityEvent;
    }

    private async Task UpdateEventAsync(CommunityEvent communityEvent, EventInput input)
    {
        ApplyInput(communityEvent, input);
        await _context.SaveChangesAsync();
    }

    private async Task DeleteEventAsync(CommunityEvent communityEvent)
    {
        communityEvent.DateOfDel = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static void ApplyInput(CommunityEvent communityEvent, EventInput input)
    {
        communityEvent.Title = input.Title.Trim();
        communityEvent.EventDate = input.EventDate!.Value;
        communityEvent.EndsAt = input.EndsAt;
        communityEvent.CommunityId = input.CommunityId;
    }

    // MVC handles required fields and binding errors before this custom validation.
    private async Task<bool> ValidateInputAsync(EventInput? input)
    {
        if (input is null)
        {
            ModelState.AddModelError(string.Empty, "Event details are required.");
            return false;
        }

        if (!ModelState.IsValid)
        {
            return false;
        }

        if (input.EventDate!.Value.Date <= EventToday)
        {
            ModelState.AddModelError(nameof(input.EventDate), "Choose a date of tomorrow or later (Vancouver time).");
        }
        if (input.EndsAt is { } end && end <= input.EventDate.Value)
            ModelState.AddModelError(nameof(input.EndsAt), "End time must be after the start time.");

        if (!await OwnsCommunityAsync(input.CommunityId))
        {
            ModelState.AddModelError(nameof(input.CommunityId), "Choose an active community you own.");
        }

        return ModelState.IsValid;
    }

    private IActionResult PageValidationError(int? id = null)
    {
        TempData["EventMessage"] = string.Join(" ", ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrEmpty(error.ErrorMessage)
                ? "Check the event details you entered."
                : error.ErrorMessage)
            .Distinct());
        return id.HasValue ? RedirectToAction(nameof(Manage), new { id }) : RedirectToAction(nameof(Index));
    }

    // Event form values are wall-clock dates in the campus timezone.
    private static DateTime EventToday => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
        DateTime.UtcNow, "America/Vancouver").Date;

    private Task<bool> OwnsCommunityAsync(int? communityId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return _context.Communities.AnyAsync(community =>
            userId != null && community.Id == communityId &&
            community.CreatorId == userId && community.DateOfDel == null);
    }
    private static EventResponse ToResponse(CommunityEvent communityEvent)
    {
        return new EventResponse(
            communityEvent.Id,
            communityEvent.CreatorId,
            communityEvent.CommunityId,
            communityEvent.Title,
            communityEvent.EventDate,
            communityEvent.DateOfDel);
    }

    // Reads wwwroot/content/events-text.json. A missing, empty or broken file gives the default text and no cards.
    private EventsContent LoadContentFromJson()
    {
        var path = Path.Combine(_env.WebRootPath, "content", "events-text.json");
        if (!System.IO.File.Exists(path))
        {
            return new EventsContent();
        }

        try
        {
            var json = System.IO.File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json)
                ? new EventsContent()
                : JsonSerializer.Deserialize<EventsContent>(json, _jsonOptions) ?? new EventsContent();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not read {Path}; using the default text and no events.", path);
            return new EventsContent();
        }
    }

    public sealed class EventInput
    {
        [Required]
        public string Title { get; init; } = string.Empty;

        [Required]
        public DateTime? EventDate { get; init; }

        [Required]
        public int? CommunityId { get; init; }
        public DateTime? EndsAt { get; init; }
        [StringLength(100)] public string? Campus { get; init; }
        [StringLength(100)] public string? Location { get; init; }
        [StringLength(2000)] public string? Description { get; init; }
    }

    public sealed record EventResponse(
        int Id,
        string? CreatorId,
        int? CommunityId,
        string Title,
        DateTime EventDate,
        DateTime? DateOfDel);

}
