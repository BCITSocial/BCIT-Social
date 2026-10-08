using BCITSocial.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BCITSocial.FrontEnd.Controllers;

public class EventsController : Controller
{
    [HttpGet]
    public IActionResult Index(string? scope, string? campus) => View(new EventsViewModel(scope, campus));
}
