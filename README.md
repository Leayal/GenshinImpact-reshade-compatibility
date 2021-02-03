# GenshinImpact-reshade-compatibility
 Reshade compatibility for Genshin Impact game. Say no more to `manually renaming dxgi.dll file back and forth`.

# Credit
 Thanks to [`Want Reshade to work?` post on miHoYo's forum](https://www.hoyolab.com/genshin/article/29341).

# Disclaimer
 I don't know if the game allows ReShade or not. **Please use ReShade at your own risk**.

# Notice
 - This application is **not** an injector/loader. It's just simply a file renamer. Thus, it will not inject ReShade.dll to the game process.
 - **Do NOT run this application as administration, unless required**. For example: Your game data is in a place that only admin can write files to. The application can't rename the ReShade file unless you elevate it as Admin.
 - The application has to run before launching the game. Run it while the game is already running will do nothing.
 - The game must be launched with the game launcher, running `GenshinImpact.exe` directly won't work, because if you do that, the game will **not load dxgi.dll and it will also rename dxgi.dll to something else**. Therefore, please launch the game with the game launcher.
 - It's recommended to keep the application running until the application says `renamed reshade back to: dxgi.dll` after you exit the game. Otherwise, you will have to rename the file back to `dxgi.dll` yourself.
 - You still have to install and configure ReShade by yourself. [ReShade download page here](https://reshade.me/#download).

# Explaination
 The application will wait for the game to be launched (by you). Then it will see whether the game directory has `dxgi.dll` file, if `dxgi.dll` exists, it will wait until the game loads the .dll file then renaming it to a random name. Finally, the application will wait until the game exits and rename the file with random name above back to `dxgi.dll`. You also don't have to restart this application everytime you launch the game.
