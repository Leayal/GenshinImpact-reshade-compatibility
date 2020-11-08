# GenshinImpact-reshade-compatibility
 Reshade compatibility for Genshin Impact game

# Credit
 Thanks to [`Want Reshade to work?` post on miHoYo's forum](https://forums.mihoyo.com/genshin/article/29341).

# Notice
 - This application is **not** an injector/loader. It's just simply a file renamer. Thus, it will not inject ReShade.dll to the game process.
 - The application has to run before launching the game. Run it while the game is already running will do nothing.
 - The game must be launched with the game launcher, running `GenshinImpact.exe` directly won't work, because if you do that, the game will **not load dxgi.dll and it will also rename dxgi.dll to something else**. Therefore, please launch the game with the game launcher.
 - It's recommended to keep the application running until the application says `renamed reshade back to: dxgi.dll` after you exit the game. Otherwise, you will have to rename the file back to `dxgi.dll` yourself.
 - You still have to install and configure ReShade by yourself. [ReShade download page here](https://reshade.me/#download).