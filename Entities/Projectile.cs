using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Entities
{
    /// <summary>
    /// Represents a projectile fired by the paddle in shoot mode.
    /// </summary>
    public class Projectile
    {
        public Rectangle Bounds { get; private set; }
        public int Speed { get; }

        public Projectile(int x, int y, int width, int height, int speed)
        {
            Bounds = new Rectangle(x, y, width, height);
            Speed = speed;
        }

        public void Update(float deltaTime)
        {
            Bounds = new Rectangle(
                Bounds.X,
                Bounds.Y - (int)(Speed * deltaTime),
                Bounds.Width,
                Bounds.Height
            );
        }

        public bool IsOffScreen(int topBoundary)
        {
            return Bounds.Y + Bounds.Height < topBoundary;
        }

        public Vector2 Center => new Vector2(Bounds.Center.X, Bounds.Center.Y);
    }
}
