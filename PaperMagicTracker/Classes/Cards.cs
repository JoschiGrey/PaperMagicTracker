using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;

namespace PaperMagicTracker.Classes
{
    /// <summary>
    /// Holds all infos about a MTG Card and Methods for deserialization of Scryfall Card Objects
    /// </summary>
    public class CardInfo : ICloneable
    {
        public CardInfo()
        {
            Name = "Invalid";
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
            var streamTask = client.GetFromJsonAsync<CardInfo>(url);

            var fetchedCard = await Guard.Against.Null(streamTask, nameof(streamTask));

            return fetchedCard ?? throw new Exception("Cardname could not be found in Scryfall");
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
        public string Manacost { get; }

        public int Count { get; set; }

        public bool IsCommander { get; set; }

        public static async Task<Dictionary<Guid, CardInfo>> GetDeckListAsync(Uri deckUri, string deckString = "")
        {
            Dictionary<Guid, CardInfo> deckList;

            var uriString = deckUri.ToString();

            if (uriString.Contains("archidekt"))
            {
                deckList = await ArchidektDeck.CreateArchidektDeck(deckUri);
                return deckList;
            }

            if (uriString.Contains("deckstats"))
            {
                //deckList = await DeckstatsDeck.ParseDeckstatsDeckAsync(deckUri);
                //return deckList;
            }

            if(StringDeck.TryParseDeckString(deckString, out deckList))
                return deckList;

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
        public static bool TryParseDeckString(string deck, out Dictionary<Guid, CardInfo> deckList)
        {
            deckList = new();
            var success = true;

            var amountAndCards = deck.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            
            foreach(var amountCardString in amountAndCards)
            {
                var amountCardSeperated = amountCardString.Split(new[] { ' ' }, 2);
                var parseSuccess = int.TryParse(amountCardSeperated[0], out var amount);

                var getSuccess = AllOracleCards.TryGetCardByName(amountCardSeperated[1], out var cardInfo);
                if (getSuccess == false || parseSuccess == false)
                    success = false;

                cardInfo.Count = amount;
                deckList.Add(cardInfo.ScryfallOracleID, cardInfo);
            }

            return success;
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
