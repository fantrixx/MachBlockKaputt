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
        public const int ScreenHeight = 645; // Increased by InfoBarHeight to maintain game area size
        public const int UIHeight = 50;
        public const int InfoBarHeight = 45;
        public const int GameAreaTop = UIHeight + 3;
        public const int GameAreaBottom = ScreenHeight - InfoBarHeight;

        // Paddle constants
        public const int PaddleWidth = 100;
        public const int PaddleHeight = 20;
        public const int PaddleSpeed = 500;
        public const int PaddleBottomMargin = 40; // Distance from GameAreaBottom

        // Ball constants
        public const int BallSize = 14;
        public const float BallSpeedIncreaseMultiplier = 1.05f;
        public const float MaxBallSpeed = 800f;
        public const int BallPaddleGap = 1; // Gap between ball and paddle when spawning

        // Projectile constants
        public const int ProjectileWidth = 6;
        public const int ProjectileHeight = 15;
        public const int ProjectileSpeed = 500;

        // Power-up constants
        public const float ShootPowerDuration = 7f;

        // Level constants
        public const int MaxLevel = 10;
        public const float LevelClearAutoStart = 2.0f;
        
        // Shop constants
        public const int MaxShopItems = 3; // Maximum number of items shown in shop
        
        // UI Layout constants
        public const int ButtonWidth = 200;
        public const int ButtonHeight = 50;
        public const int ButtonSpacing = 20;
        public const int GameOverButtonY = 120; // Distance from bottom for game over buttons
    }
}
