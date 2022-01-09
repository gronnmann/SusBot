using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SusBot_Discord
{
    public class SusDiscord
    {
        private static string _token;
        public static SusDiscord Instance;

        internal readonly Dictionary<string, ulong> _discordLinks = new Dictionary<string, ulong>();

        private readonly List<ulong> voiceModified = new List<ulong>();

        private SocketGuild _channel;
        private DiscordSocketClient _client;

        private SusFileManager _fileManager;

        public bool IsConnected = false;
        public bool IsMeeting = false;
        public bool IsStarted = false;



        public static async Task Main(string[] args)
        {
            Instance = new SusDiscord();
            Instance._fileManager = new SusFileManager(Instance);
            

            if (!Instance._fileManager.CheckForConfig(ref _token))
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: SusBotDiscord.exe [Among Us folder] [Discord Bot token]");
                    return;
                }
                if (!Instance._fileManager.LoadFiles(args[0])) return;
                _token = args[1];
            }
            
            await Instance.MainAsync();
        }


        private async Task MainAsync()
        {
            //Start file scanning
            _fileManager.UpdateStates();
            _fileManager.LoadPreviousLinks();

            StartDiscord();

            await Task.Delay(-1);
        }

        private async Task StartDiscord()
        {
            //Make bot instance and start logging
            _client = new DiscordSocketClient();
            _client.Log += LogDiscord;

            Instance.LogMod("Using Discord token: " + _token);
            await _client.LoginAsync(TokenType.Bot, _token);

            await _client.StartAsync();


            _client.Ready += () =>
            {
                LogMod("Bot is connected.");
                return Task.CompletedTask;
            };


            _client.MessageReceived += MessageReceived;
            
            
            await StateChange();
        }


        private async Task MessageReceived(SocketMessage e)
        {
            var user = e.Author;

            var msg = e.Content;

            var msgSplit = msg.Split('!');

            if (!msg.StartsWith("sus!") || msgSplit.Length < 2) return;

            var args = msg.Split(' ');

            if (args[0].Equals("sus!start"))
            {
                var channel = e.Channel as SocketGuildChannel;
                if (channel == null)
                {
                    e.Channel.SendMessageAsync("Some error occured.");
                    LogMod("Guild attempted to watch returned error: guild is null (message: '"
                           + msg + "' by " + user.Username + "#" + user.Discriminator);
                    return;
                }

                _channel = channel.Guild;

                StateChange();
                
                e.Channel.SendMessageAsync($"Starting bot in {_channel.Name}");

                LogMod("Watching guild: " + _channel.Name);
            }
            else if (args[0].Equals("sus!link"))
            {
                if (args.Length < 2)
                {
                    e.Channel.SendMessageAsync("Usage: sus!link [Among Us username]");
                    return;
                }

                _discordLinks[args[1]] = user.Id;

                var linkMsg = $"Created link {user.Username}#{user.Discriminator} ({user.Id}) - {args[1]}";
                e.Channel.SendMessageAsync(linkMsg);
                LogMod(linkMsg);
            }
            else if (args[0].Equals("sus!stop"))
            {
                e.Channel.SendMessageAsync("Stopping bot");
                await UnVoiceModifyAll();
                _channel = null;
            }
            else if (args[0].Equals("sus!savelinks"))
            {
                _fileManager.SaveLinks();
                e.Channel.SendMessageAsync("Saved all links.");
            }
        }

        internal async Task StateChange()
        {
            if (_channel == null) return;

            LogMod("");
            LogMod("State change:");
            LogMod("Connected: " + IsConnected);
            LogMod("Started: " + IsStarted);
            LogMod("Meeting: " + IsMeeting);
            LogMod("");

            if (IsConnected && IsStarted)
            {
                
                foreach (var player in _fileManager.PlayerStates)
                {
                    if (!_discordLinks.ContainsKey(player.Key)) continue;

                    var discId = _discordLinks[player.Key];

                    if (IsMeeting)
                    {
                        switch (player.Value)
                        {
                            case true:
                                await SetPlayerVoiceState(discId, false, true);
                                break;
                            case false:
                                await SetPlayerVoiceState(discId, false, false);
                                break;
                        }
                    }
                    else
                    {
                        switch (player.Value)
                        {
                            case true:
                                await SetPlayerVoiceState(discId, false, false);
                                break;
                            case false:
                                await SetPlayerVoiceState(discId, true, false);
                                break;
                        }
                    }
                    
                    //if (!player.Value) SetPlayerVoiceState(discId, true, false);
                    //LogMod("State: " + player.Key + " : " + (player.Value ? "dead" : "alive") + " (disc id: " +
                     //      discId + ")");
                }
            }
            else
            {
                UnVoiceModifyAll();
            }
        }


        private async Task SetPlayerVoiceState(ulong id, bool deaf, bool muted)
        {

            SocketGuildUser user = _channel.GetUser(id);

            if (user == null) return;
            
            LogMod($"Setting voice state: {user.Username}#{user.Discriminator}: deaf: {deaf}, muted: {muted}");
            await _channel.GetUser(id).ModifyAsync(props =>
            {
                props.Deaf = deaf;
                props.Mute = muted;
            });
            if (deaf || muted)
            {
                if (!voiceModified.Contains(id))
                {
                    voiceModified.Add(id);
                }
            }
            else
            {
                voiceModified.Remove(id);
            }
        }

        private async Task UnVoiceModifyAll()
        {
            LogMod("Unmuting all...");
            foreach (var p in voiceModified.ToList())
            {
                await SetPlayerVoiceState(p, false, false);
                voiceModified.Remove(p);
            }
        }

        private Task LogDiscord(LogMessage msg)
        {
            Console.WriteLine($"[Discord] {msg.ToString()}");
            return Task.CompletedTask;
        }

        internal async void LogMod(string msg)
        {
            Console.WriteLine($"[SusMod] [{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {msg}");
        }
    }
}
