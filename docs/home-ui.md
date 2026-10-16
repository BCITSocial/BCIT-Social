# BCIT Social Home UI

The existing Home shell is part of the main ASP.NET Core MVC application. Requires the .NET 10 SDK and the main application configuration.

## Run

Open a terminal in the project root (the directory containing `BCIT-Social.csproj`):

```sh
dotnet restore
dotnet watch run
```

Open the localhost address printed in the terminal. Alternatively, open `BCIT-Social.csproj` in Visual Studio and run it. Razor views require ASP.NET; Live Server cannot render them.

## Where to work

- `Views/Home/Index.cshtml`: Home feed skeleton and empty content areas.
- `Views/Events/Index.cshtml` and `_EventCard.cshtml`: Events page (banner, status chips, Discover/My calendar tabs, filter toolbar, card grid). Cards are not hard-coded: each entry in `wwwroot/content/events-text.json` (`{ "events": [ ... ] }`) makes one card, and with no entries no cards are shown. Entry fields mirror `EventCardViewModel`: `title`, `description`, `startsAt`/`endsAt` (ISO, e.g. `2026-10-08T17:30:00`), `hostName`, `location`, `campus`, `coverImageUrl`, `coverImageAlt`, `goingCount`, `attendees` (`name`, `photoUrl`), `rsvp` (`None`, `Interested`, `Going` or `Hosting`), `isCancelled`. A row is left out when its value is empty. The JSON is read on every request, so edit and refresh. Filters are a GET form bound to `EventsQuery`, but they do not filter the JSON entries yet. Search for `TODO(BE)` to find where backend replaces the JSON with real events, communities and the RSVP/manage endpoints. The view models live in `ViewModels/` (`EventsViewModel`, `EventCardViewModel`, `AttendeeViewModel`) and are the presentation contract; agree on them with the backend team. The page uses `_HomeLayout` like Home.
- `Views/Shared/_HomeLayout.cshtml`: shared document layout.
- `Views/Shared/_Header.cshtml` and `_Navigation.cshtml`: reusable app shell.
- `wwwroot/css/settings.css`: global colors, image references, font family, sizes, weights, and line heights.
- `wwwroot/css/home.css`: component styling, spacing, and responsive layout; consumes the global settings.
- `Models/HomeViewModel.cs`: empty presentation contract; add properties in coordination with the backend team.
- `Controllers/HomeController.cs`: the main Home controller, also serving Privacy and Error.
- `Program.cs`: shared application startup, including Identity, database registration, static assets, and routing.

The Home shell contains no data access or sample data. It now runs within the main application, which retains its existing authentication and database setup. Normal UI labels are static markup. Search, navigation to unfinished screens, and Create post are disabled. The empty regions are placeholders, not loading indicators or live data results.

Start by editing the Home view and shared partials. Add browser JavaScript under `wwwroot/js` only when an interaction needs it, using the layout's `Scripts` section for page scripts. Agree on presentation properties with the backend team before adding typed content partials. Do not put data access in views. When more routes exist, replace disabled navigation controls with links and make the active state depend on the current route.

The shell follows the shared structure and colors of the supplied Figma reference; it is not a complete implementation of the Communities screen. The earlier local demo remains separate.

## Global accessibility settings

Edit `wwwroot/css/settings.css` to change the Home shell appearance. Other pages retain their existing layout and styles. It loads before `home.css`. Keep all explicit color values, font choices, and decorative image URLs in this file; other styles reference them with `var(--setting-name)`. Text sizes use rem units to respect browser preferences. No font download is required.

There are no pictures in the starter. The optional `--image-page-background` setting accepts `none` or a local image URL relative to the stylesheet. Add future decorative image variables here too. Meaningful content images must use an HTML `img` with descriptive alt text and a source supplied by the eventual view model; CSS backgrounds cannot provide alternative text. User content is not a global theme setting.

When changing the palette, check text and focus contrast. Central settings make accessibility adjustments easier but do not automatically guarantee an accessible color combination.

## Check

```sh
dotnet build
```

Visit `/` and check the layout at desktop and mobile widths. Keyboard users can use the skip link to reach the main content.

