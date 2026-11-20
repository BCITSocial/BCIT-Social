namespace BCIT_Social.ViewModels;

// Community form: its labels, dropdown choices, and the values to show.
// Used by the create pop-up modal, the Settings pop-up, and the Create and Edit pages.
// All rendered to communites-form.json, read by CommunityController.
public class CommunityFormViewModel
{
    public CommunityFormText Text { get; init; } = new();
    public CommunityOptions Options { get; init; } = new();

    // Community being edited; null when creating.
    public int? Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";

    // The dropdowns' selected values. Empty is "None".
    public string Category { get; init; } = "";
    public string Tag { get; init; } = "";
    public string Campus { get; init; } = "";

    // Hidden Type value.
    public string Type { get; init; } = "";

    // Current cover image when editing, shown in the cover picker. Null when none.
    public string? BannerUrl { get; init; }

    // Upload limit for the cover image, in bytes. 
    // Set from Controllers/CommunityController.MAX_BANNER_BYTES.
    public long MaxBannerBytes { get; init; }

    // The limit as text ("15 MB"),
    // the cover texts with {size} filled in.
    public string MaxBannerSize => FormatSize(MaxBannerBytes);
    public string CoverSubtitle => Text.CoverSubtitle.Replace("{size}", MaxBannerSize);
    public string CoverTooLarge => Text.CoverTooLarge.Replace("{size}", MaxBannerSize);

    // Bytes to whole megabytes, e.g. 10485760 -> "10 MB".
    public static string FormatSize(long bytes) => $"{bytes / (1024 * 1024)} MB";
}
