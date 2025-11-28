using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Core
{
    /// <summary>
    /// UI-related constants for colors, sizes, and visual effects
    /// Prevents magic numbers throughout the codebase
    /// </summary>
    public static class UIConstants
    {
        // === COLORS ===
        
        // Background colors
        public static readonly Color DialogBackgroundDark = new Color(10, 10, 30);
        public static readonly Color DialogBackgroundMedium = new Color(15, 15, 35);
        public static readonly Color DialogBackgroundLight = new Color(20, 20, 40);
        public static readonly Color SpaceBackground1 = new Color(5, 5, 20);
        public static readonly Color SpaceBackground2 = new Color(10, 5, 30);
        public static readonly Color ScanlineColor = new Color(5, 5, 25);
        
        // UI Border colors
        public static readonly Color BorderBlue = new Color(100, 150, 200);
        public static readonly Color BorderBrightBlue = new Color(100, 200, 255);
        public static readonly Color BorderGold = new Color(255, 215, 0);
        public static readonly Color BorderGoldDark = new Color(200, 170, 0);
        public static readonly Color BorderGreen = new Color(100, 200, 100);
        public static readonly Color BorderGreenBright = new Color(150, 255, 150);
        public static readonly Color BorderRed = new Color(150, 50, 50);
        public static readonly Color BorderRedBright = new Color(200, 100, 100);
        
        // Button colors
        public static readonly Color ButtonGreenNormal = new Color(100, 200, 100);
        public static readonly Color ButtonGreenDark = new Color(50, 100, 50);
        public static readonly Color ButtonRedNormal = new Color(200, 100, 100);
        public static readonly Color ButtonRedDark = new Color(100, 50, 50);
        public static readonly Color ButtonBlueNormal = new Color(50, 100, 200);
        public static readonly Color ButtonBlueDark = new Color(30, 50, 100);
        public static readonly Color ButtonPurpleNormal = new Color(150, 100, 200);
        public static readonly Color ButtonPurpleDark = new Color(80, 50, 100);
        public static readonly Color ButtonDisabledNormal = new Color(60, 60, 80);
        public static readonly Color ButtonDisabledDark = new Color(40, 40, 50);
        public static readonly Color ButtonPurchasedNormal = new Color(40, 40, 50);
        public static readonly Color ButtonPurchasedDark = new Color(30, 30, 40);
        
        // Text colors
        public static readonly Color TextWhite = Color.White;
        public static readonly Color TextGold = new Color(255, 215, 0);
        public static readonly Color TextGoldDark = new Color(100, 80, 0);
        public static readonly Color TextGreen = new Color(150, 255, 150);
        public static readonly Color TextGreenEarned = new Color(150, 255, 150);
        public static readonly Color TextRed = new Color(255, 100, 100);
        public static readonly Color TextRedSpent = new Color(255, 150, 150);
        public static readonly Color TextBlue = new Color(150, 200, 255);
        public static readonly Color TextBlueBright = new Color(100, 200, 255);
        public static readonly Color TextGray = new Color(120, 120, 120);
        public static readonly Color TextGrayDim = new Color(100, 100, 110);
        public static readonly Color TextGrayOwned = new Color(120, 180, 120);
        public static readonly Color TextDarkShadow = new Color(80, 0, 0);
        public static readonly Color TextGreenShadow = new Color(0, 80, 0);
        
        // Shop item colors (reused from ShopService)
        public static readonly Color ShopSpeedUpgradeColor = new Color(100, 200, 255); // Blue - Movement
        public static readonly Color ShopExtraBallColor = new Color(255, 215, 0);      // Gold - Valuable
        public static readonly Color ShopShootModeColor = new Color(255, 100, 100);    // Red - Combat
        public static readonly Color ShopPaddleSizeColor = new Color(150, 255, 150);   // Green - Size
        public static readonly Color ShopShieldColor = new Color(200, 150, 255);       // Purple - Protection
        
        // Game entity colors
        public static readonly Color PaddleBaseDark = new Color(40, 60, 90);
        public static readonly Color PaddleBaseMedium = new Color(60, 75, 100);
        public static readonly Color PaddleHighlight = new Color(120, 140, 180);
        public static readonly Color PaddleSideDetail = new Color(80, 120, 160);
        public static readonly Color PaddleGlow = new Color(0, 150, 255);
        public static readonly Color PaddleShadow = new Color(20, 30, 45);
        public static readonly Color CannonDark = new Color(30, 35, 45);
        public static readonly Color CannonLight = new Color(50, 55, 65);
        public static readonly Color CannonEdge = new Color(70, 80, 100);
        
        // Brick colors
        public static readonly Color BrickSteel = new Color(100, 100, 110);
        public static readonly Color BrickSheen = new Color(180, 180, 190);
        public static readonly Color BrickRivet = new Color(70, 70, 80);
        public static readonly Color BrickCrack = new Color(40, 40, 50);
        public static readonly Color BrickGold = new Color(255, 215, 0);
        
        // UFO colors
        public static readonly Color UFODome = new Color(150, 150, 200);
        public static readonly Color UFOBody = new Color(100, 100, 120);
        public static readonly Color UFOHighlight = new Color(180, 180, 200);
        public static readonly Color UFOShadow = new Color(60, 60, 80);
        
        // Shield colors
        public static readonly Color ShieldPurple = new Color(200, 150, 255);
        
        // Icon colors
        public static readonly Color IconDisabled = new Color(80, 80, 90);
        
        // === SIZES & SPACING ===
        
        // Dialog box dimensions
        public const int GameOverBoxWidth = 400;
        public const int GameOverBoxHeight = 380;
        public const int GameOverBoxY = 100;
        public const int GameOverStatsBoxWidth = 300;
        public const int GameOverStatsBoxHeight = 110;
        
        public const int VictoryBoxWidth = 450;
        public const int VictoryBoxHeight = 520;
        public const int VictoryStatsBoxWidth = 350;
        public const int VictoryStatsBoxHeight = 150;
        
        public const int LevelCompleteBoxWidth = 500;
        public const int BonusBoxWidth = 360;
        public const int BonusBoxHeight = 60;
        public const int ShopBoxWidth = 420;
        public const int ShopBoxHeight = 160;
        public const int ShopItemButtonWidth = 380;
        public const int ShopItemButtonHeight = 35;
        public const int ShopItemButtonSpacing = 10;
        
        // Border widths
        public const int BorderThick = 4;
        public const int BorderMedium = 3;
        public const int BorderThin = 2;
        
        // Border offsets
        public const int BorderInnerOffset = 6;
        public const int BorderInnerOffset2 = 8;
        public const int BorderInnerWidth = 12;
        public const int BorderInnerWidth2 = 16;
        
        // Text spacing
        public const int TextLineSpacing = 25;
        public const int TextSectionSpacing = 30;
        public const int TextSmallSpacing = 40;
        public const int TextMediumSpacing = 50;
        
        // Shadow offsets
        public const int ShadowOffsetSmall = 1;
        public const int ShadowOffsetMedium = 2;
        public const int ShadowOffsetLarge = 3;
        
        // Tooltip
        public const int TooltipPadding = 10;
        public const int TooltipLineHeight = 22;
        public const int TooltipBorderWidth = 2;
        
        // === OPACITY VALUES ===
        
        public const float OverlayOpacityHigh = 0.9f;
        public const float OverlayOpacityMedium = 0.85f;
        public const float OverlayOpacityLow = 0.8f;
        public const float ShadowOpacity = 0.5f;
        public const float TooltipOpacity = 0.95f;
        public const float ScanlineOpacity = 0.7f;
        public const float DimmedIconOpacity = 0.5f;
        public const float GlowOpacity = 0.3f;
        
        // === ANIMATION VALUES ===
        
        public const float AnimationLerpSpeed = 0.3f;
        public const float AnimationLerpSpeed2 = 0.2f;
        public const float PopMaxScale = 2.5f;
        public const float GlowScaleThreshold = 1.2f;
        public const float GlowScaleMultiplier = 1.1f;
        
        // === PARTICLE VALUES ===
        
        public const int ParticleCountSmall = 15;
        public const int ParticleCountMedium = 20;
        public const int ParticleCountLarge = 30;
        
        // === GRID LAYOUT PERCENTAGES ===
        
        public const float GridLeftColumnStart = 0.05f;   // 5%
        public const float GridMiddleColumnStart = 0.30f; // 30%
        public const float GridRightColumnStart = 0.70f;  // 70%
        public const float GridRightColumnEnd = 0.95f;    // 95%
    }
}
