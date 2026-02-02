# Copilot Instructions

## Workspace map
- MyNewWeb: ASP.NET Core 8 MVC skeleton; default route {controller=Home}/{action=Index}/{id?}; see [MyNewWeb/Program.cs](MyNewWeb/Program.cs) and [MyNewWeb/Controllers/HomeController.cs](MyNewWeb/Controllers/HomeController.cs).
- MyWebApp: ASP.NET Core 10 minimal API returning Hello World; see [MyWebApp/Program.cs](MyWebApp/Program.cs).
- MyWebUbike: ASP.NET Core MVC (controllers + views) + API endpoint for Taipei YouBike open data; config key `tpiUbike` is required (see [MyWebUbike/Program.cs](MyWebUbike/Program.cs)).
- unzip_and_gzip / ungzip_and_zip / user_volte_qos_hour: .NET 8 console tools for CSV/zip pipelines (see each Program.cs).
- photo_at_location: Node.js Express server + Leaflet frontend to call Gemini 2.0 Flash for 2-step image generation; static assets in public/; entry [photo_at_location/server.js](photo_at_location/server.js).

## C# conventions
- ImplicitUsings + nullable enabled; prefer `var`, string interpolation, null-conditionals; keep Chinese UI text.
- Web apps use top-level Program.cs; register services via DI; controllers in Controllers/ with standard route attributes.
- CLI tools use internal static Main; favor streaming (buffers 64–80KB) to avoid large memory use.

## Build/run
- Restore once: `dotnet restore` at workspace root.
- MyNewWeb: `dotnet run --project MyNewWeb` (HTTPS redirection, default 7066).
- MyWebApp: `dotnet run --project MyWebApp` (Hello World root).
- MyWebUbike: `dotnet run --project MyWebUbike`; appsettings.json must contain `tpiUbike` URL for YouBike JSON.
- CLI publish pattern: `dotnet publish -c Release -r win-x64 -o ./publish --self-contained false` (see tool README for per-project paths).
- photo_at_location: `cd photo_at_location && npm install && npm start` (serves on port 3000).

## MyWebUbike specifics
- Services: `UbikeService` (DI with HttpClient + AppSettings) pulls JSON from `tpiUbike`, deserializes to `UbikeStationDto`, filters by `sarea` case-insensitive; exposes `AreaQryAsync(string area)`.
- API endpoint: [MyWebUbike/Controllers/UbikeController.cs](MyWebUbike/Controllers/UbikeController.cs) `GET /api/Ubike/areaQry?area=` returns `UbikeAreaQueryResponseDto` or error payloads.
- MVC view flow: Home/AreaQry view posts area string; controller logs exceptions and surfaces ViewBag.Error.

## CLI tool behaviors
- unzip_and_gzip: scans `*.zip`, extracts entries in-place; CSV cleaning replaces NULL tokens and scientific notation; non `imsi_imei_test*.csv` are gzipped then source CSV deleted; >4GB parts split as `name1.csv.gz`, `name2.csv.gz` with 64KB buffer; original zip removed.
- ungzip_and_zip: scans `*.gz`, streams decompress→zip per file (no temp files); cleans NULL tokens and scientific notation per line; reports size stats; leaves source .gz.
- user_volte_qos_hour: validates files named `user_volte_qos_hour*.csv`; requires exactly 24 comma fields with specific type rules (1 int, 2-4/7/10/14 string, others float, empty allowed); rewrites file with valid rows, logs bad rows to `.error.log` with timestamp + line.

## photo_at_location specifics
- Endpoint `POST /generate` expects form-data: `photo` (image, <=20MB), `lat`, `lng`, `apiKey` (Gemini). Uses multer memory store.
- Two-step Gemini flow: (1) background-only prompt using location name from Nominatim reverse geocode; (2) composite with user photo + generated background, face preservation prioritized; response is data URL base64.
- Proxy: honors `HTTPS_PROXY`/`HTTP_PROXY` via https-proxy-agent and disables axios proxy option; keep 120–180s timeouts.
- Frontend (public/app.js): Leaflet map with search via Nominatim; requires selecting map point + API key + photo before calling `/generate`; canvas renders returned image.

## Style and testing notes
- No automated tests; manual runs expected. CLI tools operate on current or passed directory; avoid deleting user files beyond documented behavior (CSV deletion after gz in unzip_and_gzip).
- Preserve existing log messages and Chinese/English mix; keep file paths cross-platform via `Path.Combine` in new C# code.
