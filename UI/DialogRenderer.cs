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

            // Pixel-Art Main Box
            int boxWidth = 500;
            int boxHeight = 550;
            int boxX = (GameConstants.ScreenWidth - boxWidth) / 2;
            int boxY = 50;

            _spriteBatch.Draw(_whitePixel, new Rectangle(boxX, boxY, boxWidth, boxHeight), new Color(10, 10, 30));
            drawPixelBox(boxX, boxY, boxWidth, boxHeight, new Color(100, 200, 100), 4);
            drawPixelBox(boxX + 8, boxY + 8, boxWidth - 16, boxHeight - 16, new Color(150, 255, 150), 2);

            int yPos = boxY + 30;

            // Title
            string title = "DONE!";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2((GameConstants.ScreenWidth - titleSize.X) / 2, yPos);
            _spriteBatch.DrawString(_font, title, titlePos + new Vector2(2, 2), new Color(0, 80, 0));
            _spriteBatch.DrawString(_font, title, titlePos, new Color(150, 255, 150));
            yPos += 50;

            // Time Bonus Info Box
            int bonusBoxWidth = 400;
            int bonusBoxX = (GameConstants.ScreenWidth - bonusBoxWidth) / 2;
            _spriteBatch.Draw(_whitePixel, new Rectangle(bonusBoxX, yPos, bonusBoxWidth, 60), new Color(20, 20, 40));
            drawPixelBox(bonusBoxX, yPos, bonusBoxWidth, 60, new Color(255, 215, 0), 2);

            yPos += 10;
            string calc = $"Time Bonus: $100 - {(int)scoreService.GameTimer}s = ${uiManager.LevelCompleteTimeBonus}";
            Vector2 calcSize = _font.MeasureString(calc);
            _spriteBatch.DrawString(_font, calc, new Vector2((GameConstants.ScreenWidth - calcSize.X) / 2, yPos), new Color(255, 215, 0));
            yPos += 25;

            // Counting animation
            if (!uiManager.MoneyAnimationDone)
            {
                string counting = $"Counting... ${uiManager.AnimatedMoney}";
                Vector2 countingSize = _font.MeasureString(counting);
                _spriteBatch.DrawString(_font, counting, new Vector2((GameConstants.ScreenWidth - countingSize.X) / 2, yPos), Color.Gray);
            }
            yPos += 35;

            // Balance Display
            if (uiManager.MoneyAnimationDone)
            {
                string balance = $"${shopService.BankBalance}";
                Vector2 balanceSize = _font.MeasureString(balance);
                Vector2 balancePos = new Vector2((GameConstants.ScreenWidth - balanceSize.X * uiManager.SlamScale) / 2 + uiManager.BalanceShake, yPos + uiManager.SlamY);

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
            yPos += 50;

            // Shop Section
            if (uiManager.MoneyAnimationDone && uiManager.SlamAnimationDone)
            {
                string shopTitle = "SHOP";
                Vector2 shopTitleSize = _font.MeasureString(shopTitle);
                _spriteBatch.DrawString(_font, shopTitle, new Vector2((GameConstants.ScreenWidth - shopTitleSize.X) / 2, yPos), new Color(100, 200, 255));
                yPos += 30;

                // Shop Box
                int shopBoxWidth = 420;
                int shopBoxX = (GameConstants.ScreenWidth - shopBoxWidth) / 2;
                int shopBoxHeight = 190;

                _spriteBatch.Draw(_whitePixel, new Rectangle(shopBoxX, yPos, shopBoxWidth, shopBoxHeight), new Color(15, 15, 35));
                drawPixelBox(shopBoxX, yPos, shopBoxWidth, shopBoxHeight, new Color(100, 150, 200), 3);

                int shopContentY = yPos + 15;
                int buttonWidth = 380;
                int buttonHeight = 35;
                int buttonX = shopBoxX + (shopBoxWidth - buttonWidth) / 2;
                int buttonSpacing = 10;

                // Shop Items
                for (int i = 0; i < 3; i++)
                {
                    ShopItem item = currentShopItems[i];
                    bool canAfford = shopService.CanAfford(item);
                    string itemText = $"{shopService.GetItemName(item)}";
                    string costText = $"${shopService.GetCost(item)}";

                    Rectangle shopButton = new Rectangle(buttonX, shopContentY + i * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);

                    Color buttonNormal = canAfford ? new Color(50, 100, 200) : new Color(60, 60, 80);
                    Color buttonDark = canAfford ? new Color(30, 50, 100) : new Color(40, 40, 50);

                    drawPixelButton(shopButton, uiManager.ShopButtonsHovered[i] && canAfford, itemText, buttonNormal, buttonDark);

                    // Cost in corner
                    if (canAfford)
                    {
                        Vector2 costSize = _font.MeasureString(costText);
                        Vector2 costPos = new Vector2(shopButton.Right - costSize.X - 8, shopButton.Y + 8);
                        _spriteBatch.DrawString(_font, costText, costPos, new Color(255, 215, 0));
                    }
                }

                yPos += 140;

                // Next Level Button
                drawPixelButton(uiManager.NextLevelButton, uiManager.NextLevelButtonHovered, "NEXT LEVEL", new Color(100, 200, 100), new Color(50, 100, 50));
            }
        }
    }
}
