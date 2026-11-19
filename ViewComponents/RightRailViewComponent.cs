using System.Text.Json;
using System.Text.Json.Serialization;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BCIT_Social.ViewComponents;

// The right rail next to the page content. _HomeLayout shows it when the page lists card slots in
// ViewData["RightRail"]; the wording and the card entries come from wwwroot/content/right-rail-text.json.
public class RightRailViewComponent : ViewComponent
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RightRailViewComponent> _logger;

    public RightRailViewComponent(IWebHostEnvironment env, ILogger<RightRailViewComponent> logger)
    {
        _env = env;
        _logger = logger;
    }

    public IViewComponentResult Invoke(IReadOnlyList<string> cards)
    {
        var content = LoadContentFromJson();
        var model = new RightRailViewModel
        {
            Cards = cards,
            // The rail text always comes from the JSON file.
            Text = content.Text ?? new RightRailText(),
            // TODO(BE): replace these with real data: the signed-in user's upcoming events and the quick links.
            Events = content.Events ?? [],
            Links = content.Links ?? [],
        };
        return View(model);
    }

    // Reads wwwroot/content/right-rail-text.json. A missing, empty or broken file gives the default text and no entries.
    private RightRailContent LoadContentFromJson()
    {
        var path = Path.Combine(_env.WebRootPath, "content", "right-rail-text.json");
        if (!File.Exists(path))
        {
            return new RightRailContent();
        }

        try
        {
            var json = File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json)
                ? new RightRailContent()
                : JsonSerializer.Deserialize<RightRailContent>(json, JsonOptions) ?? new RightRailContent();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not read {Path}; using the default text and no entries.", path);
            return new RightRailContent();
        }
    }
}
