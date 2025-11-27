using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Models
{
    /// <summary>
    /// Represents a visual particle effect with position, velocity, lifetime, and color.
    /// </summary>
    public class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; }

        public float Alpha => Lifetime / MaxLifetime;
    }
}
