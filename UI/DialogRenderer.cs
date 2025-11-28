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
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.ScreenHeight), Color.Black * 0.85f);

            // Pixel-Art Rahmen
            int boxWidth = 400;
            int boxHeight = 380;
            int boxX = (GameConstants.ScreenWidth - boxWidth) / 2;
            int boxY = 100;

            // Dunkler Hintergrund-Box
            _spriteBatch.Draw(_whitePixel, new Rectangle(boxX, boxY, boxWidth, boxHeight), new Color(10, 10, 30));

            // Doppelter Pixel-Rahmen
            drawPixelBox(boxX, boxY, boxWidth, boxHeight, new Color(150, 50, 50), 3);
            drawPixelBox(boxX + 6, boxY + 6, boxWidth - 12, boxHeight - 12, new Color(200, 100, 100), 2);

            int yPos = boxY + 30;

            // Title - Pixel Style
            string gameOverText = "GAME OVER";
            Vector2 titleSize = _font.MeasureString(gameOverText);
            Vector2 titlePos = new Vector2((GameConstants.ScreenWidth - titleSize.X) / 2, yPos);

            // Blocky shadow
            _spriteBatch.DrawString(_font, gameOverText, titlePos + new Vector2(3, 3), new Color(80, 0, 0));
            _spriteBatch.DrawString(_font, gameOverText, titlePos, new Color(255, 100, 100));
            yPos += 50;

            // Score and Time
            string scoreText = $"Score: {scoreService.Score}";
            string timeText = $"Time: {scoreService.GetFormattedTime()}";
            Vector2 scoreSize = _font.MeasureString(scoreText);
            Vector2 timeSize = _font.MeasureString(timeText);
            _spriteBatch.DrawString(_font, scoreText, new Vector2((GameConstants.ScreenWidth - scoreSize.X) / 2, yPos), Color.White);
            yPos += 30;
            _spriteBatch.DrawString(_font, timeText, new Vector2((GameConstants.ScreenWidth - timeSize.X) / 2, yPos), Color.White);
            yPos += 40;

            // Statistics - Pixel Box
            int statsBoxWidth = 300;
            int statsBoxX = (GameConstants.ScreenWidth - statsBoxWidth) / 2;
            _spriteBatch.Draw(_whitePixel, new Rectangle(statsBoxX, yPos, statsBoxWidth, 110), new Color(20, 20, 40));
            drawPixelBox(statsBoxX, yPos, statsBoxWidth, 110, new Color(100, 150, 200), 2);

            yPos += 10;
            string statsTitle = "STATISTICS";
            Vector2 statsTitleSize = _font.MeasureString(statsTitle);
            _spriteBatch.DrawString(_font, statsTitle, new Vector2((GameConstants.ScreenWidth - statsTitleSize.X) / 2, yPos), new Color(150, 200, 255));
            yPos += 30;

            string earnedText = $"Earned: ${shopService.TotalEarned}";
            string spentText = $"Spent: ${shopService.TotalSpent}";
            string profitText = $"Profit: ${shopService.TotalEarned - shopService.TotalSpent}";

            Vector2 earnedSize = _font.MeasureString(earnedText);
            Vector2 spentSize = _font.MeasureString(spentText);
            Vector2 profitSize = _font.MeasureString(profitText);

            _spriteBatch.DrawString(_font, earnedText, new Vector2((GameConstants.ScreenWidth - earnedSize.X) / 2, yPos), new Color(150, 255, 150));
            yPos += 25;
            _spriteBatch.DrawString(_font, spentText, new Vector2((GameConstants.ScreenWidth - spentSize.X) / 2, yPos), new Color(255, 150, 150));
            yPos += 25;

            Color profitColor = (shopService.TotalEarned - shopService.TotalSpent) >= 0 ? new Color(255, 215, 0) : new Color(255, 100, 100);
            _spriteBatch.DrawString(_font, profitText, new Vector2((GameConstants.ScreenWidth - profitSize.X) / 2, yPos), profitColor);

            // Pixel-Art Buttons
            drawPixelButton(uiManager.RetryButton, uiManager.RetryButtonHovered, "RETRY", new Color(100, 200, 100), new Color(50, 100, 50));
            drawPixelButton(uiManager.QuitButton, uiManager.QuitButtonHovered, "QUIT", new Color(200, 100, 100), new Color(100, 50, 50));
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

            _spriteBatch.Draw(_whitePixel, new Rectangle(boxX, boxY, boxWidth, boxHeight), new Color(10, 10, 30));
            drawPixelBox(boxX, boxY, boxWidth, boxHeight, new Color(255, 215, 0), 4);
            drawPixelBox(boxX + 8, boxY + 8, boxWidth - 16, boxHeight - 16, new Color(200, 170, 0), 2);

            int yPos = boxY + 30;
            float glowIntensity = (float)Math.Sin(uiManager.VictoryGlowTimer) * 0.3f + 0.7f;

            // Title
            string victoryText = "Winner, Winner";
            string chickenText = "Chicken Dinner!";

            Vector2 victorySize = _font.MeasureString(victoryText);
            Vector2 victoryPos = new Vector2((GameConstants.ScreenWidth - victorySize.X) / 2, yPos);
            _spriteBatch.DrawString(_font, victoryText, victoryPos + new Vector2(2, 2), new Color(100, 80, 0));
            _spriteBatch.DrawString(_font, victoryText, victoryPos, Color.Lerp(new Color(255, 215, 0), Color.White, glowIntensity * 0.3f));
            yPos += 40;

            Vector2 chickenSize = _font.MeasureString(chickenText);
            Vector2 chickenPos = new Vector2((GameConstants.ScreenWidth - chickenSize.X) / 2, yPos);
            _spriteBatch.DrawString(_font, chickenText, chickenPos + new Vector2(2, 2), new Color(100, 80, 0));
            _spriteBatch.DrawString(_font, chickenText, chickenPos, Color.Lerp(new Color(255, 215, 0), Color.White, glowIntensity * 0.3f));
            yPos += 50;

            string completionText = "All 10 Levels Completed!";
            Vector2 completionSize = _font.MeasureString(completionText);
            _spriteBatch.DrawString(_font, completionText, new Vector2((GameConstants.ScreenWidth - completionSize.X) / 2, yPos), new Color(150, 255, 150));
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
            _spriteBatch.Draw(_whitePixel, new Rectangle(statsBoxX, yPos, statsBoxWidth, statsBoxHeight), new Color(20, 20, 40));
            drawPixelBox(statsBoxX, yPos, statsBoxWidth, statsBoxHeight, new Color(100, 200, 255), 2);

            yPos += 12;
            string statsTitle = "FINANCIAL REPORT";
            Vector2 statsTitleSize = _font.MeasureString(statsTitle);
            _spriteBatch.DrawString(_font, statsTitle, new Vector2(statsBoxX + (statsBoxWidth - statsTitleSize.X) / 2, yPos), new Color(150, 200, 255));
            yPos += 28;

            string earnedText = $"Earned: ${shopService.TotalEarned}";
            string spentText = $"Spent: ${shopService.TotalSpent}";
            string profitText = $"Profit: ${shopService.TotalEarned - shopService.TotalSpent}";

            Vector2 earnedSize = _font.MeasureString(earnedText);
            Vector2 spentSize = _font.MeasureString(spentText);
            Vector2 profitSize = _font.MeasureString(profitText);

            _spriteBatch.DrawString(_font, earnedText, new Vector2(statsBoxX + (statsBoxWidth - earnedSize.X) / 2, yPos), new Color(150, 255, 150));
            yPos += 23;
            _spriteBatch.DrawString(_font, spentText, new Vector2(statsBoxX + (statsBoxWidth - spentSize.X) / 2, yPos), new Color(255, 150, 150));
            yPos += 23;

            Color profitColor = (shopService.TotalEarned - shopService.TotalSpent) >= 0 ? new Color(255, 215, 0) : new Color(255, 100, 100);
            _spriteBatch.DrawString(_font, profitText, new Vector2(statsBoxX + (statsBoxWidth - profitSize.X) / 2, yPos), profitColor);

            // Buttons
            drawPixelButton(uiManager.VictoryRetryButton, uiManager.VictoryRetryButtonHovered, "PLAY AGAIN", new Color(100, 200, 100), new Color(50, 100, 50));
            drawPixelButton(uiManager.VictoryQuitButton, uiManager.VictoryQuitButtonHovered, "QUIT", new Color(200, 100, 100), new Color(100, 50, 50));
        }

        public void DrawLevelComplete(UIManager uiManager, ScoreService scoreService, ShopService shopService,
            ShopItem[] currentShopItems,
            Action<int, int, int, int, Color, int> drawPixelBox,
            Action<Rectangle, bool, string, Color, Color> drawPixelButton)
        {
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.ScreenHeight), Color.Black * 0.8f);

            var layout = new DialogLayout.LevelCompleteLayout();
            var box = layout.DialogBox;

            // Main dialog box
            _spriteBatch.Draw(_whitePixel, box, new Color(10, 10, 30));
            drawPixelBox(box.X, box.Y, box.Width, box.Height, new Color(100, 200, 100), 4);
            drawPixelBox(box.X + 8, box.Y + 8, box.Width - 16, box.Height - 16, new Color(150, 255, 150), 2);

            // Title
            string title = "DONE!";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2((GameConstants.ScreenWidth - titleSize.X) / 2, layout.TitleY);
            _spriteBatch.DrawString(_font, title, titlePos + new Vector2(2, 2), new Color(0, 80, 0));
            _spriteBatch.DrawString(_font, title, titlePos, new Color(150, 255, 150));

            // Time Bonus Info Box
            int bonusBoxWidth = 400;
            int bonusBoxX = (GameConstants.ScreenWidth - bonusBoxWidth) / 2;
            _spriteBatch.Draw(_whitePixel, new Rectangle(bonusBoxX, layout.BonusBoxY, bonusBoxWidth, 60), new Color(20, 20, 40));
            drawPixelBox(bonusBoxX, layout.BonusBoxY, bonusBoxWidth, 60, new Color(255, 215, 0), 2);

            string calc = $"Time Bonus: $100 - {(int)scoreService.GameTimer}s = ${uiManager.LevelCompleteTimeBonus}";
            Vector2 calcSize = _font.MeasureString(calc);
            _spriteBatch.DrawString(_font, calc, new Vector2((GameConstants.ScreenWidth - calcSize.X) / 2, layout.BonusBoxY + 10), new Color(255, 215, 0));

            // Counting animation
            if (!uiManager.MoneyAnimationDone)
            {
                string counting = $"Counting... ${uiManager.AnimatedMoney}";
                Vector2 countingSize = _font.MeasureString(counting);
                _spriteBatch.DrawString(_font, counting, new Vector2((GameConstants.ScreenWidth - countingSize.X) / 2, layout.BonusBoxY + 35), Color.Gray);
            }

            // Balance Display
            if (uiManager.MoneyAnimationDone)
            {
                string balance = $"${shopService.BankBalance}";
                Vector2 balanceSize = _font.MeasureString(balance);
                Vector2 balancePos = new Vector2((GameConstants.ScreenWidth - balanceSize.X * uiManager.SlamScale) / 2 + uiManager.BalanceShake, layout.BalanceY + uiManager.SlamY);

                _spriteBatch.DrawString(_font, balance, balancePos + new Vector2(2, 2), new Color(100, 80, 0), 0f, Vector2.Zero, uiManager.SlamScale, SpriteEffects.None, 0f);
                _spriteBatch.DrawString(_font, balance, balancePos, new Color(255, 215, 0), 0f, Vector2.Zero, uiManager.SlamScale, SpriteEffects.None, 0f);

                // Purchase animation
                if (uiManager.PurchaseAnimationActive && uiManager.PurchaseAnimationTimer < 0.5f)
                {
                    string costText = $"-${uiManager.PurchaseCostAmount}";
                    Vector2 costPos = new Vector2(uiManager.PurchaseCostX, uiManager.PurchaseCostY);
                    float trailAlpha = 1f - (uiManager.PurchaseAnimationTimer / 0.5f);

                    _spriteBatch.DrawString(_font, costText, costPos + new Vector2(2, 2), new Color(100, 0, 0) * trailAlpha);
                    _spriteBatch.DrawString(_font, costText, costPos, Color.Red * trailAlpha);
                }
            }

            // Shop Section
            if (uiManager.MoneyAnimationDone && uiManager.SlamAnimationDone)
            {
                string shopTitle = "SHOP";
                Vector2 shopTitleSize = _font.MeasureString(shopTitle);
                _spriteBatch.DrawString(_font, shopTitle, new Vector2((GameConstants.ScreenWidth - shopTitleSize.X) / 2, layout.ShopTitleY), new Color(100, 200, 255));

                // Shop Box
                int shopBoxWidth = 420;
                int shopBoxX = (GameConstants.ScreenWidth - shopBoxWidth) / 2;
                int shopBoxHeight = 160;

                _spriteBatch.Draw(_whitePixel, new Rectangle(shopBoxX, layout.ShopBoxY, shopBoxWidth, shopBoxHeight), new Color(15, 15, 35));
                drawPixelBox(shopBoxX, layout.ShopBoxY, shopBoxWidth, shopBoxHeight, new Color(100, 150, 200), 3);

                int buttonWidth = 380;
                int buttonHeight = 35;
                int buttonX = shopBoxX + (shopBoxWidth - buttonWidth) / 2;
                int buttonSpacing = 10;

                // Shop Items
                for (int i = 0; i < 3; i++)
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
                    Color buttonNormal = isPurchased ? new Color(40, 40, 50) : 
                                        (canAfford ? Color.Lerp(new Color(50, 100, 200), itemColor, 0.3f) : new Color(60, 60, 80));
                    Color buttonDark = isPurchased ? new Color(30, 30, 40) :
                                      (canAfford ? Color.Lerp(new Color(30, 50, 100), itemColor, 0.2f) : new Color(40, 40, 50));

                    drawPixelButton(shopButton, uiManager.ShopButtonsHovered[i] && canAfford && !isPurchased, "", buttonNormal, buttonDark);

                    // Pixel art icon (dimmed if purchased)
                    Vector2 iconPos = new Vector2(shopButton.X + 8, shopButton.Y + 10);
                    Color iconColor = isPurchased ? new Color(80, 80, 90) : 
                                     (canAfford ? itemColor : new Color(120, 120, 120));
                    ShopIconRenderer.DrawIcon(_spriteBatch, _whitePixel, item, iconPos, iconColor);

                    // Item name (dimmed if purchased)
                    Vector2 textSize = _font.MeasureString(itemText);
                    Vector2 textPos = new Vector2(shopButton.X + 35, shopButton.Y + (shopButton.Height - textSize.Y) / 2);
                    Color textColor = isPurchased ? new Color(100, 100, 110) :
                                     (canAfford ? Color.White : new Color(120, 120, 120));
                    _spriteBatch.DrawString(_font, itemText, textPos + new Vector2(1, 1), Color.Black * 0.5f);
                    _spriteBatch.DrawString(_font, itemText, textPos, textColor);

                    // Cost or OWNED in corner
                    Vector2 costSize = _font.MeasureString(costText);
                    Vector2 costPos = new Vector2(shopButton.Right - costSize.X - 8, shopButton.Y + (shopButton.Height - costSize.Y) / 2);
                    if (isPurchased)
                    {
                        _spriteBatch.DrawString(_font, costText, costPos + new Vector2(1, 1), new Color(40, 40, 50));
                        _spriteBatch.DrawString(_font, costText, costPos, new Color(120, 180, 120));
                    }
                    else if (canAfford)
                    {
                        _spriteBatch.DrawString(_font, costText, costPos + new Vector2(1, 1), new Color(100, 80, 0));
                        _spriteBatch.DrawString(_font, costText, costPos, new Color(255, 215, 0));
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
                    
                    int tooltipWidth = (int)maxWidth + 20;
                    int tooltipHeight = lines.Length * 25 + 10;
                    int tooltipX = (GameConstants.ScreenWidth - tooltipWidth) / 2;
                    int tooltipY = layout.TooltipY - tooltipHeight;

                    // Pixel art tooltip box
                    _spriteBatch.Draw(_whitePixel, new Rectangle(tooltipX, tooltipY, tooltipWidth, tooltipHeight), new Color(10, 10, 30) * 0.95f);
                    drawPixelBox(tooltipX, tooltipY, tooltipWidth, tooltipHeight, itemColor, 2);
                    
                    // Draw description
                    int lineY = tooltipY + 8;
                    foreach (string line in lines)
                    {
                        Vector2 lineSize = _font.MeasureString(line);
                        Vector2 linePos = new Vector2(tooltipX + (tooltipWidth - lineSize.X) / 2, lineY);
                        _spriteBatch.DrawString(_font, line, linePos + new Vector2(1, 1), Color.Black * 0.7f);
                        _spriteBatch.DrawString(_font, line, linePos, Color.White * 0.9f);
                        lineY += 25;
                    }
                }

                // Reroll Button (properly spaced below shop items)
                int rerollWidth = 180;
                int rerollHeight = 30;
                int rerollX = shopBoxX + (shopBoxWidth - rerollWidth) / 2;
                uiManager.RerollButton = new Rectangle(rerollX, layout.RerollButtonY, rerollWidth, rerollHeight);
                bool canAffordReroll = shopService.CanAffordReroll();
                
                Color rerollNormal = canAffordReroll ? new Color(150, 100, 200) : new Color(60, 60, 80);
                Color rerollDark = canAffordReroll ? new Color(80, 50, 100) : new Color(40, 40, 50);
                
                drawPixelButton(uiManager.RerollButton, uiManager.RerollButtonHovered && canAffordReroll, "< REROLL $5 >", rerollNormal, rerollDark);

                // Next Level Button
                int nextButtonWidth = 200;
                int nextButtonHeight = 40;
                uiManager.NextLevelButton = DialogLayout.CalculateButton(box.X, box.Width, layout.NextButtonY, nextButtonWidth, nextButtonHeight);
                drawPixelButton(uiManager.NextLevelButton, uiManager.NextLevelButtonHovered, "NEXT LEVEL", new Color(100, 200, 100), new Color(50, 100, 50));
            }
        }
    }
}
