# RedEye

![GitHub Release](https://img.shields.io/github/v/release/ryzhpolsos/redeye)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/ryzhpolsos/redeye/total)
![GitHub top language](https://img.shields.io/github/languages/top/ryzhpolsos/redeye)
![GitHub License](https://img.shields.io/github/license/ryzhpolsos/redeye)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/ryzhpolsos/redeye)

RedEye is a free, open-source, very flexible, fully customizable and extendable Windows shell. Say goodbye to laggy Explorer with fixed taskbar size!

**Take your desktop look under YOUR control!**

## Why RedEye?
1. Explorer is laggy piece of vibecode with CPU-consuming start menu components on React Native;
2. Modern Windows lacks customizability. RedEye takes it back - it allows you to customize everything you want;
3. RedEye won't (probably) break after a Windows update. Almost all the code is using only documented Windows APIs;
4. You don't need to know any programming language to customize RedEye - just take some plugins and create your perfect UI layout with simple XML-based markup.

## Implemented features
1. Extensible modular core
2. XML-based markup
3. Plugin system and APIs for every part of the shell
4. Some basic widgets (such as window list, start menu, wallpaper, clock and so on)
5. Fully customizable hotkeys

## In development
1. WiFi network manager widget
2. Volume control (only hotkeys are present yet)
3. Tray icon support
4. Window manager :>

## Technical stack
- C# (.NET Framework 4.8)...
- and a lot of WinAPI magic :3

## Screenshots
<details>
    <summary>Expand</summary>

![Screenshot 1](https://raw.githubusercontent.com/ryzhpolsos/redeye/refs/heads/main/screenshots/1.png)

![Screenshot 2](https://raw.githubusercontent.com/ryzhpolsos/redeye/refs/heads/main/screenshots/2.png)

</details>

## Building
You'll need [.NET SDK](https://dotnet.microsoft.com/en-us/download). 

```
git clone https://github.com/ryzhpolsos/redeye
cd redeye
dotnet build -c Release
```

## Installing
1. Go to "Releases" section and download the last release
2. Extract the archive in any folder you like
3. Run the script named `install.bat`
4. Log off and log on back to see your new shell.

## Uninstalling
Run the script named `uninstall.bat` in the RedEye folder.

## License
The project is developed and distributed under [The MIT License](https://github.com/ryzhpolsos/redeye/blob/main/LICENSE).
