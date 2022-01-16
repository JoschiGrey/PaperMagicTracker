namespace PaperMagicTracker.Classes
{
    public class AppState
    {
        private bool _gameCreated;
        public event Action OnChange;
        public bool GameCreated
        {
            get => _gameCreated;
            set
            {
                if (_gameCreated == value) return;

                _gameCreated = value;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}

