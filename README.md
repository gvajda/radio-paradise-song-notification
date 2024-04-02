# Radio Paradise song info desktop notification

Optimized for Windows 11

[Download the RP_Notify app](https://github.com/gvajda/radio-paradise-song-notification/releases/latest/download/RP_Notify.exe)

![Build](https://github.com/gvajda/radio-paradise-song-notification/workflows/Build/badge.svg)

## Summary

My goal was to receive non-intrusive but detailed updates of the currently played song in [Radio Paradise](https://radioparadise.com/) and the option to send song ratings without the need to interrupt what I was doing and open the website or search for browser tabs.

<p align="center"><img src=".screenshots/notification-simple.png" alt="notification-simple"/></p>

**Disclaimer:** This is not an official Radio Paradise product. The logo is owned by Radio Paradise and the source of all displayed data - including album art - is the Radio Paradise REST API.

## Getting started

The RP_Notify app doesn't need installation, just download the .exe file using the link above and save it anywhere on your computer.

On the first start, the app will prompt you to choose the location of the RP_Notify_Data folder that will contain your saved settings, logs, etc ([more on this below](#configuration)). The size of this folder will not exceed ~20MB.

The app will also display an icon in the Windows tray, all settings can be reached by right-clicking on this icon. I recommend to drag this icon in the visible section of the Windows tray for easy access:

<p align="center"><img src=".screenshots/notification-simple.png" alt="notification-simple"/></p>

## Features

### Radio Paradise stream tracking

- **Track official Radio Paradise players**
  - Display updates of songs played in the browser or official mobile apps, including the "My Favorites" channel
  - The app will notice if you skip a song or you switch to a different channel
  - Due to how the Radio Paradise backend keeps track of API requests, this feature works without logging in to the app if the player/browser is on the same network (has the same IP), but login data is provided then you can track RP stream played on an official player at a remote location
- **Support for all channels**
  - The channels list is updated on startup (channels added/removed in the future will display properly)
  - The 2050 channel is a bit tricky, the app will display updates when a song is played, but not during the conversation
- **Audio player integration**
  - Track stream played in Foobar2000 or MusicBee audio players - [see below](#audio-player-integration)

### Notification visuals

- **Show album art**
  - Configure image size in the menu
  - Optional RP banner
- **Show song rating**
  - Optional - in case you prefer not to know the crowd rating before you send in your own

### Song rating

- **Support for rating songs**
  - Only available if the user is logged in
  *Note*: The RP_Notify app does NOT save or log your password. The app retains only the identical cookie that is stored by your browser upon logging into the official site. You have the option to erase all stored information through the menu in the system tray (App Settings/Delete app data)
- **Prompt for song rating**
  - Display a toast notification with the song rating input field 20 seconds before the song ends or if the channel is changed
  - Optional - *Tip*: very useful to grow the song pool of the "My Favorites" channel
- **Trigger the song rating tile from the Windows Action Center**
  - Like all other Windows notifications, the RP_Notify notifications will remain visible in the Windows Action Center showing a history of songs you listened to. This enables one to send a rating for a song that was played in the past.
  - This will also work when the app is not open

<p align="center"><img src=".screenshots/notification-simple.png" alt="notification-simple"/></p>

### Other

- **Tooltip**
  - Activated by hovering over the tray icon
  - Quick and short song info including remaining time

<p align="center"><img src=".screenshots/notification-simple.png" alt="notification-simple"/></p>

## Under the hood

### Configuration

On the first start, the app will prompt you to choose the location of the RP_Notify_Data folder that will contain your saved settings, logs, cached album art and the optional cookie file if you choose to log in and send song ratings using the app.

The folder options are the following:

- **Next to the application (RP_Notify.exe)**
  - The default option
  - This is the 'portable' mode - until the *RP_Notify.exe* file and the *RP_Notify_Data* folder are in the same place, it will find it and look for the settings inside
  - This option enables to use the app from a USB stick on a different computer or have multiple copies of the app with different settings and user logins
- **C:\users\YOURNAME\\.AppData\Roaming\RP_Notify_Data**
  - With this option, you can move the *RP_Notify.exe* file around, it will always start with the saved settings
- **Don't keep anything (clean up on exit)**
  - When you are not sure if you want the app to save anything just yet
  - The folder will be created because the app is implemented in a way to persist the settings when it changes, but it will be deleted when the app exits

*Note*: the folder location can be changed after the initial choice from the app settings

<p align="center"><img src=".screenshots/notification-simple.png" alt="notification-simple"/></p>

### Audio player integration

Radio Paradise can be played in any audio player using the [stream links](https://radioparadise.com/listen/stream-links) and the application works with some of them to enable/disable song notification when a stream is started/stopped and change channels based on which channel is played. This feature can be enabled one-by-one for each supported audio player. Please see the details below.

#### Foobar2000

The integration requires the *foo_beefweb* plugin for Foobar2000 that provides REST interface to the player.

[Download foobar2000 for Windows](https://www.foobar2000.org/download)

[Download foo_beefweb plugin](https://www.foobar2000.org/components/view/foo_beefweb)

#### MusicBee

The integration requires the *MusicBeeIPC* plugin for MusicBee that provides an API and SDK for several various programming languages to the player.

[Download MusicBee for Windows](https://getmusicbee.com/downloads/)

[Download MusicBeeIPC plugin](https://getmusicbee.com/forum/index.php?topic=11492.msg70007)

## Screenshots

### Simple notification when a song starts

![notification-simple](.screenshots/notification-simple.png)

### Detailed notification - when double-click on tray icon

![notification-detailed](.screenshots/notification-detailed.png)

### Tray menu

![tray-menu](.screenshots/tray-menu.png)

## About the project

This is a hobby project to obtain deeper knowledge of C# and practice

- design patterns
- dependency injection
- sync/async
- pub-sub
- REST calls
- Windows Toast notifications
- logging and error handling
