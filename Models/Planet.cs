using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Models
{
    /// <summary>
    /// Represents a planet in the background.
    /// </summary>
    public class Planet
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }
        public Color Color { get; set; }
        public bool HasRing { get; set; }
    }
}
