using PaperMagicTracker.Exceptions;

namespace PaperMagicTracker.Classes
{
    public static class Game
    {
        public static async Task TaskCreateAsync(Uri uri)
        {
            await InitializeAsync(uri);
        }

        public static async Task CreateAsync(string deckString, ILogger logger)
        {
            await InitializeFromStringAsync(deckString, logger);
        }

        private static async Task InitializeAsync(Uri uri)
        {
            var deckTask = CardInfo.GetDeckListAsync(uri);

            foreach(var zoneEnum in Enum.GetValues<Zones>())
            {
                Zone newZone = new(zoneEnum);
                GameZones.Add(zoneEnum, newZone);
            }

            List<CardInfo> commander = new();

            var deckList = await deckTask;

            AllCards = deckList;

            foreach (var entry in deckList)
            {
                if (!entry.Value.IsCommander) continue;
                commander.Add(entry.Value);
                deckList.Remove(entry.Key);
            }

            GameZones[Zones.Library].AddMultipleCards(deckList.Values.ToList());

            GameZones[Zones.Command].AddMultipleCards(commander);
        }

        private static async Task InitializeFromStringAsync(string deckString, ILogger logger)
        {
            var deckListTask = StringDeck.ParseDeckStringAsync(deckString, logger);

            foreach (var zoneEnum in Enum.GetValues<Zones>())
            {
                Zone newZone = new(zoneEnum);
                GameZones.Add(zoneEnum, newZone);
            }

            List<CardInfo> commander = new();

            var deckList = await deckListTask;

            AllCards = deckList;
            Console.WriteLine("set decklist to allCards");

            foreach (var (key, value) in deckList.Where(entry => entry.Value.IsCommander))
            {
                commander.Add(value);
                deckList.Remove(key);
            }

            GameZones[Zones.Library].Cards = deckList;
            Console.WriteLine("set decklist to library");

            GameZones[Zones.Command].AddMultipleCards(commander);
        }

        public static void AdvanceTurn()
        {
            var startTurn = TurnCount;
            TurnCount ++;

            TurnAdvance advance = new(startTurn, TurnCount);

            GameLogger.AddEntry(advance);
        }

        public static async Task ResetGame(string deckString, ILogger logger)
        {
            GameZones.Clear();
            FailedToFetchCards.Clear();
            AllCards.Clear();
            TurnCount = 1;
            GameLogger = new CardLog();

            var createTask = CreateAsync(deckString, logger);
            logger.LogInformation("All Game information got resetted");
            await createTask;
        }

        public static CardLog GameLogger { get; private set; } = new();

        public static int TurnCount { get; private set; } = 1;

        /// <summary>
        /// A set containing all game Zones
        /// </summary>
        public static Dictionary<Zones, Zone> GameZones { get; } = new();

        /// <summary>
        /// All cards that got added irrelevant of its current zone.
        /// </summary>
        public static Dictionary<Guid, CardInfo> AllCards { get; private set; } = new();

        public static List<string> FailedToFetchCards { get; set; } = new();

    }
}
