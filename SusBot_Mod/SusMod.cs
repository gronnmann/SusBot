using System.IO;
using Il2CppSystem.Collections.Generic;
using MelonLoader;

namespace SusBot_Mod
{
    public class SusMod : MelonMod
    {
        private static readonly List<PlayerControl> Players = PlayerControl.AllPlayerControls;

        private static bool _gameStarted;
        private static string _lastExiledPlayer = "";
        private static bool _currentMeetingStatus;
        private static bool _connected;

        private readonly Dictionary<string, bool> _gameStates = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _playerStates = new Dictionary<string, bool>();


        public override void OnApplicationStart()
        {
            LoggerInstance.Msg("SusBotMod loaded.");
        }


        public override void OnUpdate()
        {
            var client = AmongUsClient.Instance;
            if (client == null) return;

            //Always check if connected
            if (client.AmConnected != _connected)
            {
                _connected = client.AmConnected;
                LoggerInstance.Msg("Connection state change: Connected = " + _connected);
                UpdateInfoFile();
                _playerStates.Clear();
            }


            //Check if game started state different - if then trigger to send player data and reset last voted off
            var isGameStarted = client.IsGameStarted;
            if (isGameStarted != _gameStarted)
            {
                _lastExiledPlayer = "";
                _gameStarted = isGameStarted;
                LoggerInstance.Msg("Game start state change: GameStarted = " + _gameStarted);
                UpdateInfoFile();
            }

            //check if meeting state different - if then trigger to send player data
            var meetingHud = MeetingHud.Instance;
            var isMeeting = meetingHud != null;

            if (isMeeting != _currentMeetingStatus)
            {
                _currentMeetingStatus = isMeeting;
                UpdateInfoFile();
            }

            //Always keep track of players voted out - they are not dead at moment of being voted out
            if (isMeeting && meetingHud.exiledPlayer != null) _lastExiledPlayer = meetingHud.exiledPlayer.PlayerName;
        }

        private void UpdateInfoFile()
        {
            _gameStates.Clear();
            _playerStates.Clear();

            _gameStates["connectedState"] = _connected;
            _gameStates["gameStarted"] = _gameStarted;
            _gameStates["meetingState"] = _currentMeetingStatus;


            /*LoggerInstance.Msg("");
            LoggerInstance.Msg("Meeting state change: " + _currentMeetingStatus);
            LoggerInstance.Msg("Last exiled player: " + _lastExiledPlayer);
            LoggerInstance.Msg("");*/

            foreach (var p in Players)
            {
                var data = p.Data;

                //Either voted off or really dead
                var dead = p.name == _lastExiledPlayer || data.IsDead;


                _playerStates[p.name] = dead;

                //LoggerInstance.Msg("Player: " + p.name + ", dead: " + dead);
            }
            //LoggerInstance.Msg("");

            DictionaryToFile("SusBot_gameStates.txt", _gameStates);
            DictionaryToFile("SusBot_playerStates.txt", _playerStates);
        }


        //Simple method for saving dictionaries as lines in file.
        private static void DictionaryToFile(string fileName, Dictionary<string, bool> dictionary)
        {
            using (var file = new StreamWriter(fileName))
            {
                foreach (var state in dictionary)
                {
                    if (state.Key.Contains(":")) continue;

                    file.WriteLineAsync(state.Key + ":" + state.Value);
                }
            }
        }
    }
}