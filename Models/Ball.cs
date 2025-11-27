using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Models
{
    /// <summary>
    /// Represents a ball entity in the game with position, velocity, and launch state.
    /// </summary>
    public class Ball
    {
        public Rectangle Rect { get; set; }
        public Vector2 Velocity { get; set; }
        public bool IsLaunched { get; set; }

        public Ball(Rectangle rect, Vector2 velocity, bool isLaunched = false)
        {
            Rect = rect;
            Velocity = velocity;
            IsLaunched = isLaunched;
        }

        public Vector2 Center => new Vector2(Rect.Center.X, Rect.Center.Y);
    }
}
