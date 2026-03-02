# RedEye

RedEye is a free, open-source, very flexible, fully customizable and extendable Windows shell. Say goodbye to laggy Explorer with fixed taskbar size!


![GitHub Release](https://img.shields.io/github/v/release/ryzhpolsos/redeye)
![GitHub commit activity](https://img.shields.io/github/commit-activity/w/ryzhpolsos/redeye)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/ryzhpolsos/redeye/total)
![GitHub top language](https://img.shields.io/github/languages/top/ryzhpolsos/redeye)
![GitHub License](https://img.shields.io/github/license/ryzhpolsos/redeye)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/ryzhpolsos/redeye)

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
- C# (.NET Framework 4.8),
- A bit of C...
- and a lot of WinAPI magic :3

## Screenshots
<details>
    <summary>Expand</summary>

    ![Screenshot 1](https://raw.githubusercontent.com/ryzhpolsos/redeye/refs/heads/main/screenshots/1.png)

    ![Screenshot 2](https://raw.githubusercontent.com/ryzhpolsos/redeye/refs/heads/main/screenshots/2.png)

</details>

## Building
You'll need [.NET SDK](https://dotnet.microsoft.com/en-us/download) and MinGW-w64 (i recomment portable [w64devkit](https://github.com/skeeto/w64devkit)).

```
git clone https://github.com/ryzhpolsos/redeye
cd redeye
dotnet build -c Release -p Wmx=1
```

## Installing
After building, run the following command in the project root folder:
```
reg add "HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /d "%CD%\bin\Release\netframework4.8\redeye.exe" /t REG_SZ /f
```
Then log off and log on back to see your new shell.

## Uninstalling
Run the following command:
```
HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon
```
Then log off and log on back.

## License
The project is developed and distributed under [The MIT License](https://github.com/ryzhpolsos/redeye/blob/main/LICENSE).
