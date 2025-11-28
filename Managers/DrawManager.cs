using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Models;
using AlleywayMonoGame.Services;
using AlleywayMonoGame.Systems;
using AlleywayMonoGame.UI;
using AlleywayMonoGame.Core;
using System;
using System.Collections.Generic;

namespace AlleywayMonoGame.Managers
{
    /// <summary>
    /// Handles all rendering operations for the game
    /// </summary>
    public class DrawManager
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _whitePixel;
        private readonly Texture2D? _ballTexture;
        private readonly Texture2D? _paddleTexture;
        private readonly Texture2D? _heartTexture;
        private readonly SpriteFont? _font;
        
        private readonly ScoreService _scoreService;
        private readonly ShopService _shopService;
        private readonly PowerUpManager _powerUpManager;
        private readonly ParticleSystem _particleSystem;
        private readonly FloatingTextSystem _floatingTextSystem;
        private readonly DialogRenderer? _dialogRenderer;

        public DrawManager(
            SpriteBatch spriteBatch,
            Texture2D whitePixel,
            Texture2D? ballTexture,
            Texture2D? paddleTexture,
            Texture2D? heartTexture,
            SpriteFont? font,
            ScoreService scoreService,
            ShopService shopService,
            PowerUpManager powerUpManager,
            ParticleSystem particleSystem,
            FloatingTextSystem floatingTextSystem,
            DialogRenderer? dialogRenderer)
        {
            _spriteBatch = spriteBatch;
            _whitePixel = whitePixel;
            _ballTexture = ballTexture;
            _paddleTexture = paddleTexture;
            _heartTexture = heartTexture;
            _font = font;
            _scoreService = scoreService;
            _shopService = shopService;
            _powerUpManager = powerUpManager;
            _particleSystem = particleSystem;
            _floatingTextSystem = floatingTextSystem;
            _dialogRenderer = dialogRenderer;
        }

        public void DrawUIArea()
        {
            // Dark gradient background
            for (int i = 0; i < 5; i++)
            {
                float alpha = 0.8f - (i * 0.1f);
                _spriteBatch.Draw(_whitePixel, new Rectangle(0, i * 8, GameConstants.ScreenWidth, 8), new Color(5, 5, 25) * alpha);
            }
            
            // Pixel stars in UI
            for (int x = 0; x < GameConstants.ScreenWidth; x += 40)
            {
                for (int y = 5; y < GameConstants.UIHeight - 5; y += 15)
                {
                    if ((x + y) % 80 == 0)
                    {
                        _spriteBatch.Draw(_whitePixel, new Rectangle(x, y, 1, 1), Color.White * 0.3f);
                    }
                }
            }
            
            // Border
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, GameConstants.UIHeight, GameConstants.ScreenWidth, 2), new Color(100, 150, 255));
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, GameConstants.UIHeight + 2, GameConstants.ScreenWidth, 1), new Color(150, 200, 255) * 0.5f);
        }

        public void DrawPaddle(Paddle paddle)
        {
            var bounds = paddle.Bounds;

            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X, bounds.Y + 2, bounds.Width, bounds.Height), new Color(20, 30, 45) * 0.6f);

            if (_paddleTexture != null)
                _spriteBatch.Draw(_paddleTexture, bounds, new Color(40, 60, 90));

            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 2, bounds.Y + 3, bounds.Width - 4, bounds.Height - 6), new Color(60, 75, 100));
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 8, bounds.Y + 2, bounds.Width - 16, 4), new Color(120, 140, 180));
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 5, bounds.Y + 4, 2, bounds.Height - 8), new Color(80, 120, 160));
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + bounds.Width - 7, bounds.Y + 4, 2, bounds.Height - 8), new Color(80, 120, 160));

            float glowPulse = (float)Math.Sin(_scoreService.GameTimer * 3) * 0.3f + 0.7f;
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + bounds.Width / 2 - 1, bounds.Y + 6, 2, bounds.Height - 12), new Color(0, 150, 255) * glowPulse);

            if (_powerUpManager.CannonExtension > 0f)
            {
                int cannonWidth = 8;
                int cannonHeight = (int)(20 * _powerUpManager.CannonExtension);
                int cannonX = bounds.X + bounds.Width / 2 - cannonWidth / 2;
                int cannonY = bounds.Y - cannonHeight;

                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX, cannonY, cannonWidth, cannonHeight), new Color(50, 55, 65));
                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX + 1, cannonY, cannonWidth - 2, cannonHeight), new Color(30, 35, 45));

                if (_powerUpManager.CannonExtension > 0.8f)
                {
                    _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX + 2, cannonY, cannonWidth - 4, 3), Color.Orange * glowPulse);
                }

                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX, cannonY + 2, 2, cannonHeight - 4), new Color(70, 80, 100));
                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX + cannonWidth - 2, cannonY + 2, 2, cannonHeight - 4), new Color(70, 80, 100));
            }
        }

        public void DrawBalls(List<Ball> balls)
        {
            foreach (var ball in balls)
            {
                if (_ballTexture != null)
                {
                    if (_powerUpManager.MultiBallChaosActive && balls.Count > 1)
                    {
                        float pulseTimer = (float)_scoreService.GameTimer * 8f;
                        float pulseScale = 1f + (float)Math.Sin(pulseTimer) * 0.15f;
                        
                        int pulseSize = (int)(ball.Rect.Width * pulseScale);
                        int pulseOffset = (ball.Rect.Width - pulseSize) / 2;
                        
                        Rectangle pulseRect = new Rectangle(
                            ball.Rect.X + pulseOffset,
                            ball.Rect.Y + pulseOffset,
                            pulseSize,
                            pulseSize
                        );
                        
                        float glowIntensity = (float)Math.Sin(pulseTimer) * 0.5f + 0.5f;
                        Color[] chaosColors = { Color.Magenta, Color.Cyan, Color.Yellow, Color.Lime };
                        int ballIndex = balls.IndexOf(ball);
                        Color glowColor = chaosColors[ballIndex % chaosColors.Length];
                        
                        int haloSize = pulseSize + (int)(6 * glowIntensity);
                        Rectangle haloRect = new Rectangle(
                            ball.Rect.X + (ball.Rect.Width - haloSize) / 2,
                            ball.Rect.Y + (ball.Rect.Width - haloSize) / 2,
                            haloSize,
                            haloSize
                        );
                        _spriteBatch.Draw(_ballTexture, haloRect, glowColor * (0.3f * glowIntensity));
                        
                        _spriteBatch.Draw(_ballTexture, pulseRect, Color.White);
                    }
                    else
                    {
                        _spriteBatch.Draw(_ballTexture, ball.Rect, Color.Silver);
                    }
                }
            }
        }

        public void DrawBricks(List<Brick> bricks)
        {
            for (int i = 0; i < bricks.Count; i++)
            {
                var brick = bricks[i];
                int brickHeight = 20;
                int row = (brick.Bounds.Y - 50) / (brickHeight + 2);
                Color brickColor = Brick.GetColorForRow(row);

                bool isSpecial = brick.Type == BrickType.Special && !_powerUpManager.CanShoot && !_powerUpManager.MultiBallChaosActive;

                if (brick.IsSteel)
                {
                    DrawSteelBrick(brick);
                }
                else if (isSpecial)
                {
                    DrawSpecialBrick(brick, brickColor);
                }
                else
                {
                    DrawNormalBrick(brick, brickColor);
                }
            }
        }

        private void DrawSteelBrick(Brick brick)
        {
            Color steelColor = new Color(100, 100, 110);
            _spriteBatch.Draw(_whitePixel, brick.Bounds, steelColor);

            Color sheenColor = new Color(180, 180, 190);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, brick.Bounds.Width - 4, 3), sheenColor);

            Color rivetColor = new Color(70, 70, 80);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 3, brick.Bounds.Y + 3, 2, 2), rivetColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.Right - 5, brick.Bounds.Y + 3, 2, 2), rivetColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 3, brick.Bounds.Bottom - 5, 2, 2), rivetColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.Right - 5, brick.Bounds.Bottom - 5, 2, 2), rivetColor);

            Color crackColor = new Color(40, 40, 50);
            int hits = brick.SteelHitsRemaining;

            if (hits <= 4)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 8, brick.Bounds.Y + 4, 1, 6), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 8, brick.Bounds.Y + 4, 6, 1), crackColor);
            }

            if (hits <= 3)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 14, brick.Bounds.Y + 7, 1, 8), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 14, brick.Bounds.Y + 7, 10, 1), crackColor);
            }

            if (hits <= 2)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 20, brick.Bounds.Y + 3, 1, 12), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 6, brick.Bounds.Y + 12, 12, 1), crackColor);
                _spriteBatch.Draw(_whitePixel, brick.Bounds, Color.Black * 0.2f);
            }

            if (hits <= 1)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 4, brick.Bounds.Y + 8, 1, 8), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 24, brick.Bounds.Y + 10, 1, 6), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 10, brick.Bounds.Y + 15, 15, 1), crackColor);
                _spriteBatch.Draw(_whitePixel, brick.Bounds, Color.Black * 0.4f);
            }
        }

        private void DrawSpecialBrick(Brick brick, Color brickColor)
        {
            float flickerAlpha = (float)Math.Sin(_powerUpManager.FlickerTimer * 2) * 0.5f + 0.5f;
            Color goldColor = new Color(255, 215, 0);
            Color specialColor = Color.Lerp(goldColor, Color.White, flickerAlpha);

            _spriteBatch.Draw(_whitePixel, brick.Bounds, specialColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, brick.Bounds.Width - 4, brick.Bounds.Height - 4), brickColor * 0.9f);
            
            int sparkleOffset = (int)(_powerUpManager.FlickerTimer * 10) % 4;
            for (int sx = 0; sx < brick.Bounds.Width; sx += 8)
            {
                for (int sy = 0; sy < brick.Bounds.Height; sy += 6)
                {
                    if ((sx + sy + sparkleOffset) % 12 == 0)
                    {
                        _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + sx, brick.Bounds.Y + sy, 1, 1), Color.White * flickerAlpha);
                    }
                }
            }
        }

        private void DrawNormalBrick(Brick brick, Color brickColor)
        {
            _spriteBatch.Draw(_whitePixel, brick.Bounds, brickColor);
            
            Color highlightColor = Color.Lerp(brickColor, Color.White, 0.4f);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, brick.Bounds.Width - 4, 2), highlightColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, 2, brick.Bounds.Height - 4), highlightColor);
            
            Color shadowColor = brickColor * 0.5f;
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 2, brick.Bounds.Bottom - 4, brick.Bounds.Width - 4, 2), shadowColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.Right - 4, brick.Bounds.Y + 2, 2, brick.Bounds.Height - 4), shadowColor);
        }

        public void DrawProjectiles(List<Projectile> projectiles)
        {
            foreach (var proj in projectiles)
            {
                _spriteBatch.Draw(_whitePixel, proj.Bounds, Color.Yellow);
            }
        }

        public void DrawUFO(UFO ufo)
        {
            Rectangle bounds = ufo.Bounds;
            int centerX = bounds.X + bounds.Width / 2;
            int centerY = bounds.Y + bounds.Height / 2;

            int domeHeight = bounds.Height / 3;
            Color domeColor = new Color(150, 150, 200);
            _spriteBatch.Draw(_whitePixel, new Rectangle(centerX - 12, bounds.Y, 24, domeHeight), domeColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(centerX - 6, bounds.Y + 2, 12, domeHeight - 4), Color.Cyan * 0.7f);

            Color bodyColor = new Color(100, 100, 120);
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X, centerY - 4, bounds.Width, 8), bodyColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 2, centerY - 3, bounds.Width - 4, 2), new Color(180, 180, 200));
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 2, centerY + 1, bounds.Width - 4, 2), new Color(60, 60, 80));

            float lightFlicker = (float)Math.Sin(_scoreService.GameTimer * 10) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(Color.Red, Color.Yellow, lightFlicker);
            
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 8, centerY, 3, 3), lightColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.Right - 11, centerY, 3, 3), lightColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(centerX - 1, centerY, 3, 3), Color.Green * lightFlicker);

            if (lightFlicker > 0.7f)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(centerX - 2, centerY + 4, 4, 10), Color.Cyan * 0.3f);
            }
        }

        public void DrawParticles()
        {
            foreach (var particle in _particleSystem.Particles)
            {
                var rect = new Rectangle(
                    (int)(particle.Position.X - particle.Size / 2f),
                    (int)(particle.Position.Y - particle.Size / 2f),
                    Math.Max(1, (int)particle.Size),
                    Math.Max(1, (int)particle.Size)
                );
                _spriteBatch.Draw(_whitePixel, rect, particle.Color * particle.Alpha);
            }
        }

        public void DrawFloatingTexts()
        {
            if (_font == null) return;

            foreach (var text in _floatingTextSystem.FloatingTexts)
            {
                Vector2 textSize = _font.MeasureString(text.Text);
                Vector2 textPos = new Vector2(text.Position.X - textSize.X / 2, text.Position.Y);

                for (int ox = -4; ox <= 4; ox++)
                {
                    for (int oy = -4; oy <= 4; oy++)
                    {
                        if (ox != 0 || oy != 0)
                        {
                            _spriteBatch.DrawString(_font, text.Text, textPos + new Vector2(ox, oy), Color.Black * text.Alpha);
                        }
                    }
                }
                _spriteBatch.DrawString(_font, text.Text, textPos, Color.Cyan * text.Alpha);
            }
        }

        public void DrawUI(int currentLevel)
        {
            if (_font == null) return;

            string scoreStr = $"{_scoreService.Score:D8}";
            _spriteBatch.DrawString(_font, scoreStr, new Vector2(GameConstants.ScreenWidth - 120, 15), Color.White);

            string bankStr = $"${_shopService.BankBalance}";
            _spriteBatch.DrawString(_font, bankStr, new Vector2(GameConstants.ScreenWidth - 250, 15), Color.Gold);

            for (int i = 0; i < _scoreService.Lives; i++)
            {
                if (_heartTexture != null)
                {
                    int heartX = 10 + i * 28;
                    int heartY = 13;
                    
                    // Draw heart
                    _spriteBatch.Draw(_heartTexture, new Rectangle(heartX, heartY, 24, 24), Color.Red);
                    
                    // Draw shield overlay if shield is active (only on first heart)
                    if (i == 0 && _shopService.HasShield)
                    {
                        DrawShieldOverlay(heartX, heartY);
                    }
                }
            }

            _spriteBatch.DrawString(_font, $"Level: {currentLevel}/10", new Vector2(10 + _scoreService.Lives * 28 + 10, 15), Color.White);

            string timerText = _scoreService.GetFormattedTime();
            Vector2 timerSize = _font.MeasureString(timerText);
            _spriteBatch.DrawString(_font, timerText, new Vector2((GameConstants.ScreenWidth - timerSize.X) / 2, 15), Color.White);
        }

        private void DrawShieldOverlay(int x, int y)
        {
            // Shield color with pulsing effect
            float pulse = (float)Math.Sin(_scoreService.GameTimer * 4f) * 0.3f + 0.7f;
            Color shieldColor = new Color(200, 150, 255) * pulse;
            
            // Shield shape around heart (medieval style)
            // Top
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 8, y - 2, 8, 2), shieldColor);
            
            // Upper sides
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 6, y, 2, 2), shieldColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 16, y, 2, 2), shieldColor);
            
            // Middle sides
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 4, y + 2, 2, 8), shieldColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 18, y + 2, 2, 8), shieldColor);
            
            // Lower sides narrowing
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 6, y + 10, 2, 4), shieldColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 16, y + 10, 2, 4), shieldColor);
            
            // Bottom point
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 8, y + 14, 2, 3), shieldColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 14, y + 14, 2, 3), shieldColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 10, y + 17, 4, 2), shieldColor);
            
            // Cross decoration (lighter)
            Color crossColor = Color.Lerp(shieldColor, Color.White, 0.5f);
            // Vertical line
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 11, y + 4, 2, 8), crossColor);
            // Horizontal line
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + 7, y + 7, 10, 2), crossColor);
        }

        public void DrawShootModeIndicator()
        {
            if (_powerUpManager.CanShoot && _font != null)
            {
                float textFlicker = (float)Math.Sin(_powerUpManager.FlickerTimer * 1.5f) * 0.5f + 0.5f;
                string powerUpText = "SPACE TO SHOOT";
                Vector2 textSize = _font.MeasureString(powerUpText);
                Vector2 textPos = new Vector2((GameConstants.ScreenWidth - textSize.X) / 2, GameConstants.ScreenHeight / 2 - 90);
                float alpha = 0.3f + textFlicker * 0.4f;

                for (int ox = -2; ox <= 2; ox++)
                {
                    for (int oy = -2; oy <= 2; oy++)
                    {
                        if (ox != 0 || oy != 0)
                        {
                            _spriteBatch.DrawString(_font, powerUpText, textPos + new Vector2(ox, oy), Color.Black * (alpha * 0.5f));
                        }
                    }
                }
                _spriteBatch.DrawString(_font, powerUpText, textPos, Color.Yellow * alpha);
            }
        }

        public void DrawPixelBox(int x, int y, int width, int height, Color color, int thickness = 2)
        {
            _spriteBatch.Draw(_whitePixel, new Rectangle(x, y, width, thickness), color);
            _spriteBatch.Draw(_whitePixel, new Rectangle(x, y + height - thickness, width, thickness), color);
            _spriteBatch.Draw(_whitePixel, new Rectangle(x, y, thickness, height), color);
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + width - thickness, y, thickness, height), color);
        }

        public void DrawPixelButton(Rectangle rect, bool hovered, string text, Color normalColor, Color hoverColor)
        {
            Color buttonColor = hovered ? hoverColor : normalColor;
            _spriteBatch.Draw(_whitePixel, rect, buttonColor * 0.8f);
            DrawPixelBox(rect.X, rect.Y, rect.Width, rect.Height, buttonColor);

            if (_font != null)
            {
                Vector2 textSize = _font.MeasureString(text);
                Vector2 textPos = new Vector2(rect.X + (rect.Width - textSize.X) / 2, rect.Y + (rect.Height - textSize.Y) / 2);

                for (int ox = -2; ox <= 2; ox++)
                {
                    for (int oy = -2; oy <= 2; oy++)
                    {
                        if (ox != 0 || oy != 0)
                        {
                            _spriteBatch.DrawString(_font, text, textPos + new Vector2(ox, oy), Color.Black);
                        }
                    }
                }
                _spriteBatch.DrawString(_font, text, textPos, Color.White);
            }
        }

        public void DrawGameOver(UIManager uiManager)
        {
            _dialogRenderer?.DrawGameOver(uiManager, _scoreService, _shopService, DrawPixelBox, DrawPixelButton);
        }

        public void DrawVictory(UIManager uiManager)
        {
            _dialogRenderer?.DrawVictory(uiManager, _scoreService, _shopService, DrawPixelBox, DrawPixelButton);
        }

        public void DrawLevelComplete(UIManager uiManager, ShopItem[] currentShopItems)
        {
            _dialogRenderer?.DrawLevelComplete(uiManager, _scoreService, _shopService, currentShopItems, DrawPixelBox, DrawPixelButton);
        }
    }
}
