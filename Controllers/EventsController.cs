using System.Text.Json;
using System.Text.Json.Serialization;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BCIT_Social.Controllers;

// Handles /Events: reads the filters from the URL, builds the page data, and shows the view.
public class EventsController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IWebHostEnvironment env, ILogger<EventsController> logger)
    {
        _env = env;
        _logger = logger;
    }

    // GET /Events?q=...&campus=...  The URL values are bound into "query".
    [HttpGet]
    public IActionResult Index([FromQuery] EventsQuery query)
    {
        var model = new EventsViewModel(query)
        {
            // TODO(BE): set Communities to the signed-in user's communities, and replace LoadEventsFromJson()
            // with real events filtered by model.Query/Community/Campus/Range/Sort/Status.
            Events = LoadEventsFromJson(),
        };
        return View(model);
    }

    // Reads the cards from wwwroot/content/events-text.json. No file or no entries means no cards.
    private IReadOnlyList<EventCardViewModel> LoadEventsFromJson()
    {
        var path = Path.Combine(_env.WebRootPath, "content", "events-text.json");
        if (!System.IO.File.Exists(path))
        {
            return [];
        }

        try
        {
            var json = System.IO.File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<EventsContent>(json, JsonOptions)?.Events ?? [];
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not read {Path}; showing no events.", path);
            return [];
        }
    }
}
