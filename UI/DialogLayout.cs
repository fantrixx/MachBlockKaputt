using Microsoft.Xna.Framework;
using AlleywayMonoGame.Core;

namespace AlleywayMonoGame.UI
{
    /// <summary>
    /// Helper class for consistent dialog layouts
    /// Prevents UI element overlapping and provides standard spacing
    /// </summary>
    public class DialogLayout
    {
        public const int Padding = 20;
        public const int ItemSpacing = 10;
        public const int SectionSpacing = 30;
        public const int ButtonHeight = 35;
        public const int SmallButtonHeight = 30;
        
        /// <summary>
        /// Calculates centered dialog box with safe margins
        /// </summary>
        public static Rectangle CalculateDialogBox(int width, int height)
        {
            int x = (GameConstants.ScreenWidth - width) / 2;
            int y = (GameConstants.ScreenHeight - height) / 2;
            
            // Ensure dialog fits on screen with margin
            const int safeMargin = 20;
            if (y < safeMargin)
                y = safeMargin;
            if (y + height > GameConstants.ScreenHeight - safeMargin)
                y = GameConstants.ScreenHeight - height - safeMargin;
            
            return new Rectangle(x, y, width, height);
        }
        
        /// <summary>
        /// Calculates button position within a container
        /// </summary>
        public static Rectangle CalculateButton(int containerX, int containerWidth, int y, int buttonWidth, int buttonHeight)
        {
            int x = containerX + (containerWidth - buttonWidth) / 2;
            return new Rectangle(x, y, buttonWidth, buttonHeight);
        }
        
        /// <summary>
        /// Layout for level complete dialog
        /// </summary>
        public class LevelCompleteLayout
        {
            public Rectangle DialogBox { get; }
            public int TitleY { get; }
            public int BonusBoxY { get; }
            public int BalanceY { get; }
            public int ShopTitleY { get; }
            public int ShopBoxY { get; }
            public int ShopItemsStartY { get; }
            public int BudgetY { get; }
            public int RerollButtonY { get; }
            public int NextButtonY { get; }
            public int TooltipY { get; }
            
            public LevelCompleteLayout()
            {
                // Calculate total required height
                int titleHeight = 50;
                int bonusBoxHeight = 50;
                int shopTitleHeight = 40;
                int shopBoxHeight = 165; // 3 items * (35 + 10 spacing)
                int budgetHeight = 40;
                int rerollHeight = 45;
                int nextButtonHeight = 50;
                
                int totalHeight = Padding * 2 + titleHeight + bonusBoxHeight + 
                                 shopTitleHeight + shopBoxHeight + budgetHeight + rerollHeight + nextButtonHeight;
                
                DialogBox = CalculateDialogBox(500, totalHeight);
                
                int y = DialogBox.Y + Padding;
                TitleY = y;
                
                y += titleHeight;
                BonusBoxY = y;
                
                y += bonusBoxHeight + SectionSpacing;
                ShopTitleY = y;
                
                y += shopTitleHeight;
                ShopBoxY = y;
                ShopItemsStartY = ShopBoxY + 15;
                
                y += shopBoxHeight + ItemSpacing;
                BudgetY = y;
                
                y += budgetHeight;
                RerollButtonY = y;
                
                y += rerollHeight;
                TooltipY = RerollButtonY - 10; // Above reroll button
                NextButtonY = y;
            }
        }
    }
}
