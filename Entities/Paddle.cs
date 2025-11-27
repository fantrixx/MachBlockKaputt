using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Entities
{
    /// <summary>
    /// Represents the player-controlled paddle entity.
    /// </summary>
    public class Paddle
    {
        public Rectangle Bounds { get; set; }
        public Vector2 Velocity { get; set; }
        public float SpeedMultiplier { get; set; } = 1.0f;
        
        private readonly int _baseSpeed;
        private readonly int _screenWidth;

        public Paddle(int x, int y, int width, int height, int baseSpeed, int screenWidth)
        {
            Bounds = new Rectangle(x, y, width, height);
            _baseSpeed = baseSpeed;
            _screenWidth = screenWidth;
            Velocity = Vector2.Zero;
        }

        public int X
        {
            get => Bounds.X;
            set => Bounds = new Rectangle(value, Bounds.Y, Bounds.Width, Bounds.Height);
        }

        public int Y => Bounds.Y;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public Point Center => Bounds.Center;

        public void MoveLeft(float deltaTime)
        {
            Velocity = new Vector2(-_baseSpeed * SpeedMultiplier, 0);
            ApplyMovement(deltaTime);
        }

        public void MoveRight(float deltaTime)
        {
            Velocity = new Vector2(_baseSpeed * SpeedMultiplier, 0);
            ApplyMovement(deltaTime);
        }

        public void Stop()
        {
            Velocity = Vector2.Zero;
        }

        private void ApplyMovement(float deltaTime)
        {
            int newX = Bounds.X + (int)(Velocity.X * deltaTime);
            newX = Math.Max(0, Math.Min(newX, _screenWidth - Bounds.Width));
            X = newX;
        }
    }
}
