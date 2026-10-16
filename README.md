# BCIT Social

One ASP.NET Core MVC application targeting .NET 10. Run commands from this directory:

```sh
dotnet restore
dotnet watch run
```

The application uses the existing SQL Server connection configuration and ASP.NET Core Identity. Open `BCIT-Social.csproj` in Visual Studio, or use the localhost URL printed by the CLI.

## Project structure

- `Controllers/`: MVC controllers, including the single Home controller.
- `Models/`: database models and view models.
- `Data/`: database context and Entity Framework migrations.
- `Views/`: all MVC views and shared layouts/partials.
- `Areas/Identity/`: Identity Razor Pages configuration.
- `wwwroot/`: CSS, JavaScript, static content, and third-party libraries.
- `Properties/`: local launch profiles.
- `docs/`: UI development notes.
- `Program.cs`: application startup, authentication, database registration, and routing.

The Home starter UI is now `Views/Home/Index.cshtml`, using `Views/Shared/_HomeLayout.cshtml` and `wwwroot/css/home.css`. The other pages retain `_Layout.cshtml` and `site.css`. These layouts serve different existing page designs. There is no separate frontend project or startup process.

See [Home UI notes](docs/home-ui.md) for styling conventions. Build with `dotnet build`.
