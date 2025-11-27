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
        private int _baseWidth;
        public bool IsEnlarged { get; set; }
        public float PermanentSizeMultiplier { get; private set; } = 1.0f;

        public Paddle(int x, int y, int width, int height, int baseSpeed, int screenWidth)
        {
            Bounds = new Rectangle(x, y, width, height);
            _baseSpeed = baseSpeed;
            _screenWidth = screenWidth;
            _baseWidth = width;
            Velocity = Vector2.Zero;
            IsEnlarged = false;
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

        public void Enlarge()
        {
            if (!IsEnlarged)
            {
                int centerX = Bounds.Center.X;
                int newWidth = _baseWidth * 2;
                Bounds = new Rectangle(centerX - newWidth / 2, Bounds.Y, newWidth, Bounds.Height);
                IsEnlarged = true;
            }
        }

        public void Shrink()
        {
            if (IsEnlarged)
            {
                int centerX = Bounds.Center.X;
                Bounds = new Rectangle(centerX - _baseWidth / 2, Bounds.Y, _baseWidth, Bounds.Height);
                IsEnlarged = false;
            }
        }

        public void ApplyPermanentSizeIncrease(float multiplier)
        {
            PermanentSizeMultiplier = multiplier;
            int centerX = Bounds.Center.X;
            int newBaseWidth = (int)(_baseWidth / (PermanentSizeMultiplier - 0.04f) * PermanentSizeMultiplier);
            
            // Update both base width and current bounds
            _baseWidth = newBaseWidth;
            
            if (IsEnlarged)
            {
                // If enlarged, keep it at 2x the new base width
                Bounds = new Rectangle(centerX - (_baseWidth * 2) / 2, Bounds.Y, _baseWidth * 2, Bounds.Height);
            }
            else
            {
                // Normal size
                Bounds = new Rectangle(centerX - _baseWidth / 2, Bounds.Y, _baseWidth, Bounds.Height);
            }
        }
    }
}
