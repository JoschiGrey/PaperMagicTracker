using MathNet.Numerics.Distributions;

namespace PaperMagicTracker.Classes
{
    public enum Zones{
        Library,
        Hand,
        Battlefield,
        Graveyard,
        Exile,
        Command
    }

    public class Zone
    {
        public Zone(Zones zone, Dictionary<Guid, CardInfo> startingCards = null)
        {
            startingCards ??= new Dictionary<Guid, CardInfo>();

            Id = zone;
            Cards = startingCards;

            var name = Enum.GetName(typeof(Zones), Id);

            if (string.IsNullOrEmpty(name))
            {
                name = "NameNotFound";
            }

            Name = name;
        }

        public void RemoveAllCard()
        {
            Cards = new Dictionary<Guid, CardInfo>();
        }

        /// <summary>
        /// Adds a card to this list and removes it from the donator.
        /// </summary>
        /// <param name="donator"></param>
        /// <param name="cardToAdd"></param>
        /// <returns></returns>
        public void MoveCard(Zone donator, CardInfo cardToAdd)
        {
            AddCard(cardToAdd);
            donator.RemoveCard(cardToAdd);

            Game.GameLogger.AddEntry(donator, this, cardToAdd);
        }

        /// <summary>
        /// Decreases the count of a card from the dictionary and removes it if it hits 0
        /// </summary>
        /// <param name="cardToRemove"></param>
        public void RemoveCard(CardInfo cardToRemove)
        {
            Guid scryfallOracleId = cardToRemove.ScryfallOracleID;

            Cards[scryfallOracleId].Count--;

            if(Cards[scryfallOracleId].Count <=0)
            {
                Cards.Remove(scryfallOracleId);
            }
        }

        public void AddCard(CardInfo newCard)
        {
            if (Cards.ContainsKey(newCard.ScryfallOracleID))
            {
                Cards[newCard.ScryfallOracleID].Count += 1;
                return;
            }

            var copy = (CardInfo)newCard.Clone();

            copy.Count = 1;

            Cards.Add(copy.ScryfallOracleID, copy);
        }

        public void AddMultipleCards(List<CardInfo> cardList)
        {
            foreach (var card in cardList)
                AddCard(card);
        }

        public int NonLandCount()
        {
            var nonlandCount = CardList.
                Where(i => !i.TypeLine.Contains("Land")).
                    Sum(i => i.Count);
            return nonlandCount;
        }

        public int GetAverageCmC()
        {
            var averageCmC = CardList.Where(i => !i.TypeLine.Contains("Land")).
                Sum(i => i.Count * i.ConvertedManaCost);
            return (int)averageCmC;
        }

        /// <summary>
        /// Returns the probability of drawing a card with the given type in X draws
        /// </summary>
        /// <returns></returns>
        public float GetProbabilityOfType(string cardType, int draws)
        {
            if(CardCount == 0)
                return 0;

            var countOfCardWithType = CardList.
                Where(i => i.TypeLine.Contains(cardType)).
                    Sum(i => i.Count);

            if (!Hypergeometric.IsValidParameterSet(CardCount, countOfCardWithType, draws))
            {
                return 0;
            }

            var probability = Hypergeometric.CDF(CardCount, countOfCardWithType, draws, 0);

            var probabilityOfAtLeastOne = 1 - probability;

            return (float)Math.Round(probabilityOfAtLeastOne, 2);
        }

        public Zones Id { get; }

        public string Name { get; }

        /// <summary>
        /// Key: OracleID Value: Id
        /// </summary>
        public Dictionary<Guid, CardInfo> Cards { get; set; }

        public int CardCount
        {
            get
            {
                var overallCardCount = Cards.Values.Sum(i => i.Count);
                return overallCardCount;
            }
        }

        public  IReadOnlyList<CardInfo> CardList => Cards.Values.ToList();

        public static bool TryGetZone(string searchString, out Zones zoneEnum)
        {
            foreach (var zone in Enum.GetValues<Zones>())
            {
                var enumToSearchFor = Enum.GetName(typeof(Zones), zone);

                if (string.IsNullOrEmpty(enumToSearchFor))
                    continue;

                if (!searchString.Contains(enumToSearchFor)) 
                    continue;

                zoneEnum = zone;
                return true;
            }

            zoneEnum = new Zones();
            return false;
        }

    }
}
