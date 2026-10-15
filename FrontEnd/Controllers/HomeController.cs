using BCITSocial.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BCITSocial.FrontEnd.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new HomeViewModel());
}
