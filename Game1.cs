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
using AlleywayMonoGame.Managers;
using AlleywayMonoGame.UI;

namespace AlleywayMonoGame
{
    /// <summary>
    /// Main game class - refactored to use modular architecture
    /// </summary>
    public class Game1 : Game
    {
        #region Fields

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
        
        // Managers
        private BackgroundManager _backgroundManager = null!;
        private UIManager _uiManager = null!;
        private PowerUpManager _powerUpManager = null!;
        
        // UI Rendering
        private DialogRenderer? _dialogRenderer;

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

        // Shop upgrades
        private int _extraBallsPurchased;
        private bool _startWithShootMode;
        private ShopItem[] _currentShopItems = new ShopItem[3];

        #endregion

        #region Initialization

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
            
            // Initialize managers
            _backgroundManager = new BackgroundManager();
            _uiManager = new UIManager();

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

            // Initialize background
            _backgroundManager.Initialize();

            // Generate first level
            GenerateLevel();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize audio service
            _audioService = new AudioService();
            
            // Initialize PowerUpManager after AudioService is ready
            _powerUpManager = new PowerUpManager(_paddle, _projectiles, _audioService);

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
                
                // Initialize DialogRenderer after font is loaded
                if (_font != null && _whitePixel != null)
                {
                    _dialogRenderer = new DialogRenderer(_spriteBatch, _font, _whitePixel);
                }
            }
            catch
            {
                _font = null;
            }
        }

        #endregion

        #region Update Methods

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

            if (_uiManager.LevelComplete)
            {
                UpdateLevelComplete(dt);
                return;
            }

            // Normal gameplay
            UpdateGameplay(dt);

            base.Update(gameTime);
        }

        #endregion

        #region Game State Updates

        private void UpdateVictory(float dt)
        {
            _uiManager.VictoryGlowTimer += dt * 2f;

            var mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            _uiManager.VictoryRetryButtonHovered = _uiManager.VictoryRetryButton.Contains(mousePos);
            _uiManager.VictoryQuitButtonHovered = _uiManager.VictoryQuitButton.Contains(mousePos);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_uiManager.VictoryRetryButtonHovered)
                {
                    RestartGame();
                }
                else if (_uiManager.VictoryQuitButtonHovered)
                {
                    Exit();
                }
            }
        }

        private void UpdateGameOver(float dt)
        {
            var mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            _uiManager.RetryButtonHovered = _uiManager.RetryButton.Contains(mousePos);
            _uiManager.QuitButtonHovered = _uiManager.QuitButton.Contains(mousePos);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_uiManager.RetryButtonHovered)
                {
                    RestartGame();
                }
                else if (_uiManager.QuitButtonHovered)
                {
                    Exit();
                }
            }
        }

        private void UpdateLevelComplete(float dt)
        {
            _uiManager.AnimationTimer += dt;

            // Money counting animation
            if (!_uiManager.MoneyAnimationDone && _uiManager.AnimationTimer > 1f)
            {
                // Play charge up sound once when counting starts
                if (!_uiManager.ChargeUpSoundPlayed)
                {
                    _audioService.PlayChargeUp();
                    _uiManager.ChargeUpSoundPlayed = true;
                }
                
                float animSpeed = dt * 200f;
                _uiManager.AnimatedMoney += (int)animSpeed;
                if (_uiManager.AnimatedMoney >= _uiManager.LevelCompleteTimeBonus)
                {
                    _uiManager.AnimatedMoney = _uiManager.LevelCompleteTimeBonus;
                    _uiManager.MoneyAnimationDone = true;
                    _shopService.AddMoney(_uiManager.LevelCompleteTimeBonus);
                    
                    _uiManager.SlamY = -100f;
                    _uiManager.SlamVelocity = 0f;
                    _uiManager.SlamScale = 1f;
                    _uiManager.SlamAnimationDone = false;
                }
            }

            // Slam animation
            if (_uiManager.MoneyAnimationDone && !_uiManager.SlamAnimationDone)
            {
                UpdateSlamAnimation(dt);
            }

            _uiManager.GlowPulse += dt * 3f;

            // Purchase animation
            if (_uiManager.PurchaseAnimationActive)
            {
                UpdatePurchaseAnimation(dt);
            }

            // Shop interaction
            UpdateShopInteraction(dt);
        }

        private void UpdateSlamAnimation(float dt)
        {
            float gravity = 1200f * dt;
            _uiManager.SlamVelocity += gravity;
            _uiManager.SlamY += _uiManager.SlamVelocity;

            float targetY = 0f;
            if (_uiManager.SlamY >= targetY)
            {
                _uiManager.SlamY = targetY;
                float prevVelocity = _uiManager.SlamVelocity;
                _uiManager.SlamVelocity *= -0.4f;
                _uiManager.SlamScale = 1.2f;

                if (prevVelocity > 0f && Math.Abs(prevVelocity) > 50f)
                {
                    Vector2 impactPos = new Vector2(GameConstants.ScreenWidth / 2, 40 + 3 * 50 + 30);
                    _particleSystem.SpawnDustCloud(impactPos, 20);
                }

                if (Math.Abs(_uiManager.SlamVelocity) < 30f)
                {
                    _uiManager.SlamVelocity = 0f;
                    _uiManager.SlamAnimationDone = true;
                }
            }

            if (_uiManager.SlamScale > 1f)
            {
                _uiManager.SlamScale -= dt * 1.5f;
                if (_uiManager.SlamScale < 1f) _uiManager.SlamScale = 1f;
            }
        }

        private void UpdatePurchaseAnimation(float dt)
        {
            _uiManager.PurchaseAnimationTimer += dt;

            if (_uiManager.PurchaseAnimationTimer < 0.5f)
            {
                float progress = _uiManager.PurchaseAnimationTimer / 0.5f;
                float eased = 1f - (float)Math.Pow(1f - progress, 3);
                _uiManager.PurchaseCostX += (GameConstants.ScreenWidth / 2 - _uiManager.PurchaseCostX) * eased * dt * 8f;
            }
            else if (_uiManager.PurchaseAnimationTimer < 0.8f)
            {
                if (_uiManager.PurchaseAnimationTimer >= 0.5f && _uiManager.PurchaseAnimationTimer - dt < 0.5f)
                {
                    Vector2 impactPos = new Vector2(GameConstants.ScreenWidth / 2, 40 + 3 * 50 + 10);
                    _particleSystem.SpawnExplosion(impactPos, 20, Color.Gold);
                }

                float shakeIntensity = 10f * (1f - (_uiManager.PurchaseAnimationTimer - 0.5f) / 0.3f);
                _uiManager.BalanceShake = ((float)new Random().NextDouble() - 0.5f) * shakeIntensity * 2f;
            }
            else
            {
                _uiManager.PurchaseAnimationActive = false;
                _uiManager.BalanceShake = 0f;
            }
        }

        private void UpdateShopInteraction(float dt)
        {
            var mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            for (int i = 0; i < 3; i++)
            {
                _uiManager.ShopButtonsHovered[i] = _uiManager.ShopButtons[i].Contains(mousePos);
            }
            _uiManager.NextLevelButtonHovered = _uiManager.NextLevelButton.Contains(mousePos);

            if (mouseState.LeftButton == ButtonState.Pressed && !_uiManager.PurchaseAnimationActive && _uiManager.MoneyAnimationDone)
            {
                // Check each shop button
                for (int i = 0; i < 3; i++)
                {
                    if (_uiManager.ShopButtonsHovered[i] && _shopService.CanAfford(_currentShopItems[i]))
                    {
                        StartPurchaseAnimation(_currentShopItems[i], i);
                        _shopService.Purchase(_currentShopItems[i]);
                        
                        // Apply the purchase
                        switch (_currentShopItems[i])
                        {
                            case ShopItem.SpeedUpgrade:
                                _paddle.SpeedMultiplier = _shopService.PaddleSpeedMultiplier;
                                break;
                            case ShopItem.ExtraBall:
                                _extraBallsPurchased++;
                                break;
                            case ShopItem.ShootMode:
                                _startWithShootMode = true;
                                break;
                            case ShopItem.PaddleSize:
                                // Apply permanent size increase
                                _paddle.ApplyPermanentSizeIncrease(_shopService.PaddleSizeMultiplier);
                                break;
                        }
                        
                        _audioService.PlayCashRegister();
                        break;
                    }
                }
                
                if (_uiManager.NextLevelButtonHovered)
                {
                    AdvanceToNextLevel();
                }
            }
        }

        private void StartPurchaseAnimation(ShopItem item, int buttonIndex)
        {
            _uiManager.PurchaseAnimationActive = true;
            _uiManager.PurchaseCostAmount = _shopService.GetCost(item);
            _uiManager.PurchaseCostX = _uiManager.ShopButtons[buttonIndex].X - 150;
            _uiManager.PurchaseCostY = 40 + 3 * 50;
            _uiManager.PurchaseAnimationTimer = 0f;
        }

        #endregion

        #region Gameplay Logic

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

            // Update background
            _backgroundManager.Update(dt);

            // Update flicker for special bricks
            _powerUpManager.FlickerTimer += dt * 10f;

            // Update shoot power-up
            UpdateShootPowerUp(dt, kb);

            // Update big paddle power-up
            UpdateBigPaddle(dt);

            // Update projectiles
            UpdateProjectiles(dt);

            // Update paddle
            UpdatePaddle(dt, kb);

            // Update balls
            UpdateBalls(dt, kb);

            // Check projectile collisions
            CheckProjectileCollisions();

            // Check for level complete
            if (_bricks.Count == 0 && !_uiManager.LevelComplete)
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
            _powerUpManager.UpdateShootMode(dt);

            if (_powerUpManager.CanShoot)
            {
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
        }

        private void UpdateBigPaddle(float dt)
        {
            _powerUpManager.UpdateBigPaddle(dt);
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
            bool wasSpecialBrick = brick.Type == BrickType.Special;

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

            // Special brick: randomly activate one of three power-ups (only if no power-up active)
            if (wasSpecialBrick && !_powerUpManager.CanShoot && !_powerUpManager.BigPaddleActive)
            {
                var random = new Random();
                int powerUpType = random.Next(3); // 0, 1, or 2
                
                switch (powerUpType)
                {
                    case 0: // Shoot mode
                        _powerUpManager.ActivateShootMode();
                        _floatingTextSystem.AddText("SHOOT MODE!", brick.Center, Color.Yellow, 3f);
                        break;
                        
                    case 1: // Extra ball
                        SpawnExtraBall(brick.Center);
                        _floatingTextSystem.AddText("+BALL", brick.Center, Color.White, 3f);
                        break;
                        
                    case 2: // Big paddle
                        _powerUpManager.ActivateBigPaddle();
                        _floatingTextSystem.AddText("BIG PADDLE!", brick.Center, Color.Cyan, 3f);
                        break;
                }
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

        #endregion

        #region Game Flow & Level Management

        private void OnAllBallsLost()
        {
            _powerUpManager.CanShoot = false;
            _powerUpManager.ShootPowerTimer = 0f;
            _projectiles.Clear();

            _scoreService.LoseLife();

            if (_scoreService.IsGameOver)
            {
                SetupGameOverUI();
                _audioService.PlayGameOver();
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
                _audioService.PlayVictory();
                return;
            }

            _uiManager.LevelComplete = true;
            _scoreService.StopTimer();
            _uiManager.AnimationTimer = 0f;
            _uiManager.MoneyAnimationDone = false;
            _uiManager.AnimatedMoney = 0;
            _uiManager.ChargeUpSoundPlayed = false;

            _uiManager.LevelCompleteTimeBonus = _shopService.CalculateTimeBonus(_scoreService.GameTimer);

            SetupShopUI();
            _audioService.PlayLevelComplete();

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
            _uiManager.LevelComplete = false;
            _scoreService.ResetTimer();
            
            // Clear all active game elements
            _projectiles.Clear();
            _particleSystem.Clear();
            _floatingTextSystem.Clear();
            _powerUpManager.CanShoot = false;
            _powerUpManager.ShootPowerTimer = 0f;
            _powerUpManager.CannonExtension = 0f;
            _powerUpManager.BigPaddleActive = false;
            _powerUpManager.BigPaddleTimer = 0f;
            
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
            _paddle.ApplyPermanentSizeIncrease(_shopService.PaddleSizeMultiplier);

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
                _powerUpManager.ActivateShootMode(startWithShootMode: true);
                _startWithShootMode = false;
            }

            _scoreService.StartTimer();
        }

        private void RestartGame()
        {
            _scoreService.Reset();
            _shopService = new ShopService();
            _gameState.Reset();
            _uiManager.ResetLevelComplete();
            _powerUpManager.ResetAll();
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

        #endregion

        #region UI Setup Methods

        private void SetupGameOverUI()
        {
            // Grid-basiertes Layout: Buttons ganz unten positionieren
            int buttonWidth = 150;
            int buttonHeight = 50;
            int buttonSpacing = 20;
            int buttonY = GameConstants.ScreenHeight - 80; // Fester Abstand vom unteren Rand
            
            _uiManager.RetryButton = new Rectangle(
                GameConstants.ScreenWidth / 2 - buttonWidth - buttonSpacing / 2,
                buttonY,
                buttonWidth,
                buttonHeight
            );
            _uiManager.QuitButton = new Rectangle(
                GameConstants.ScreenWidth / 2 + buttonSpacing / 2,
                buttonY,
                buttonWidth,
                buttonHeight
            );
        }

        private void SetupVictoryUI()
        {
            int buttonWidth = 150;
            int buttonHeight = 50;
            int buttonSpacing = 20;
            _uiManager.VictoryRetryButton = new Rectangle(
                GameConstants.ScreenWidth / 2 - buttonWidth - buttonSpacing / 2,
                GameConstants.ScreenHeight - 120,
                buttonWidth,
                buttonHeight
            );
            _uiManager.VictoryQuitButton = new Rectangle(
                GameConstants.ScreenWidth / 2 + buttonSpacing / 2,
                GameConstants.ScreenHeight - 120,
                buttonWidth,
                buttonHeight
            );
        }

        private void SetupShopUI()
        {
            // Generate random shop items
            _currentShopItems = _shopService.GetRandomShopItems(3);
            
            // Shop Box dimensions (must match DrawLevelComplete)
            int shopBoxWidth = 420;
            int shopBoxX = (GameConstants.ScreenWidth - shopBoxWidth) / 2;
            int shopContentY = 395; // yPos from DrawLevelComplete (335) + 15 offset + box position adjustments
            
            int buttonWidth = 380;
            int buttonHeight = 35;
            int buttonX = shopBoxX + (shopBoxWidth - buttonWidth) / 2;
            int buttonSpacing = 10;
            
            for (int i = 0; i < 3; i++)
            {
                _uiManager.ShopButtons[i] = new Rectangle(
                    buttonX,
                    shopContentY + i * (buttonHeight + buttonSpacing),
                    buttonWidth,
                    buttonHeight
                );
            }

            _uiManager.NextLevelButton = new Rectangle(
                GameConstants.ScreenWidth / 2 - 100,
                shopContentY + 3 * (buttonHeight + buttonSpacing) + 20,
                200,
                50
            );
        }

        #endregion

        #region Drawing Methods

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            if (_whitePixel != null)
            {
                _backgroundManager.Draw(_spriteBatch, _whitePixel);
                DrawGame();

                if (_gameState.IsVictory)
                {
                    DrawVictory();
                }
                else if (_scoreService.IsGameOver)
                {
                    DrawGameOver();
                }
                else if (_uiManager.LevelComplete)
                {
                    DrawLevelComplete();
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGame()
        {
            // UI Area - Pixel Art style with darker space theme
            // Dark gradient background
            for (int i = 0; i < 5; i++)
            {
                float alpha = 0.8f - (i * 0.1f);
                _spriteBatch.Draw(_whitePixel, new Rectangle(0, i * 8, GameConstants.ScreenWidth, 8), new Color(5, 5, 25) * alpha);
            }
            
            // Pixel stars in UI
            for (int x = 0; x < GameConstants.ScreenWidth; x += 40)
            {
                for (int y = 5; y < GameConstants.UIHeight - 5; y += 15)
                {
                    if ((x + y) % 80 == 0)
                    {
                        _spriteBatch.Draw(_whitePixel, new Rectangle(x, y, 1, 1), Color.White * 0.3f);
                    }
                }
            }
            
            // Border - Pixel Art style
            // Top border
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, GameConstants.UIHeight, GameConstants.ScreenWidth, 2), new Color(100, 150, 255));
            // Bottom glow
            _spriteBatch.Draw(_whitePixel, new Rectangle(0, GameConstants.UIHeight + 2, GameConstants.ScreenWidth, 1), new Color(150, 200, 255) * 0.5f);

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
            if (_powerUpManager.CanShoot && _font != null)
            {
                float textFlicker = (float)Math.Sin(_powerUpManager.FlickerTimer * 1.5f) * 0.5f + 0.5f;
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

        private void DrawPixelBox(int x, int y, int width, int height, Color color, int thickness)
        {
            // Top
            _spriteBatch.Draw(_whitePixel, new Rectangle(x, y, width, thickness), color);
            // Bottom
            _spriteBatch.Draw(_whitePixel, new Rectangle(x, y + height - thickness, width, thickness), color);
            // Left
            _spriteBatch.Draw(_whitePixel, new Rectangle(x, y, thickness, height), color);
            // Right
            _spriteBatch.Draw(_whitePixel, new Rectangle(x + width - thickness, y, thickness, height), color);
        }

        private void DrawPixelButton(Rectangle button, bool isHovered, string text, Color normalColor, Color darkColor)
        {
            if (_font == null) return;
            
            Color buttonColor = isHovered ? Color.Lerp(normalColor, Color.White, 0.3f) : normalColor;
            
            // Main button body
            _spriteBatch.Draw(_whitePixel, button, buttonColor);
            
            // Pixel-Art 3D effect
            // Top highlight
            _spriteBatch.Draw(_whitePixel, new Rectangle(button.X + 3, button.Y + 3, button.Width - 6, 2), Color.Lerp(buttonColor, Color.White, 0.5f));
            // Left highlight
            _spriteBatch.Draw(_whitePixel, new Rectangle(button.X + 3, button.Y + 3, 2, button.Height - 6), Color.Lerp(buttonColor, Color.White, 0.5f));
            // Bottom shadow
            _spriteBatch.Draw(_whitePixel, new Rectangle(button.X + 3, button.Bottom - 5, button.Width - 6, 2), darkColor);
            // Right shadow
            _spriteBatch.Draw(_whitePixel, new Rectangle(button.Right - 5, button.Y + 3, 2, button.Height - 6), darkColor);
            
            // Border
            DrawPixelBox(button.X, button.Y, button.Width, button.Height, Color.Black, 2);
            
            // Text
            Vector2 textSize = _font.MeasureString(text);
            Vector2 textPos = new Vector2(button.X + (button.Width - textSize.X) / 2, button.Y + (button.Height - textSize.Y) / 2);
            _spriteBatch.DrawString(_font, text, textPos + new Vector2(1, 1), Color.Black * 0.5f);
            _spriteBatch.DrawString(_font, text, textPos, Color.White);
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
            if (_powerUpManager.CannonExtension > 0f)
            {
                int cannonWidth = 8;
                int cannonHeight = (int)(20 * _powerUpManager.CannonExtension);
                int cannonX = paddle.X + paddle.Width / 2 - cannonWidth / 2;
                int cannonY = paddle.Y - cannonHeight;

                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX, cannonY, cannonWidth, cannonHeight), new Color(50, 55, 65));
                _spriteBatch.Draw(_whitePixel, new Rectangle(cannonX + 1, cannonY, cannonWidth - 2, cannonHeight), new Color(30, 35, 45));

                if (_powerUpManager.CannonExtension > 0.8f)
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

                bool isSpecial = brick.Type == BrickType.Special && !_powerUpManager.CanShoot && !_powerUpManager.BigPaddleActive;

                if (isSpecial)
                {
                    // Animated special block - Pixel Art style
                    float flickerAlpha = (float)Math.Sin(_powerUpManager.FlickerTimer * 2) * 0.5f + 0.5f;
                    Color goldColor = new Color(255, 215, 0);
                    Color brightColor = Color.White;
                    Color specialColor = Color.Lerp(goldColor, brightColor, flickerAlpha);

                    // Main body
                    _spriteBatch.Draw(_whitePixel, brick.Bounds, specialColor);
                    
                    // Inner area
                    _spriteBatch.Draw(_whitePixel, 
                        new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, brick.Bounds.Width - 4, brick.Bounds.Height - 4), 
                        brickColor * 0.9f);
                    
                    // Sparkle effect - pixel dots
                    int sparkleOffset = (int)(_powerUpManager.FlickerTimer * 10) % 4;
                    for (int sx = 0; sx < brick.Bounds.Width; sx += 8)
                    {
                        for (int sy = 0; sy < brick.Bounds.Height; sy += 6)
                        {
                            if ((sx + sy + sparkleOffset) % 12 == 0)
                            {
                                _spriteBatch.Draw(_whitePixel, 
                                    new Rectangle(brick.Bounds.X + sx, brick.Bounds.Y + sy, 1, 1), 
                                    Color.White * flickerAlpha);
                            }
                        }
                    }
                }
                else
                {
                    // Normal block - Pixel Art 3D style
                    // Main body
                    _spriteBatch.Draw(_whitePixel, brick.Bounds, brickColor);
                    
                    // Top highlight (lighter)
                    Color highlightColor = Color.Lerp(brickColor, Color.White, 0.4f);
                    _spriteBatch.Draw(_whitePixel, 
                        new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, brick.Bounds.Width - 4, 2), 
                        highlightColor);
                    
                    // Left highlight
                    _spriteBatch.Draw(_whitePixel, 
                        new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, 2, brick.Bounds.Height - 4), 
                        highlightColor);
                    
                    // Bottom shadow (darker)
                    Color shadowColor = brickColor * 0.5f;
                    _spriteBatch.Draw(_whitePixel, 
                        new Rectangle(brick.Bounds.X + 2, brick.Bounds.Bottom - 4, brick.Bounds.Width - 4, 2), 
                        shadowColor);
                    
                    // Right shadow
                    _spriteBatch.Draw(_whitePixel, 
                        new Rectangle(brick.Bounds.Right - 4, brick.Bounds.Y + 2, 2, brick.Bounds.Height - 4), 
                        shadowColor);
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
            if (_dialogRenderer == null) return;
            _dialogRenderer.DrawGameOver(_uiManager, _scoreService, _shopService, DrawPixelBox, DrawPixelButton);
        }

        private void DrawVictory()
        {
            if (_dialogRenderer == null) return;
            _dialogRenderer.DrawVictory(_uiManager, _scoreService, _shopService, DrawPixelBox, DrawPixelButton);
        }

        private void DrawLevelComplete()
        {
            if (_dialogRenderer == null) return;
            _dialogRenderer.DrawLevelComplete(_uiManager, _scoreService, _shopService, _currentShopItems, DrawPixelBox, DrawPixelButton);
        }

        #endregion
    }
}
