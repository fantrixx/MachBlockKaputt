using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AlleywayMonoGame.Core;
using AlleywayMonoGame.Managers;
using AlleywayMonoGame.Services;
using System;

namespace AlleywayMonoGame.UI
{
    /// <summary>
    /// Renders all game dialogs (Game Over, Victory, Level Complete)
    /// Extracted from Game1 to keep it maintainable
    /// </summary>
    public class DialogRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _whitePixel;

        public DialogRenderer(SpriteBatch spriteBatch, SpriteFont font, Texture2D whitePixel)
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _whitePixel = whitePixel;
        }

        public void DrawGameOver(UIManager uiManager, ScoreService scoreService, ShopService shopService,
            Action<int, int, int, int, Color, int> drawPixelBox,
            Action<Rectangle, bool, string, Color, Color> drawPixelButton)
        {
            // Dunkler Overlay
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.ScreenHeight), Color.Black * UIConstants.OverlayOpacityMedium);

            // Pixel-Art Rahmen
            int boxWidth = UIConstants.GameOverBoxWidth;
            int boxHeight = UIConstants.GameOverBoxHeight;
            int boxX = (GameConstants.ScreenWidth - boxWidth) / 2;
            int boxY = UIConstants.GameOverBoxY;

            // Dunkler Hintergrund-Box
            _spriteBatch.Draw(_whitePixel, new Rectangle(boxX, boxY, boxWidth, boxHeight), UIConstants.DialogBackgroundDark);

            // Doppelter Pixel-Rahmen
            drawPixelBox(boxX, boxY, boxWidth, boxHeight, UIConstants.BorderRed, UIConstants.BorderMedium);
            drawPixelBox(boxX + UIConstants.BorderInnerOffset, boxY + UIConstants.BorderInnerOffset, boxWidth - UIConstants.BorderInnerWidth, boxHeight - UIConstants.BorderInnerWidth, UIConstants.BorderRedBright, UIConstants.BorderThin);

            int yPos = boxY + UIConstants.TextSectionSpacing;

            // Title - Pixel Style
            string gameOverText = "GAME OVER";
            Vector2 titleSize = _font.MeasureString(gameOverText);
            Vector2 titlePos = new Vector2((GameConstants.ScreenWidth - titleSize.X) / 2, yPos);

            // Blocky shadow
            _spriteBatch.DrawString(_font, gameOverText, titlePos + new Vector2(UIConstants.ShadowOffsetLarge, UIConstants.ShadowOffsetLarge), UIConstants.TextDarkShadow);
            _spriteBatch.DrawString(_font, gameOverText, titlePos, UIConstants.TextRed);
            yPos += 50;

            // Score and Time
            string scoreText = $"Score: {scoreService.Score}";
            string timeText = $"Time: {scoreService.GetFormattedTime()}";
            Vector2 scoreSize = _font.MeasureString(scoreText);
            Vector2 timeSize = _font.MeasureString(timeText);
            _spriteBatch.DrawString(_font, scoreText, new Vector2((GameConstants.ScreenWidth - scoreSize.X) / 2, yPos), Color.White);
            yPos += UIConstants.TextSectionSpacing;
            _spriteBatch.DrawString(_font, timeText, new Vector2((GameConstants.ScreenWidth - timeSize.X) / 2, yPos), Color.White);
            yPos += UIConstants.TextSmallSpacing;

            // Statistics - Pixel Box
            int statsBoxWidth = UIConstants.GameOverStatsBoxWidth;
            int statsBoxX = (GameConstants.ScreenWidth - statsBoxWidth) / 2;
            _spriteBatch.Draw(_whitePixel, new Rectangle(statsBoxX, yPos, statsBoxWidth, UIConstants.GameOverStatsBoxHeight), UIConstants.DialogBackgroundLight);
            drawPixelBox(statsBoxX, yPos, statsBoxWidth, UIConstants.GameOverStatsBoxHeight, UIConstants.BorderBlue, UIConstants.BorderThin);

            yPos += UIConstants.TooltipPadding;
            string statsTitle = "STATISTICS";
            Vector2 statsTitleSize = _font.MeasureString(statsTitle);
            _spriteBatch.DrawString(_font, statsTitle, new Vector2((GameConstants.ScreenWidth - statsTitleSize.X) / 2, yPos), UIConstants.TextBlue);
            yPos += UIConstants.TextSectionSpacing;

            string earnedText = $"Earned: ${shopService.TotalEarned}";
            string spentText = $"Spent: ${shopService.TotalSpent}";
            string profitText = $"Profit: ${shopService.TotalEarned - shopService.TotalSpent}";

            Vector2 earnedSize = _font.MeasureString(earnedText);
            Vector2 spentSize = _font.MeasureString(spentText);
            Vector2 profitSize = _font.MeasureString(profitText);

            _spriteBatch.DrawString(_font, earnedText, new Vector2((GameConstants.ScreenWidth - earnedSize.X) / 2, yPos), UIConstants.TextGreenEarned);
            yPos += UIConstants.TextLineSpacing;
            _spriteBatch.DrawString(_font, spentText, new Vector2((GameConstants.ScreenWidth - spentSize.X) / 2, yPos), UIConstants.TextRedSpent);
            yPos += UIConstants.TextLineSpacing;

            Color profitColor = (shopService.TotalEarned - shopService.TotalSpent) >= 0 ? UIConstants.TextGold : UIConstants.TextRed;
            _spriteBatch.DrawString(_font, profitText, new Vector2((GameConstants.ScreenWidth - profitSize.X) / 2, yPos), profitColor);

            // Pixel-Art Buttons
            drawPixelButton(uiManager.RetryButton, uiManager.RetryButtonHovered, "RETRY", UIConstants.ButtonGreenNormal, UIConstants.ButtonGreenDark);
            drawPixelButton(uiManager.QuitButton, uiManager.QuitButtonHovered, "QUIT", UIConstants.ButtonRedNormal, UIConstants.ButtonRedDark);
        }

        public void DrawVictory(UIManager uiManager, ScoreService scoreService, ShopService shopService,
            Action<int, int, int, int, Color, int> drawPixelBox,
            Action<Rectangle, bool, string, Color, Color> drawPixelButton)
        {
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.ScreenHeight), Color.Black * 0.9f);

            // Pixel-Art Victory Box
            int boxWidth = 450;
            int boxHeight = 450;
            int boxX = (GameConstants.ScreenWidth - boxWidth) / 2;
            int boxY = 70;

            _spriteBatch.Draw(_whitePixel, new Rectangle(boxX, boxY, boxWidth, boxHeight), UIConstants.DialogBackgroundDark);
            drawPixelBox(boxX, boxY, boxWidth, boxHeight, UIConstants.BorderGold, UIConstants.BorderThick);
            drawPixelBox(boxX + UIConstants.BorderInnerOffset2, boxY + UIConstants.BorderInnerOffset2, boxWidth - UIConstants.BorderInnerWidth2, boxHeight - UIConstants.BorderInnerWidth2, UIConstants.BorderGoldDark, UIConstants.BorderThin);

            int yPos = boxY + UIConstants.TextSectionSpacing;
            float glowIntensity = (float)Math.Sin(uiManager.VictoryGlowTimer) * 0.3f + 0.7f;

            // Title
            string victoryText = "Winner, Winner";
            string chickenText = "Chicken Dinner!";

            Vector2 victorySize = _font.MeasureString(victoryText);
            Vector2 victoryPos = new Vector2((GameConstants.ScreenWidth - victorySize.X) / 2, yPos);
            _spriteBatch.DrawString(_font, victoryText, victoryPos + new Vector2(UIConstants.ShadowOffsetMedium, UIConstants.ShadowOffsetMedium), UIConstants.TextGoldDark);
            _spriteBatch.DrawString(_font, victoryText, victoryPos, Color.Lerp(UIConstants.TextGold, Color.White, glowIntensity * UIConstants.GlowOpacity));
            yPos += UIConstants.TextSmallSpacing;

            Vector2 chickenSize = _font.MeasureString(chickenText);
            Vector2 chickenPos = new Vector2((GameConstants.ScreenWidth - chickenSize.X) / 2, yPos);
            _spriteBatch.DrawString(_font, chickenText, chickenPos + new Vector2(UIConstants.ShadowOffsetMedium, UIConstants.ShadowOffsetMedium), UIConstants.TextGoldDark);
            _spriteBatch.DrawString(_font, chickenText, chickenPos, Color.Lerp(UIConstants.TextGold, Color.White, glowIntensity * UIConstants.GlowOpacity));
            yPos += UIConstants.TextMediumSpacing;

            string completionText = "All 10 Levels Completed!";
            Vector2 completionSize = _font.MeasureString(completionText);
            _spriteBatch.DrawString(_font, completionText, new Vector2((GameConstants.ScreenWidth - completionSize.X) / 2, yPos), UIConstants.TextGreen);
            yPos += 35;

            // Score
            string scoreText = $"Score: {scoreService.Score}";
            Vector2 scoreSize = _font.MeasureString(scoreText);
            _spriteBatch.DrawString(_font, scoreText, new Vector2((GameConstants.ScreenWidth - scoreSize.X) / 2, yPos), Color.White);
            yPos += 35;

            // Stats Box
            int statsBoxWidth = 350;
            int statsBoxHeight = 115;
            int statsBoxX = boxX + (boxWidth - statsBoxWidth) / 2;
            _spriteBatch.Draw(_whitePixel, new Rectangle(statsBoxX, yPos, statsBoxWidth, statsBoxHeight), UIConstants.DialogBackgroundLight);
            drawPixelBox(statsBoxX, yPos, statsBoxWidth, statsBoxHeight, UIConstants.BorderBrightBlue, UIConstants.BorderThin);

            yPos += 12;
            string statsTitle = "FINANCIAL REPORT";
            Vector2 statsTitleSize = _font.MeasureString(statsTitle);
            _spriteBatch.DrawString(_font, statsTitle, new Vector2(statsBoxX + (statsBoxWidth - statsTitleSize.X) / 2, yPos), UIConstants.TextBlue);
            yPos += 28;

            string earnedText = $"Earned: ${shopService.TotalEarned}";
            string spentText = $"Spent: ${shopService.TotalSpent}";
            string profitText = $"Profit: ${shopService.TotalEarned - shopService.TotalSpent}";

            Vector2 earnedSize = _font.MeasureString(earnedText);
            Vector2 spentSize = _font.MeasureString(spentText);
            Vector2 profitSize = _font.MeasureString(profitText);

            _spriteBatch.DrawString(_font, earnedText, new Vector2(statsBoxX + (statsBoxWidth - earnedSize.X) / 2, yPos), UIConstants.TextGreenEarned);
            yPos += 23;
            _spriteBatch.DrawString(_font, spentText, new Vector2(statsBoxX + (statsBoxWidth - spentSize.X) / 2, yPos), UIConstants.TextRedSpent);
            yPos += 23;

            Color profitColor = (shopService.TotalEarned - shopService.TotalSpent) >= 0 ? UIConstants.TextGold : UIConstants.TextRed;
            _spriteBatch.DrawString(_font, profitText, new Vector2(statsBoxX + (statsBoxWidth - profitSize.X) / 2, yPos), profitColor);

            // Buttons
            drawPixelButton(uiManager.VictoryRetryButton, uiManager.VictoryRetryButtonHovered, "PLAY AGAIN", UIConstants.ButtonGreenNormal, UIConstants.ButtonGreenDark);
            drawPixelButton(uiManager.VictoryQuitButton, uiManager.VictoryQuitButtonHovered, "QUIT", UIConstants.ButtonRedNormal, UIConstants.ButtonRedDark);
        }

        public void DrawLevelComplete(UIManager uiManager, ScoreService scoreService, ShopService shopService,
            ShopItem[] currentShopItems,
            Action<int, int, int, int, Color, int> drawPixelBox,
            Action<Rectangle, bool, string, Color, Color> drawPixelButton)
        {
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.ScreenHeight), Color.Black * UIConstants.OverlayOpacityLow);

            var layout = new DialogLayout.LevelCompleteLayout();
            var box = layout.DialogBox;

            // Main dialog box
            _spriteBatch.Draw(_whitePixel, box, UIConstants.DialogBackgroundDark);
            drawPixelBox(box.X, box.Y, box.Width, box.Height, UIConstants.BorderGreen, UIConstants.BorderThick);
            drawPixelBox(box.X + UIConstants.BorderInnerOffset2, box.Y + UIConstants.BorderInnerOffset2, box.Width - UIConstants.BorderInnerWidth2, box.Height - UIConstants.BorderInnerWidth2, UIConstants.BorderGreenBright, UIConstants.BorderThin);

            // Title
            string title = "DONE!";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2((GameConstants.ScreenWidth - titleSize.X) / 2, layout.TitleY);
            _spriteBatch.DrawString(_font, title, titlePos + new Vector2(UIConstants.ShadowOffsetMedium, UIConstants.ShadowOffsetMedium), UIConstants.TextGreenShadow);
            _spriteBatch.DrawString(_font, title, titlePos, UIConstants.TextGreen);

            // Time Bonus Info Box
            int bonusBoxWidth = UIConstants.BonusBoxWidth;
            int bonusBoxX = (GameConstants.ScreenWidth - bonusBoxWidth) / 2;
            _spriteBatch.Draw(_whitePixel, new Rectangle(bonusBoxX, layout.BonusBoxY, bonusBoxWidth, UIConstants.BonusBoxHeight), UIConstants.DialogBackgroundLight);
            drawPixelBox(bonusBoxX, layout.BonusBoxY, bonusBoxWidth, UIConstants.BonusBoxHeight, UIConstants.BorderGold, UIConstants.BorderThin);

            // Time Bonus Calculation with animated money
            int displayedBonus = uiManager.MoneyAnimationDone ? uiManager.LevelCompleteTimeBonus : uiManager.AnimatedMoney;
            string calc = $"Time Bonus: $100 - {(int)scoreService.GameTimer}s = ${displayedBonus}";
            Vector2 calcSize = _font.MeasureString(calc);
            
            // Highlight the bonus amount during animation
            Color calcColor = uiManager.MoneyAnimationDone ? UIConstants.TextGold : Color.Lerp(Color.Gray, UIConstants.TextGold, (float)uiManager.AnimatedMoney / uiManager.LevelCompleteTimeBonus);
            _spriteBatch.DrawString(_font, calc, new Vector2((GameConstants.ScreenWidth - calcSize.X) / 2, layout.BonusBoxY + 10), calcColor);

            // Purchase animation (show anytime after money animation)
            if (uiManager.MoneyAnimationDone && uiManager.PurchaseAnimationActive && uiManager.PurchaseAnimationTimer < 0.5f)
            {
                string costText = $"-${uiManager.PurchaseCostAmount}";
                Vector2 costPos = new Vector2(uiManager.PurchaseCostX, uiManager.PurchaseCostY);
                float trailAlpha = 1f - (uiManager.PurchaseAnimationTimer / 0.5f);

                _spriteBatch.DrawString(_font, costText, costPos + new Vector2(UIConstants.ShadowOffsetMedium, UIConstants.ShadowOffsetMedium), UIConstants.TextDarkShadow * trailAlpha);
                _spriteBatch.DrawString(_font, costText, costPos, Color.Red * trailAlpha);
            }

            // Shop Section - appears immediately after counting, before pop animation
            if (uiManager.MoneyAnimationDone)
            {
                string shopTitle = "SHOP";
                Vector2 shopTitleSize = _font.MeasureString(shopTitle);
                _spriteBatch.DrawString(_font, shopTitle, new Vector2((GameConstants.ScreenWidth - shopTitleSize.X) / 2, layout.ShopTitleY), UIConstants.TextBlueBright);

                // Shop Box
                int shopBoxWidth = UIConstants.ShopBoxWidth;
                int shopBoxX = (GameConstants.ScreenWidth - shopBoxWidth) / 2;
                int shopBoxHeight = UIConstants.ShopBoxHeight;

                _spriteBatch.Draw(_whitePixel, new Rectangle(shopBoxX, layout.ShopBoxY, shopBoxWidth, shopBoxHeight), UIConstants.DialogBackgroundMedium);
                drawPixelBox(shopBoxX, layout.ShopBoxY, shopBoxWidth, shopBoxHeight, UIConstants.BorderBlue, UIConstants.BorderMedium);

                int buttonWidth = UIConstants.ShopItemButtonWidth;
                int buttonHeight = UIConstants.ShopItemButtonHeight;
                int buttonX = shopBoxX + (shopBoxWidth - buttonWidth) / 2;
                int buttonSpacing = UIConstants.ShopItemButtonSpacing;

                // Shop Items
                for (int i = 0; i < currentShopItems.Length; i++)
                {
                    ShopItem item = currentShopItems[i];
                    bool isPurchased = shopService.IsPurchased(item);
                    bool canAfford = !isPurchased && shopService.CanAfford(item);
                    Color itemColor = shopService.GetItemColor(item);
                    string itemText = $"{shopService.GetItemName(item)}";
                    string costText = isPurchased ? "OWNED" : $"${shopService.GetCost(item)}";

                    int itemY = layout.ShopItemsStartY + i * (buttonHeight + buttonSpacing);
                    Rectangle shopButton = new Rectangle(buttonX, itemY, buttonWidth, buttonHeight);
                    uiManager.ShopButtons[i] = shopButton;

                    // Button background with item color tint (grayed out if purchased)
                    Color buttonNormal = isPurchased ? UIConstants.ButtonPurchasedNormal : 
                                        (canAfford ? Color.Lerp(UIConstants.ButtonBlueNormal, itemColor, UIConstants.AnimationLerpSpeed) : UIConstants.ButtonDisabledNormal);
                    Color buttonDark = isPurchased ? UIConstants.ButtonPurchasedDark :
                                      (canAfford ? Color.Lerp(UIConstants.ButtonBlueDark, itemColor, UIConstants.AnimationLerpSpeed2) : UIConstants.ButtonDisabledDark);

                    drawPixelButton(shopButton, uiManager.ShopButtonsHovered[i] && canAfford && !isPurchased, "", buttonNormal, buttonDark);

                    // Pixel art icon (dimmed if purchased)
                    Vector2 iconPos = new Vector2(shopButton.X + 8, shopButton.Y + 10);
                    Color iconColor = isPurchased ? UIConstants.IconDisabled : 
                                     (canAfford ? itemColor : UIConstants.TextGray);
                    ShopIconRenderer.DrawIcon(_spriteBatch, _whitePixel, item, iconPos, iconColor);

                    // Item name (dimmed if purchased)
                    Vector2 textSize = _font.MeasureString(itemText);
                    Vector2 textPos = new Vector2(shopButton.X + 35, shopButton.Y + (shopButton.Height - textSize.Y) / 2);
                    Color textColor = isPurchased ? UIConstants.TextGrayDim :
                                     (canAfford ? Color.White : UIConstants.TextGray);
                    _spriteBatch.DrawString(_font, itemText, textPos + new Vector2(UIConstants.ShadowOffsetSmall, UIConstants.ShadowOffsetSmall), Color.Black * UIConstants.ShadowOpacity);
                    _spriteBatch.DrawString(_font, itemText, textPos, textColor);

                    // Cost or OWNED in corner
                    Vector2 costSize = _font.MeasureString(costText);
                    Vector2 costPos = new Vector2(shopButton.Right - costSize.X - 8, shopButton.Y + (shopButton.Height - costSize.Y) / 2);
                    if (isPurchased)
                    {
                        _spriteBatch.DrawString(_font, costText, costPos + new Vector2(UIConstants.ShadowOffsetSmall, UIConstants.ShadowOffsetSmall), UIConstants.ButtonPurchasedNormal);
                        _spriteBatch.DrawString(_font, costText, costPos, UIConstants.TextGrayOwned);
                    }
                    else if (canAfford)
                    {
                        _spriteBatch.DrawString(_font, costText, costPos + new Vector2(UIConstants.ShadowOffsetSmall, UIConstants.ShadowOffsetSmall), UIConstants.TextGoldDark);
                        _spriteBatch.DrawString(_font, costText, costPos, UIConstants.TextGold);
                    }
                }

                // Hover Tooltip (above reroll button to prevent overlap)
                if (uiManager.HoveredShopItem >= 0 && uiManager.HoveredShopItem < 3)
                {
                    ShopItem hoveredItem = currentShopItems[uiManager.HoveredShopItem];
                    string description = shopService.GetItemDescription(hoveredItem);
                    Color itemColor = shopService.GetItemColor(hoveredItem);

                    // Measure tooltip size
                    string[] lines = description.Split('\n');
                    float maxWidth = 0;
                    foreach (string line in lines)
                    {
                        float width = _font.MeasureString(line).X;
                        if (width > maxWidth) maxWidth = width;
                    }
                    
                    int tooltipWidth = (int)maxWidth + UIConstants.TooltipPadding * 2;
                    int tooltipHeight = lines.Length * UIConstants.TooltipLineHeight + UIConstants.TooltipPadding;
                    int tooltipX = (GameConstants.ScreenWidth - tooltipWidth) / 2;
                    int tooltipY = layout.TooltipY - tooltipHeight;

                    // Pixel art tooltip box
                    _spriteBatch.Draw(_whitePixel, new Rectangle(tooltipX, tooltipY, tooltipWidth, tooltipHeight), UIConstants.DialogBackgroundDark * UIConstants.TooltipOpacity);
                    drawPixelBox(tooltipX, tooltipY, tooltipWidth, tooltipHeight, itemColor, UIConstants.TooltipBorderWidth);
                    
                    // Draw description
                    int lineY = tooltipY + 8;
                    foreach (string line in lines)
                    {
                        Vector2 lineSize = _font.MeasureString(line);
                        Vector2 linePos = new Vector2(tooltipX + (tooltipWidth - lineSize.X) / 2, lineY);
                        _spriteBatch.DrawString(_font, line, linePos + new Vector2(UIConstants.ShadowOffsetSmall, UIConstants.ShadowOffsetSmall), Color.Black * UIConstants.ScanlineOpacity);
                        _spriteBatch.DrawString(_font, line, linePos, Color.White * 0.9f);
                        lineY += UIConstants.TooltipLineHeight;
                    }
                }

                // Budget Display (between shop and reroll button)
                // Label always visible, number pops after 1 second delay
                string budgetLabel = "BUDGET: ";
                string budgetValue = $"${shopService.BankBalance}";
                
                Vector2 labelSize = _font.MeasureString(budgetLabel);
                Vector2 valueSize = _font.MeasureString(budgetValue);
                
                // Center the whole thing
                float totalWidth = labelSize.X + valueSize.X;
                float startX = (GameConstants.ScreenWidth - totalWidth) / 2;
                
                // Draw label (always visible when shop appears)
                Vector2 labelPos = new Vector2(startX, layout.BudgetY);
                _spriteBatch.DrawString(_font, budgetLabel, labelPos + new Vector2(UIConstants.ShadowOffsetMedium, UIConstants.ShadowOffsetMedium), UIConstants.TextGoldDark);
                _spriteBatch.DrawString(_font, budgetLabel, labelPos, UIConstants.TextGold);
                
                // Draw value ONLY after delay is over (with pop animation if scale > 1)
                if (uiManager.SlamDelayTimer <= 0)
                {
                    Vector2 valuePos = new Vector2(startX + labelSize.X, layout.BudgetY);
                    
                    // Glow effect when scale is big
                    if (uiManager.SlamScale > UIConstants.GlowScaleThreshold)
                    {
                        float glowSize = uiManager.SlamScale * UIConstants.GlowScaleMultiplier;
                        _spriteBatch.DrawString(_font, budgetValue, valuePos, UIConstants.TextGold * UIConstants.ShadowOpacity, 0f, Vector2.Zero, glowSize, SpriteEffects.None, 0f);
                    }
                    
                    _spriteBatch.DrawString(_font, budgetValue, valuePos + new Vector2(UIConstants.ShadowOffsetMedium, UIConstants.ShadowOffsetMedium), UIConstants.TextGoldDark, 0f, Vector2.Zero, uiManager.SlamScale, SpriteEffects.None, 0f);
                    _spriteBatch.DrawString(_font, budgetValue, valuePos, UIConstants.TextGold, 0f, Vector2.Zero, uiManager.SlamScale, SpriteEffects.None, 0f);
                }

                // Reroll Button (properly spaced below shop items)
                int rerollWidth = 180;
                int rerollHeight = 30;
                int rerollX = shopBoxX + (shopBoxWidth - rerollWidth) / 2;
                uiManager.RerollButton = new Rectangle(rerollX, layout.RerollButtonY, rerollWidth, rerollHeight);
                bool canAffordReroll = shopService.CanAffordReroll();
                
                Color rerollNormal = canAffordReroll ? UIConstants.ButtonPurpleNormal : UIConstants.ButtonDisabledNormal;
                Color rerollDark = canAffordReroll ? UIConstants.ButtonPurpleDark : UIConstants.ButtonDisabledDark;
                
                drawPixelButton(uiManager.RerollButton, uiManager.RerollButtonHovered && canAffordReroll, "< REROLL $5 >", rerollNormal, rerollDark);

                // Next Level Button
                int nextButtonWidth = 200;
                int nextButtonHeight = 40;
                uiManager.NextLevelButton = DialogLayout.CalculateButton(box.X, box.Width, layout.NextButtonY, nextButtonWidth, nextButtonHeight);
                drawPixelButton(uiManager.NextLevelButton, uiManager.NextLevelButtonHovered, "NEXT LEVEL", UIConstants.ButtonGreenNormal, UIConstants.ButtonGreenDark);
            }
        }
    }
}
