namespace AlleywayMonoGame.Models
{
    /// <summary>
    /// Represents a star in the background.
    /// </summary>
    public class Star
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Size { get; set; }
        public float Brightness { get; set; }
        public float TwinkleSpeed { get; set; }
        public float TwinkleOffset { get; set; }
    }
}
