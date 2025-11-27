namespace AlleywayMonoGame.Core
{
    /// <summary>
    /// Manages the current state of the game.
    /// </summary>
    public class GameStateManager
    {
        public GameState CurrentState { get; private set; } = GameState.Playing;
        public int CurrentLevel { get; set; } = 1;

        public bool IsPlaying => CurrentState == GameState.Playing;
        public bool IsGameOver => CurrentState == GameState.GameOver;
        public bool IsLevelComplete => CurrentState == GameState.LevelComplete;
        public bool IsLevelCleared => CurrentState == GameState.LevelCleared;
        public bool IsVictory => CurrentState == GameState.Victory;

        public void SetPlaying()
        {
            CurrentState = GameState.Playing;
        }

        public void SetGameOver()
        {
            CurrentState = GameState.GameOver;
        }

        public void SetLevelComplete()
        {
            CurrentState = GameState.LevelComplete;
        }

        public void SetLevelCleared()
        {
            CurrentState = GameState.LevelCleared;
        }

        public void SetVictory()
        {
            CurrentState = GameState.Victory;
        }

        public void NextLevel()
        {
            CurrentLevel++;
            SetPlaying();
        }

        public void Reset()
        {
            CurrentLevel = 1;
            SetPlaying();
        }
    }

    public enum GameState
    {
        Playing,
        LevelCleared,
        LevelComplete,
        GameOver,
        Victory
    }
}
