using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BCIT_Social.ViewModels;

// The create event popup (a front-end mockup for now). The fields match CommunityEvent so a
// future create event action can bind it.
public class CreateEventViewModel
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = null!;

    // The browser's datetime-local value, in the user's local time.
    [Required]
    public DateTime? EventDate { get; set; }

    // Optional: the community hosting the event. The user must be a member.
    public int? CommunityId { get; set; }

    // Up to 3 tags, the same tags posts use.
    public List<int> TagIds { get; set; } = new();

    // The tag choices for the Tags picker. Never read from the form.
    [BindNever]
    public List<SelectListItem> Tags { get; set; } = new();

    // The user's communities for the "Hosted by" dropdown. Never read from the form.
    [BindNever]
    public List<SelectListItem> Communities { get; set; } = new();
}
