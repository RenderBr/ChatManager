using Newtonsoft.Json;
using System.IO;
using TShockAPI;
using System.Collections.Generic;

namespace ChatManager
{
	class Config
	{
		// Activate Username validation
		public bool ApplyNameValidator;

		// Checks to ban or disconnect joining users
		public bool BanIfInappropriateUsername;

		// Checks to allow nonalphanumeric characters in
		public bool CheckForAlphaNumInUsername;

		// Checks to allow spacekeys in names
		public bool CheckMaxSpacesInUsername;

		// Amount of space keys to allow
		public int MaxSpacesInUsername;

		// Activate Chat filter
		public bool ApplyChatFilter;

		// Speed in which messages are sent in chat.
		public int MsgSpamIntervalInSec;

		// boot reasons
		public Dictionary<string, string> DisconnectReasons;

		public static Config Read()
		{
			string configPath = Path.Combine(TShock.SavePath, "CMConfig.json");
			if (!File.Exists(configPath))
			{
				File.WriteAllText(configPath, JsonConvert.SerializeObject(Default(), Formatting.Indented));
				return Default();
			}
			try
			{
				var args = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
				if (args.DisconnectReasons.Keys.Count != 5)
				{
					TShock.Log.ConsoleError("[ChatManager] Configuration file was invalid. Please back-up and delete the current one to generate a new valid config file!");
					return Default();
				}
				return args;
			}
			catch
			{
				return Default();
			}
		}

		private static Config Default()
		{
			return new Config()
			{
				ApplyNameValidator = true,
				BanIfInappropriateUsername = false,
				CheckForAlphaNumInUsername = true,
				CheckMaxSpacesInUsername = true,
				MaxSpacesInUsername = 3,
				ApplyChatFilter = true,
				MsgSpamIntervalInSec = 5,

				DisconnectReasons = new Dictionary<string, string>
				{
					{ "MaxLengthReason", "Your name is too long. Please rejoin with a shorter name. Maximum amount of characters is 20." },
					{ "MaxSpacesReason", "You have too many spaces in your name. The maximum allowed is {amountofspaces}. Please rejoin with a shorter name." },
					{ "InappNameReason", "Inappropriate names are not allowed. Please rejoin with a tolerable name." },
					{ "NonAlphaNumReason", "Foreign characters in your username are now allowed. Please rejoin with only alphanumeric characters." },
					{ "SpamMessageReason", "You are sending messages too fast. Please slow down." }
				}
			};
		}
	}
}
