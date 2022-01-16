using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PaperMagicTracker.Classes { 
    public class DeckstatsDeck
    {
        private static readonly HttpClient client = new();
        public static async Task<Dictionary<Guid, CardInfo>> ParseDeckstatsDeckAsync(Uri uri, HttpClient client)
        {
            var streamTask = GetDeckstatsDeckAsync(uri, client);

            Dictionary<Guid, CardInfo> outputDic = new();

            var deckObject = await streamTask;
            Parallel.ForEach(deckObject.Sections, (section, _) =>
            {
                //Console.WriteLine("Started Processing Section: " + section.Name);
                Parallel.ForEach(section.Cards, (card, _) =>
                {
                    //Console.WriteLine("Started Processing: " + card.Name);
                    var cardName = card.Name;
                    AllOracleCards.TryGetCardByName(cardName, out CardInfo cardInfo);
                    cardInfo.Count = card.Amount;
                    outputDic.Add(cardInfo.ScryfallOracleID, cardInfo);
                    //Console.WriteLine("Finished" + cardInfo.Name);
                });
                //Console.WriteLine("Finished Processing Section: " + section.Name);
            });

            return outputDic;
        }

        private static async Task<DeckstatsDeck> GetDeckstatsDeckAsync(Uri deckstatsUri, HttpClient client)
        {
            var getRequestUri = ParseDeckstatsUri(deckstatsUri);
        
            //var streamTask = client.GetStreamAsync(getRequestUri);

            //var deckObject = await JsonSerializer.DeserializeAsync<DeckstatsDeck>(await streamTask);

            client.DefaultRequestHeaders.Clear();
            var deckObject = await client.GetFromJsonAsync<DeckstatsDeck>(getRequestUri);

            return deckObject is not null? deckObject : throw new Exception("Deserialization of DeckstatsDeck failed");
        }

        public static Uri ParseDeckstatsUri(Uri deckstatsUri)
        {
            //A bit hacky, but to don't wanna do RegEx right now.
            var deckIdFused = deckstatsUri.Segments[3];
            var deckIdList = deckIdFused.Split("-");
            var deckId = deckIdList[0];
            var userId = deckstatsUri.Segments[2].Replace("/", "");

            Uri apiGetRequest = new($"https://deckstats.net/api.php?action=get_deck&id_type=saved&owner_id={userId}&id={deckId}&response_type=json");

            return apiGetRequest;
        }

        [JsonConstructor]
        public DeckstatsDeck(
            IReadOnlyList<Section> sections,
            string name
        )
        {
            this.Sections = sections;
            this.Name = name;
        }

        [JsonPropertyName("sections")]
        public IReadOnlyList<Section> Sections { get; }

        [JsonPropertyName("name")]
        public string Name { get; }
    }

    public class DeckstatsCard
    {
        [JsonConstructor]
        public DeckstatsCard(
            string name,
            int amount,
            bool valid,
            int idprintings
        )
        {
            this.Name = name;
            this.Amount = amount;
            this.Valid = valid;
            this.Idprintings = idprintings;
        }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("amount")]
        public int Amount { get; }

        [JsonPropertyName("valid")]
        public bool Valid { get; }

        [JsonPropertyName("idprintings")]
        public int Idprintings { get; }
    }

    public class Section
    {
        [JsonConstructor]
        public Section(
            string name,
            int amount,
            int amountNonbasic,
            IReadOnlyList<DeckstatsCard> cards
        )
        {
            this.Name = name;
            this.Amount = amount;
            this.AmountNonbasic = amountNonbasic;
            this.Cards = cards;
        }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("amount")]
        public int Amount { get; }

        [JsonPropertyName("amount_nonbasic")]
        public int AmountNonbasic { get; }

        [JsonPropertyName("cards")]
        public IReadOnlyList<DeckstatsCard> Cards { get; }
    }
}
