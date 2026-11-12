namespace BCIT_Social.ViewModels;

// One row in the Quick links card. A Url starting with http opens in a new tab.
public class RailLinkViewModel
{
    public string Title { get; init; } = "";
    // Short line under the title. Leave empty to hide it.
    public string Detail { get; init; } = "";
    public string Url { get; init; } = "";
    // Icon name: book, people, pin, code or chat. Anything else shows an arrow.
    public string Icon { get; init; } = "";

    public bool IsExternal => Url.StartsWith("http", StringComparison.OrdinalIgnoreCase);
}
