using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace ChatManager
{
    [ApiVersion(2, 1)]
    public class ChatManager : TerrariaPlugin
    {
        private static ProfanityDictionary _dict;

        private static ProfanityFilter.ProfanityFilter _filter;

        private static DateTime[] ChatCooldown = new DateTime[256];

        private static Config Config = Config.Read();

        public ChatManager(Main game) : base(game)
        {
            Order = 1;
        }

        public override string Author
            => "Rozen4334";

        public override string Description
            => "A plugin that brings chat & username management to a new level!";

        public override string Name
            => "ChatManager";

        public override Version Version
            => new Version(1, 2);

        public override void Initialize()
        {
            Reload();
            GeneralHooks.ReloadEvent += ReloadWithArgs;
            ServerApi.Hooks.NetGetData.Register(this, OnPlayerJoin);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);

            PlayerHooks.PlayerChat += OnPlayerChat;
            
            Commands.ChatCommands.Add(new Command(Extensions.ManageChat, ProfanityManager, "managefilter", "mf"));
        }

        public void OnGreet(GreetPlayerEventArgs args)
        {
            ChatCooldown[args.Who] = DateTime.MinValue;
        }

        private static void OnPlayerJoin(GetDataEventArgs args)
        {
            if (Config.ApplyNameValidator == false)
                return;

            if (args.MsgID == PacketTypes.PlayerInfo)
            {
                TSPlayer player = TShock.Players[args.Msg.whoAmI];

                string[] iUserName = null;
                string userName = null;

                using (BinaryReader reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    reader.BaseStream.Position += 3;
                    iUserName = reader.ReadString().Split(' ');
                    userName = reader.ReadString();
                }

                // max length violation
                if (userName.Length > 20)
                {
                    Config.DisconnectReasons.TryGetValue("MaxLengthReason", out string value);
                    player.Disconnect(value);
                    return;
                }

                // max spacekeys violation
                if (iUserName.Length > Config.MaxSpacesInUsername + 1 && Config.CheckMaxSpacesInUsername == true)
                {
                    Config.DisconnectReasons.TryGetValue("MaxSpacesReason", out string value);
                    value.Replace("{amountofspaces}", Config.MaxSpacesInUsername.ToString());
                    player.Disconnect(value);
                    return;
                }

                // splitting username trigger
                for (int i = 0; i < iUserName.Length; i++)
                {
                    // inappropriate name trigger
                    if (_filter.ContainsProfanity(iUserName[i]))
                    {
                        Config.DisconnectReasons.TryGetValue("InappNameReason", out string value);
                        if (Config.BanIfInappropriateUsername == true)
                            player.Ban(value);
                        else
                            player.Disconnect(value);
                        return;
                    }

                    // alphanumeric trigger
                    if (!iUserName[i].IsAlphaNumeric() && Config.CheckForAlphaNumInUsername == true)
                    {
                        Config.DisconnectReasons.TryGetValue("NonAlphaNumReason", out string value);
                        player.Disconnect(value);
                        return;
                    }
                }
            }
        }

        private static void OnPlayerChat(PlayerChatEventArgs args)
        {
            if (Config.ApplyChatFilter == false)
                return;
            var iPlayer = args.Player.Index;

            if (ChatCooldown[iPlayer] != DateTime.MinValue && ChatCooldown[iPlayer] > DateTime.UtcNow.AddSeconds( - Config.MsgSpamIntervalInSec))
            {
                args.Player.SendErrorMessage("You are sending messages too fast. Please wait.");
                args.Handled = true;
                return;
            }

            if (!args.Player.HasPermission(Extensions.IgnoreSpamFilter))
                ChatCooldown[iPlayer] = DateTime.UtcNow;

            int raw = args.TShockFormattedText.LastIndexOf(args.RawText);

            if (args.Player.HasPermission(Extensions.IgnoreChatFilter))
            {
                args.TShockFormattedText = args.TShockFormattedText.Substring(0, raw) + args.RawText.AddColorTags();
                return;
            }
            args.TShockFormattedText = args.TShockFormattedText.Substring(0, raw) + _filter.CensorString(args.RawText).AddColorTags();
        }

        private void ProfanityManager(CommandArgs args)
        {
            var sub = args.Parameters.Count == 0 ? "help" : args.Parameters[0];
            switch (sub)
            {
                default:
                case "help":
                    {
                        var help = new List<string>()
                        {
                            "Command for adding and deleting chat/username filters to the server. (alias: /mf)",
                            "/managefilter add - Adds a word to the filter.",
                            "/managefilter del - Deletes a word from the filter.",
                            "/managefilter list - Lists the contents of the filter.",
                        };

                        args.Player.SendInfoMessage(string.Join("\n", help));
                    }
                    break;
                case "add":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Expected a word to add.");
                            return;
                        }

                        string badWord = string.Join(" ", args.Parameters.Skip(1));

                        bool added = AddNewProfanity(badWord);

                        if (added)
                        {
                            args.Player.SendSuccessMessage($"Successfully added {badWord} as profanity.");
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Profanity list already contains this word.");
                        }
                    }
                    break;
                case "delete":
                case "del":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Expected a word to remove.");
                            return;
                        }

                        string badWord = string.Join(" ", args.Parameters.Skip(1));

                        bool success = _dict.BannedWords.Remove(badWord) | _filter.RemoveProfanity(badWord);
                        _dict.Write();

                        if (success)
                        {
                            args.Player.SendSuccessMessage($"Successfully removed {badWord} as profanity.");
                        }
                        else
                        {
                            args.Player.SendSuccessMessage($"Could not find {badWord} to remove.");
                        }
                    }
                    break;
                case "list":
                    {
                        if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out int pg))
                        {
                            return;
                        }

                        PaginationTools.SendPage(args.Player, pg, _dict.BannedWords, _dict.BannedWords.Count,
                           new PaginationTools.Settings
                           {
                               HeaderFormat = "Profanity List ({0}/{1})",
                               FooterFormat = "Type /managefilter list {0} for more.",
                               NothingToDisplayString = "There are currently no profanity items to filter."
                           });
                        break;
                    }
            }
        }

        private bool AddNewProfanity(string args)
        {
            bool added = _dict.BannedWords.Add(args);
            _filter.AddProfanity(args);
            _dict.Write();

            return added;
        }

        private void ReloadWithArgs(ReloadEventArgs args)
        {
            try
            {
                Reload();
                args.Player.SendSuccessMessage("[ChatManager] Successfully reloaded config & filter!");
            }
            catch
            {
                TShock.Log.ConsoleError("Unable to reload config, check for any missed characters like ',' and quotes");
            }
        }

        private void Reload()
        {
            Config = Config.Read();
            _dict = ProfanityDictionary.Read();
            _filter = new ProfanityFilter.ProfanityFilter(_dict.BannedWords.ToArray());
        }
    }
}
