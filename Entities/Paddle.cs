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
        private readonly int _originalBaseWidth; // Store original width
        private int _baseWidth;
        private int _targetWidth;
        private int _currentAnimWidth;
        
        public bool IsEnlarged { get; set; }
        public float PermanentSizeMultiplier { get; private set; } = 1.0f;
        public bool IsAnimating { get; private set; }
        public float AnimationProgress { get; private set; }

        public Paddle(int x, int y, int width, int height, int baseSpeed, int screenWidth)
        {
            Bounds = new Rectangle(x, y, width, height);
            _baseSpeed = baseSpeed;
            _screenWidth = screenWidth;
            _baseWidth = width;
            _originalBaseWidth = width; // Store original
            _currentAnimWidth = width;
            _targetWidth = width;
            Velocity = Vector2.Zero;
            IsEnlarged = false;
            IsAnimating = false;
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
                _targetWidth = _baseWidth * 2;
                IsEnlarged = true;
                StartAnimation();
            }
        }

        public void Shrink()
        {
            if (IsEnlarged)
            {
                _targetWidth = _baseWidth;
                IsEnlarged = false;
                StartAnimation();
            }
        }

        private void StartAnimation()
        {
            IsAnimating = true;
            AnimationProgress = 0f;
            _currentAnimWidth = Bounds.Width;
        }

        public void UpdateAnimation(float deltaTime)
        {
            if (!IsAnimating) return;

            AnimationProgress += deltaTime * 8f; // Fast animation (0.125 seconds)

            if (AnimationProgress >= 1f)
            {
                AnimationProgress = 1f;
                IsAnimating = false;
            }

            // Smooth easing (ease-out)
            float t = 1f - (float)System.Math.Pow(1f - AnimationProgress, 3f);
            int newWidth = (int)(_currentAnimWidth + (_targetWidth - _currentAnimWidth) * t);

            int centerX = Bounds.Center.X;
            Bounds = new Rectangle(centerX - newWidth / 2, Bounds.Y, newWidth, Bounds.Height);
        }

        public void ApplyPermanentSizeIncrease(float multiplier)
        {
            PermanentSizeMultiplier = multiplier;
            int centerX = Bounds.Center.X;
            
            // Calculate new base width from original width
            _baseWidth = (int)(_originalBaseWidth * PermanentSizeMultiplier);
            
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
