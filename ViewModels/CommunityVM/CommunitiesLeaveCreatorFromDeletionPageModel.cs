using System;
using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Author: Joshua Trepanier
/// Last Revised: Nov 19, 2026
/// Description: Model for grabbing the new owners ID of a community from the
/// deletion page (seperate from LeaveCreatorModel as I only need the creatorId)
/// </summary>
public class CommunitiesLeaveCreatorFromDeletionPageModel
{
    public string CreatorId { get; set; } = "";
}
