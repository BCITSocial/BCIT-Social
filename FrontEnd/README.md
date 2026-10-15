# BCIT Social frontend starter

Minimal ASP.NET Core MVC / Razor Home shell for the frontend team. Requires the .NET 10 SDK. No npm or additional NuGet packages are needed.

## Run

Open a terminal in `FrontEnd`:

```sh
dotnet restore
dotnet watch run
```

Open the localhost address printed in the terminal. Alternatively, open `BCITSocial.FrontEnd.csproj` in Visual Studio and run it. Razor views require ASP.NET; Live Server cannot render them.

## Where to work

- `Views/Home/Index.cshtml`: Home feed skeleton and empty content areas.
- `Views/Shared/_Layout.cshtml`: shared document layout.
- `Views/Shared/_Header.cshtml` and `_Navigation.cshtml`: reusable app shell.
- `wwwroot/css/settings.css`: global colors, image references, font family, sizes, weights, and line heights.
- `wwwroot/css/site.css`: component styling, spacing, and responsive layout; consumes the global settings.
- `Models/HomeViewModel.cs`: empty presentation contract; add properties in coordination with the backend team.
- `Controllers/HomeController.cs`: GET-only action that renders the Home view.
- `Program.cs`: minimum MVC registration, static assets, and routing.

The controller and host exist only so Razor can run locally. There is no business logic, authentication, API, database, persistence, or sample data. Normal UI labels are static markup. Search, navigation to unfinished screens, and Create post are disabled. The empty regions are placeholders, not loading indicators or live data results.

Start by editing the Home view and shared partials. Add browser JavaScript under `wwwroot/js` only when an interaction needs it, using the layout's `Scripts` section for page scripts. Agree on presentation properties with the backend team before adding typed content partials. Do not put data access in views. When more routes exist, replace disabled navigation controls with links and make the active state depend on the current route.

The shell follows the shared structure and colors of the supplied Figma reference; it is not a complete implementation of the Communities screen. The earlier local demo remains separate.

## Global accessibility settings

Edit `wwwroot/css/settings.css` to change the appearance across all views. It loads before `site.css`. Keep all explicit color values, font choices, and decorative image URLs in this file; other styles reference them with `var(--setting-name)`. Text sizes use rem units to respect browser preferences. No font download is required.

There are no pictures in the starter. The optional `--image-page-background` setting accepts `none` or a local image URL relative to the stylesheet. Add future decorative image variables here too. Meaningful content images must use an HTML `img` with descriptive alt text and a source supplied by the eventual view model; CSS backgrounds cannot provide alternative text. User content is not a global theme setting.

When changing the palette, check text and focus contrast. Central settings make accessibility adjustments easier but do not automatically guarantee an accessible color combination.

## Check

```sh
dotnet build
```

Visit `/` and check the layout at desktop and mobile widths. Keyboard users can use the skip link to reach the main content.
