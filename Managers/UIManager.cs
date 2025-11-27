using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlleywayMonoGame.Managers
{
    /// <summary>
    /// Manages UI state for level complete, game over, and victory screens.
    /// </summary>
    public class UIManager
    {
        // Level complete state
        public bool LevelComplete { get; set; }
        public float AnimationTimer { get; set; }
        public int AnimatedMoney { get; set; }
        public bool MoneyAnimationDone { get; set; }
        public int LevelCompleteTimeBonus { get; set; }
        public bool ChargeUpSoundPlayed { get; set; }
        
        // Money slam animation
        public float SlamY { get; set; }
        public float SlamVelocity { get; set; }
        public float SlamScale { get; set; } = 1f;
        public bool SlamAnimationDone { get; set; }
        public float GlowPulse { get; set; }
        
        // Purchase animation
        public bool PurchaseAnimationActive { get; set; }
        public float PurchaseCostX { get; set; }
        public float PurchaseCostY { get; set; }
        public int PurchaseCostAmount { get; set; }
        public float PurchaseAnimationTimer { get; set; }
        public float BalanceShake { get; set; }
        
        // UI Buttons - Level Complete
        public Rectangle NextLevelButton { get; set; }
        public bool NextLevelButtonHovered { get; set; }
        public Rectangle[] ShopButtons { get; } = new Rectangle[3];
        public bool[] ShopButtonsHovered { get; } = new bool[3];
        
        // UI Buttons - Game Over
        public Rectangle RetryButton { get; set; }
        public Rectangle QuitButton { get; set; }
        public bool RetryButtonHovered { get; set; }
        public bool QuitButtonHovered { get; set; }

        // UI Buttons - Victory
        public Rectangle VictoryRetryButton { get; set; }
        public Rectangle VictoryQuitButton { get; set; }
        public bool VictoryRetryButtonHovered { get; set; }
        public bool VictoryQuitButtonHovered { get; set; }
        
        public float VictoryGlowTimer { get; set; }

        public void ResetLevelComplete()
        {
            LevelComplete = false;
            AnimationTimer = 0f;
            MoneyAnimationDone = false;
            AnimatedMoney = 0;
            ChargeUpSoundPlayed = false;
            SlamY = 0f;
            SlamVelocity = 0f;
            SlamScale = 1f;
            SlamAnimationDone = false;
            GlowPulse = 0f;
            PurchaseAnimationActive = false;
        }

        public void UpdateHoverStates(MouseState mouseState)
        {
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            // Level complete buttons
            for (int i = 0; i < 3; i++)
            {
                ShopButtonsHovered[i] = ShopButtons[i].Contains(mousePos);
            }
            NextLevelButtonHovered = NextLevelButton.Contains(mousePos);

            // Game over buttons
            RetryButtonHovered = RetryButton.Contains(mousePos);
            QuitButtonHovered = QuitButton.Contains(mousePos);

            // Victory buttons
            VictoryRetryButtonHovered = VictoryRetryButton.Contains(mousePos);
            VictoryQuitButtonHovered = VictoryQuitButton.Contains(mousePos);
        }
    }
}
