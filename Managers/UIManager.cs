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
        public Rectangle RerollButton { get; set; }
        public bool RerollButtonHovered { get; set; }
        public int HoveredShopItem { get; set; } = -1;
        
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
            HoveredShopItem = -1;
            for (int i = 0; i < 3; i++)
            {
                ShopButtonsHovered[i] = ShopButtons[i].Contains(mousePos);
                if (ShopButtonsHovered[i])
                {
                    HoveredShopItem = i;
                }
            }
            NextLevelButtonHovered = NextLevelButton.Contains(mousePos);
            RerollButtonHovered = RerollButton.Contains(mousePos);

            // Game over buttons
            RetryButtonHovered = RetryButton.Contains(mousePos);
            QuitButtonHovered = QuitButton.Contains(mousePos);

            // Victory buttons
            VictoryRetryButtonHovered = VictoryRetryButton.Contains(mousePos);
            VictoryQuitButtonHovered = VictoryQuitButton.Contains(mousePos);
        }

        /// <summary>
        /// Updates the money slam animation when money is earned
        /// </summary>
        public void UpdateSlamAnimation(float deltaTime)
        {
            if (SlamAnimationDone) return;

            const float gravity = 2000f;
            const float bounceAbsorption = 0.7f;
            const float restThreshold = 50f;

            // Apply physics
            SlamVelocity += gravity * deltaTime;
            float oldY = SlamY;
            SlamY += SlamVelocity * deltaTime;

            // Bounce at bottom
            if (SlamY >= 0 && oldY < 0)
            {
                SlamY = 0;
                SlamVelocity = -SlamVelocity * bounceAbsorption;

                // Impact scale effect
                SlamScale = 1.3f;

                // Check if animation should end
                if (System.Math.Abs(SlamVelocity) < restThreshold)
                {
                    SlamAnimationDone = true;
                    SlamVelocity = 0;
                    SlamScale = 1f;
                }
            }

            // Scale animation (squash & stretch)
            if (SlamScale > 1f)
            {
                SlamScale -= deltaTime * 3f;
                if (SlamScale < 1f) SlamScale = 1f;
            }

            // Glow pulse
            GlowPulse += deltaTime * 4f;
        }

        /// <summary>
        /// Updates the purchase animation when an item is bought
        /// </summary>
        public void UpdatePurchaseAnimation(float deltaTime)
        {
            if (!PurchaseAnimationActive) return;

            const float duration = 0.8f;
            PurchaseAnimationTimer += deltaTime;

            if (PurchaseAnimationTimer < duration)
            {
                float progress = PurchaseAnimationTimer / duration;
                float targetX = 40 + 350;
                float targetY = 50;

                // Smooth movement with easing
                float t = 1f - (float)System.Math.Pow(1f - progress, 3f);
                PurchaseCostX += (targetX - PurchaseCostX) * t * deltaTime * 5f;
                PurchaseCostY += (targetY - PurchaseCostY) * t * deltaTime * 5f;

                // Shake balance text on impact
                if (progress > 0.85f)
                {
                    BalanceShake = (float)System.Math.Sin(progress * 30f) * 5f * (1f - progress);
                }
            }
            else
            {
                PurchaseAnimationActive = false;
                PurchaseAnimationTimer = 0f;
                BalanceShake = 0f;
            }
        }

        /// <summary>
        /// Starts a purchase animation
        /// </summary>
        public void StartPurchaseAnimation(float startX, float startY, int cost)
        {
            PurchaseAnimationActive = true;
            PurchaseCostX = startX;
            PurchaseCostY = startY;
            PurchaseCostAmount = cost;
            PurchaseAnimationTimer = 0f;
            BalanceShake = 0f;
        }
    }
}
