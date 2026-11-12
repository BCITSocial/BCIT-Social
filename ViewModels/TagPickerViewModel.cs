using Microsoft.AspNetCore.Mvc.Rendering;

namespace BCIT_Social.ViewModels;

// One "Tags" field: a row of chips the user can tick, up to Max of them (Views/Shared/_TagPicker.cshtml,
// limited by wwwroot/js/tag-picker.js). Used by the post, community and event forms.
public class TagPickerViewModel
{
    // Form field name for the ticked values, e.g. "TagIds". Null when a script reads the ticks instead.
    public string? Name { get; set; }

    public string Legend { get; set; } = "Tags";

    // Each chip's value and label. Selected chips start ticked.
    public IEnumerable<SelectListItem> Options { get; set; } = [];

    public int Max { get; set; } = 3;

    // At least this many must be ticked before the form submits.
    public int Min { get; set; }

    // Hooks the picker for page scripts, e.g. "community-tags".
    public string? Id { get; set; }
}
