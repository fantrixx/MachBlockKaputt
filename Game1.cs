using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using AlleywayMonoGame.Models;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Services;
using AlleywayMonoGame.Systems;
using AlleywayMonoGame.Core;

namespace AlleywayMonoGame
{
    /// <summary>
    /// Main game class - refactored to use modular architecture
    /// </summary>
    public class Game1 : Game
    {
        // Core MonoGame components
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = null!;

        // Services (Business Logic)
        private AudioService _audioService = null!;
        private ScoreService _scoreService = null!;
        private ShopService _shopService = null!;

        // Systems (Game Logic)
        private ParticleSystem _particleSystem = null!;
        private FloatingTextSystem _floatingTextSystem = null!;
        private CollisionSystem _collisionSystem = null!;
        private LevelSystem _levelSystem = null!;
        private GameStateManager _gameState = null!;

        // Entities
        private Paddle _paddle = null!;
        private List<Ball> _balls = new List<Ball>();
        private List<Brick> _bricks = new List<Brick>();
        private List<Projectile> _projectiles = new List<Projectile>();

        // Textures
        private SpriteFont? _font;
        private Texture2D? _whitePixel;
        private Texture2D? _ballTexture;
        private Texture2D? _paddleTexture;
        private Texture2D? _heartTexture;

        // Input
        private KeyboardState _previousKeyState;

        // Power-up state
        private bool _canShoot;
        private float _shootPowerTimer;
        private float _cannonExtension;
        private float _flickerTimer;

        // Level complete state
        private bool _levelComplete;
        private float _animationTimer;
        private int _animatedMoney;
        private bool _moneyAnimationDone;
        private int _levelCompleteTimeBonus;
        private bool _chargeUpSoundPlayed;
        
        // Money slam animation
        private float _slamY;
        private float _slamVelocity;
        private float _slamScale = 1f;
        private bool _slamAnimationDone;
        private float _glowPulse;
        
        // Purchase animation
        private bool _purchaseAnimationActive;
        private float _purchaseCostX;
        private float _purchaseCostY;
        private int _purchaseCostAmount;
        private float _purchaseAnimationTimer;
        private float _balanceShake;
        
        // UI state
        private Rectangle _nextLevelButton;
        private bool _nextLevelButtonHovered;
        private Rectangle[] _shopButtons = new Rectangle[3];
        private bool[] _shopButtonsHovered = new bool[3];
        
        // Game over state
        private Rectangle _retryButton;
        private Rectangle _quitButton;
        private bool _retryButtonHovered;
        private bool _quitButtonHovered;

        // Shop upgrades
        private int _extraBallsPurchased;
        private bool _startWithShootMode;

        // Victory state
        private float _victoryGlowTimer;
        private Rectangle _victoryRetryButton;
        private Rectangle _victoryQuitButton;
        private bool _victoryRetryButtonHovered;
        private bool _victoryQuitButtonHovered;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GameConstants.ScreenWidth;
            _graphics.PreferredBackBufferHeight = GameConstants.ScreenHeight;
        }

        protected override void Initialize()
        {
            // Initialize services
            _scoreService = new ScoreService(initialLives: 1);
            _shopService = new ShopService();
            _gameState = new GameStateManager();

            // Initialize systems
            _particleSystem = new ParticleSystem();
            _floatingTextSystem = new FloatingTextSystem();
            _collisionSystem = new CollisionSystem();
            _levelSystem = new LevelSystem(GameConstants.ScreenWidth, GameConstants.GameAreaTop);

            // Initialize paddle
            _paddle = new Paddle(
                GameConstants.ScreenWidth / 2 - GameConstants.PaddleWidth / 2,
                GameConstants.ScreenHeight - 40,
                GameConstants.PaddleWidth,
                GameConstants.PaddleHeight,
                GameConstants.PaddleSpeed,
                GameConstants.ScreenWidth
            );

            // Initialize first ball
            ResetBall();

            // Initialize input
            _previousKeyState = Keyboard.GetState();

            // Generate first level
            GenerateLevel();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize audio service
            _audioService = new AudioService();

            // Create base textures
            _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });

            // Create game textures
            _ballTexture = TextureFactory.CreateCircleTexture(GraphicsDevice, GameConstants.BallSize, Color.White);
            _paddleTexture = TextureFactory.CreateRoundedRectangleTexture(
                GraphicsDevice, 
                GameConstants.PaddleWidth, 
                GameConstants.PaddleHeight, 
                8, 
                Color.White
            );
            _heartTexture = TextureFactory.CreateHeartTexture(GraphicsDevice, 20, Color.White);

            // Load font
            try
            {
                _font = Content.Load<SpriteFont>("DefaultFont");
            }
            catch
            {
                _font = null;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update based on game state
            if (_gameState.IsVictory)
            {
                UpdateVictory(dt);
                return;
            }

            if (_scoreService.IsGameOver)
            {
                UpdateGameOver(dt);
                return;
            }

            if (_levelComplete)
            {
                UpdateLevelComplete(dt);
                return;
            }

            // Normal gameplay
            UpdateGameplay(dt);

            base.Update(gameTime);
        }

        private void UpdateVictory(float dt)
        {
            _victoryGlowTimer += dt * 2f;

            var mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            _victoryRetryButtonHovered = _victoryRetryButton.Contains(mousePos);
            _victoryQuitButtonHovered = _victoryQuitButton.Contains(mousePos);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_victoryRetryButtonHovered)
                {
                    RestartGame();
                }
                else if (_victoryQuitButtonHovered)
                {
                    Exit();
                }
            }
        }

        private void UpdateGameOver(float dt)
        {
            var mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            _retryButtonHovered = _retryButton.Contains(mousePos);
            _quitButtonHovered = _quitButton.Contains(mousePos);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_retryButtonHovered)
                {
                    RestartGame();
                }
                else if (_quitButtonHovered)
                {
                    Exit();
                }
            }
        }

        private void UpdateLevelComplete(float dt)
        {
            _animationTimer += dt;

            // Money counting animation
            if (!_moneyAnimationDone && _animationTimer > 1f)
            {
                // Play charge up sound once when counting starts
                if (!_chargeUpSoundPlayed)
                {
                    _audioService.PlayChargeUp();
                    _chargeUpSoundPlayed = true;
                }
                
                float animSpeed = dt * 200f;
                _animatedMoney += (int)animSpeed;
                if (_animatedMoney >= _levelCompleteTimeBonus)
                {
                    _animatedMoney = _levelCompleteTimeBonus;
                    _moneyAnimationDone = true;
                    _shopService.AddMoney(_levelCompleteTimeBonus);
                    
                    _slamY = -100f;
                    _slamVelocity = 0f;
                    _slamScale = 1f;
                    _slamAnimationDone = false;
                }
            }

            // Slam animation
            if (_moneyAnimationDone && !_slamAnimationDone)
            {
                UpdateSlamAnimation(dt);
            }

            _glowPulse += dt * 3f;

            // Purchase animation
            if (_purchaseAnimationActive)
            {
                UpdatePurchaseAnimation(dt);
            }

            // Shop interaction
            UpdateShopInteraction(dt);
        }

        private void UpdateSlamAnimation(float dt)
        {
            float gravity = 1200f * dt;
            _slamVelocity += gravity;
            _slamY += _slamVelocity;

            float targetY = 0f;
            if (_slamY >= targetY)
            {
                _slamY = targetY;
                float prevVelocity = _slamVelocity;
                _slamVelocity *= -0.4f;
                _slamScale = 1.2f;

                if (prevVelocity > 0f && Math.Abs(prevVelocity) > 50f)
                {
                    Vector2 impactPos = new Vector2(GameConstants.ScreenWidth / 2, 40 + 3 * 50 + 30);
                    _particleSystem.SpawnDustCloud(impactPos, 20);
                }

                if (Math.Abs(_slamVelocity) < 30f)
                {
                    _slamVelocity = 0f;
                    _slamAnimationDone = true;
                }
            }

            if (_slamScale > 1f)
            {
                _slamScale -= dt * 1.5f;
                if (_slamScale < 1f) _slamScale = 1f;
            }
        }

        private void UpdatePurchaseAnimation(float dt)
        {
            _purchaseAnimationTimer += dt;

            if (_purchaseAnimationTimer < 0.5f)
            {
                float progress = _purchaseAnimationTimer / 0.5f;
                float eased = 1f - (float)Math.Pow(1f - progress, 3);
                _purchaseCostX += (GameConstants.ScreenWidth / 2 - _purchaseCostX) * eased * dt * 8f;
            }
            else if (_purchaseAnimationTimer < 0.8f)
            {
                if (_purchaseAnimationTimer >= 0.5f && _purchaseAnimationTimer - dt < 0.5f)
                {
                    Vector2 impactPos = new Vector2(GameConstants.ScreenWidth / 2, 40 + 3 * 50 + 10);
                    _particleSystem.SpawnExplosion(impactPos, 20, Color.Gold);
                }

                float shakeIntensity = 10f * (1f - (_purchaseAnimationTimer - 0.5f) / 0.3f);
                _balanceShake = ((float)new Random().NextDouble() - 0.5f) * shakeIntensity * 2f;
            }
            else
            {
                _purchaseAnimationActive = false;
                _balanceShake = 0f;
            }
        }

        private void UpdateShopInteraction(float dt)
        {
            var mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            for (int i = 0; i < 3; i++)
            {
                _shopButtonsHovered[i] = _shopButtons[i].Contains(mousePos);
            }
            _nextLevelButtonHovered = _nextLevelButton.Contains(mousePos);

            if (mouseState.LeftButton == ButtonState.Pressed && !_purchaseAnimationActive && _moneyAnimationDone)
            {
                if (_shopButtonsHovered[0] && _shopService.CanAfford(ShopItem.SpeedUpgrade))
                {
                    StartPurchaseAnimation(ShopItem.SpeedUpgrade, 0);
                    _shopService.Purchase(ShopItem.SpeedUpgrade);
                    _paddle.SpeedMultiplier = _shopService.PaddleSpeedMultiplier;
                    _audioService.PlayCashRegister();
                }
                else if (_shopButtonsHovered[1] && _shopService.CanAfford(ShopItem.ExtraBall))
                {
                    StartPurchaseAnimation(ShopItem.ExtraBall, 1);
                    _shopService.Purchase(ShopItem.ExtraBall);
                    _extraBallsPurchased++;
                    _audioService.PlayCashRegister();
                }
                else if (_shopButtonsHovered[2] && _shopService.CanAfford(ShopItem.ShootMode))
                {
                    StartPurchaseAnimation(ShopItem.ShootMode, 2);
                    _shopService.Purchase(ShopItem.ShootMode);
                    _startWithShootMode = true;
                    _audioService.PlayCashRegister();
                }
                else if (_nextLevelButtonHovered)
                {
                    AdvanceToNextLevel();
                }
            }
        }

        private void StartPurchaseAnimation(ShopItem item, int buttonIndex)
        {
            _purchaseAnimationActive = true;
            _purchaseCostAmount = _shopService.GetCost(item);
            _purchaseCostX = _shopButtons[buttonIndex].X - 150;
            _purchaseCostY = 40 + 3 * 50;
            _purchaseAnimationTimer = 0f;
        }

        private void UpdateGameplay(float dt)
        {
            var kb = Keyboard.GetState();

            // Cheat: P to win level
            if (kb.IsKeyDown(Keys.P) && _previousKeyState.IsKeyUp(Keys.P))
            {
                _bricks.Clear();
            }

            // Cheat: O to win all 10 levels
            if (kb.IsKeyDown(Keys.O) && _previousKeyState.IsKeyUp(Keys.O))
            {
                _gameState.CurrentLevel = 10;
                _bricks.Clear();
            }

            // Update timer
            _scoreService.UpdateTimer(dt);

            // Update flicker for special bricks
            _flickerTimer += dt * 10f;

            // Update shoot power-up
            UpdateShootPowerUp(dt, kb);

            // Update projectiles
            UpdateProjectiles(dt);

            // Update paddle
            UpdatePaddle(dt, kb);

            // Update balls
            UpdateBalls(dt, kb);

            // Check projectile collisions
            CheckProjectileCollisions();

            // Check for level complete
            if (_bricks.Count == 0 && !_levelComplete)
            {
                OnLevelComplete();
            }

            // Update particle systems
            _particleSystem.Update(dt);
            _floatingTextSystem.Update(dt);

            _previousKeyState = kb;
        }

        private void UpdateShootPowerUp(float dt, KeyboardState kb)
        {
            if (_canShoot)
            {
                _shootPowerTimer -= dt;
                if (_shootPowerTimer <= 0)
                {
                    _canShoot = false;
                }

                if (_cannonExtension < 1f)
                {
                    _cannonExtension += dt * 3f;
                    if (_cannonExtension > 1f) _cannonExtension = 1f;
                }

                // Shooting
                if (kb.IsKeyDown(Keys.Space) && _previousKeyState.IsKeyUp(Keys.Space))
                {
                    var projectile = new Projectile(
                        _paddle.Center.X - GameConstants.ProjectileWidth / 2,
                        _paddle.Y - GameConstants.ProjectileHeight,
                        GameConstants.ProjectileWidth,
                        GameConstants.ProjectileHeight,
                        GameConstants.ProjectileSpeed
                    );
                    _projectiles.Add(projectile);
                    _audioService.PlayRocketLaunch();
                }
            }
            else
            {
                if (_cannonExtension > 0f)
                {
                    _cannonExtension -= dt * 3f;
                    if (_cannonExtension < 0f) _cannonExtension = 0f;
                }
            }
        }

        private void UpdateProjectiles(float dt)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                _projectiles[i].Update(dt);

                // Smoke trail
                _particleSystem.SpawnSmokeTrail(_projectiles[i].Center);

                // Remove off-screen
                if (_projectiles[i].IsOffScreen(GameConstants.GameAreaTop))
                {
                    _projectiles.RemoveAt(i);
                }
            }
        }

        private void UpdatePaddle(float dt, KeyboardState kb)
        {
            if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
            {
                _paddle.MoveLeft(dt);
            }
            else if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
            {
                _paddle.MoveRight(dt);
            }
            else
            {
                _paddle.Stop();
            }
        }

        private void UpdateBalls(float dt, KeyboardState kb)
        {
            for (int i = _balls.Count - 1; i >= 0; i--)
            {
                var ball = _balls[i];

                if (!ball.IsLaunched)
                {
                    // Attach to paddle
                    ball.Rect = new Rectangle(
                        _paddle.Center.X - GameConstants.BallSize / 2,
                        _paddle.Y - GameConstants.BallSize - 1,
                        GameConstants.BallSize,
                        GameConstants.BallSize
                    );

                    if (kb.IsKeyDown(Keys.Space))
                    {
                        ball.IsLaunched = true;
                        _scoreService.StartTimer();
                    }
                }
                else
                {
                    // Move ball
                    ball.Rect = new Rectangle(
                        ball.Rect.X + (int)(ball.Velocity.X * dt),
                        ball.Rect.Y + (int)(ball.Velocity.Y * dt),
                        ball.Rect.Width,
                        ball.Rect.Height
                    );
                }
            }

            // Check collisions
            var collisionResult = _collisionSystem.CheckBallCollisions(
                _balls,
                _paddle,
                _bricks,
                GameConstants.ScreenWidth,
                GameConstants.GameAreaTop,
                GameConstants.ScreenHeight
            );

            // Handle paddle hits
            if (collisionResult.PaddleHit)
            {
                _audioService.PlayPaddleHit();
            }

            // Handle brick hits
            foreach (var (index, brick) in collisionResult.BricksHit.OrderByDescending(b => b.index))
            {
                HandleBrickDestruction(index, brick);
            }

            // Handle lost balls
            foreach (int ballIndex in collisionResult.BallsLost.OrderByDescending(i => i))
            {
                _balls.RemoveAt(ballIndex);

                if (_balls.Count == 0)
                {
                    OnAllBallsLost();
                }
            }
        }

        private void HandleBrickDestruction(int index, Brick brick, bool fromProjectile = false)
        {
            bool wasShootBrick = brick.Type == BrickType.ShootPowerUp;
            bool wasExtraBallBrick = brick.Type == BrickType.ExtraBall;

            _bricks.RemoveAt(index);

            // Calculate brick color
            int brickHeight = 20;
            int row = (brick.Bounds.Y - 50) / (brickHeight + 2);
            Color brickColor = Brick.GetColorForRow(row);

            // Effects
            _particleSystem.SpawnExplosion(brick.Center, 24, brickColor);
            
            // Different sounds for projectile vs ball
            if (fromProjectile)
            {
                _audioService.PlayProjectileExplosion();
            }
            else
            {
                _audioService.PlayExplosion();
            }
            
            _scoreService.AddBrickScore();

            // Power-ups (only if shoot mode not active)
            if (wasShootBrick && !_canShoot)
            {
                _canShoot = true;
                _shootPowerTimer = GameConstants.ShootPowerDuration;
            }

            if (wasExtraBallBrick && !_canShoot)
            {
                SpawnExtraBall(brick.Center);
                _floatingTextSystem.AddText("+BALL", brick.Center, Color.White, 3f);
            }
        }

        private void CheckProjectileCollisions()
        {
            var collisions = _collisionSystem.CheckProjectileCollisions(
                _projectiles,
                _bricks,
                GameConstants.GameAreaTop
            );

            foreach (var (projIndex, brickIndex, brick) in collisions.OrderByDescending(c => c.projectileIndex))
            {
                if (brickIndex == -1)
                {
                    // Off-screen removal
                    _projectiles.RemoveAt(projIndex);
                }
                else
                {
                    // Hit brick
                    HandleBrickDestruction(brickIndex, brick, fromProjectile: true);
                    _projectiles.RemoveAt(projIndex);
                }
            }
        }

        private void OnAllBallsLost()
        {
            _canShoot = false;
            _shootPowerTimer = 0f;
            _projectiles.Clear();

            _scoreService.LoseLife();

            if (_scoreService.IsGameOver)
            {
                SetupGameOverUI();
            }
            else
            {
                ResetBall();
            }
        }

        private void OnLevelComplete()
        {
            // Check for victory (all 10 levels completed)
            if (_gameState.CurrentLevel >= 10)
            {
                _gameState.SetVictory();
                SetupVictoryUI();
                return;
            }

            _levelComplete = true;
            _scoreService.StopTimer();
            _animationTimer = 0f;
            _moneyAnimationDone = false;
            _animatedMoney = 0;
            _chargeUpSoundPlayed = false;

            _levelCompleteTimeBonus = _shopService.CalculateTimeBonus(_scoreService.GameTimer);

            SetupShopUI();

            // Stop all balls
            foreach (var ball in _balls)
            {
                ball.Velocity = Vector2.Zero;
                ball.IsLaunched = false;
            }
        }

        private void AdvanceToNextLevel()
        {
            // Check for victory (all 10 levels completed)
            if (_gameState.CurrentLevel >= 10)
            {
                _gameState.SetVictory();
                SetupVictoryUI();
                return;
            }

            _gameState.NextLevel();
            _levelComplete = false;
            _scoreService.ResetTimer();
            
            // Clear all active game elements
            _projectiles.Clear();
            _particleSystem.Clear();
            _floatingTextSystem.Clear();
            _canShoot = false;
            _shootPowerTimer = 0f;
            _cannonExtension = 0f;
            
            GenerateLevel();

            // Reset paddle
            _paddle = new Paddle(
                GameConstants.ScreenWidth / 2 - GameConstants.PaddleWidth / 2,
                GameConstants.ScreenHeight - 40,
                GameConstants.PaddleWidth,
                GameConstants.PaddleHeight,
                GameConstants.PaddleSpeed,
                GameConstants.ScreenWidth
            );
            _paddle.SpeedMultiplier = _shopService.PaddleSpeedMultiplier;

            // Reset balls
            _balls.Clear();
            ResetBall();

            // Spawn extra balls if purchased
            for (int i = 0; i < _extraBallsPurchased; i++)
            {
                float angle = -90f + (i + 1) * 30f;
                float radians = angle * (float)Math.PI / 180f;
                float speed = 200f;

                var extraBall = new Ball(
                    new Rectangle(
                        GameConstants.ScreenWidth / 2 - GameConstants.BallSize / 2,
                        _paddle.Y - GameConstants.BallSize - 1,
                        GameConstants.BallSize,
                        GameConstants.BallSize
                    ),
                    new Vector2((float)Math.Cos(radians) * speed, (float)Math.Sin(radians) * speed),
                    true
                );
                _balls.Add(extraBall);
            }
            _extraBallsPurchased = 0;

            // Start with shoot mode if purchased
            if (_startWithShootMode)
            {
                _canShoot = true;
                _shootPowerTimer = 6f;
                _startWithShootMode = false;
            }

            _scoreService.StartTimer();
        }

        private void RestartGame()
        {
            _scoreService.Reset();
            _shopService = new ShopService();
            _gameState.Reset();
            _levelComplete = false;
            _canShoot = false;
            _projectiles.Clear();
            _particleSystem.Clear();
            _floatingTextSystem.Clear();

            GenerateLevel();
            ResetBall();

            _paddle = new Paddle(
                GameConstants.ScreenWidth / 2 - GameConstants.PaddleWidth / 2,
                GameConstants.ScreenHeight - 40,
                GameConstants.PaddleWidth,
                GameConstants.PaddleHeight,
                GameConstants.PaddleSpeed,
                GameConstants.ScreenWidth
            );
        }

        private void GenerateLevel()
        {
            var levelData = _levelSystem.GenerateLevel(_gameState.CurrentLevel);
            _bricks = levelData.Bricks;
        }

        private void ResetBall()
        {
            _balls.Clear();
            var ball = new Ball(
                new Rectangle(
                    GameConstants.ScreenWidth / 2 - GameConstants.BallSize / 2,
                    GameConstants.ScreenHeight - 40 - GameConstants.BallSize - 1,
                    GameConstants.BallSize,
                    GameConstants.BallSize
                ),
                new Vector2(150, -150),
                false
            );
            _balls.Add(ball);
        }

        private void SpawnExtraBall(Vector2 position)
        {
            var ball = new Ball(
                new Rectangle(
                    (int)position.X - GameConstants.BallSize / 2,
                    (int)position.Y,
                    GameConstants.BallSize,
                    GameConstants.BallSize
                ),
                new Vector2(0, 150f),
                true
            );
            _balls.Add(ball);
        }

        private void SetupGameOverUI()
        {
            int buttonWidth = 150;
            int buttonHeight = 50;
            int buttonSpacing = 20;
            _retryButton = new Rectangle(
                GameConstants.ScreenWidth / 2 - buttonWidth - buttonSpacing / 2,
                GameConstants.ScreenHeight / 2 + 60,
                buttonWidth,
                buttonHeight
            );
            _quitButton = new Rectangle(
                GameConstants.ScreenWidth / 2 + buttonSpacing / 2,
                GameConstants.ScreenHeight / 2 + 60,
                buttonWidth,
                buttonHeight
            );
        }

        private void SetupVictoryUI()
        {
            int buttonWidth = 150;
            int buttonHeight = 50;
            int buttonSpacing = 20;
            _victoryRetryButton = new Rectangle(
                GameConstants.ScreenWidth / 2 - buttonWidth - buttonSpacing / 2,
                GameConstants.ScreenHeight - 120,
                buttonWidth,
                buttonHeight
            );
            _victoryQuitButton = new Rectangle(
                GameConstants.ScreenWidth / 2 + buttonSpacing / 2,
                GameConstants.ScreenHeight - 120,
                buttonWidth,
                buttonHeight
            );
        }

        private void SetupShopUI()
        {
            int buttonWidth = 220;
            int buttonHeight = 35;
            int shopButtonStartY = 340;
            
            for (int i = 0; i < 3; i++)
            {
                _shopButtons[i] = new Rectangle(
                    GameConstants.ScreenWidth / 2 - buttonWidth / 2,
                    shopButtonStartY + i * 42,
                    buttonWidth,
                    buttonHeight
                );
            }

            _nextLevelButton = new Rectangle(
                GameConstants.ScreenWidth / 2 - 100,
                shopButtonStartY + 3 * 42 + 20,
                200,
                50
            );
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (_whitePixel != null)
            {
                DrawGame();

                if (_gameState.IsVictory)
                {
                    DrawVictory();
                }
                else if (_scoreService.IsGameOver)
                {
                    DrawGameOver();
                }
                else if (_levelComplete)
                {
                    DrawLevelComplete();
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGame()
        {
            // UI Area
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.UIHeight), Color.DarkBlue * 0.8f);
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, GameConstants.UIHeight, GameConstants.ScreenWidth, 3), Color.White);

            // Paddle
            DrawPaddle();

            // Balls
            foreach (var ball in _balls)
            {
                if (_ballTexture != null)
                    _spriteBatch.Draw(_ballTexture, ball.Rect, Color.Silver);
            }

            // Bricks
            DrawBricks();

            // Projectiles
            foreach (var proj in _projectiles)
            {
                _spriteBatch.Draw(_whitePixel, proj.Bounds, Color.Yellow);
            }

            // Particles
            foreach (var particle in _particleSystem.Particles)
            {
                var rect = new Rectangle(
                    (int)(particle.Position.X - particle.Size / 2f),
                    (int)(particle.Position.Y - particle.Size / 2f),
                    Math.Max(1, (int)particle.Size),
                    Math.Max(1, (int)particle.Size)
                );
                _spriteBatch.Draw(_whitePixel, rect, particle.Color * particle.Alpha);
            }

            // Floating texts
            if (_font != null)
            {
                foreach (var text in _floatingTextSystem.FloatingTexts)
                {
                    Vector2 textSize = _font.MeasureString(text.Text);
                    Vector2 textPos = new Vector2(text.Position.X - textSize.X / 2, text.Position.Y);

                    // Outline
                    for (int ox = -4; ox <= 4; ox++)
                    {
                        for (int oy = -4; oy <= 4; oy++)
                        {
                            if (ox != 0 || oy != 0)
                            {
                                _spriteBatch.DrawString(_font, text.Text, textPos + new Vector2(ox, oy), Color.Black * text.Alpha);
                            }
                        }
                    }
                    _spriteBatch.DrawString(_font, text.Text, textPos, Color.Cyan * text.Alpha);
                }
            }

            // UI
            DrawUI();

            // Shoot mode indicator
            if (_canShoot && _font != null)
            {
                float textFlicker = (float)Math.Sin(_flickerTimer * 1.5f) * 0.5f + 0.5f;
                string powerUpText = "SPACE TO SHOOT";
                Vector2 textSize = _font.MeasureString(powerUpText);
                Vector2 textPos = new Vector2((GameConstants.ScreenWidth - textSize.X) / 2, GameConstants.ScreenHeight / 2 - 90);
                float alpha = 0.3f + textFlicker * 0.4f;

                for (int ox = -2; ox <= 2; ox++)
                {
                    for (int oy = -2; oy <= 2; oy++)
                    {
                        if (ox != 0 || oy != 0)
                        {
                            _spriteBatch.DrawString(_font, powerUpText, textPos + new Vector2(ox, oy), Color.Black * (alpha * 0.5f));
                        }
                    }
                }
                _spriteBatch.DrawString(_font, powerUpText, textPos, Color.Yellow * alpha);
            }
        }

        private void DrawPaddle()
        {
            if (_paddleTexture == null) return;

            var paddle = _paddle.Bounds;

            // Shadow
            _spriteBatch.Draw(_paddleTexture, new Rectangle(paddle.X, paddle.Y + 3, paddle.Width, paddle.Height), Color.Black * 0.4f);

            // Base
            _spriteBatch.Draw(_paddleTexture, paddle, new Color(40, 50, 70));

            // Inner gradient
            _spriteBatch.Draw(_paddleTexture, new Rectangle(paddle.X + 2, paddle.Y + 3, paddle.Width - 4, paddle.Height - 6), new Color(60, 75, 100));

            // Top highlight
            _spriteBatch.Draw(_whitePixel, new Rectangle(paddle.X + 8, paddle.Y + 2, paddle.Width - 16, 4), new Color(120, 140, 180));

            // Side accents
            _spriteBatch.Draw(_whitePixel, new Rectangle(paddle.X + 5, paddle.Y + 4, 2, paddle.Height - 8), new Color(80, 120, 160));
            _spriteBatch.Draw(_whitePixel, new Rectangle(paddle.X + paddle.Width - 7, paddle.Y + 4, 2, paddle.Height - 8), new Color(80, 120, 160));

            // Center stripe
            float glowPulse = (float)Math.Sin(_scoreService.GameTimer * 3) * 0.3f + 0.7f;
            _spriteBatch.Draw(_whitePixel, new Rectangle(paddle.X + paddle.Width / 2 - 1, paddle.Y + 6, 2, paddle.Height - 12), new Color(0, 150, 255) * glowPulse);

            // Cannon
            if (_cannonExtension > 0f)
            {
                int cannonWidth = 8;
                int cannonHeight = (int)(20 * _cannonExtension);
                int cannonX = paddle.X + paddle.Width / 2 - cannonWidth / 2;
                int cannonY = paddle.Y - cannonHeight;

                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX, cannonY, cannonWidth, cannonHeight), new Color(50, 55, 65));
                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX + 1, cannonY, cannonWidth - 2, cannonHeight), new Color(30, 35, 45));

                if (_cannonExtension > 0.8f)
                {
                    _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX + 2, cannonY, cannonWidth - 4, 3), Color.Orange * glowPulse);
                }

                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX, cannonY + 2, 2, cannonHeight - 4), new Color(70, 80, 100));
                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX + cannonWidth - 2, cannonY + 2, 2, cannonHeight - 4), new Color(70, 80, 100));
            }
        }

        private void DrawBricks()
        {
            for (int i = 0; i < _bricks.Count; i++)
            {
                var brick = _bricks[i];
                int brickHeight = 20;
                int row = (brick.Bounds.Y - 50) / (brickHeight + 2);
                Color brickColor = Brick.GetColorForRow(row);

                bool isSpecial = (brick.Type == BrickType.ShootPowerUp || brick.Type == BrickType.ExtraBall) && !_canShoot;

                if (isSpecial)
                {
                    float flickerAlpha = (float)Math.Sin(_flickerTimer * 2) * 0.5f + 0.5f;
                    Color goldColor = new Color(255, 215, 0);
                    Color brightColor = Color.White;
                    Color specialColor = Color.Lerp(goldColor, brightColor, flickerAlpha);

                    _spriteBatch.Draw(_whitePixel, brick.Bounds, specialColor);
                    _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, brick.Bounds.Width - 4, brick.Bounds.Height - 4), brickColor * 0.8f);
                }
                else
                {
                    _spriteBatch.Draw(_whitePixel, brick.Bounds, brickColor);
                }
            }
        }

        private void DrawUI()
        {
            if (_font == null) return;

            // Score
            string scoreStr = $"{_scoreService.Score:D8}";
            _spriteBatch.DrawString(_font, scoreStr, new Vector2(GameConstants.ScreenWidth - 120, 15), Color.White);

            // Bank balance
            string bankStr = $"${_shopService.BankBalance}";
            _spriteBatch.DrawString(_font, bankStr, new Vector2(GameConstants.ScreenWidth - 250, 15), Color.Gold);

            // Hearts
            for (int i = 0; i < _scoreService.Lives; i++)
            {
                if (_heartTexture != null)
                {
                    _spriteBatch.Draw(_heartTexture, new Rectangle(10 + i * 28, 13, 24, 24), Color.Red);
                }
            }

            // Level
            _spriteBatch.DrawString(_font, $"Level: {_gameState.CurrentLevel}/10", new Vector2(10 + _scoreService.Lives * 28 + 10, 15), Color.White);

            // Timer
            string timerText = _scoreService.GetFormattedTime();
            Vector2 timerSize = _font.MeasureString(timerText);
            _spriteBatch.DrawString(_font, timerText, new Vector2((GameConstants.ScreenWidth - timerSize.X) / 2, 15), Color.White);
        }

        private void DrawGameOver()
        {
            if (_font == null || _whitePixel == null) return;

            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.ScreenHeight), Color.Black * 0.8f);

            int yPos = GameConstants.ScreenHeight / 2 - 150;

            // Title
            string gameOverText = "GAME OVER";
            Vector2 titleSize = _font.MeasureString(gameOverText);
            Vector2 titlePos = new Vector2((GameConstants.ScreenWidth - titleSize.X) / 2, yPos);

            for (int ox = -2; ox <= 2; ox++)
            {
                for (int oy = -2; oy <= 2; oy++)
                {
                    if (ox != 0 || oy != 0)
                    {
                        _spriteBatch.DrawString(_font, gameOverText, titlePos + new Vector2(ox, oy), Color.DarkRed);
                    }
                }
            }
            _spriteBatch.DrawString(_font, gameOverText, titlePos, Color.Red);
            yPos += 50;

            // Score and Time
            string scoreText = $"Score: {_scoreService.Score}";
            string timeText = $"Time: {_scoreService.GetFormattedTime()}";
            Vector2 scoreSize = _font.MeasureString(scoreText);
            Vector2 timeSize = _font.MeasureString(timeText);
            _spriteBatch.DrawString(_font, scoreText, new Vector2((GameConstants.ScreenWidth - scoreSize.X) / 2, yPos), Color.White);
            yPos += 30;
            _spriteBatch.DrawString(_font, timeText, new Vector2((GameConstants.ScreenWidth - timeSize.X) / 2, yPos), Color.White);
            yPos += 50;

            // Statistics
            string statsTitle = "=== STATISTICS ===";
            Vector2 statsTitleSize = _font.MeasureString(statsTitle);
            _spriteBatch.DrawString(_font, statsTitle, new Vector2((GameConstants.ScreenWidth - statsTitleSize.X) / 2, yPos), Color.Cyan);
            yPos += 35;

            string earnedText = $"Total Earned: ${_shopService.TotalEarned}";
            string spentText = $"Total Spent: ${_shopService.TotalSpent}";
            string profitText = $"Profit: ${_shopService.TotalEarned - _shopService.TotalSpent}";
            
            Vector2 earnedSize = _font.MeasureString(earnedText);
            Vector2 spentSize = _font.MeasureString(spentText);
            Vector2 profitSize = _font.MeasureString(profitText);
            
            _spriteBatch.DrawString(_font, earnedText, new Vector2((GameConstants.ScreenWidth - earnedSize.X) / 2, yPos), Color.LightGreen);
            yPos += 30;
            _spriteBatch.DrawString(_font, spentText, new Vector2((GameConstants.ScreenWidth - spentSize.X) / 2, yPos), Color.LightCoral);
            yPos += 30;
            
            Color profitColor = (_shopService.TotalEarned - _shopService.TotalSpent) >= 0 ? Color.Gold : Color.Red;
            _spriteBatch.DrawString(_font, profitText, new Vector2((GameConstants.ScreenWidth - profitSize.X) / 2, yPos), profitColor);
            yPos += 50;

            // Buttons
            Color retryColor = _retryButtonHovered ? Color.LightGreen : Color.Green;
            _spriteBatch.Draw(_whitePixel, _retryButton, retryColor);
            string retryText = "RETRY";
            Vector2 retryTextSize = _font.MeasureString(retryText);
            _spriteBatch.DrawString(_font, retryText, new Vector2(_retryButton.X + (_retryButton.Width - retryTextSize.X) / 2, _retryButton.Y + (_retryButton.Height - retryTextSize.Y) / 2), Color.Black);

            Color quitColor = _quitButtonHovered ? Color.LightCoral : Color.DarkRed;
            _spriteBatch.Draw(_whitePixel, _quitButton, quitColor);
            string quitText = "QUIT";
            Vector2 quitTextSize = _font.MeasureString(quitText);
            _spriteBatch.DrawString(_font, quitText, new Vector2(_quitButton.X + (_quitButton.Width - quitTextSize.X) / 2, _quitButton.Y + (_quitButton.Height - quitTextSize.Y) / 2), Color.White);
        }

        private void DrawVictory()
        {
            if (_font == null || _whitePixel == null) return;

            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.ScreenHeight), Color.Black * 0.85f);

            int yPos = 80;

            // Main Title with glow effect
            string victoryText = "Winner, Winner";
            string chickenText = "Chicken Dinner!";
            
            float glowIntensity = (float)Math.Sin(_victoryGlowTimer) * 0.3f + 0.7f;
            Color glowColor = new Color(255, 215, 0) * glowIntensity; // Gold with pulse

            // Winner, Winner
            Vector2 victorySize = _font.MeasureString(victoryText);
            Vector2 victoryPos = new Vector2((GameConstants.ScreenWidth - victorySize.X) / 2, yPos);

            // Outer glow
            for (int r = 8; r > 0; r -= 2)
            {
                float alpha = (1f - (r / 8f)) * 0.3f * glowIntensity;
                for (int angle = 0; angle < 360; angle += 30)
                {
                    float rad = angle * (float)Math.PI / 180f;
                    Vector2 offset = new Vector2((float)Math.Cos(rad) * r, (float)Math.Sin(rad) * r);
                    _spriteBatch.DrawString(_font, victoryText, victoryPos + offset, glowColor * alpha);
                }
            }

            // Shadow
            for (int ox = -3; ox <= 3; ox++)
            {
                for (int oy = -3; oy <= 3; oy++)
                {
                    if (ox != 0 || oy != 0)
                    {
                        _spriteBatch.DrawString(_font, victoryText, victoryPos + new Vector2(ox, oy), Color.DarkGoldenrod);
                    }
                }
            }
            _spriteBatch.DrawString(_font, victoryText, victoryPos, Color.Gold);
            yPos += 60;

            // Chicken Dinner!
            Vector2 chickenSize = _font.MeasureString(chickenText);
            Vector2 chickenPos = new Vector2((GameConstants.ScreenWidth - chickenSize.X) / 2, yPos);

            // Outer glow
            for (int r = 8; r > 0; r -= 2)
            {
                float alpha = (1f - (r / 8f)) * 0.3f * glowIntensity;
                for (int angle = 0; angle < 360; angle += 30)
                {
                    float rad = angle * (float)Math.PI / 180f;
                    Vector2 offset = new Vector2((float)Math.Cos(rad) * r, (float)Math.Sin(rad) * r);
                    _spriteBatch.DrawString(_font, chickenText, chickenPos + offset, glowColor * alpha);
                }
            }

            // Shadow
            for (int ox = -3; ox <= 3; ox++)
            {
                for (int oy = -3; oy <= 3; oy++)
                {
                    if (ox != 0 || oy != 0)
                    {
                        _spriteBatch.DrawString(_font, chickenText, chickenPos + new Vector2(ox, oy), Color.DarkGoldenrod);
                    }
                }
            }
            _spriteBatch.DrawString(_font, chickenText, chickenPos, Color.Gold);
            yPos += 80;

            // Completion message
            string completionText = "All 10 Levels Completed!";
            Vector2 completionSize = _font.MeasureString(completionText);
            _spriteBatch.DrawString(_font, completionText, new Vector2((GameConstants.ScreenWidth - completionSize.X) / 2, yPos), Color.LightGreen);
            yPos += 50;

            // Score and Time
            string scoreText = $"Final Score: {_scoreService.Score}";
            string timeText = $"Total Time: {_scoreService.GetFormattedTime()}";
            Vector2 scoreSize = _font.MeasureString(scoreText);
            Vector2 timeSize = _font.MeasureString(timeText);
            _spriteBatch.DrawString(_font, scoreText, new Vector2((GameConstants.ScreenWidth - scoreSize.X) / 2, yPos), Color.White);
            yPos += 30;
            _spriteBatch.DrawString(_font, timeText, new Vector2((GameConstants.ScreenWidth - timeSize.X) / 2, yPos), Color.White);
            yPos += 50;

            // Statistics
            string statsTitle = "=== FINANCIAL REPORT ===";
            Vector2 statsTitleSize = _font.MeasureString(statsTitle);
            _spriteBatch.DrawString(_font, statsTitle, new Vector2((GameConstants.ScreenWidth - statsTitleSize.X) / 2, yPos), Color.Cyan);
            yPos += 35;

            string earnedText = $"Total Earned: ${_shopService.TotalEarned}";
            string spentText = $"Total Spent: ${_shopService.TotalSpent}";
            string profitText = $"Net Profit: ${_shopService.TotalEarned - _shopService.TotalSpent}";
            
            Vector2 earnedSize = _font.MeasureString(earnedText);
            Vector2 spentSize = _font.MeasureString(spentText);
            Vector2 profitSize = _font.MeasureString(profitText);
            
            _spriteBatch.DrawString(_font, earnedText, new Vector2((GameConstants.ScreenWidth - earnedSize.X) / 2, yPos), Color.LightGreen);
            yPos += 30;
            _spriteBatch.DrawString(_font, spentText, new Vector2((GameConstants.ScreenWidth - spentSize.X) / 2, yPos), Color.LightCoral);
            yPos += 30;
            
            Color profitColor = (_shopService.TotalEarned - _shopService.TotalSpent) >= 0 ? Color.Gold : Color.Red;
            
            // Profit with glow if positive
            if ((_shopService.TotalEarned - _shopService.TotalSpent) >= 0)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    for (int oy = -1; oy <= 1; oy++)
                    {
                        if (ox != 0 || oy != 0)
                        {
                            _spriteBatch.DrawString(_font, profitText, new Vector2((GameConstants.ScreenWidth - profitSize.X) / 2 + ox, yPos + oy), Color.DarkGoldenrod * glowIntensity);
                        }
                    }
                }
            }
            _spriteBatch.DrawString(_font, profitText, new Vector2((GameConstants.ScreenWidth - profitSize.X) / 2, yPos), profitColor);

            // Buttons
            Color retryColor = _victoryRetryButtonHovered ? Color.LightGreen : Color.Green;
            _spriteBatch.Draw(_whitePixel, _victoryRetryButton, retryColor);
            string retryText = "PLAY AGAIN";
            Vector2 retryTextSize = _font.MeasureString(retryText);
            _spriteBatch.DrawString(_font, retryText, new Vector2(_victoryRetryButton.X + (_victoryRetryButton.Width - retryTextSize.X) / 2, _victoryRetryButton.Y + (_victoryRetryButton.Height - retryTextSize.Y) / 2), Color.Black);

            Color quitColor = _victoryQuitButtonHovered ? Color.LightCoral : Color.DarkRed;
            _spriteBatch.Draw(_whitePixel, _victoryQuitButton, quitColor);
            string quitText = "QUIT";
            Vector2 quitTextSize = _font.MeasureString(quitText);
            _spriteBatch.DrawString(_font, quitText, new Vector2(_victoryQuitButton.X + (_victoryQuitButton.Width - quitTextSize.X) / 2, _victoryQuitButton.Y + (_victoryQuitButton.Height - quitTextSize.Y) / 2), Color.White);
        }

        private void DrawLevelComplete()
        {
            if (_font == null || _whitePixel == null) return;

            _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, GameConstants.ScreenWidth, GameConstants.ScreenHeight), Color.Black * 0.7f);

            const int ROW_HEIGHT = 50;
            const int START_Y = 40;
            int currentRow = 0;

            // Title
            string title = "DONE!";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2((GameConstants.ScreenWidth - titleSize.X) / 2, START_Y + currentRow * ROW_HEIGHT);

            for (int ox = -2; ox <= 2; ox++)
            {
                for (int oy = -2; oy <= 2; oy++)
                {
                    if (ox != 0 || oy != 0)
                    {
                        _spriteBatch.DrawString(_font, title, titlePos + new Vector2(ox, oy), Color.DarkGreen);
                    }
                }
            }
            _spriteBatch.DrawString(_font, title, titlePos, Color.LightGreen);
            currentRow++;

            // Time bonus calculation
            string calc = $"Time Bonus: $100 - {(int)_scoreService.GameTimer}s = ${_levelCompleteTimeBonus}";
            Vector2 calcSize = _font.MeasureString(calc);
            _spriteBatch.DrawString(_font, calc, new Vector2((GameConstants.ScreenWidth - calcSize.X) / 2, START_Y + currentRow * ROW_HEIGHT), Color.Yellow);
            currentRow++;

            // Counting animation
            if (!_moneyAnimationDone)
            {
                string counting = $"Counting... ${_animatedMoney}";
                Vector2 countingSize = _font.MeasureString(counting);
                _spriteBatch.DrawString(_font, counting, new Vector2((GameConstants.ScreenWidth - countingSize.X) / 2, START_Y + currentRow * ROW_HEIGHT), Color.Gray);
            }
            currentRow++;

            // Final balance
            if (_moneyAnimationDone)
            {
                string balance = $"${_shopService.BankBalance}";
                Vector2 balanceSize = _font.MeasureString(balance);
                Vector2 balancePos = new Vector2((GameConstants.ScreenWidth - balanceSize.X * _slamScale) / 2 + _balanceShake, START_Y + currentRow * ROW_HEIGHT + _slamY);

                float glowIntensity = (float)Math.Sin(_glowPulse) * 0.15f + 0.2f;
                for (int ox = -2; ox <= 2; ox++)
                {
                    for (int oy = -2; oy <= 2; oy++)
                    {
                        if (ox != 0 || oy != 0)
                        {
                            float distance = (float)Math.Sqrt(ox * ox + oy * oy);
                            if (distance <= 2f)
                            {
                                _spriteBatch.DrawString(_font, balance, balancePos + new Vector2(ox, oy), Color.Gold * (glowIntensity / distance), 0f, Vector2.Zero, _slamScale, SpriteEffects.None, 0f);
                            }
                        }
                    }
                }
                _spriteBatch.DrawString(_font, balance, balancePos, Color.Gold, 0f, Vector2.Zero, _slamScale, SpriteEffects.None, 0f);

                // Purchase animation
                if (_purchaseAnimationActive && _purchaseAnimationTimer < 0.5f)
                {
                    string costText = $"-${_purchaseCostAmount}";
                    Vector2 costPos = new Vector2(_purchaseCostX, _purchaseCostY);

                    float trailAlpha = 1f - (_purchaseAnimationTimer / 0.5f);
                    for (int i = 1; i <= 3; i++)
                    {
                        _spriteBatch.DrawString(_font, costText, costPos + new Vector2(-i * 15, 0), Color.Red * (trailAlpha * 0.3f));
                    }

                    for (int ox = -2; ox <= 2; ox++)
                    {
                        for (int oy = -2; oy <= 2; oy++)
                        {
                            if (ox != 0 || oy != 0)
                            {
                                _spriteBatch.DrawString(_font, costText, costPos + new Vector2(ox, oy), Color.DarkRed * 0.7f);
                            }
                        }
                    }
                    _spriteBatch.DrawString(_font, costText, costPos, Color.Red);
                }
            }
            currentRow += 2;

            // Shop
            if (_moneyAnimationDone && _slamAnimationDone)
            {
                string shopTitle = "=== SHOP ===";
                Vector2 shopTitleSize = _font.MeasureString(shopTitle);
                _spriteBatch.DrawString(_font, shopTitle, new Vector2((GameConstants.ScreenWidth - shopTitleSize.X) / 2, START_Y + currentRow * ROW_HEIGHT), Color.Cyan);

                // Shop border
                int shopBoxX = GameConstants.ScreenWidth / 2 - 130;
                int shopBoxY = START_Y + currentRow * ROW_HEIGHT - 5;
                int shopBoxWidth = 260;
                int shopBoxHeight = 205;

                _spriteBatch.Draw(_whitePixel, new Rectangle(shopBoxX, shopBoxY, shopBoxWidth, 3), Color.Cyan);
                _spriteBatch.Draw(_whitePixel, new Rectangle(shopBoxX, shopBoxY + shopBoxHeight - 3, shopBoxWidth, 3), Color.Cyan);
                _spriteBatch.Draw(_whitePixel, new Rectangle(shopBoxX, shopBoxY, 3, shopBoxHeight), Color.Cyan);
                _spriteBatch.Draw(_whitePixel, new Rectangle(shopBoxX + shopBoxWidth - 3, shopBoxY, 3, shopBoxHeight), Color.Cyan);

                // Shop buttons
                string[] shopTexts = { "+3% Speed $25", "Extra Ball $5", "Shoot 6s $15" };

                for (int i = 0; i < 3; i++)
                {
                    bool canAfford = (i == 0 && _shopService.CanAfford(ShopItem.SpeedUpgrade)) ||
                                    (i == 1 && _shopService.CanAfford(ShopItem.ExtraBall)) ||
                                    (i == 2 && _shopService.CanAfford(ShopItem.ShootMode));

                    Color buttonColor = _shopButtonsHovered[i] && canAfford ? Color.LightBlue : (canAfford ? Color.Blue : Color.DarkGray);
                    _spriteBatch.Draw(_whitePixel, _shopButtons[i], buttonColor);

                    Vector2 textSize = _font.MeasureString(shopTexts[i]);
                    Vector2 textPos = new Vector2(
                        _shopButtons[i].X + (_shopButtons[i].Width - textSize.X) / 2,
                        _shopButtons[i].Y + (_shopButtons[i].Height - textSize.Y) / 2
                    );
                    _spriteBatch.DrawString(_font, shopTexts[i], textPos, canAfford ? Color.White : Color.Gray);
                }

                // Next level button
                Color nextColor = _nextLevelButtonHovered ? Color.LightGreen : Color.Green;
                _spriteBatch.Draw(_whitePixel, _nextLevelButton, nextColor);
                string nextText = "NEXT LEVEL";
                Vector2 nextTextSize = _font.MeasureString(nextText);
                Vector2 nextTextPos = new Vector2(
                    _nextLevelButton.X + (_nextLevelButton.Width - nextTextSize.X) / 2,
                    _nextLevelButton.Y + (_nextLevelButton.Height - nextTextSize.Y) / 2
                );
                _spriteBatch.DrawString(_font, nextText, nextTextPos, Color.Black);
            }
        }
    }
}
