using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SusBot_Discord
{
    public class SusFileManager
    {
        private static readonly string configFileName = "SusBot_Discord.txt";
        private static readonly string linksFileName = "SusBot_Discord_Links.txt";
        
        private readonly SusDiscord _modInstance;
        private string _gamestateLocation = "";
        private string _playerStateLocation = "";


        public Dictionary<string, bool> PlayerStates = new Dictionary<string, bool>();

        internal SusFileManager(SusDiscord modInstance)
        {
            this._modInstance = modInstance;
        }

        internal bool LoadPreviousLinks()
        {
            if (!File.Exists(linksFileName)) return false;
            
            _modInstance.LogMod("Links file found. Attempting to load.");

            foreach (var line in File.ReadAllLines(linksFileName))
            {
                var split = line.Split(':');
                if (split.Length != 2) return false;
                ulong discId = 0;
                if (!ulong.TryParse(split[1], out discId)) continue;
                _modInstance._discordLinks[split[0]] = discId;
                _modInstance.LogMod($"Loaded link: {split[0]} - {discId}");
            }

            return true;
        }

        internal void SaveLinks()
        {
            using (var file = new StreamWriter(linksFileName))
            {
                foreach (var state in _modInstance._discordLinks)
                {
                    if (state.Key.Contains(":")) continue;
                    file.WriteLineAsync(state.Key + ":" + state.Value);
                    _modInstance.LogMod($"Saving link: {state.Key} - {state.Value}");
                }
            }
        }
        
        
        internal bool CheckForConfig(ref string tokenString)
        {
            if (!File.Exists(configFileName)) return false;

            _modInstance.LogMod("Config file found. Attempting to load.");
            
            string gameDirLoc = "";
            int found = 0;
            foreach (var line in File.ReadAllLines(configFileName))
            {
                var split = line.Split('|');
                if (split.Length != 2) return false;
                if (split[0] == "DiscordToken")
                {
                    tokenString = split[1];
                    found++;
                }
                else if (split[0] == "AmongUsLocation")
                {
                    gameDirLoc = split[1];
                    found++;
                }
            }

            if (found != 2)
            {
                _modInstance.LogMod("Could not load config.");
                return false;
            }

            return LoadFiles(gameDirLoc);

        }
        
        internal bool LoadFiles(string gameDirectory)
        {
            _modInstance.LogMod("Using Game Directory: " + gameDirectory);

            if (!File.Exists(gameDirectory + "\\Among Us.exe"))
            {
                _modInstance.LogMod("No Among Us executable found at address. Exiting.");
                return false;
            }

            _gamestateLocation = gameDirectory + "\\SusBot_gameStates.txt";
            _playerStateLocation = gameDirectory + "\\SusBot_playerStates.txt";

            while (!File.Exists(_playerStateLocation) || !File.Exists(_gamestateLocation))
            {
                _modInstance.LogMod(
                    "No game OR player state files found. Please check if Mod is running and press any key to check again.");
                Console.ReadKey();
            }


            return true;
        }


        internal async Task UpdateStates()
        {
            while (true)
            {
                PlayerStates = linesFromFile(_playerStateLocation);

                var tempGameStates = linesFromFile(_gamestateLocation);


                if (tempGameStates["connectedState"] != SusDiscord.Instance.IsConnected)
                {
                    SusDiscord.Instance.IsConnected = tempGameStates["connectedState"];
                    SusDiscord.Instance.StateChange();
                }

                if (tempGameStates["gameStarted"] != SusDiscord.Instance.IsStarted)
                {
                    SusDiscord.Instance.IsStarted = tempGameStates["gameStarted"];
                    SusDiscord.Instance.StateChange();
                }

                if (tempGameStates["meetingState"] != SusDiscord.Instance.IsMeeting)
                {
                    SusDiscord.Instance.IsMeeting = tempGameStates["meetingState"];
                    SusDiscord.Instance.StateChange();
                }

                await Task.Delay(100);
            }
        }

        private static Dictionary<string, bool> linesFromFile(string fileLocation)
        {
            var linesDic = new Dictionary<string, bool>();

            var lines = File.ReadAllLines(fileLocation);

            foreach (var line in lines)
            {
                var split = line.Split(':');

                linesDic[split[0]] = bool.Parse(split[1]);
            }

            return linesDic;
        }
    }
}