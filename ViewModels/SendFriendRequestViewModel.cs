using System.ComponentModel.DataAnnotations;

namespace BCIT_Social.ViewModels;

public class SendFriendRequestViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
}
