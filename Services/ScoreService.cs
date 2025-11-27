namespace AlleywayMonoGame.Services
{
    /// <summary>
    /// Service for managing game score and statistics.
    /// </summary>
    public class ScoreService
    {
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public float GameTimer { get; private set; }
        public bool TimerRunning { get; set; }

        private const int BrickPoints = 100;

        public ScoreService(int initialLives = 1)
        {
            Score = 0;
            Lives = initialLives;
            GameTimer = 0f;
            TimerRunning = false;
        }

        public void AddBrickScore()
        {
            Score += BrickPoints;
        }

        public void LoseLife()
        {
            Lives--;
        }

        public void Reset()
        {
            Score = 0;
            Lives = 1;
            GameTimer = 0f;
            TimerRunning = false;
        }

        public void UpdateTimer(float deltaTime)
        {
            if (TimerRunning)
            {
                GameTimer += deltaTime;
            }
        }

        public void StartTimer()
        {
            TimerRunning = true;
        }

        public void StopTimer()
        {
            TimerRunning = false;
        }

        public void ResetTimer()
        {
            GameTimer = 0f;
            TimerRunning = false;
        }

        public bool IsGameOver => Lives <= 0;
        
        public string GetFormattedTime()
        {
            int totalSeconds = (int)GameTimer;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
