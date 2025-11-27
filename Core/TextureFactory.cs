using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlleywayMonoGame.Core
{
    /// <summary>
    /// Helper class for creating procedural textures.
    /// </summary>
    public static class TextureFactory
    {
        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int diameter, Color color)
        {
            var tex = new Texture2D(graphicsDevice, diameter, diameter);
            var data = new Color[diameter * diameter];
            float radius = diameter / 2f;
            float center = radius - 0.5f;
            float radiusSq = radius * radius;
            
            Vector2 lightPos = new Vector2(center - radius * 0.3f, center - radius * 0.3f);

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distSq = dx * dx + dy * dy;
                    int i = y * diameter + x;
                    
                    if (distSq <= radiusSq)
                    {
                        float lightDx = x - lightPos.X;
                        float lightDy = y - lightPos.Y;
                        float lightDist = (float)System.Math.Sqrt(lightDx * lightDx + lightDy * lightDy);
                        
                        float edgeDist = (float)System.Math.Sqrt(distSq);
                        float normalizedDist = edgeDist / radius;
                        
                        float brightness = 1.0f - (lightDist / (radius * 1.5f));
                        brightness = System.Math.Max(0.3f, System.Math.Min(1.2f, brightness));
                        
                        float edgeFade = 1.0f - (normalizedDist * normalizedDist * 0.4f);
                        brightness *= edgeFade;
                        
                        Color shadedColor = new Color(
                            (int)(220 * brightness),
                            (int)(220 * brightness),
                            (int)(230 * brightness),
                            255
                        );
                        
                        data[i] = shadedColor;
                    }
                    else
                    {
                        data[i] = Color.Transparent;
                    }
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateRoundedRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, int radius, Color color)
        {
            var tex = new Texture2D(graphicsDevice, width, height);
            var data = new Color[width * height];

            int r = System.Math.Max(0, radius);
            int rSq = r * r;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = false;

                    if (x >= r && x < width - r && y >= 0 && y < height) inside = true;
                    if (y >= r && y < height - r && x >= 0 && x < width) inside = true;

                    if (!inside)
                    {
                        int cx = 0, cy = 0;
                        if (x < r && y < r) { cx = r - 1; cy = r - 1; }
                        else if (x >= width - r && y < r) { cx = width - r; cy = r - 1; }
                        else if (x < r && y >= height - r) { cx = r - 1; cy = height - r; }
                        else if (x >= width - r && y >= height - r) { cx = width - r; cy = height - r; }

                        int dx = x - cx;
                        int dy = y - cy;
                        if (dx * dx + dy * dy <= rSq) inside = true;
                    }

                    data[y * width + x] = inside ? color : Color.Transparent;
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateHeartTexture(GraphicsDevice graphicsDevice, int size, Color color)
        {
            var tex = new Texture2D(graphicsDevice, size, size);
            var data = new Color[size * size];
            
            float centerX = size / 2f;
            float centerY = size / 2.5f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - centerX) / (size * 0.35f);
                    float ny = -(y - centerY) / (size * 0.35f);
                    
                    float x2 = nx * nx;
                    float y2 = ny * ny;
                    
                    float left = (x2 + y2 - 1.0f);
                    left = left * left * left;
                    float right = x2 * ny * ny * ny;
                    
                    bool isHeart = (left - right) <= 0.0f;
                    
                    data[y * size + x] = isHeart ? color : Color.Transparent;
                }
            }
            
            tex.SetData(data);
            return tex;
        }
    }
}
