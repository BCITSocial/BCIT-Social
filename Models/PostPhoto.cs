namespace BCIT_Social.Models;

public class PostPhoto
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public string StorageKey { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSizeBytes { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}