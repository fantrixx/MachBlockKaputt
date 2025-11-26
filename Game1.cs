using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace AlleywayMonoGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = null!;

        private int screenWidth = 800;
        private int screenHeight = 600;
        private int uiHeight = 50; // Höhe des UI-Bereichs oben
        private int gameAreaTop; // Start des Spielfelds (nach UI + Linie)

        private Rectangle paddle;
        private Vector2 paddleVelocity;
        private int paddleSpeed = 400;

        // Ball system - support multiple balls
        private class Ball
        {
            public Rectangle Rect;
            public Vector2 Velocity;
            public bool IsLaunched;
        }
        
        private List<Ball> balls = new List<Ball>();
        private int ballSize = 10;

        private List<Rectangle> bricks = new List<Rectangle>();
        private int level = 1;
        private bool levelCleared = false;
        private float levelClearTimer = 0f;
        private float levelClearAutoStart = 2.0f; // seconds until auto-start next level

        private SpriteFont? font;
        private Texture2D? white;
        private Texture2D? ballTexture;
        private Texture2D? paddleTexture;
        private Texture2D? heartTexture;
        private float ballSpeedIncrease = 1.05f; // multiplier applied on each brick hit
        private float maxBallSpeed = 800f; // cap to prevent runaway speed

        private int score = 0;
        private int lives = 1;
        private int bankBalance = 0; // Money for shop upgrades

        // Special power-up system
        private List<int> specialBricks = new List<int>(); // indices of special bricks (shoot power)
        private List<int> extraBallBricks = new List<int>(); // indices of extra ball bricks
        private bool canShoot = false;
        private float shootPowerTimer = 0f;
        private float shootPowerDuration = 7f; // 7 seconds
        private List<Rectangle> projectiles = new List<Rectangle>();
        private int projectileSpeed = 500;
        private KeyboardState previousKeyState;
        private float flickerTimer = 0f;
        
        // Game over state
        private bool gameOver = false;
        private Rectangle retryButton;
        private Rectangle quitButton;
        private bool retryButtonHovered = false;
        private bool quitButtonHovered = false;
        
        // Level complete state
        private bool levelComplete = false;
        private float animationTimer = 0f;
        private int animatedMoney = 0;
        private bool moneyAnimationDone = false;
        private Rectangle nextLevelButton;
        private bool nextLevelButtonHovered = false;
        private Rectangle[] shopButtons = new Rectangle[3];
        private bool[] shopButtonsHovered = new bool[3];
        private int levelCompleteTimeBonus = 0;
        
        // Money slam animation
        private float slamY = 0f; // Y position for slam animation
        private float slamVelocity = 0f; // Velocity for bounce
        private float slamScale = 1f; // Scale for impact effect
        private bool slamAnimationDone = false;
        private float glowPulse = 0f; // For pulsing glow effect
        
        // Shop upgrades (persistent across levels)
        private float paddleSpeedMultiplier = 1.0f;
        private int extraBallsPurchased = 0; // Count of extra balls to spawn
        private bool startWithShootMode = false;
        private bool specialBricksSetForLevel = false; // Track if special bricks were already set
        
        // Timer system
        private float gameTimer = 0f;
        private bool timerRunning = false;

        // simple particle system for block explosions
        private class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Lifetime;
            public float MaxLifetime;
            public float Size;
            public Color Color;
        }

        private List<Particle> particles = new List<Particle>();
        
        // Floating text animation system
        private class FloatingText
        {
            public string Text = "";
            public Vector2 Position;
            public float Lifetime;
            public float MaxLifetime;
            public Color Color;
        }
        
        private List<FloatingText> floatingTexts = new List<FloatingText>();
        private Random rand = new Random();
        private SoundEffect? explosionSound;
        private SoundEffect? paddleSound;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = screenWidth;
            _graphics.PreferredBackBufferHeight = screenHeight;
        }

        protected override void Initialize()
        {
            gameAreaTop = uiHeight + 3; // UI + Linie (3px dick)
            
            paddle = new Rectangle(screenWidth / 2 - 50, screenHeight - 40, 100, 20);
            paddleVelocity = Vector2.Zero;

            // Initialize with one ball
            balls.Clear();
            balls.Add(new Ball
            {
                Rect = new Rectangle(screenWidth / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize),
                Velocity = new Vector2(150, -150),
                IsLaunched = false
            });

            previousKeyState = Keyboard.GetState();

            InitBricks();

            base.Initialize();
        }

        private void InitBricks()
        {
            SetupLevel(level);
        }

        private void SetupLevel(int lvl)
        {
            bricks.Clear();
            
            // Progressive difficulty: more bricks each level
            int totalLevels = 10;
            int baseRows = 3;
            int baseCols = 6;
            
            // Increase rows and cols with level (capped at level 10)
            int currentLevel = Math.Min(lvl, totalLevels);
            int currentRows = baseRows + (currentLevel - 1);
            int currentCols = baseCols + (currentLevel - 1);
            
            int brickWidth = screenWidth / currentCols;
            int brickHeight = Math.Max(10, 20 - currentLevel); // Smaller bricks at higher levels
            int brickStartY = gameAreaTop + 30;

            int pattern = (lvl - 1) % 4; // choose among 4 formations
            switch (pattern)
            {
                // pattern 0: full grid (classic)
                case 0:
                    for (int r = 0; r < currentRows; r++)
                    {
                        for (int c = 0; c < currentCols; c++)
                        {
                            bricks.Add(new Rectangle(c * brickWidth, brickStartY + r * brickHeight, brickWidth - 2, brickHeight - 2));
                        }
                    }
                    break;

                // pattern 1: pyramid
                case 1:
                    for (int r = 0; r < currentRows; r++)
                    {
                        int cols = Math.Max(1, currentCols - r * 2);
                        int startCol = r;
                        for (int c = 0; c < cols; c++)
                        {
                            var rect = new Rectangle((startCol + c) * brickWidth + 2, brickStartY + r * (brickHeight + 2), brickWidth - 4, brickHeight);
                            bricks.Add(rect);
                        }
                    }
                    break;

                // pattern 2: checkerboard
                case 2:
                    for (int r = 0; r < currentRows; r++)
                    {
                        for (int c = 0; c < currentCols; c++)
                        {
                            if ((r + c) % 2 == 0)
                            {
                                var rect = new Rectangle(c * brickWidth + 2, brickStartY + r * (brickHeight + 2), brickWidth - 4, brickHeight);
                                bricks.Add(rect);
                            }
                        }
                    }
                    break;

                // pattern 3: gaps (every third column empty)
                default:
                    for (int r = 0; r < currentRows; r++)
                    {
                        for (int c = 0; c < currentCols; c++)
                        {
                            if (c % 3 != 0)
                            {
                                var rect = new Rectangle(c * brickWidth + 2, brickStartY + r * (brickHeight + 2), brickWidth - 4, brickHeight);
                                bricks.Add(rect);
                            }
                        }
                    }
                    break;
            }
            
            // Only set special bricks once per level (not when shoot mode ends)
            if (!specialBricksSetForLevel)
            {
                specialBricks.Clear();
                extraBallBricks.Clear();
                
                int shootCount = Math.Max(1, (bricks.Count * 40) / 100); // 40% shoot power
                int extraBallCount = Math.Max(1, (bricks.Count * 40) / 100); // 40% extra ball
                
                // Add shoot power bricks (try multiple times to ensure we get enough)
                int attempts = 0;
                while (specialBricks.Count < shootCount && attempts < shootCount * 3)
                {
                    int randomIndex = rand.Next(bricks.Count);
                    if (!specialBricks.Contains(randomIndex) && !extraBallBricks.Contains(randomIndex))
                    {
                        specialBricks.Add(randomIndex);
                    }
                    attempts++;
                }
                
                // Add extra ball bricks (try multiple times to ensure we get enough)
                attempts = 0;
                while (extraBallBricks.Count < extraBallCount && attempts < extraBallCount * 3)
                {
                    int randomIndex = rand.Next(bricks.Count);
                    if (!specialBricks.Contains(randomIndex) && !extraBallBricks.Contains(randomIndex))
                    {
                        extraBallBricks.Add(randomIndex);
                    }
                    attempts++;
                }
                
                specialBricksSetForLevel = true;
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            white = new Texture2D(GraphicsDevice, 1, 1);
            white.SetData(new[] { Color.White });

            // create a circular texture for the ball
            ballTexture = CreateCircleTexture(GraphicsDevice, ballSize, Color.White);

            // create rounded paddle texture based on current paddle size
            if (paddle.Width > 0 && paddle.Height > 0)
            {
                paddleTexture = CreateRoundedRectangleTexture(GraphicsDevice, paddle.Width, paddle.Height, 8, Color.White);
            }

            // create heart texture for lives display
            heartTexture = CreateHeartTexture(GraphicsDevice, 20, Color.White);

            // create a short explosion/tap sound at runtime (no external file required)
            try
            {
                explosionSound = CreateExplosionSoundEffect(440, 0.12f, 0.6f);
                // create a different short click/tap sound for paddle hits
                paddleSound = CreateExplosionSoundEffect(1000, 0.06f, 0.85f);
            }
            catch
            {
                explosionSound = null;
            }

            // load a default SpriteFont fallback
            try
            {
                font = Content.Load<SpriteFont>("DefaultFont");
                System.Diagnostics.Debug.WriteLine($"Font loaded successfully: {font != null}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Font load error: {ex.Message}");
                font = null;
            }
            
            // Ensure font is loaded, otherwise try alternative path
            if (font == null)
            {
                try
                {
                    font = Content.Load<SpriteFont>("Content/DefaultFont");
                    System.Diagnostics.Debug.WriteLine("Font loaded from Content/DefaultFont");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Font still null - no text will be displayed");
                }
            }
        }

        private Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int diameter, Color color)
        {
            var tex = new Texture2D(graphicsDevice, diameter, diameter);
            var data = new Color[diameter * diameter];
            float radius = diameter / 2f;
            float center = radius - 0.5f;
            float radiusSq = radius * radius;
            
            // Light source position (top-left for 3D effect)
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
                        // Calculate distance from light source for 3D shading
                        float lightDx = x - lightPos.X;
                        float lightDy = y - lightPos.Y;
                        float lightDist = (float)Math.Sqrt(lightDx * lightDx + lightDy * lightDy);
                        
                        // Normalize distance to edge
                        float edgeDist = (float)Math.Sqrt(distSq);
                        float normalizedDist = edgeDist / radius;
                        
                        // Create 3D lighting effect
                        float brightness = 1.0f - (lightDist / (radius * 1.5f));
                        brightness = Math.Max(0.3f, Math.Min(1.2f, brightness));
                        
                        // Darken edges for sphere effect
                        float edgeFade = 1.0f - (normalizedDist * normalizedDist * 0.4f);
                        brightness *= edgeFade;
                        
                        // Apply brightness to color (silver/white with shading)
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

        private Texture2D CreateRoundedRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, int radius, Color color)
        {
            var tex = new Texture2D(graphicsDevice, width, height);
            var data = new Color[width * height];

            // corner centers
            int r = Math.Max(0, radius);
            int rSq = r * r;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = false;

                    // center rect
                    if (x >= r && x < width - r && y >= 0 && y < height) inside = true;
                    if (y >= r && y < height - r && x >= 0 && x < width) inside = true;

                    // corners
                    if (!inside)
                    {
                        int cx = 0, cy = 0;
                        if (x < r && y < r) { cx = r - 1; cy = r - 1; } // top-left
                        else if (x >= width - r && y < r) { cx = width - r; cy = r - 1; } // top-right
                        else if (x < r && y >= height - r) { cx = r - 1; cy = height - r; } // bottom-left
                        else if (x >= width - r && y >= height - r) { cx = width - r; cy = height - r; } // bottom-right

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

        private Texture2D CreateHeartTexture(GraphicsDevice graphicsDevice, int size, Color color)
        {
            var tex = new Texture2D(graphicsDevice, size, size);
            var data = new Color[size * size];
            
            float centerX = size / 2f;
            float centerY = size / 2.5f; // Position heart higher
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Normalized coordinates centered around heart
                    float nx = (x - centerX) / (size * 0.35f);
                    float ny = -(y - centerY) / (size * 0.35f); // Flip Y for proper orientation
                    
                    // Better heart shape formula
                    float x2 = nx * nx;
                    float y2 = ny * ny;
                    
                    // Standard heart equation: (x²+y²-1)³ - x²*y³ ≤ 0
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

        private void IncreaseBallSpeed(Ball ball)
        {
            if (ball.Velocity == Vector2.Zero)
                return;

            float speed = ball.Velocity.Length();
            speed = (float)Math.Min(maxBallSpeed, speed * ballSpeedIncrease);
            ball.Velocity = Vector2.Normalize(ball.Velocity) * speed;
        }

        private void DrawDigit(char digit, int x, int y)
        {
            // Simple 7-segment display style digits
            if (white == null) return;
            
            int w = 12; // width
            int h = 20; // height
            int t = 2;  // thickness
            
            // Define which segments to draw for each digit (top, middle, bottom, topleft, topright, bottomleft, bottomright)
            bool[] segments = digit switch
            {
                '0' => new[] { true, false, true, true, true, true, true },
                '1' => new[] { false, false, false, false, true, false, true },
                '2' => new[] { true, true, true, false, true, true, false },
                '3' => new[] { true, true, true, false, true, false, true },
                '4' => new[] { false, true, false, true, true, false, true },
                '5' => new[] { true, true, true, true, false, false, true },
                '6' => new[] { true, true, true, true, false, true, true },
                '7' => new[] { true, false, false, false, true, false, true },
                '8' => new[] { true, true, true, true, true, true, true },
                '9' => new[] { true, true, true, true, true, false, true },
                _ => new[] { false, false, false, false, false, false, false }
            };
            
            Color c = Color.White;
            
            // Draw segments
            if (segments[0]) _spriteBatch.Draw(white, new Rectangle(x, y, w, t), c); // top
            if (segments[1]) _spriteBatch.Draw(white, new Rectangle(x, y + h / 2, w, t), c); // middle
            if (segments[2]) _spriteBatch.Draw(white, new Rectangle(x, y + h - t, w, t), c); // bottom
            if (segments[3]) _spriteBatch.Draw(white, new Rectangle(x, y, t, h / 2), c); // top left
            if (segments[4]) _spriteBatch.Draw(white, new Rectangle(x + w - t, y, t, h / 2), c); // top right
            if (segments[5]) _spriteBatch.Draw(white, new Rectangle(x, y + h / 2, t, h / 2), c); // bottom left
            if (segments[6]) _spriteBatch.Draw(white, new Rectangle(x + w - t, y + h / 2, t, h / 2), c); // bottom right
        }

        private void SpawnExplosion(Vector2 center, int count = 20, Color? baseColor = null)
        {
            Color bCol = baseColor ?? Color.White;
            for (int i = 0; i < count; i++)
            {
                double angle = rand.NextDouble() * Math.PI * 2.0;
                float speed = (float)(50 + rand.NextDouble() * 300);
                // slight random variation around base color
                float vr = (float)(0.8 + rand.NextDouble() * 0.4);
                float vg = (float)(0.8 + rand.NextDouble() * 0.4);
                float vb = (float)(0.8 + rand.NextDouble() * 0.4);
                var col = new Color(
                    Math.Min(1f, bCol.R / 255f * vr),
                    Math.Min(1f, bCol.G / 255f * vg),
                    Math.Min(1f, bCol.B / 255f * vb));

                var p = new Particle()
                {
                    Position = center,
                    Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed),
                    MaxLifetime = (float)(0.4 + rand.NextDouble() * 0.9),
                    Lifetime = (float)(0.4 + rand.NextDouble() * 0.9),
                    Size = (float)(2 + rand.NextDouble() * 6),
                    Color = col
                };
                particles.Add(p);
            }
        }

        private SoundEffect CreateExplosionSoundEffect(int frequency = 440, float durationSeconds = 0.12f, float volume = 0.5f)
        {
            // produce a short sine wave WAV in memory (16-bit PCM, 44100 Hz, mono)
            const int sampleRate = 44100;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            short bitsPerSample = 16;
            short channels = 1;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);

            // RIFF header
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write((int)(36 + samples * channels * bitsPerSample / 8));
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            // fmt chunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write((int)16);
            bw.Write((short)1); // PCM
            bw.Write(channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write(bitsPerSample);
            // data chunk header
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write((int)(samples * channels * bitsPerSample / 8));

            double amplitude = 32760 * volume;
            double t = 0;
            double dt = 1.0 / sampleRate;
            for (int i = 0; i < samples; i++)
            {
                // simple decaying sine tone for a short "pop"-like effect
                double env = Math.Exp(-3.0 * t);
                short sample = (short)(amplitude * env * Math.Sin(2.0 * Math.PI * frequency * t));
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            // SoundEffect.FromStream expects a WAV stream
            return SoundEffect.FromStream(ms);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Handle game over state
            if (gameOver)
            {
                var mouseState = Mouse.GetState();
                Point mousePos = new Point(mouseState.X, mouseState.Y);
                
                // Check button hovers
                retryButtonHovered = retryButton.Contains(mousePos);
                quitButtonHovered = quitButton.Contains(mousePos);
                
                // Check button clicks
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (retryButtonHovered)
                    {
                        // Restart game
                        gameOver = false;
                        lives = 1;
                        score = 0;
                        level = 1;
                        gameTimer = 0f;
                        timerRunning = false;
                        InitBricks();
                        balls.Clear();
                        balls.Add(new Ball
                        {
                            Rect = new Rectangle(screenWidth / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize),
                            Velocity = new Vector2(150, -150),
                            IsLaunched = false
                        });
                        paddle = new Rectangle(screenWidth / 2 - 50, screenHeight - 40, 100, 20);
                        canShoot = false;
                        projectiles.Clear();
                        particles.Clear();
                    }
                    else if (quitButtonHovered)
                    {
                        Exit();
                    }
                }
                
                return; // Skip normal game updates
            }
            
            // Handle level complete state with shop
            if (levelComplete)
            {
                animationTimer += dt;
                
                // Animate money counting
                if (!moneyAnimationDone && animationTimer > 1f)
                {
                    float animSpeed = dt * 200f; // Count up money (fast)
                    animatedMoney += (int)animSpeed;
                    if (animatedMoney >= levelCompleteTimeBonus)
                    {
                        animatedMoney = levelCompleteTimeBonus;
                        moneyAnimationDone = true;
                        bankBalance += levelCompleteTimeBonus;
                        
                        // Start slam animation
                        slamY = -100f; // Start above screen
                        slamVelocity = 0f;
                        slamScale = 1f;
                        slamAnimationDone = false;
                    }
                }
                
                // Slam animation for final balance
                if (moneyAnimationDone && !slamAnimationDone)
                {
                    float gravity = 1200f * dt; // Slower gravity
                    slamVelocity += gravity;
                    slamY += slamVelocity;
                    
                    float targetY = 165f; // Final position
                    if (slamY >= targetY)
                    {
                        slamY = targetY;
                        slamVelocity *= -0.4f; // Bounce with damping
                        slamScale = 1.2f; // Impact effect
                        
                        // Create dust cloud on first impact
                        if (Math.Abs(slamVelocity) > 100f)
                        {
                            Vector2 impactPos = new Vector2(screenWidth / 2, 140 + 25 + targetY + 20);
                            for (int i = 0; i < 15; i++)
                            {
                                float angle = rand.Next(360) * (float)Math.PI / 180f;
                                float speed = 50f + rand.Next(100);
                                particles.Add(new Particle
                                {
                                    Position = impactPos,
                                    Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed * 0.3f),
                                    Lifetime = 1.5f + (float)rand.NextDouble() * 1.0f,
                                    MaxLifetime = 1.5f + (float)rand.NextDouble() * 1.0f,
                                    Size = 4f + rand.Next(6),
                                    Color = Color.Gray
                                });
                            }
                        }
                        
                        // Stop bouncing when velocity is low
                        if (Math.Abs(slamVelocity) < 30f)
                        {
                            slamVelocity = 0f;
                            slamAnimationDone = true;
                        }
                    }
                    
                    // Scale returns to normal
                    if (slamScale > 1f)
                    {
                        slamScale -= dt * 1.5f;
                        if (slamScale < 1f) slamScale = 1f;
                    }
                }
                
                // Pulsing glow effect
                glowPulse += dt * 3f;
                
                var mouseState = Mouse.GetState();
                Point mousePos = new Point(mouseState.X, mouseState.Y);
                
                // Check shop button hovers
                for (int i = 0; i < 3; i++)
                {
                    shopButtonsHovered[i] = shopButtons[i].Contains(mousePos);
                }
                nextLevelButtonHovered = nextLevelButton.Contains(mousePos);
                
                // Handle clicks
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    // Shop button 1: +3% paddle speed for $25
                    if (shopButtonsHovered[0] && bankBalance >= 25 && moneyAnimationDone)
                    {
                        bankBalance -= 25;
                        paddleSpeedMultiplier += 0.03f;
                    }
                    // Shop button 2: Extra ball for $5 (can buy multiple)
                    else if (shopButtonsHovered[1] && bankBalance >= 5 && moneyAnimationDone)
                    {
                        bankBalance -= 5;
                        extraBallsPurchased++;
                    }
                    // Shop button 3: Start with shoot mode for $15
                    else if (shopButtonsHovered[2] && bankBalance >= 15 && moneyAnimationDone)
                    {
                        bankBalance -= 15;
                        startWithShootMode = true;
                    }
                    // Next level button
                    else if (nextLevelButtonHovered && moneyAnimationDone)
                    {
                        level++;
                        levelComplete = false;
                        gameTimer = 0f;
                        specialBricksSetForLevel = false; // Reset for new level
                        SetupLevel(level);
                        
                        // Reset ball/paddle
                        paddle = new Rectangle(screenWidth / 2 - 50, screenHeight - 40, 100, 20);
                        balls.Clear();
                        balls.Add(new Ball
                        {
                            Rect = new Rectangle(screenWidth / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize),
                            Velocity = new Vector2(150, -150),
                            IsLaunched = false
                        });
                        
                        // Apply shop upgrades
                        // Launch all purchased extra balls immediately
                        for (int i = 0; i < extraBallsPurchased; i++)
                        {
                            float angle = -90f + (i + 1) * 30f; // Spread balls at different angles
                            float radians = angle * (float)Math.PI / 180f;
                            float speed = 200f;
                            balls.Add(new Ball
                            {
                                Rect = new Rectangle(screenWidth / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize),
                                Velocity = new Vector2((float)Math.Cos(radians) * speed, (float)Math.Sin(radians) * speed),
                                IsLaunched = true // Launch immediately
                            });
                        }
                        extraBallsPurchased = 0; // Reset after use
                        
                        if (startWithShootMode)
                        {
                            canShoot = true;
                            shootPowerTimer = 6f;
                            startWithShootMode = false; // One-time use
                        }
                        
                        timerRunning = true; // Start timer immediately
                    }
                }
                
                return; // Skip normal game updates
            }

            // Handle level cleared state: wait for user input or auto-advance
            if (levelCleared)
            {
                levelClearTimer += dt;
                var kbPause = Keyboard.GetState();
                if (kbPause.IsKeyDown(Keys.Space) || levelClearTimer >= levelClearAutoStart)
                {
                    // start next level
                    levelCleared = false;
                    levelClearTimer = 0f;
                    SetupLevel(level);
                    // reset ball/paddle
                    paddle = new Rectangle(screenWidth / 2 - 50, screenHeight - 40, 100, 20);
                    balls.Clear();
                    balls.Add(new Ball
                    {
                        Rect = new Rectangle(screenWidth / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize),
                        Velocity = new Vector2(150, -150),
                        IsLaunched = false
                    });
                }
                return; // skip normal updates while waiting
            }

            var kb = Keyboard.GetState();
            
            // CHEAT: Press P to instantly win level (for testing)
            if (kb.IsKeyDown(Keys.P) && previousKeyState.IsKeyUp(Keys.P))
            {
                bricks.Clear(); // Clear all bricks to trigger level complete
            }
            
            // Update game timer if running
            if (timerRunning)
            {
                gameTimer += dt;
            }
            
            // Update flicker timer for special bricks and text
            flickerTimer += dt * 10f; // faster flicker
            
            // Update shoot power-up timer
            if (canShoot)
            {
                shootPowerTimer -= dt;
                if (shootPowerTimer <= 0)
                {
                    canShoot = false;
                }
            }
            
            // Shooting mechanic (Space key)
            if (canShoot && kb.IsKeyDown(Keys.Space) && previousKeyState.IsKeyUp(Keys.Space))
            {
                // Create projectile from paddle center
                Rectangle projectile = new Rectangle(paddle.X + paddle.Width / 2 - 3, paddle.Y - 15, 6, 15);
                projectiles.Add(projectile);
            }
            previousKeyState = kb;
            
            // Update projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                Rectangle proj = projectiles[i];
                proj.Y -= (int)(projectileSpeed * dt);
                projectiles[i] = proj;
                
                // Create smoke trail behind projectile
                if (rand.Next(100) < 80) // 80% chance per frame
                {
                    Vector2 smokePos = new Vector2(proj.X + proj.Width / 2, proj.Y + proj.Height);
                    particles.Add(new Particle
                    {
                        Position = smokePos,
                        Velocity = new Vector2((rand.Next(-20, 20)), 30f + rand.Next(40)), // Moves down slightly
                        Lifetime = 6.0f, // 6 seconds - very visible
                        MaxLifetime = 6.0f,
                        Size = 8f + rand.Next(6), // Larger particles
                        Color = Color.White // Bright white smoke
                    });
                }
                
                // Remove if off-screen
                if (proj.Y + proj.Height < gameAreaTop)
                {
                    projectiles.RemoveAt(i);
                }
            }

            if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
                paddleVelocity.X = -paddleSpeed * paddleSpeedMultiplier;
            else if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
                paddleVelocity.X = paddleSpeed * paddleSpeedMultiplier;
            else
                paddleVelocity.X = 0;

            paddle.X += (int)(paddleVelocity.X * dt);
            if (paddle.X < 0) paddle.X = 0;
            if (paddle.X + paddle.Width > screenWidth) paddle.X = screenWidth - paddle.Width;

            // Update all balls
            for (int ballIndex = balls.Count - 1; ballIndex >= 0; ballIndex--)
            {
                Ball ball = balls[ballIndex];
                
                if (!ball.IsLaunched)
                {
                    ball.Rect = new Rectangle(paddle.X + paddle.Width / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize);
                    if (kb.IsKeyDown(Keys.Space))
                    {
                        ball.IsLaunched = true;
                        // Start timer when first ball is launched
                        if (!timerRunning)
                        {
                            timerRunning = true;
                        }
                    }
                }
                else
                {
                    ball.Rect = new Rectangle(
                        ball.Rect.X + (int)(ball.Velocity.X * dt),
                        ball.Rect.Y + (int)(ball.Velocity.Y * dt),
                        ball.Rect.Width,
                        ball.Rect.Height
                    );

                    // wall collisions
                    if (ball.Rect.X <= 0) { ball.Rect = new Rectangle(0, ball.Rect.Y, ball.Rect.Width, ball.Rect.Height); ball.Velocity = new Vector2(-ball.Velocity.X, ball.Velocity.Y); }
                    if (ball.Rect.X + ball.Rect.Width >= screenWidth) { ball.Rect = new Rectangle(screenWidth - ball.Rect.Width, ball.Rect.Y, ball.Rect.Width, ball.Rect.Height); ball.Velocity = new Vector2(-ball.Velocity.X, ball.Velocity.Y); }
                    
                    // top collision with UI separator line
                    if (ball.Rect.Y <= gameAreaTop) 
                    { 
                        ball.Rect = new Rectangle(ball.Rect.X, gameAreaTop, ball.Rect.Width, ball.Rect.Height);
                        ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y); 
                    }

                    // paddle collision
                    if (ball.Rect.Intersects(paddle))
                    {
                        ball.Rect = new Rectangle(ball.Rect.X, paddle.Y - ball.Rect.Height - 1, ball.Rect.Width, ball.Rect.Height);
                        ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y);
                        // change X based on where it hits the paddle
                        float hitPos = (ball.Rect.Center.X - paddle.X) / (float)paddle.Width - 0.5f;
                        ball.Velocity = new Vector2(ball.Velocity.X + hitPos * 300, ball.Velocity.Y);
                        // play paddle hit sound (different from brick explosion)
                        try { paddleSound?.Play(); } catch { }
                    }
                    
                    // Ball-to-ball collision
                    for (int otherIndex = ballIndex - 1; otherIndex >= 0; otherIndex--)
                    {
                        Ball otherBall = balls[otherIndex];
                        if (!otherBall.IsLaunched) continue;
                        
                        Vector2 ball1Center = new Vector2(ball.Rect.Center.X, ball.Rect.Center.Y);
                        Vector2 ball2Center = new Vector2(otherBall.Rect.Center.X, otherBall.Rect.Center.Y);
                        float distance = Vector2.Distance(ball1Center, ball2Center);
                        float minDistance = ballSize; // Both balls have same radius
                        
                        if (distance < minDistance && distance > 0)
                        {
                            // Calculate collision normal
                            Vector2 normal = Vector2.Normalize(ball1Center - ball2Center);
                            
                            // Separate balls to avoid overlap
                            float overlap = minDistance - distance;
                            Vector2 separation = normal * (overlap / 2f);
                            ball.Rect = new Rectangle(
                                (int)(ball.Rect.X + separation.X),
                                (int)(ball.Rect.Y + separation.Y),
                                ball.Rect.Width, ball.Rect.Height
                            );
                            otherBall.Rect = new Rectangle(
                                (int)(otherBall.Rect.X - separation.X),
                                (int)(otherBall.Rect.Y - separation.Y),
                                otherBall.Rect.Width, otherBall.Rect.Height
                            );
                            
                            // Calculate relative velocity
                            Vector2 relativeVelocity = ball.Velocity - otherBall.Velocity;
                            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);
                            
                            // Only resolve if balls are moving towards each other
                            if (velocityAlongNormal > 0)
                            {
                                // Apply impulse (elastic collision)
                                Vector2 impulse = normal * velocityAlongNormal;
                                ball.Velocity -= impulse;
                                otherBall.Velocity += impulse;
                            }
                        }
                    }

                    // brick collisions
                    for (int i = bricks.Count - 1; i >= 0; i--)
                    {
                            if (ball.Rect.Intersects(bricks[i]))
                        {
                            var b = bricks[i];
                            var center = new Vector2(b.Center.X, b.Center.Y);
                            // compute brick color based on its row
                            int brickHeight = 20;
                            int row = (b.Y - 50) / (brickHeight + 2);
                            Color brickColor;
                            switch (row % 5)
                            {
                                case 0: brickColor = Color.Red; break;
                                case 1: brickColor = Color.Orange; break;
                                case 2: brickColor = Color.Yellow; break;
                                case 3: brickColor = Color.Green; break;
                                default: brickColor = Color.Blue; break;
                            }

                            // Check if this was a special brick
                            bool wasShootBrick = specialBricks.Contains(i);
                            bool wasExtraBallBrick = extraBallBricks.Contains(i);
                            
                            System.Diagnostics.Debug.WriteLine($"Brick hit: wasExtraBallBrick={wasExtraBallBrick}, canShoot={canShoot}");
                            
                            bricks.RemoveAt(i);
                            
                            // Update special bricks indices (all indices after removed one shift down)
                            for (int j = specialBricks.Count - 1; j >= 0; j--)
                            {
                                if (specialBricks[j] == i)
                                {
                                    specialBricks.RemoveAt(j);
                                }
                                else if (specialBricks[j] > i)
                                {
                                    specialBricks[j]--;
                                }
                            }
                            
                            // Update extra ball bricks indices
                            for (int j = extraBallBricks.Count - 1; j >= 0; j--)
                            {
                                if (extraBallBricks[j] == i)
                                {
                                    extraBallBricks.RemoveAt(j);
                                }
                                else if (extraBallBricks[j] > i)
                                {
                                    extraBallBricks[j]--;
                                }
                            }
                            
                            // Activate shoot power-up if shoot brick AND not already active
                            if (wasShootBrick && !canShoot)
                            {
                                canShoot = true;
                                shootPowerTimer = shootPowerDuration;
                                System.Diagnostics.Debug.WriteLine("SHOOT POWER-UP ACTIVATED!");
                            }
                            
                            // Spawn extra ball if extra ball brick hit (ONLY when shoot mode is NOT active)
                            if (wasExtraBallBrick && !canShoot)
                            {
                                System.Diagnostics.Debug.WriteLine($"Creating floating text at position {center}");
                                
                                // Create extra ball at brick position, falling downward
                                balls.Add(new Ball
                                {
                                    Rect = new Rectangle((int)center.X - ballSize / 2, (int)center.Y, ballSize, ballSize),
                                    Velocity = new Vector2(0, 150f), // Fall straight down initially
                                    IsLaunched = true
                                });
                                
                                // Create floating text at brick position - make it VERY visible
                                floatingTexts.Add(new FloatingText
                                {
                                    Text = "+BALL",
                                    Position = center, // At brick position
                                    Lifetime = 3f, // Longer lifetime
                                    MaxLifetime = 3f,
                                    Color = Color.White
                                });
                                
                                System.Diagnostics.Debug.WriteLine($"EXTRA BALL SPAWNED! FloatingTexts count: {floatingTexts.Count}");
                            }
                            
                            // spawn explosion at brick center with brick color
                            SpawnExplosion(center, 24, brickColor);
                            // play short hit sound
                            try { explosionSound?.Play(); } catch { }
                            score += 100;

                            // simple collision response: invert Y
                            ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y);
                            // slightly increase ball speed to raise difficulty
                            IncreaseBallSpeed(ball);
                            break;
                        }
                    }
                
                    // bottom - lose life if ball goes off screen
                    if (ball.Rect.Y > screenHeight)
                    {
                        // Remove this ball
                        balls.RemoveAt(ballIndex);
                        
                        // If no balls left, lose a life and end special modes
                        if (balls.Count == 0)
                        {
                            // End all special modes when no balls left
                            canShoot = false;
                            shootPowerTimer = 0f;
                            projectiles.Clear();
                            
                            lives--;
                            if (lives <= 0)
                            {
                                // Game over
                                gameOver = true;
                                timerRunning = false;
                                
                                // Setup buttons
                                int buttonWidth = 150;
                                int buttonHeight = 50;
                                int buttonSpacing = 20;
                                retryButton = new Rectangle(screenWidth / 2 - buttonWidth - buttonSpacing / 2, screenHeight / 2 + 60, buttonWidth, buttonHeight);
                                quitButton = new Rectangle(screenWidth / 2 + buttonSpacing / 2, screenHeight / 2 + 60, buttonWidth, buttonHeight);
                            }
                            else
                            {
                                // Create new ball at paddle
                                balls.Add(new Ball
                                {
                                    Rect = new Rectangle(paddle.X + paddle.Width / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize),
                                    Velocity = new Vector2(150, -150),
                                    IsLaunched = false
                                });
                            }
                        }
                        continue; // Skip to next ball, this one was removed
                    }
                }  // end of else (ball launched)
            }  // end of ball loop
                
            // Projectile collisions with bricks
            for (int p = projectiles.Count - 1; p >= 0; p--)
            {
                    Rectangle proj = projectiles[p];
                    bool projectileHit = false;
                    
                    for (int i = bricks.Count - 1; i >= 0; i--)
                    {
                        if (proj.Intersects(bricks[i]))
                        {
                            var b = bricks[i];
                            var center = new Vector2(b.Center.X, b.Center.Y);
                            int brickHeight = 20;
                            int row = (b.Y - 50) / (brickHeight + 2);
                            Color brickColor;
                            switch (row % 5)
                            {
                                case 0: brickColor = Color.Red; break;
                                case 1: brickColor = Color.Orange; break;
                                case 2: brickColor = Color.Yellow; break;
                                case 3: brickColor = Color.Green; break;
                                default: brickColor = Color.Blue; break;
                            }
                            
                            bool wasSpecial = specialBricks.Contains(i);
                            bool wasExtraBall = extraBallBricks.Contains(i);
                            
                            bricks.RemoveAt(i);
                            
                            // Update special brick indices
                            for (int j = specialBricks.Count - 1; j >= 0; j--)
                            {
                                if (specialBricks[j] == i)
                                {
                                    specialBricks.RemoveAt(j);
                                }
                                else if (specialBricks[j] > i)
                                {
                                    specialBricks[j]--;
                                }
                            }
                            
                            // Update extra ball brick indices
                            for (int j = extraBallBricks.Count - 1; j >= 0; j--)
                            {
                                if (extraBallBricks[j] == i)
                                {
                                    extraBallBricks.RemoveAt(j);
                                }
                                else if (extraBallBricks[j] > i)
                                {
                                    extraBallBricks[j]--;
                                }
                            }
                            
                            // Don't activate any power-ups during shoot mode (projectiles destroy special bricks as normal bricks)
                            // Power-ups are disabled when canShoot is true
                            
                            SpawnExplosion(center, 24, brickColor);
                            try { explosionSound?.Play(); } catch { }
                            score += 100;
                            
                            projectileHit = true;
                            break;
                        }
                    }
                    
                    if (projectileHit)
                    {
                        projectiles.RemoveAt(p);
                    }
                }

            // if all bricks cleared -> level finished
            if (bricks.Count == 0 && !levelComplete)
            {
                levelComplete = true;
                levelCleared = false;
                timerRunning = false; // Stop timer when all bricks cleared
                animationTimer = 0f;
                moneyAnimationDone = false;
                animatedMoney = 0;
                
                // Calculate time bonus: 100$ - time in seconds
                levelCompleteTimeBonus = Math.Max(0, 100 - (int)gameTimer);
                
                // Setup shop buttons - aligned with border in Draw
                // yOffset in Draw: 80 (stats) + 60 (money) = 140, then +60 for shop = 200
                // Shop starts at y=200, border at y=195, title at y=205, buttons start at y=235
                int buttonWidth = 220;
                int buttonHeight = 35;
                int shopStartY = 235; // Matches Draw rendering position
                for (int i = 0; i < 3; i++)
                {
                    shopButtons[i] = new Rectangle(screenWidth / 2 - buttonWidth / 2, shopStartY + i * 42, buttonWidth, buttonHeight);
                }
                
                // Next level button - below shop border
                nextLevelButton = new Rectangle(screenWidth / 2 - 100, shopStartY + 3 * 42 + 15, 200, 50);
                
                // stop all balls movement
                foreach (Ball b in balls)
                {
                    b.Velocity = Vector2.Zero;
                    b.IsLaunched = false;
                }
            }

            // update particles
            for (int pi = particles.Count - 1; pi >= 0; pi--)
            {
                    var pr = particles[pi];
                    pr.Lifetime -= dt;
                    if (pr.Lifetime <= 0f)
                    {
                        particles.RemoveAt(pi);
                        continue;
                    }
                    pr.Position += pr.Velocity * dt;
                    // simple damping and gravity
                    pr.Velocity *= 0.99f;
                    pr.Velocity += new Vector2(0, 200f * dt);
                }
            
            // update floating texts
            for (int i = floatingTexts.Count - 1; i >= 0; i--)
            {
                var text = floatingTexts[i];
                text.Lifetime -= dt;
                if (text.Lifetime <= 0f)
                {
                    floatingTexts.RemoveAt(i);
                    continue;
                }
                // Float upward slowly
                text.Position = new Vector2(text.Position.X, text.Position.Y - 30f * dt);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            if (white != null)
            {
                // Draw UI area background (darker blue)
                _spriteBatch.Draw(white, new Rectangle(0, 0, screenWidth, uiHeight), Color.DarkBlue * 0.8f);
                
                // Draw separator line between UI and game area (3px thick, white)
                _spriteBatch.Draw(white, new Rectangle(0, uiHeight, screenWidth, 3), Color.White);
                
                // draw paddle with rounded corners texture (tinted black)
                if (paddleTexture != null)
                {
                    _spriteBatch.Draw(paddleTexture, paddle, Color.Black);
                }
                else
                {
                    _spriteBatch.Draw(white, paddle, Color.Black);
                }

                // draw all balls (use circular texture if available)
                foreach (Ball ball in balls)
                {
                    if (ballTexture != null)
                        _spriteBatch.Draw(ballTexture, ball.Rect, Color.Silver);
                    else
                        _spriteBatch.Draw(white, ball.Rect, Color.Silver);
                }

                // draw bricks
                Color[] colors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue };
                for (int i = 0; i < bricks.Count; i++)
                {
                    // Calculate row based on Y position
                    int row = (bricks[i].Y - 50) / 22; // approximate row calculation
                    Color brickColor = colors[row % colors.Length];
                    
                    // Special bricks are MUCH more visible (only when shoot mode is NOT active)
                    // Both shoot power and extra ball bricks are deactivated during shoot mode
                    bool isSpecialBrick = (specialBricks.Contains(i) || extraBallBricks.Contains(i)) && !canShoot;
                    
                    if (isSpecialBrick)
                    {
                        // Strong flickering between gold and bright white
                        float flickerAlpha = (float)Math.Sin(flickerTimer * 2) * 0.5f + 0.5f; // faster flicker
                        Color goldColor = new Color(255, 215, 0); // Gold
                        Color brightColor = Color.White;
                        Color specialColor = Color.Lerp(goldColor, brightColor, flickerAlpha);
                        
                        // Draw with border effect for extra visibility
                        Rectangle innerRect = new Rectangle(bricks[i].X + 2, bricks[i].Y + 2, bricks[i].Width - 4, bricks[i].Height - 4);
                        _spriteBatch.Draw(white, bricks[i], specialColor); // Outer glow
                        _spriteBatch.Draw(white, innerRect, brickColor * 0.8f); // Inner original color
                    }
                    else
                    {
                        _spriteBatch.Draw(white, bricks[i], brickColor);
                    }
                }
                
                // draw projectiles (yellow laser shots)
                for (int p = 0; p < projectiles.Count; p++)
                {
                    _spriteBatch.Draw(white, projectiles[p], Color.Yellow);
                }

                // draw particles (explosions)
                for (int pi = 0; pi < particles.Count; pi++)
                {
                    var pr = particles[pi];
                    float alpha = Math.Max(0f, pr.Lifetime / pr.MaxLifetime);
                    var col = pr.Color * alpha;
                    var rect = new Rectangle((int)(pr.Position.X - pr.Size / 2f), (int)(pr.Position.Y - pr.Size / 2f), Math.Max(1, (int)pr.Size), Math.Max(1, (int)pr.Size));
                    _spriteBatch.Draw(white, rect, col);
                }
                
                // draw floating texts
                for (int i = 0; i < floatingTexts.Count; i++)
                {
                    var floatingText = floatingTexts[i];
                    if (font != null)
                    {
                        float alpha = Math.Max(0f, floatingText.Lifetime / floatingText.MaxLifetime);
                        Vector2 textSize = font.MeasureString(floatingText.Text);
                        Vector2 textPos = new Vector2(floatingText.Position.X - textSize.X / 2, floatingText.Position.Y);
                        
                        // Draw with thick BLACK outline for maximum contrast
                        for (int offsetX = -4; offsetX <= 4; offsetX++)
                        {
                            for (int offsetY = -4; offsetY <= 4; offsetY++)
                            {
                                if (offsetX != 0 || offsetY != 0)
                                {
                                    _spriteBatch.DrawString(font, floatingText.Text, textPos + new Vector2(offsetX, offsetY), Color.Black * alpha);
                                }
                            }
                        }
                        // Draw main text in bright cyan/white for maximum visibility against any background
                        _spriteBatch.DrawString(font, floatingText.Text, textPos, Color.Cyan * alpha);
                    }
                }

                // UI Score Display (in UI area top right) - always visible
                string scoreStr = $"{score:D8}";
                if (font != null)
                {
                    _spriteBatch.DrawString(font, scoreStr, new Vector2(screenWidth - 120, 15), Color.White);
                    
                    // Bank balance display (left of score)
                    string bankStr = $"${bankBalance}";
                    _spriteBatch.DrawString(font, bankStr, new Vector2(screenWidth - 250, 15), Color.Gold);
                }
                else
                {
                    // Fallback: draw each digit as simple rectangles (always visible)
                    int startX = screenWidth - 170;
                    int startY = 15;
                    for (int i = 0; i < scoreStr.Length; i++)
                    {
                        char digit = scoreStr[i];
                        DrawDigit(digit, startX + i * 18, startY);
                    }
                }
                
                // UI text (if font available) - Lives and Level in UI area
                
                // Draw hearts for lives (cool icons!)
                for (int i = 0; i < lives; i++)
                {
                    if (heartTexture != null)
                    {
                        _spriteBatch.Draw(heartTexture, new Rectangle(10 + i * 28, 13, 24, 24), Color.Red);
                    }
                    else
                    {
                        // Fallback: simple rectangles
                        _spriteBatch.Draw(white, new Rectangle(10 + i * 28, 15, 20, 10), Color.Red);
                    }
                }

                if (font != null)
                {
                    // show level after hearts
                    _spriteBatch.DrawString(font, $"Level: {level}/10", new Vector2(10 + lives * 28 + 10, 15), Color.White);
                    
                    // Draw game timer in center top of UI
                    int totalSeconds = (int)gameTimer;
                    int minutes = totalSeconds / 60;
                    int seconds = totalSeconds % 60;
                    string timerText = $"{minutes:D2}:{seconds:D2}";
                    Vector2 timerSize = font.MeasureString(timerText);
                    _spriteBatch.DrawString(font, timerText, new Vector2((screenWidth - timerSize.X) / 2, 15), Color.White);
                }
            }
            
            // Draw flickering "SPACE to Shoot" text when power-up is active (in game area)
            if (canShoot)
            {
                float textFlicker = (float)Math.Sin(flickerTimer * 1.5f) * 0.5f + 0.5f;
                
                if (font != null)
                {
                    string powerUpText = "SPACE TO SHOOT";
                    Vector2 textSize = font.MeasureString(powerUpText);
                    // Zentriere den Text und stelle sicher, dass er im sichtbaren Bereich ist
                    Vector2 textPos = new Vector2((screenWidth - textSize.X) / 2, screenHeight / 2 - 90);
                    
                    // Transparente Anzeige mit sanftem Flackern
                    float alpha = 0.3f + textFlicker * 0.4f; // Zwischen 30% und 70% Transparenz
                    
                    // Leichter schwarzer Schatten für Lesbarkeit
                    for (int offsetX = -2; offsetX <= 2; offsetX++)
                    {
                        for (int offsetY = -2; offsetY <= 2; offsetY++)
                        {
                            if (offsetX != 0 || offsetY != 0)
                            {
                                _spriteBatch.DrawString(font, powerUpText, textPos + new Vector2(offsetX, offsetY), Color.Black * (alpha * 0.5f));
                            }
                        }
                    }
                    // Haupttext in leuchtendem Gelb mit Transparenz
                    _spriteBatch.DrawString(font, powerUpText, textPos, Color.Yellow * alpha);
                }
            }
            
            // draw level cleared overlay
            if (levelCleared)
            {
                // semi-transparent overlay
                if (white != null)
                {
                    _spriteBatch.Draw(white, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.6f);
                }

                string msg = $"Level {level - 1} cleared! Press Space to start Level {level}";
                if (font != null)
                {
                    var size = font.MeasureString(msg);
                    _spriteBatch.DrawString(font, msg, new Vector2((screenWidth - size.X) / 2, (screenHeight - size.Y) / 2), Color.White);
                }
                else if (white != null)
                {
                    // draw a simple box and text-less prompt (no font available)
                    _spriteBatch.Draw(white, new Rectangle(screenWidth/2 - 150, screenHeight/2 - 30, 300, 60), Color.Gray);
                }
            }
            
            // draw level complete overlay with shop
            if (levelComplete)
            {
                // semi-transparent overlay
                if (white != null)
                {
                    _spriteBatch.Draw(white, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.7f);
                }

                if (font != null)
                {
                    // Title
                    string title = "DONE!";
                    Vector2 titleSize = font.MeasureString(title);
                    Vector2 titlePos = new Vector2((screenWidth - titleSize.X) / 2, 30);
                    
                    // Green glow effect
                    for (int offsetX = -2; offsetX <= 2; offsetX++)
                    {
                        for (int offsetY = -2; offsetY <= 2; offsetY++)
                        {
                            if (offsetX != 0 || offsetY != 0)
                            {
                                _spriteBatch.DrawString(font, title, titlePos + new Vector2(offsetX, offsetY), Color.DarkGreen);
                            }
                        }
                    }
                    _spriteBatch.DrawString(font, title, titlePos, Color.LightGreen);
                    
                    // Stats
                    int yOffset = 80;
                    string livesText = $"Lives: {lives}";
                    string timeText = $"Time: {(int)gameTimer / 60:D2}:{(int)gameTimer % 60:D2}";
                    _spriteBatch.DrawString(font, livesText, new Vector2(screenWidth / 2 - 100, yOffset), Color.White);
                    _spriteBatch.DrawString(font, timeText, new Vector2(screenWidth / 2 - 100, yOffset + 25), Color.White);
                    
                    // Money calculation animation
                    yOffset += 60;
                    string calc1 = $"Time Bonus: $100 - {(int)gameTimer}s = ${levelCompleteTimeBonus}";
                    _spriteBatch.DrawString(font, calc1, new Vector2(screenWidth / 2 - 140, yOffset), Color.Yellow);
                    
                    if (!moneyAnimationDone)
                    {
                        string calc2 = $"Counting... ${animatedMoney}";
                        _spriteBatch.DrawString(font, calc2, new Vector2(screenWidth / 2 - 140, yOffset + 25), Color.Gray);
                    }
                    
                    // Space between calculation and balance/shop
                    yOffset += 55;
                    
                    // Shop section - only show after money animation is done
                    if (moneyAnimationDone && slamAnimationDone)
                    {
                        // Shop section with border (moved down to make room for balance)
                        int shopYOffset = yOffset + 60;
                        int shopBoxX = screenWidth / 2 - 130;
                        int shopBoxY = yOffset - 5;
                        int shopBoxWidth = 260;
                        int shopBoxHeight = 165; // Smaller to not overlap Next button
                        
                        // Draw shop border
                        _spriteBatch.Draw(white, new Rectangle(shopBoxX, shopBoxY, shopBoxWidth, 3), Color.Cyan);
                        _spriteBatch.Draw(white, new Rectangle(shopBoxX, shopBoxY + shopBoxHeight - 3, shopBoxWidth, 3), Color.Cyan);
                        _spriteBatch.Draw(white, new Rectangle(shopBoxX, shopBoxY, 3, shopBoxHeight), Color.Cyan);
                        _spriteBatch.Draw(white, new Rectangle(shopBoxX + shopBoxWidth - 3, shopBoxY, 3, shopBoxHeight), Color.Cyan);
                        
                        // Shop title
                        string shopTitle = "=== SHOP ===";
                        Vector2 shopTitleSize = font.MeasureString(shopTitle);
                        _spriteBatch.DrawString(font, shopTitle, new Vector2((screenWidth - shopTitleSize.X) / 2, yOffset + 5), Color.Cyan);
                        
                        // Shop buttons with smaller text
                        string[] shopTexts = {
                            "+3% Speed $25",
                            "Extra Ball $5",
                            "Shoot 6s $15"
                        };
                        
                        for (int i = 0; i < 3; i++)
                        {
                            bool canAfford = (i == 0 && bankBalance >= 25) || (i == 1 && bankBalance >= 5) || (i == 2 && bankBalance >= 15);
                            Color buttonColor = shopButtonsHovered[i] && canAfford ? Color.LightBlue : (canAfford ? Color.Blue : Color.DarkGray);
                            _spriteBatch.Draw(white, shopButtons[i], buttonColor);
                            
                            Vector2 textSize = font.MeasureString(shopTexts[i]);
                            Vector2 textPos = new Vector2(
                                shopButtons[i].X + (shopButtons[i].Width - textSize.X) / 2,
                                shopButtons[i].Y + (shopButtons[i].Height - textSize.Y) / 2
                            );
                            _spriteBatch.DrawString(font, shopTexts[i], textPos, canAfford ? Color.White : Color.Gray);
                        }
                        
                        // Next level button
                        Color nextColor = nextLevelButtonHovered ? Color.LightGreen : Color.Green;
                        _spriteBatch.Draw(white, nextLevelButton, nextColor);
                        string nextText = "NEXT LEVEL";
                        Vector2 nextTextSize = font.MeasureString(nextText);
                        Vector2 nextTextPos = new Vector2(
                            nextLevelButton.X + (nextLevelButton.Width - nextTextSize.X) / 2,
                            nextLevelButton.Y + (nextLevelButton.Height - nextTextSize.Y) / 2
                        );
                        _spriteBatch.DrawString(font, nextText, nextTextPos, Color.Black);
                    }
                    
                    // Draw final balance AFTER shop so it appears on top
                    if (moneyAnimationDone)
                    {
                        string finalBalance = $"${bankBalance}";
                        Vector2 balanceSize = font.MeasureString(finalBalance);
                        Vector2 balancePos = new Vector2((screenWidth - balanceSize.X * slamScale) / 2, yOffset + slamY);
                        
                        // Subtle pulsing glow effect (readable text)
                        float glowIntensity = (float)Math.Sin(glowPulse) * 0.15f + 0.2f; // 0.05 to 0.35
                        for (int ox = -2; ox <= 2; ox++)
                        {
                            for (int oy = -2; oy <= 2; oy++)
                            {
                                if (ox != 0 || oy != 0)
                                {
                                    float distance = (float)Math.Sqrt(ox * ox + oy * oy);
                                    if (distance <= 2f)
                                    {
                                        _spriteBatch.DrawString(font, finalBalance, balancePos + new Vector2(ox, oy), 
                                            Color.Gold * (glowIntensity / distance), 0f, Vector2.Zero, slamScale, SpriteEffects.None, 0f);
                                    }
                                }
                            }
                        }
                        
                        _spriteBatch.DrawString(font, finalBalance, balancePos, Color.Gold, 0f, Vector2.Zero, slamScale, SpriteEffects.None, 0f);
                    }
                }
            }
            
            // draw game over overlay
            if (gameOver)
            {
                // semi-transparent dark overlay
                if (white != null)
                {
                    _spriteBatch.Draw(white, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.8f);
                }

                if (font != null)
                {
                    // Game Over title
                    string gameOverText = "GAME OVER";
                    Vector2 titleSize = font.MeasureString(gameOverText);
                    Vector2 titlePos = new Vector2((screenWidth - titleSize.X) / 2, screenHeight / 2 - 80);
                    
                    // Draw with red glow effect
                    for (int offsetX = -2; offsetX <= 2; offsetX++)
                    {
                        for (int offsetY = -2; offsetY <= 2; offsetY++)
                        {
                            if (offsetX != 0 || offsetY != 0)
                            {
                                _spriteBatch.DrawString(font, gameOverText, titlePos + new Vector2(offsetX, offsetY), Color.DarkRed);
                            }
                        }
                    }
                    _spriteBatch.DrawString(font, gameOverText, titlePos, Color.Red);
                    
                    // Final score and time
                    string scoreText = $"Score: {score}";
                    string timeText = $"Time: {(int)gameTimer / 60:D2}:{(int)gameTimer % 60:D2}";
                    Vector2 scoreSize = font.MeasureString(scoreText);
                    Vector2 timeSize = font.MeasureString(timeText);
                    _spriteBatch.DrawString(font, scoreText, new Vector2((screenWidth - scoreSize.X) / 2, screenHeight / 2 - 20), Color.White);
                    _spriteBatch.DrawString(font, timeText, new Vector2((screenWidth - timeSize.X) / 2, screenHeight / 2 + 10), Color.White);
                    
                    // Draw retry button
                    Color retryColor = retryButtonHovered ? Color.LightGreen : Color.Green;
                    _spriteBatch.Draw(white, retryButton, retryColor);
                    string retryText = "RETRY";
                    Vector2 retryTextSize = font.MeasureString(retryText);
                    Vector2 retryTextPos = new Vector2(
                        retryButton.X + (retryButton.Width - retryTextSize.X) / 2,
                        retryButton.Y + (retryButton.Height - retryTextSize.Y) / 2
                    );
                    _spriteBatch.DrawString(font, retryText, retryTextPos, Color.Black);
                    
                    // Draw quit button
                    Color quitColor = quitButtonHovered ? Color.LightCoral : Color.DarkRed;
                    _spriteBatch.Draw(white, quitButton, quitColor);
                    string quitText = "QUIT";
                    Vector2 quitTextSize = font.MeasureString(quitText);
                    Vector2 quitTextPos = new Vector2(
                        quitButton.X + (quitButton.Width - quitTextSize.X) / 2,
                        quitButton.Y + (quitButton.Height - quitTextSize.Y) / 2
                    );
                    _spriteBatch.DrawString(font, quitText, quitTextPos, Color.White);
                }
            }
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
