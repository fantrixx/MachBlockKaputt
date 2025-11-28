using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlleywayMonoGame.Core
{
    /// <summary>
    /// Contains game-wide constants and configuration.
    /// </summary>
    public static class GameConstants
    {
        // Screen dimensions
        public const int ScreenWidth = 800;
        public const int ScreenHeight = 600;
        public const int UIHeight = 50;
        public const int InfoBarHeight = 45;
        public const int GameAreaTop = UIHeight + 3;

        // Paddle constants
        public const int PaddleWidth = 100;
        public const int PaddleHeight = 20;
        public const int PaddleSpeed = 500;

        // Ball constants
        public const int BallSize = 14;
        public const float BallSpeedIncreaseMultiplier = 1.05f;
        public const float MaxBallSpeed = 800f;

        // Projectile constants
        public const int ProjectileWidth = 6;
        public const int ProjectileHeight = 15;
        public const int ProjectileSpeed = 500;

        // Power-up constants
        public const float ShootPowerDuration = 7f;

        // Level constants
        public const int MaxLevel = 10;
        public const float LevelClearAutoStart = 2.0f;
    }
}
