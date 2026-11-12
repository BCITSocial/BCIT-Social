namespace BCIT_Social.Models;

// Represents a photo attached to a post.
public class PostPhoto
{
    // Unique identifier for the photo record.
    public int Id { get; set; }

    // Post containing the photo.
    public int PostId { get; set; }

    // Post associated with this photo.
    public Post Post { get; set; } = null!;

    // Storage key used to locate the photo.
    public string StorageKey { get; set; } = null!;

    // Original filename of the uploaded photo.
    public string OriginalFileName { get; set; } = null!;

    // MIME type of the photo.
    public string ContentType { get; set; } = null!;

    // Size of the photo in bytes.
    public long FileSizeBytes { get; set; }

    // Position of the photo when displayed with the post.
    public int DisplayOrder { get; set; }

    // Records when the photo record was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}