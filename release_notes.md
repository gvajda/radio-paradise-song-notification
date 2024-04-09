# Major updates

See details in README

- Send rating on past songs from Windows Action Center
- Added login form with masked out password field
- Updated icons
- Handle 2050 channel
- Option to choose app data folder location

## Features

- Added option to store cache next to the application executable
- Added option to migrate config folder
- Added animated GIF files to README
- Updated README with config related info
- Limited concurrent player watchers to one at a time
- Added farewell toast when user logs out
- Added option to display RP banner on detailed toast

## Enhancements

- Reduced .exe size and memory usage
- Cleaned up NuGet packages (completely removed Newtonsoft.Json and RestSharp)
- Updated all to packages to latest version
- Log file readability improvements
- Moved RP tracking into an IPlayerWatcher implementation
- Implemented factory pattern for RpApiClient
- Added HTTP retry policy
- Implemented factory pattern for Beefweb client and MusicBeeIPC
- Minor tweaks to album art cache handling
- Cleaned up cache handling when chosen on startup
- Refactored RpApiHandler to use System.Net.Http instead of RestClient
- Organized event handlers in RpApplicationCore
- Handled corrupt cookies
- Extended GitHub workflow with release draft creation
- Disposed Logger after each write (don't lock the log files)

## Miscellaneous

- Renamed ToastHandler
- Renamed TrayIconMenu
- Renamed and consolidated Beefweb client
- Renamed to cleanup various parts of the code
- Cleaned up NuGet packages and using statements
- Cleaned up bloated generated code for Beefweb client
- Moved all event handlers in RpApplicationCore and organized them
