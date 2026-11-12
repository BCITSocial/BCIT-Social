namespace BCIT_Social.ViewModels;

// A file or image sent in the conversation, shown under "Shared media" in the details panel.
public class SharedMediaViewModel
{
    public int Id { get; init; }
    public string FileName { get; init; } = "";
    // Leave null for files without a preview; the tile then shows a file icon.
    public string? ThumbnailUrl { get; init; }
    public DateTime SharedAt { get; init; }
}
