using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AlleywayMonoGame.Services;

namespace AlleywayMonoGame.UI
{
    /// <summary>
    /// Renders pixel-art style icons for shop items
    /// </summary>
    public static class ShopIconRenderer
    {
        private const int IconSize = 16;
        
        /// <summary>
        /// Draws a pixel-art icon for a shop item
        /// </summary>
        public static void DrawIcon(SpriteBatch spriteBatch, Texture2D whitePixel, ShopItem item, Vector2 position, Color color)
        {
            switch (item)
            {
                case ShopItem.SpeedUpgrade:
                    DrawSpeedIcon(spriteBatch, whitePixel, position, color);
                    break;
                case ShopItem.ExtraBall:
                    DrawBallIcon(spriteBatch, whitePixel, position, color);
                    break;
                case ShopItem.ShootMode:
                    DrawShootIcon(spriteBatch, whitePixel, position, color);
                    break;
                case ShopItem.PaddleSize:
                    DrawPaddleSizeIcon(spriteBatch, whitePixel, position, color);
                    break;
            }
        }
        
        /// <summary>
        /// Speed upgrade icon - Arrow pointing right with motion lines
        /// </summary>
        private static void DrawSpeedIcon(SpriteBatch spriteBatch, Texture2D whitePixel, Vector2 pos, Color color)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            
            // Arrow shaft
            DrawPixel(spriteBatch, whitePixel, x + 2, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 3, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 4, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 8, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 9, y + 7, color);
            
            // Arrow head
            DrawPixel(spriteBatch, whitePixel, x + 10, y + 6, color);
            DrawPixel(spriteBatch, whitePixel, x + 11, y + 5, color);
            DrawPixel(spriteBatch, whitePixel, x + 12, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 13, y + 5, color);
            DrawPixel(spriteBatch, whitePixel, x + 14, y + 6, color);
            DrawPixel(spriteBatch, whitePixel, x + 13, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 14, y + 8, color);
            DrawPixel(spriteBatch, whitePixel, x + 13, y + 9, color);
            DrawPixel(spriteBatch, whitePixel, x + 12, y + 10, color);
            DrawPixel(spriteBatch, whitePixel, x + 11, y + 9, color);
            DrawPixel(spriteBatch, whitePixel, x + 10, y + 8, color);
            
            // Motion lines (dimmed)
            Color dimmed = color * 0.5f;
            DrawPixel(spriteBatch, whitePixel, x + 0, y + 5, dimmed);
            DrawPixel(spriteBatch, whitePixel, x + 0, y + 9, dimmed);
        }
        
        /// <summary>
        /// Extra ball icon - Circular ball with shine
        /// </summary>
        private static void DrawBallIcon(SpriteBatch spriteBatch, Texture2D whitePixel, Vector2 pos, Color color)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            
            // Circle outline (8x8 centered)
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 3, color);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 3, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 3, color);
            
            DrawPixel(spriteBatch, whitePixel, x + 4, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 8, y + 4, color);
            
            DrawPixel(spriteBatch, whitePixel, x + 3, y + 5, color);
            DrawPixel(spriteBatch, whitePixel, x + 9, y + 5, color);
            
            DrawPixel(spriteBatch, whitePixel, x + 3, y + 6, color);
            DrawPixel(spriteBatch, whitePixel, x + 9, y + 6, color);
            
            DrawPixel(spriteBatch, whitePixel, x + 3, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 9, y + 7, color);
            
            DrawPixel(spriteBatch, whitePixel, x + 3, y + 8, color);
            DrawPixel(spriteBatch, whitePixel, x + 9, y + 8, color);
            
            DrawPixel(spriteBatch, whitePixel, x + 4, y + 9, color);
            DrawPixel(spriteBatch, whitePixel, x + 8, y + 9, color);
            
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 10, color);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 10, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 10, color);
            
            // Fill
            for (int fy = 5; fy <= 8; fy++)
            {
                for (int fx = 4; fx <= 8; fx++)
                {
                    DrawPixel(spriteBatch, whitePixel, x + fx, y + fy, color);
                }
            }
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 9, color);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 9, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 9, color);
            
            // Shine highlight
            Color bright = Color.White * 0.8f;
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 5, bright);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 5, bright);
        }
        
        /// <summary>
        /// Shoot mode icon - Upward arrow/projectile
        /// </summary>
        private static void DrawShootIcon(SpriteBatch spriteBatch, Texture2D whitePixel, Vector2 pos, Color color)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            
            // Arrow head
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 2, color);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 3, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 3, color);
            DrawPixel(spriteBatch, whitePixel, x + 8, y + 3, color);
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 8, y + 4, color);
            DrawPixel(spriteBatch, whitePixel, x + 9, y + 4, color);
            
            // Shaft
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 5, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 6, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 7, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 8, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 9, color);
            DrawPixel(spriteBatch, whitePixel, x + 7, y + 10, color);
            
            // Fins
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 10, color);
            DrawPixel(spriteBatch, whitePixel, x + 6, y + 10, color);
            DrawPixel(spriteBatch, whitePixel, x + 8, y + 10, color);
            DrawPixel(spriteBatch, whitePixel, x + 9, y + 10, color);
            DrawPixel(spriteBatch, whitePixel, x + 5, y + 11, color);
            DrawPixel(spriteBatch, whitePixel, x + 9, y + 11, color);
        }
        
        /// <summary>
        /// Paddle size icon - Wide horizontal bar
        /// </summary>
        private static void DrawPaddleSizeIcon(SpriteBatch spriteBatch, Texture2D whitePixel, Vector2 pos, Color color)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            
            // Paddle bar (wider)
            for (int px = 2; px <= 12; px++)
            {
                DrawPixel(spriteBatch, whitePixel, x + px, y + 6, color);
                DrawPixel(spriteBatch, whitePixel, x + px, y + 7, color);
                DrawPixel(spriteBatch, whitePixel, x + px, y + 8, color);
            }
            
            // Outline
            for (int px = 2; px <= 12; px++)
            {
                DrawPixel(spriteBatch, whitePixel, x + px, y + 5, color * 0.7f);
                DrawPixel(spriteBatch, whitePixel, x + px, y + 9, color * 0.7f);
            }
            
            // Arrows showing growth
            Color arrowColor = color * 0.8f;
            // Left arrow
            DrawPixel(spriteBatch, whitePixel, x + 0, y + 7, arrowColor);
            DrawPixel(spriteBatch, whitePixel, x + 1, y + 6, arrowColor);
            DrawPixel(spriteBatch, whitePixel, x + 1, y + 8, arrowColor);
            
            // Right arrow
            DrawPixel(spriteBatch, whitePixel, x + 14, y + 7, arrowColor);
            DrawPixel(spriteBatch, whitePixel, x + 13, y + 6, arrowColor);
            DrawPixel(spriteBatch, whitePixel, x + 13, y + 8, arrowColor);
        }
        
        private static void DrawPixel(SpriteBatch spriteBatch, Texture2D whitePixel, int x, int y, Color color)
        {
            spriteBatch.Draw(whitePixel, new Rectangle(x, y, 1, 1), color);
        }
    }
}
