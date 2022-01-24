using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Blazored.LocalStorage;

namespace PaperMagicTracker.Classes
{
    public static class AllOracleCards
    {
        public static async Task FormAllOracleCardDictionary(HttpClient client, ILogger logger, ILocalStorageService localStorage)
        {
            var allCardFromStorage = GetAllOracleCardsDictionary(client, logger, localStorage);
            var byNameFromStorage = GetAllOracleCardsByName(client, logger, localStorage);

            List<Task<bool>> taskList = new() { allCardFromStorage, byNameFromStorage };

            while (taskList.Count > 0)
            {
                var finishedTask = await Task.WhenAny(taskList);
                taskList.Remove(finishedTask);
                if (await finishedTask == false && finishedTask == allCardFromStorage)
                {
                    try
                    {
                        logger.LogTrace($"Save {AllOracleCardsDictionary} to storage");
                        await localStorage.SetItemAsync(nameof(AllOracleCardsDictionary), AllOracleCardsDictionary);
                    }
                    catch (Microsoft.JSInterop.JSException e)
                    {
                        logger.LogInformation($"Skipped saving {AllOracleCardsDictionary} to storage, probably because of missing storage size");
                    }
                    
                }
                else if (await finishedTask == false && finishedTask == byNameFromStorage)
                {
                    logger.LogTrace($"Saved {AllOracleCardsByName} to storage");
                    await localStorage.SetItemAsync(nameof(AllOracleCardsByName), AllOracleCardsByName);
                }
            }
        }

        public static async Task<bool> GetAllOracleCardsByName(HttpClient client, ILogger logger, ILocalStorageService localStorage)
        {
            logger.LogTrace("Started byNameTask");

            bool gotFromStorage = true;

            Dictionary<string, Guid>? dict;

    
            dict = await localStorage.GetItemAsync<Dictionary<string, Guid>>(nameof(AllOracleCardsByName));
            logger.LogTrace($"Try fetch {nameof(AllOracleCardsByName)} from storage");

            if(dict is null)
            {
                var path = @"sample-data\AllOracleCardsByName.json";
                dict = await client.GetFromJsonAsync<Dictionary<string, Guid>>(path);

                gotFromStorage = false;
                logger.LogTrace($"Fetched {nameof(AllOracleCardsByName)} from json");
            }

            AllOracleCardsByName = dict ?? throw new InvalidOperationException("AllOracleCardsByName Dic failed somehow");
            logger.LogTrace("Finished byNameTask");

            return gotFromStorage;
        }

        public static async Task<bool> GetAllOracleCardsDictionary(HttpClient client, ILogger logger, ILocalStorageService localStorage)
        {
            logger.LogTrace("Started allCardsTask");

            bool gotFromStorage = true;



            
            var dict = await localStorage.GetItemAsync<Dictionary<Guid, CardInfo>>(nameof(AllOracleCardsDictionary));
            logger.LogTrace($"TryToFetch {nameof(AllOracleCardsDictionary)} from storage");
            

            if(dict is null)
            {
                var path = @"sample-data\AllOracleCardsDictionary.json";
                dict = await client.GetFromJsonAsync<Dictionary<Guid, CardInfo>>(path);

                gotFromStorage = false;
                logger.LogTrace($"Fetched {nameof(AllOracleCardsDictionary)} from json");
            }

            AllOracleCardsDictionary = dict ?? throw new InvalidOperationException($"{nameof(AllOracleCardsDictionary)} Dic failed somehow");
            logger.LogTrace("Finished allCardsTask");

            return gotFromStorage;
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
                card = new CardInfo(cleanedName);
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
