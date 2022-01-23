using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using PaperMagicTracker.Exceptions;

namespace PaperMagicTracker.Classes
{
    /// <summary>
    /// Holds all infos about a MTG Card and Methods for deserialization of Scryfall Card Objects
    /// </summary>
    public class CardInfo : ICloneable
    {
        public CardInfo(string name)
        {
            Name = name;
            ScryfallUri = new("https://en.wikipedia.org/wiki/HTTP_404");
            TypeLine = "Invalid";
        }
        /// <summary>
        /// Fetches a card from Scryfall by name asynchronously
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<CardInfo> GetCardByNameAsync(string name)
        {
            client.DefaultRequestHeaders.Accept.Clear();

            string url = "https://api.scryfall.com/cards/named/?fuzzy=" + name;


            try
            {
                var streamTask = client.GetFromJsonAsync<CardInfo>(url);
                var fetchedCard = await streamTask;
                return fetchedCard ?? throw new CardRetrievalException(name);
            }
            catch
            {
                throw new CardRetrievalException(name);
            }
        }

        [JsonConstructor]
        public CardInfo(string name, Guid scryfallid, Uri scryfalluri, Guid scryfalloracleid, decimal convertedmanacost, string typeline, string manacost)
        {
            Name = name;
            ScryfallId = scryfallid;
            ScryfallUri = scryfalluri;
            ScryfallOracleID = scryfalloracleid;
            ConvertedManaCost = convertedmanacost;
            TypeLine = typeline;
            Manacost = manacost;
            Count = 1;
        }

        private static readonly HttpClient client = new();

        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Unique Identifier for each card. Not consistent between variants and reprints!
        /// </summary>
        [JsonPropertyName("id")]
        public Guid ScryfallId { get; } = Guid.NewGuid();

        [JsonPropertyName("uri")]
        public Uri ScryfallUri { get; }

        /// <summary>
        /// Identifier that is consistent between reprints and variants.
        /// </summary>
        [JsonPropertyName("oracle_id")]
        public Guid ScryfallOracleID { get; } = Guid.NewGuid();

        [JsonPropertyName("cmc")]
        public decimal ConvertedManaCost { get; }

        [JsonPropertyName("type_line")]
        public string TypeLine { get; }

        [JsonPropertyName("mana_cost")]
        public string? Manacost { get; }

        public int Count { get; set; }

        public bool IsCommander { get; set; }

        public static async Task<Dictionary<Guid, CardInfo>> GetDeckListAsync(Uri deckUri, string deckString = "")
        {

            var uriString = deckUri.ToString();

            if (uriString.Contains("archidekt"))
            {
                return await ArchidektDeck.CreateArchidektDeckAsync(deckUri);
            }

            if (uriString.Contains("deckstats"))
            {
                //deckList = await DeckstatsDeck.ParseDeckstatsDeckAsync(deckUri);
                //return deckList;
            }

            //If we cannot detect the correct side just throw a exception
            throw new NotImplementedException("No available Method for Parsing the Deck has succeeded");
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public static class StringDeck
    {
        public static async Task<Dictionary<Guid, CardInfo>> ParseDeckStringAsyncOld(string deck)
        {
            Dictionary<Guid,CardInfo> deckList = new();
            var failedEntries = new List<string>();

            var amountAndCards = deck.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            
            foreach(var amountCardString in amountAndCards)
            {
                var amountCardSeperated = amountCardString.Split(new[] { ' ' }, 2);

                var cardName = amountCardSeperated.Length == 2 ? amountCardSeperated[1] : amountCardSeperated[0];
                Console.WriteLine(cardName);

                var unparesedAmount = amountCardSeperated[0];
                Console.WriteLine(unparesedAmount);

                var parseSuccess = int.TryParse(unparesedAmount, out var amount);

                var getSuccess = AllOracleCards.TryGetCardByName(cardName, out var cardInfo);

                if (!getSuccess)
                {
                    try
                    {
                        var cardInfoTask = CardInfo.GetCardByNameAsync(cardName);
                        getSuccess = true;
                        cardInfo = await cardInfoTask;
                    }
                    catch (Exception ex)
                    {
                        getSuccess = false;
                    }
                }

                if (getSuccess == false || parseSuccess == false)
                {
                    failedEntries.Add(amountCardString);
                    
                    Console.WriteLine(amountCardString);

                    continue;
                }

                cardInfo.Count = amount;
                deckList.Add(cardInfo.ScryfallOracleID, cardInfo);
            }

            Game.FailedToFetchCards = failedEntries;

            return deckList;
        }

        public static async Task<Dictionary<Guid, CardInfo>> ParseDeckStringAsync(string deck)
        {
            Dictionary<Guid, CardInfo> deckList = new();
            var failedEntries = new List<string>();

            var amountAndCards = deck.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            //Matches any amount of numbers followed by any amount of whitespaces. PLS wizrads don't introduce cards that actually start with a digit ffs
            var pattern = @"^([\d]*)\s*";
            var rg = new Regex(pattern, RegexOptions.IgnoreCase);

            foreach (var amountCardString in amountAndCards)
            {
                Console.WriteLine(amountCardString);

                var amountString = rg.Match(amountCardString).ToString();
                int.TryParse(amountString, out var amount);

                var nameString = rg.Replace(amountCardString, "");

                if (!AllOracleCards.TryGetCardByName(nameString, out var cardInfo))
                {
                    try
                    {
                        Console.WriteLine("Fallback to scryfall for: " + nameString);
                        cardInfo = await CardInfo.GetCardByNameAsync(nameString);
                    }
                    catch (CardRetrievalException e)
                    {
                        Console.WriteLine("exception in fallback adding to failed entries");
                        failedEntries.Add(e.FailedCard);
                        continue;
                    }
                }

                cardInfo.Count = amount;
                deckList.Add(cardInfo.ScryfallOracleID, cardInfo);
                Console.WriteLine($"Added {cardInfo.Name} to deckList");
            }

            Game.FailedToFetchCards = failedEntries;

            Console.WriteLine("returning deckList");
            return deckList;
        }


        /*
        public static bool TryParseRegexDeckString(string deck, out Dictionary<Guid, CardInfo> deckList)
        {
            var pattern = @"\d{1,3} ((.|\n)*?)(?=(\d|$))";
            var rg = new Regex(pattern, RegexOptions.IgnoreCase);


            var matches = rg.Matches(deck);

            var success = true;
            deckList = new Dictionary<Guid, CardInfo>();

            foreach (var match in matches)
            {
                var splitMatches = match.ToString().Split(new[] { ' ' }, 2);

                var parseSucces = int.TryParse(splitMatches[0], out var count);

                var getSuccess = AllOracleCards.TryGetCardByName(splitMatches[1], out var cardInfo);

                //indicate that any card wasn't parsed correctly
                if (getSuccess == false || parseSucces == false)
                    success = false;

                cardInfo.Count = count;
                deckList.Add(cardInfo.ScryfallOracleID, cardInfo);
            }
            return success;
        }
        */
    }


}
