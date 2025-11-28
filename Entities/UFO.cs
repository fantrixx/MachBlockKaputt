using Microsoft.Xna.Framework;
using System;

namespace AlleywayMonoGame.Entities
{
    /// <summary>
    /// Mysterious UFO that flies across the screen
    /// </summary>
    public class UFO
    {
        public Rectangle Bounds { get; set; }
        public Vector2 Velocity { get; set; }
        public bool IsActive { get; set; }
        public bool IsExiting { get; set; }
        public float TimeAlive { get; set; }
        
        private readonly int _screenWidth;
        private readonly int _minY = 100; // Stay above bricks
        private readonly int _maxY = 200;

        public UFO(int screenWidth)
        {
            _screenWidth = screenWidth;
            IsActive = false;
            IsExiting = false;
            TimeAlive = 0f;
        }

        public Point Center => Bounds.Center;

        public void Spawn(Random random)
        {
            // Random Y position (above bricks area)
            int y = random.Next(_minY, _maxY);
            
            // Random direction (left to right or right to left)
            bool fromLeft = random.Next(2) == 0;
            
            int width = 60;
            int height = 30;
            float speed = random.Next(120, 181); // 120-180 pixels/second (increased)
            
            if (fromLeft)
            {
                // Start from left, move right
                Bounds = new Rectangle(-width, y, width, height);
                Velocity = new Vector2(speed, 0);
            }
            else
            {
                // Start from right, move left
                Bounds = new Rectangle(_screenWidth, y, width, height);
                Velocity = new Vector2(-speed, 0);
            }
            
            IsActive = true;
            IsExiting = false;
            TimeAlive = 0f;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            TimeAlive += deltaTime;
            
            // Move UFO
            Bounds = new Rectangle(
                Bounds.X + (int)(Velocity.X * deltaTime),
                Bounds.Y,
                Bounds.Width,
                Bounds.Height
            );

            // Check if reached center or beyond
            bool movingRight = Velocity.X > 0;
            bool reachedCenter = movingRight 
                ? Bounds.X > _screenWidth / 2 
                : Bounds.Right < _screenWidth / 2;

            // Start exiting once past center
            if (reachedCenter && !IsExiting)
            {
                IsExiting = true;
            }

            // Check if off screen
            if (movingRight && Bounds.X > _screenWidth + 100)
            {
                IsActive = false;
            }
            else if (!movingRight && Bounds.Right < -100)
            {
                IsActive = false;
            }
        }

        public void Destroy()
        {
            IsActive = false;
        }
    }
}
