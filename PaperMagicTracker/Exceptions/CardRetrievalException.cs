namespace PaperMagicTracker.Exceptions
{
    public class CardRetrievalException : Exception
    {
        public CardRetrievalException(List<string> failedList)
        {
            FailedList = failedList;
        }

        public CardRetrievalException(string failedCard)
        {
            FailedCard = failedCard;
        }

        public CardRetrievalException(string message, List<string> failedList) : base(message)
        {
            FailedList = failedList;
        }

        public CardRetrievalException(string message, Exception inner, List<string> failedList) : base(message, inner)
        {
            FailedList = failedList;
        }

        public List<string> FailedList { get; } = new List<string>();

        public string FailedCard { get; } = "";

    }
}

