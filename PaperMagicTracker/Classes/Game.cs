namespace PaperMagicTracker.Classes
{
    public class Game
    {
        public static async Task<Game> CreateAsync(Uri uri)
        {
            var newGame = new Game();
            await newGame.InitializeAsync(uri);

            return newGame;
        }

        public static Game CreateFromString(string deckString)
        {
            var newGame = new Game();
            newGame.Initialize(deckString);
            GlobalStaticResources.CurrentGame = newGame;
            return newGame;
        }

        private async Task InitializeAsync(Uri uri)
        {
            var deckTask = CardInfo.GetDeckListAsync(uri);

            foreach(var zoneEnum in Enum.GetValues<Zones>())
            {
                Zone newZone = new(zoneEnum, this);
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

        private void Initialize(string deckString)
        {
            if(!StringDeck.TryParseDeckString(deckString, out var deckList))
                return;

            foreach (var zoneEnum in Enum.GetValues<Zones>())
            {
                Zone newZone = new(zoneEnum, this);
                GameZones.Add(zoneEnum, newZone);
            }

            List<CardInfo> commander = new();

            AllCards = deckList;

            foreach (var (key, value) in deckList.Where(entry => entry.Value.IsCommander))
            {
                commander.Add(value);
                deckList.Remove(key);
            }

            GameZones[Zones.Library].Cards = deckList;

            GameZones[Zones.Command].AddMultipleCards(commander);
        }

        public void AdvanceTurn()
        {
            var startTurn = TurnCount;
            TurnCount ++;

            TurnAdvance advance = new(startTurn, TurnCount);

            GameLogger.AddEntry(advance);
        }

        private Game()
        {
            GameLogger = new CardLog();
        }

        public CardLog GameLogger { get; }

        public int TurnCount { get; private set; } = 1;

        /// <summary>
        /// A set containing all game Zones
        /// </summary>
        public Dictionary<Zones, Zone> GameZones { get; } = new();

        /// <summary>
        /// All cards that got added irrelevant of its current zone.
        /// </summary>
        public Dictionary<Guid, CardInfo> AllCards { get; private set; } = new();



    }
}
