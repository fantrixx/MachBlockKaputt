using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using AlleywayMonoGame.Models;
using AlleywayMonoGame.Core;

namespace AlleywayMonoGame.Managers
{
    /// <summary>
    /// Manages the retro space background rendering.
    /// </summary>
    public class BackgroundManager
    {
        private readonly List<Star> _stars = new List<Star>();
        private readonly List<ShootingStar> _shootingStars = new List<ShootingStar>();
        private float _nebulaOffset;
        private float _nextShootingStarTime;
        private readonly Random _random = new Random();
        public void Initialize()
        {
            var random = new Random();
            
            // Create stars with parallax effect
            for (int i = 0; i < 100; i++)
            {
                _stars.Add(new Star
                {
                    X = random.Next(0, GameConstants.ScreenWidth),
                    Y = random.Next(0, GameConstants.ScreenHeight),
                    Size = random.Next(1, 3),
                    Brightness = 0.3f + (float)random.NextDouble() * 0.7f,
                    TwinkleSpeed = 0.5f + (float)random.NextDouble() * 2f,
                    TwinkleOffset = (float)random.NextDouble() * MathF.PI * 2
                });
            }
            
            // Schedule first shooting star
            _nextShootingStarTime = 5f + (float)random.NextDouble() * 10f;
        }

        public void Update(float deltaTime)
        {
            _nebulaOffset += deltaTime * 5f;
            
            // Update shooting stars
            for (int i = _shootingStars.Count - 1; i >= 0; i--)
            {
                var star = _shootingStars[i];
                star.X += star.VelocityX * deltaTime;
                star.Y += star.VelocityY * deltaTime;
                star.Life -= deltaTime;
                
                if (star.Life <= 0)
                {
                    _shootingStars.RemoveAt(i);
                }
            }
            
            // Spawn new shooting star occasionally
            _nextShootingStarTime -= deltaTime;
            if (_nextShootingStarTime <= 0)
            {
                SpawnShootingStar();
                _nextShootingStarTime = 8f + (float)_random.NextDouble() * 15f; // Every 8-23 seconds
            }
        }
        
        private void SpawnShootingStar()
        {
            // Spawn from top or right side
            bool fromTop = _random.Next(2) == 0;
            
            _shootingStars.Add(new ShootingStar
            {
                X = fromTop ? _random.Next(GameConstants.ScreenWidth) : GameConstants.ScreenWidth,
                Y = fromTop ? 0 : _random.Next(GameConstants.ScreenHeight / 2),
                VelocityX = -200 - _random.Next(150),
                VelocityY = 100 + _random.Next(100),
                Life = 1.5f,
                Length = 20 + _random.Next(20)
            });
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel)
        {
            // Dark space gradient
            int gradientSteps = 20;
            for (int i = 0; i < gradientSteps; i++)
            {
                float t = i / (float)gradientSteps;
                Color color = Color.Lerp(new Color(5, 5, 20), new Color(10, 5, 30), t);
                int height = GameConstants.ScreenHeight / gradientSteps;
                spriteBatch.Draw(whitePixel, new Rectangle(0, i * height, GameConstants.ScreenWidth, height + 1), color);
            }
            
            // Shooting stars
            foreach (var shootingStar in _shootingStars)
            {
                DrawShootingStar(spriteBatch, whitePixel, shootingStar);
            }
            
            // Stars with twinkle effect
            float time = _nebulaOffset;
            foreach (var star in _stars)
            {
                float twinkle = MathF.Sin(time * star.TwinkleSpeed + star.TwinkleOffset) * 0.5f + 0.5f;
                float alpha = star.Brightness * (0.5f + twinkle * 0.5f);
                
                if (star.Size == 1)
                {
                    spriteBatch.Draw(whitePixel, new Rectangle(star.X, star.Y, 1, 1), Color.White * alpha);
                }
                else if (star.Size == 2)
                {
                    // Cross shape for larger stars
                    spriteBatch.Draw(whitePixel, new Rectangle(star.X, star.Y, 2, 2), Color.White * alpha);
                    spriteBatch.Draw(whitePixel, new Rectangle(star.X - 1, star.Y, 1, 2), Color.White * alpha * 0.7f);
                    spriteBatch.Draw(whitePixel, new Rectangle(star.X + 2, star.Y, 1, 2), Color.White * alpha * 0.7f);
                    spriteBatch.Draw(whitePixel, new Rectangle(star.X, star.Y - 1, 2, 1), Color.White * alpha * 0.7f);
                    spriteBatch.Draw(whitePixel, new Rectangle(star.X, star.Y + 2, 2, 1), Color.White * alpha * 0.7f);
                }
            }
        }

        private void DrawShootingStar(SpriteBatch spriteBatch, Texture2D whitePixel, ShootingStar star)
        {
            float alpha = Math.Min(1f, star.Life * 2f); // Fade in/out
            
            // Draw trail
            for (int i = 0; i < star.Length; i += 2)
            {
                float t = i / (float)star.Length;
                float trailAlpha = alpha * (1f - t);
                int trailX = (int)(star.X + star.VelocityX * t * 0.01f);
                int trailY = (int)(star.Y + star.VelocityY * t * 0.01f);
                
                spriteBatch.Draw(whitePixel, new Rectangle(trailX, trailY, 2, 2), Color.White * trailAlpha);
            }
            
            // Draw head (brighter)
            spriteBatch.Draw(whitePixel, new Rectangle((int)star.X, (int)star.Y, 3, 3), Color.White * alpha);
        }
    }
    
    public class ShootingStar
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Life { get; set; }
        public int Length { get; set; }
    }
}
