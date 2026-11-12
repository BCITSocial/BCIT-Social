namespace BCIT_Social.Models;

// Provides error information for the error view.
public class ErrorViewModel
{
    // Identifier for the request associated with the error.
    public string? RequestId { get; set; }

    // Indicates whether a request identifier should be displayed.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
