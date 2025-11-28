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
        public int SteelHitsRemaining { get; set; } // For steel bricks (5 hits to destroy)

        public Brick(Rectangle bounds, BrickType type = BrickType.Normal)
        {
            Bounds = bounds;
            Type = type;
            SteelHitsRemaining = 0;
        }

        public Vector2 Center => new Vector2(Bounds.Center.X, Bounds.Center.Y);
        
        public bool IsSteel => Type == BrickType.Steel;
        
        public void ConvertToSteel()
        {
            Type = BrickType.Steel;
            SteelHitsRemaining = 5;
        }
        
        public bool HitSteel()
        {
            if (!IsSteel) return false;
            
            SteelHitsRemaining--;
            return SteelHitsRemaining <= 0; // Returns true when destroyed
        }
        
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
        Special,
        Steel
    }
}
