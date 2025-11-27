using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Entities
{
    /// <summary>
    /// Represents a destructible brick in the game.
    /// </summary>
    public class Brick
    {
        public Rectangle Bounds { get; }
        public BrickType Type { get; set; }

        public Brick(Rectangle bounds, BrickType type = BrickType.Normal)
        {
            Bounds = bounds;
            Type = type;
        }

        public Vector2 Center => new Vector2(Bounds.Center.X, Bounds.Center.Y);
        
        public static Color GetColorForRow(int row)
        {
            return (row % 5) switch
            {
                0 => Color.Red,
                1 => Color.Orange,
                2 => Color.Yellow,
                3 => Color.Green,
                _ => Color.Blue
            };
        }
    }

    public enum BrickType
    {
        Normal,
        Special
    }
}
