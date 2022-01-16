using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;

namespace PaperMagicTracker.Classes
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<ArchidektDeck>(myJsonResponse);
    public class ArchidektDeck
    {
        private static readonly HttpClient client = new();
        

        private static async Task<ArchidektDeck> GetArchidektDecklist(Uri uri)
        {
            CancellationTokenSource cts;
            //client.DefaultRequestHeaders.Accept.Clear();

            string url = "https://archidekt.com/api";
            var pathAndQuery = uri.PathAndQuery;
            url = url + pathAndQuery + "/small/?format=json";

            cts = new CancellationTokenSource();

            //using var request = new HttpRequestMessage(HttpMethod.Get, url + pathAndQuery + "/small/");
            //request.SetBrowserResponseStreamingEnabled(true); // Enable response streaming

            // Be sure to use HttpCompletionOption.ResponseHeadersRead
            //using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            //using var stream = await response.Content.ReadAsStreamAsync();


            //var streamTask = client.GetStreamAsync(url + pathAndQuery + "/small/");


            //var archidektDeck =  await JsonSerializer.DeserializeAsync<ArchidektDeck>(stream);

            var archidektDeck = await client.GetFromJsonAsync<ArchidektDeck>(url);
            Guard.Against.Null(archidektDeck, nameof(archidektDeck), "Something went wrong with the Archidekt Deserialization");
            
            return archidektDeck;
        }

        /// <summary>
        /// Takes a ArchidektDeckRoot and creates a Dictionary<OracleId, Card> from all cards that are included in the deck
        /// </summary>
        /// <param name="archidektDeck"></param>
        /// <returns></returns>
        public static async Task<Dictionary<Guid, CardInfo>> CreateArchidektDeck(Uri uri)
        {
            var deckRoot = await GetArchidektDecklist(uri);

            var inDeckHash = FormInDeckHash();

            Dictionary<Guid, CardInfo> outputDic = new();

            Parallel.ForEach(deckRoot.Cards, card =>
            {
                if (CheckIfInDeck(card))
                {
                    Guid scryfallId = new Guid(card.Card.ScryfallIdString);
                    var oracleId = IdToOracleId.GetOracleId(scryfallId);

                    CardInfo cardToAdd = AllOracleCards.GetCardByOracleId(oracleId);

                    cardToAdd.Count = card.Quantity;

                    cardToAdd.IsCommander = CheckIfCommander(card);

                    outputDic.Add(oracleId, cardToAdd);


                }
            });

            return outputDic;

            HashSet<string> FormInDeckHash()
            {
                HashSet<string> inDeckHash = new();

                foreach (var category in deckRoot.Categories)
                {
                    if (category.IncludedInDeck)
                    {
                        inDeckHash.Add(category.Name);
                    }
                }

                return inDeckHash;
            }

            bool CheckIfInDeck(ArchidektCard card)
            {
                bool outputBool = false;
                Parallel.ForEach(card.Categories, (cat, state) =>
                {
                    if (inDeckHash.Contains(cat))
                    {
                        outputBool = true;
                        state.Stop();
                    }
                }
                );
                return outputBool;
            }

            bool CheckIfCommander(ArchidektCard card)
            {
                foreach(var category in card.Categories)
                {
                    if (category.Contains("ommand"))
                        return true;
                }
                return false;
            }
        }

        [JsonConstructor]
        public ArchidektDeck(int id, List<ArchidektCard> cards, List<Category> categories, string name)
        {
            Id = id;
            Cards = cards;
            Categories = categories;
            Name = name;
        }

        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("cards")]
        public List<ArchidektCard> Cards { get; } = new List<ArchidektCard>();

        [JsonPropertyName("categories")]
        public List<Category> Categories { get; } = new List<Category>();

        [JsonPropertyName("name")]
        public string Name { get; }
    }


    public class ArchidektCard
    {
        [JsonConstructor]
        public ArchidektCard(int quantity, List<string> categories, SingleCard card)
        {
            Quantity = quantity;
            Categories = categories;
            Card = card;
        }

        [JsonPropertyName("quantity")]
        public int Quantity { get; }

        [JsonPropertyName("categories")]
        public List<string> Categories { get; }

        [JsonPropertyName("card")]
        public SingleCard Card { get; }
    }

    public class SingleCard
    {
        [JsonConstructor]
        public SingleCard(string scryfallidstring, OracleCard oracleCard)
        {
            ScryfallIdString = scryfallidstring;

            ScryfallId = Guid.TryParse(scryfallidstring, out Guid newGuid) ? newGuid : new Guid();

            OracleCard = oracleCard;
        }

        [JsonPropertyName("uid")]
        public string ScryfallIdString { get; }

        public Guid ScryfallId { get; }

        [JsonPropertyName("oracleCard")]
        public OracleCard OracleCard {get; }
    }

    public class OracleCard
    {
        [JsonConstructor]
        public OracleCard(string name)
        {
            Name = name;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Category
    {
        [JsonConstructor]
        public Category(int id, string name, bool includedInDeck, bool includedInPrice, bool isPremier)
        {
            Id = id;
            Name = name;
            IncludedInDeck = includedInDeck;
            IncludedInPrice = includedInPrice;
            IsPremier = isPremier;
        }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("includedInDeck")]
        public bool IncludedInDeck { get; set; }

        [JsonPropertyName("includedInPrice")]
        public bool IncludedInPrice { get; set; }

        [JsonPropertyName("isPremier")]
        public bool IsPremier { get; set; }
    }



}