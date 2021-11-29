using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TShockAPI;

namespace ChatManager
{
    public class ProfanityDictionary
    {
        private static readonly string FilePath = Path.Combine(TShock.SavePath, "CMFilter.json");

        public HashSet<string> BannedWords { get; set; }

        public bool CheckRegistered { get; set; }

        public static ProfanityDictionary Read()
        {
            ProfanityDictionary dict;
            if (File.Exists(FilePath))
            {
                dict = JsonConvert.DeserializeObject<ProfanityDictionary>(File.ReadAllText(FilePath));
            }
            else
            {
                dict = new ProfanityDictionary
                {
                    BannedWords = new HashSet<string>(Extensions.DefaultBadWords),
                    CheckRegistered = true
                };

                dict.Write();
            }

            return dict;
        }

        public void Write()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
