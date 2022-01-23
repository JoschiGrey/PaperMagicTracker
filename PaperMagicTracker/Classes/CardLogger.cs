using PaperMagicTracker.Interfaces;

namespace PaperMagicTracker.Classes
{
    public class CardLog
    {
        public void AddEntry(ILoggingObject logEntryObject)
        {
            GameLog.Add(logEntryObject);
        }

        public void AddEntry(Zone donator, Zone acceptor, CardInfo card)
        {
            CardTransfer newEnty = new(donator, acceptor, card);
            GameLog.Add(newEnty);
        }

        public void RemoveLastEntry()
        {
            GameLog.RemoveAt(GameLog.Count() - 1);
        }

        public void Print()
        {
            foreach(var entry in GameLog)
            {
                Console.WriteLine(entry.Print());
            }
        }

        public List<ILoggingObject> GameLog { get; private set; } = new();
    }

    public readonly struct CardTransfer : ILoggingObject
    {
        public CardTransfer(Zone donator, Zone acceptor, CardInfo transferedCard)
        {
            Donator = donator;
            Acceptor = acceptor;
            TransferedCard = transferedCard;
        }

        public string Print()
        {
            var shortDonator = Donator.Name;
            var shortAcceptor = Acceptor.Name;
            var cardName = TransferedCard.Name;
            var time = TimeStamp.ToShortTimeString;

            var output = $"{TimeStamp}: {Donator.Name} -> {Acceptor.Name} | {TransferedCard.Name}";
            return output;
        }

        public override string ToString()
        {
            return Print();
        }

        public Zone Donator { get; }
        public Zone Acceptor { get; }
        public CardInfo TransferedCard { get; }
        public DateTime TimeStamp => DateTime.Now;
    }

    public readonly struct TurnAdvance : ILoggingObject
    {
        public TurnAdvance(int from, int to)
        {
            TurnBefore = from;
            TurnAfter = to;
        }

        public string Print()
        {
            return $"{TimeStamp}: From Turn {TurnBefore} to Turn {TurnAfter}";
        }

        public override string ToString()
        {
            return Print();
        }

        public int TurnAfter { get; }

        public int TurnBefore { get; }

        public DateTime TimeStamp => DateTime.Now;
    }
}
