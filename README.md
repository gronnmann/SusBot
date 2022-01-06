# SusBot

![Showcase of bot](https://gfycat.com/glaringfoolhardyarabianhorse.gif)

Bot for automatic deafening of players during the round in the game Among Us. Keep in mind it's my first C# project, so it might contain a lot of bugs. Consists of two modules, both needed for the operation.
You should install AmongUs Module first, and then Discord Module.

Both can be found in the Releases tab.

## AmongUs Module
The AmongUs Module requires [MelonLoader](https://github.com/LavaGang/MelonLoader), and is basically an mod for AmongUs, which reads the game data and dumps it in two files in the base game directory:
```
SusBot_gameStates.txt
SusBot_playerStates.txt
```
###### Installation of AmongUs Module
1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader). For AmongUs, you need to use the launch option "--melonloader.disablestartscreen", as else the game will crash.
2. Launch the game. A Mods folder should have been generated in the game files.
3. Put the SusBot_Mod.dll file in the Mods folder.
4. You're done! Try joining a game, and see if the game data folders mentioned earlier appear in the Among Us folder.

## Discord Module
The Discord Module reads the files dumped by the AmongUs Module, and uses them to detect changes in the game's state, and then deafens the alive players during the round.
###### Installation of AmongUs Module
1. [Create a Discord Bot](https://www.writebots.com/discord-bot-token/), and get it's token ready. Make sure it has permissions to both read and write messages and deafen players.
2. Unzip the SusBot_Discord.exe and it's corresponding files to a freely chosen folder.
3. Create a new file, SusBot_Discord.txt. Here, you need to put these two lines:
```
AmongUsLocation|(location to Among Us directory here)
DiscordToken|(Discord Bot token you got in Step 1)
```
Save the file, and make sure it's in the same directory as SusBot_Discord.exe.
4. Open the command line, and navigate to the folder. Launch the bot using SusBot_Discord.exe

Alternate Step 3:
Instead of creating the text file, you can also launch the bot using the command:
```
SusBot_Discord.exe [Among Us directory] [Discord Token]
```
Keep in mind the alternate way has shown to be buggy if there's any spaces in any of the directories.

###### Launching of bot
After installing the bot, you need to launch it every time you want to use the features. The bot doesn't remember any discord user - among us player - links, so you need to follow this instructions everytime you restart it.
1. Make every player in the game use the command
```
sus!link [Among Us username]
```
This links their Among Us name with their Discord user.

2. Use the command
```
sus!start
```
This binds the bot to your server, and makes it start working. When done playing, it can be unlinked using
```
sus!stop
```
Alternatively, you can just exit the program. Note the Among Us Nick - Discord user links will then be gone.

