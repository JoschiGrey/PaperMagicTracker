using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace PaperMagicTracker.Classes
{
    public static class AllOracleCards
    {
        public static async Task FormAllOracleCardDictionary(HttpClient client)
        {
            string path = @"sample-data\AllOracleCardsByName.json";
            Console.WriteLine("Started byNameTask");
            var byNameTask = client.GetFromJsonAsync<Dictionary<string, Guid>>(path);

            var path2 = @"sample-data\AllOracleCardsDictionary.json";
            Console.WriteLine("Started byOracleIdTask");
            var byOracleIdTask = client.GetFromJsonAsync<Dictionary<Guid, CardInfo>>(path2);

            var dictionaryTasks = new List<Task> { byNameTask, byOracleIdTask };
            while (dictionaryTasks.Count > 0)
            {
                Task finishedTask = await Task.WhenAny(dictionaryTasks);

                if (finishedTask == byNameTask)
                {
                    AllOracleCardsByName = await byNameTask ?? throw new InvalidOperationException("AllOracleCardsByName Dic failed somehow");
                    Console.WriteLine("Finished byNameTask");
                }

                else if (finishedTask == byOracleIdTask)
                {
                    AllOracleCardsDictionary = await byOracleIdTask ?? throw new InvalidOperationException("AllOracleCardsDictionary failed somehow");
                    Console.WriteLine("Finished byOracleIdTask");
                }

                dictionaryTasks.Remove(finishedTask);
            }
        }
        public static CardInfo GetCardByOracleId(Guid oracleId)
        {
            return AllOracleCardsDictionary[oracleId];
        }

        /// <summary>
        /// Tries to remove all escape characters that are left in the cardname.
        /// </summary>
        /// <param name="uncleanedName"></param>
        /// <returns></returns>
        public static string CleanCardNameString(string uncleanedName)
        {
            var escapeSequencesToRemove = new List<string>()
            {
                "\\",
                "\a",
                "\b",
                "\f",
                "\n",
                "\r",
                "\t",
                "\v"
            };

            var unescapedInput = Regex.Unescape(uncleanedName);

            foreach (var eSeq in escapeSequencesToRemove)
            {
                unescapedInput = unescapedInput.Replace(@eSeq, "");
            }

            var cleanedName = unescapedInput;

            return cleanedName;
        }

        public static bool TryGetCardByName(string name, out CardInfo card)
        {
            var cleanedName = CleanCardNameString(name);

            if (!AllOracleCardsByName.ContainsKey(cleanedName))
            {
                card = new CardInfo();
                return false;
            }

            var oracleId = AllOracleCardsByName[cleanedName];

            card = GetCardByOracleId(oracleId);

            return true;
        }

        /// <summary>
        /// Key:ScryfallOracleId Value:Card contains a card object for each unique OracleId in Scryfall
        /// </summary>
        private static Dictionary<Guid, CardInfo> AllOracleCardsDictionary { get; set; } = new();

        /// <summary>
        /// Key:CardName Value:Card
        /// </summary>
        private static Dictionary<string, Guid> AllOracleCardsByName { get; set; } = new();
    }
}
