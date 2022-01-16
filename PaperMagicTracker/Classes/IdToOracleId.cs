using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.GuardClauses;

namespace PaperMagicTracker.Classes
{
    public static class IdToOracleId
    {
        public static Guid GetOracleId(Guid scryfallId)
        {
            return IdDictionary[scryfallId];
        }

        public static async Task CreateIdToOracleIdJsonAsync()
        {
            throw new NotImplementedException();
            string defaultCardPath = @"C:\\Users\\dima\\source\\repos\\TestHTTPGet and so on\\ressources\\default-cards-20211227100242.json";

            Dictionary<Guid, Guid> ScrydallIdToOracleID = new();

            await using (FileStream fs = new(defaultCardPath, FileMode.Open, FileAccess.Read))
            {

                var cardList = JsonSerializer.DeserializeAsyncEnumerable<CardInfo>(fs);
                await foreach (var card in cardList)
                {
                    Guard.Against.Null(card, nameof(card), "Something went wrong with the deserialization of IdToOracleId");
                    ScrydallIdToOracleID.Add(card.ScryfallId, card.ScryfallOracleID);
                }
            }

            await using MemoryStream ms = new();
            await JsonSerializer.SerializeAsync(ms, ScrydallIdToOracleID);

            StreamReader reader = new(ms);
            string json = await reader.ReadToEndAsync();

            await File.WriteAllTextAsync("ScryfallIdToOracleIDDictionary", json);
        }

        public static async Task FormIdToOracleIdDic(HttpClient client)
        {
            string path = @"sample-data\ScryfallIdToOracleIDDictionary.json";

            var deserializationTask = client.GetFromJsonAsync<Dictionary<Guid, Guid>>(path);

            var dic = await Guard.Against.Null(deserializationTask, nameof(deserializationTask));
            IdDictionary = dic;
        }


        /// <summary>
        /// Key:ScryfallId Value:ScryfallOracleId
        /// </summary>
        private static Dictionary<Guid, Guid> IdDictionary { get; set; } = new Dictionary<Guid, Guid>();

    }

}
