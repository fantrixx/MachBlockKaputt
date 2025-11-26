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
        private SpriteBatch _spriteBatch;

        private int screenWidth = 800;
        private int screenHeight = 600;

        private Rectangle paddle;
        private Vector2 paddleVelocity;
        private int paddleSpeed = 400;

        private Rectangle ballRect;
        private Vector2 ballVelocity;
        private int ballSize = 10;
        private bool ballLaunched = false;

        private List<Rectangle> bricks = new List<Rectangle>();
        // reduced layout for faster testing: total bricks = brickRows * brickCols = 1 * 5 = 5
        private int brickRows = 1;
        private int brickCols = 5;
        private int level = 1;
        private bool levelCleared = false;
        private float levelClearTimer = 0f;
        private float levelClearAutoStart = 2.0f; // seconds until auto-start next level

        private SpriteFont? font;
        private Texture2D? white;
        private Texture2D? ballTexture;
        private Texture2D? paddleTexture;
        private float ballSpeedIncrease = 1.05f; // multiplier applied on each brick hit
        private float maxBallSpeed = 800f; // cap to prevent runaway speed

        private int score = 0;
        private int lives = 3;

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
            paddle = new Rectangle(screenWidth / 2 - 50, screenHeight - 40, 100, 20);
            paddleVelocity = Vector2.Zero;

            ballRect = new Rectangle(screenWidth / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize);
            ballVelocity = new Vector2(150, -150);

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
            int brickWidth = screenWidth / brickCols;
            int brickHeight = 20;

            int pattern = (lvl - 1) % 4; // choose among 4 formations
            switch (pattern)
            {
                // pattern 0: full grid (classic)
                case 0:
                    for (int r = 0; r < brickRows; r++)
                    {
                        for (int c = 0; c < brickCols; c++)
                        {
                            var rect = new Rectangle(c * brickWidth + 2, 50 + r * (brickHeight + 2), brickWidth - 4, brickHeight);
                            bricks.Add(rect);
                        }
                    }
                    break;

                // pattern 1: pyramid
                case 1:
                    for (int r = 0; r < brickRows; r++)
                    {
                        int cols = brickCols - r * 2;
                        int startCol = r;
                        for (int c = 0; c < cols; c++)
                        {
                            var rect = new Rectangle((startCol + c) * brickWidth + 2, 50 + r * (brickHeight + 2), brickWidth - 4, brickHeight);
                            bricks.Add(rect);
                        }
                    }
                    break;

                // pattern 2: checkerboard
                case 2:
                    for (int r = 0; r < brickRows; r++)
                    {
                        for (int c = 0; c < brickCols; c++)
                        {
                            if ((r + c) % 2 == 0)
                            {
                                var rect = new Rectangle(c * brickWidth + 2, 50 + r * (brickHeight + 2), brickWidth - 4, brickHeight);
                                bricks.Add(rect);
                            }
                        }
                    }
                    break;

                // pattern 3: gaps (every third column empty)
                default:
                    for (int r = 0; r < brickRows; r++)
                    {
                        for (int c = 0; c < brickCols; c++)
                        {
                            if (c % 3 != 0)
                            {
                                var rect = new Rectangle(c * brickWidth + 2, 50 + r * (brickHeight + 2), brickWidth - 4, brickHeight);
                                bricks.Add(rect);
                            }
                        }
                    }
                    break;
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
            }
            catch
            {
                font = null;
            }
        }

        private Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int diameter, Color color)
        {
            var tex = new Texture2D(graphicsDevice, diameter, diameter);
            var data = new Color[diameter * diameter];
            float radius = diameter / 2f;
            float center = radius - 0.5f;
            float radiusSq = radius * radius;

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distSq = dx * dx + dy * dy;
                    int i = y * diameter + x;
                    data[i] = distSq <= radiusSq ? color : Color.Transparent;
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

        private void IncreaseBallSpeed()
        {
            if (ballVelocity == Vector2.Zero)
                return;

            float speed = ballVelocity.Length();
            speed = (float)Math.Min(maxBallSpeed, speed * ballSpeedIncrease);
            ballVelocity = Vector2.Normalize(ballVelocity) * speed;
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
                    ballRect = new Rectangle(screenWidth / 2 - ballSize / 2, paddle.Y - ballSize - 1, ballSize, ballSize);
                    ballVelocity = new Vector2(150, -150);
                    ballLaunched = false;
                }
                return; // skip normal updates while waiting
            }

            var kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
                paddleVelocity.X = -paddleSpeed;
            else if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
                paddleVelocity.X = paddleSpeed;
            else
                paddleVelocity.X = 0;

            paddle.X += (int)(paddleVelocity.X * dt);
            if (paddle.X < 0) paddle.X = 0;
            if (paddle.X + paddle.Width > screenWidth) paddle.X = screenWidth - paddle.Width;

            if (!ballLaunched)
            {
                ballRect.X = paddle.X + paddle.Width / 2 - ballSize / 2;
                if (kb.IsKeyDown(Keys.Space)) ballLaunched = true;
            }
            else
            {
                ballRect.X += (int)(ballVelocity.X * dt);
                ballRect.Y += (int)(ballVelocity.Y * dt);

                // wall collisions
                if (ballRect.X <= 0) { ballRect.X = 0; ballVelocity.X *= -1; }
                if (ballRect.X + ballRect.Width >= screenWidth) { ballRect.X = screenWidth - ballRect.Width; ballVelocity.X *= -1; }
                if (ballRect.Y <= 0) { ballRect.Y = 0; ballVelocity.Y *= -1; }

                // paddle collision
                if (ballRect.Intersects(paddle))
                {
                    ballRect.Y = paddle.Y - ballRect.Height - 1;
                    ballVelocity.Y *= -1;
                    // change X based on where it hits the paddle
                    float hitPos = (ballRect.Center.X - paddle.X) / (float)paddle.Width - 0.5f;
                    ballVelocity.X += hitPos * 300;
                    // play paddle hit sound (different from brick explosion)
                    try { paddleSound?.Play(); } catch { }
                }

                // brick collisions
                for (int i = bricks.Count - 1; i >= 0; i--)
                {
                        if (ballRect.Intersects(bricks[i]))
                        {
                            var b = bricks[i];
                            var center = new Vector2(b.Center.X, b.Center.Y);
                            // compute brick color based on its row so explosions match brick color
                            int brickWidth = screenWidth / brickCols;
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

                            bricks.RemoveAt(i);
                            // spawn explosion at brick center with brick color
                            SpawnExplosion(center, 24, brickColor);
                            // play short hit sound
                            try { explosionSound?.Play(); } catch { }
                            score += 100;

                            // simple collision response: invert Y
                            ballVelocity.Y *= -1;
                            // slightly increase ball speed to raise difficulty
                            IncreaseBallSpeed();
                            break;
                        }
                }

                // if all bricks cleared -> level finished
                if (bricks.Count == 0)
                {
                    level++;
                    levelCleared = true;
                    levelClearTimer = 0f;
                    // stop ball movement
                    ballVelocity = Vector2.Zero;
                    ballLaunched = false;
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

                // bottom - lose life
                if (ballRect.Y > screenHeight)
                {
                    lives--;
                    if (lives <= 0)
                    {
                        // reset game
                        lives = 3;
                        score = 0;
                        InitBricks();
                    }
                    ballLaunched = false;
                    ballVelocity = new Vector2(150, -150);
                    ballRect.X = paddle.X + paddle.Width / 2 - ballSize / 2;
                    ballRect.Y = paddle.Y - ballSize - 1;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            if (white != null)
            {
                // draw paddle with rounded corners texture (tinted black)
                if (paddleTexture != null)
                {
                    _spriteBatch.Draw(paddleTexture, paddle, Color.Black);
                }
                else
                {
                    _spriteBatch.Draw(white, paddle, Color.Black);
                }

                // draw ball (use circular texture if available)
                if (ballTexture != null)
                    _spriteBatch.Draw(ballTexture, ballRect, Color.Yellow);
                else
                    _spriteBatch.Draw(white, ballRect, Color.Yellow);

                // draw bricks
                Color[] colors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue };
                for (int i = 0; i < bricks.Count; i++)
                {
                    _spriteBatch.Draw(white, bricks[i], colors[(i / brickCols) % colors.Length]);
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

                // UI
                string scoreText = $"Score: {score:D8}"; // 8-digit score with leading zeros
                string livesText = $"Lives: {lives}";
                if (font != null)
                {
                    // draw lives on left
                    _spriteBatch.DrawString(font, livesText, new Vector2(10, 10), Color.White);
                    // draw 8-digit score on right (black text for visibility on blue background)
                    var scoreSize = font.MeasureString(scoreText);
                    _spriteBatch.DrawString(font, scoreText, new Vector2(screenWidth - scoreSize.X - 10, 10), Color.Black);
                    // show level
                    _spriteBatch.DrawString(font, $"Level: {level}", new Vector2(screenWidth - 120, 40), Color.White);
                }
                else
                {
                    // Fallback UI without a SpriteFont: draw a score bar and life blocks
                    int scoreBarWidth = System.Math.Min(200, score / 10);
                    _spriteBatch.Draw(white, new Rectangle(10, 10, scoreBarWidth > 0 ? scoreBarWidth : 1, 10), Color.White);
                    for (int i = 0; i < lives; i++)
                        _spriteBatch.Draw(white, new Rectangle(10 + i * 22, 30, 20, 10), Color.Red);
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
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
