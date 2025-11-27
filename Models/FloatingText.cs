using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Models
{
    /// <summary>
    /// Represents floating text that displays temporarily on screen with fade-out effect.
    /// </summary>
    public class FloatingText
    {
        public string Text { get; set; } = "";
        public Vector2 Position { get; set; }
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }
        public Color Color { get; set; }

        public float Alpha => Lifetime / MaxLifetime;
    }
}
