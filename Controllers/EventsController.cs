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
        var content = LoadContentFromJson();
        var model = new EventsViewModel(query)
        {
            // The page text always comes from the JSON file.
            Text = content.Text ?? new EventsText(),
            // TODO(BE): set Communities to the signed-in user's communities, and replace content.Events
            // with real events filtered by model.Query/Community/Campus/Range/Sort/Status.
            Events = content.Events ?? [],
        };
        return View(model);
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
                : JsonSerializer.Deserialize<EventsContent>(json, JsonOptions) ?? new EventsContent();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not read {Path}; using the default text and no events.", path);
            return new EventsContent();
        }
    }
}
