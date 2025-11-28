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
using AlleywayMonoGame.Input;
using AlleywayMonoGame.Controllers;

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
        private UFOManager _ufoManager = null!;
        private DrawManager _drawManager = null!;
        private GameplayManager _gameplayManager = null!;
        
        // Controllers
        private InputHandler _inputHandler = null!;
        private GameFlowController _gameFlowController = null!;
        private CollisionHandler _collisionHandler = null!;
        
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
            
            // Initialize input handler
            _inputHandler = new InputHandler();

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
            
            // Initialize UFOManager
            _ufoManager = new UFOManager(GameConstants.ScreenWidth, _audioService);
            
            // Initialize controllers after all services are ready
            _gameFlowController = new GameFlowController(
                _scoreService,
                _shopService,
                _gameState,
                _levelSystem,
                _uiManager,
                _powerUpManager,
                _particleSystem,
                _floatingTextSystem,
                _audioService
            );
            
            _collisionHandler = new CollisionHandler(
                _scoreService,
                _particleSystem,
                _floatingTextSystem,
                _audioService
            );

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

            // Initialize DrawManager
            if (_whitePixel != null)
            {
                _drawManager = new DrawManager(
                    _spriteBatch,
                    _whitePixel,
                    _ballTexture,
                    _paddleTexture,
                    _heartTexture,
                    _font,
                    _scoreService,
                    _shopService,
                    _powerUpManager,
                    _particleSystem,
                    _floatingTextSystem,
                    _dialogRenderer
                );
            }

            // Initialize GameplayManager
            _gameplayManager = new GameplayManager(
                _balls,
                _bricks,
                _projectiles,
                _paddle,
                _collisionSystem,
                _particleSystem,
                _floatingTextSystem,
                _audioService,
                _powerUpManager,
                _scoreService
            );
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

            var dialogInput = _inputHandler.HandleDialogInput(_uiManager);
            if (dialogInput.RetryClicked)
            {
                var restartResult = _gameFlowController.RestartGame();
                ApplyRestartResult(restartResult);
            }
            else if (dialogInput.QuitClicked)
            {
                Exit();
            }
        }

        private void UpdateGameOver(float dt)
        {
            var dialogInput = _inputHandler.HandleDialogInput(_uiManager);
            if (dialogInput.RetryClicked)
            {
                var restartResult = _gameFlowController.RestartGame();
                ApplyRestartResult(restartResult);
            }
            else if (dialogInput.QuitClicked)
            {
                Exit();
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
                    
                    // Don't start slam animation yet - wait a frame
                }
            }

            // Start slam animation after money counting is complete
            if (_uiManager.MoneyAnimationDone && !_uiManager.SlamAnimationDone)
            {
                // Initialize slam on first frame after money animation
                if (_uiManager.SlamY == 0 && _uiManager.SlamVelocity == 0 && _uiManager.SlamScale == 1f)
                {
                    _uiManager.SlamY = -100f;
                    _uiManager.SlamVelocity = 0f;
                }
                
                _uiManager.UpdateSlamAnimation(dt);
                
                // Visual effects for slam animation - check if just landed
                if (_uiManager.SlamY == 0 && Math.Abs(_uiManager.SlamVelocity) > 50f)
                {
                    // Calculate position based on dialog layout
                    var layout = new DialogLayout.LevelCompleteLayout();
                    Vector2 impactPos = new Vector2(GameConstants.ScreenWidth / 2, layout.BalanceY + 15);
                    _particleSystem.SpawnDustCloud(impactPos, 25);
                    _audioService.PlayPaddleHit(); // Impact sound
                }
            }

            _uiManager.GlowPulse += dt * 3f;

            // Purchase animation
            if (_uiManager.PurchaseAnimationActive)
            {
                _uiManager.UpdatePurchaseAnimation(dt);
                
                // Visual effects for purchase animation  
                float prevTimer = _uiManager.PurchaseAnimationTimer - dt;
                if (_uiManager.PurchaseAnimationTimer >= 0.5f && prevTimer < 0.5f)
                {
                    Vector2 impactPos = new Vector2(GameConstants.ScreenWidth / 2, 40 + 3 * 50 + 10);
                    _particleSystem.SpawnExplosion(impactPos, 20, Color.Gold);
                }
            }

            // Shop interaction using InputHandler
            var shopInput = _inputHandler.HandleShopInput(_uiManager, _uiManager.PurchaseAnimationActive, _uiManager.MoneyAnimationDone);
            if (shopInput.ShopButtonClicked)
            {
                var purchaseResult = _gameFlowController.ProcessPurchase(
                    _currentShopItems[shopInput.ShopItemClicked],
                    _paddle,
                    ref _extraBallsPurchased,
                    ref _startWithShootMode,
                    shopInput.ShopItemClicked
                );
                
                if (purchaseResult.Success)
                {
                    _uiManager.StartPurchaseAnimation(
                        purchaseResult.AnimationX,
                        purchaseResult.AnimationY,
                        purchaseResult.Cost
                    );
                }
            }
            
            if (shopInput.RerollClicked)
            {
                if (_shopService.Reroll())
                {
                    _currentShopItems = _shopService.GetRandomShopItems(3);
                    _audioService.PlayPowerUp(); // Reroll sound effect
                }
            }
            
            if (shopInput.NextLevelClicked)
            {
                var levelResult = _gameFlowController.AdvanceToNextLevel(
                    _balls,
                    _projectiles,
                    ref _extraBallsPurchased,
                    ref _startWithShootMode,
                    SetupVictoryUI
                );
                
                if (levelResult.IsVictory)
                {
                    _audioService.PlayVictory();
                }
                else
                {
                    _bricks = levelResult.NewBricks;
                    _paddle = levelResult.NewPaddle;
                    
                    // Reset UFO when starting new level
                    _ufoManager.Reset();
                    
                    // Recreate PowerUpManager with new paddle reference
                    _powerUpManager = new PowerUpManager(_paddle, _projectiles, _audioService);
                }
            }
        }

        #endregion

        #region Gameplay Logic

        private void UpdateGameplay(float dt)
        {
            // Cheat codes using InputHandler
            var (winLevel, winAll) = _inputHandler.CheckCheatCodes();
            if (winLevel)
            {
                _bricks.Clear();
            }
            if (winAll)
            {
                _gameState.CurrentLevel = 10;
                _bricks.Clear();
            }

            // Test UFO spawn with U key
            if (Keyboard.GetState().IsKeyDown(Keys.U))
            {
                _ufoManager.ForceSpawn();
            }

            // Update timer
            _scoreService.UpdateTimer(dt);

            // Update background
            _backgroundManager.Update(dt);

            // Update flicker for special bricks
            _powerUpManager.FlickerTimer += dt * 10f;

            // Update UFO
            _ufoManager.Update(dt);

            // Update shoot power-up
            UpdateShootPowerUp(dt);

            // Update big paddle power-up
            UpdateBigPaddle(dt);

            // Update projectiles
            UpdateProjectiles(dt);

            // Update paddle
            UpdatePaddle(dt);

            // Update balls
            UpdateBalls(dt);

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
            
            // Update input states at the end of frame
            _inputHandler.UpdatePreviousStates();
        }

        private void UpdateShootPowerUp(float dt)
        {
            _powerUpManager.UpdateShootMode(dt);

            if (_powerUpManager.CanShoot)
            {
                // Shooting using InputHandler
                if (_inputHandler.CheckShootInput())
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

        private void UpdatePaddle(float dt)
        {
            _inputHandler.HandlePaddleInput(_paddle, dt);
            _paddle.UpdateAnimation(dt);
        }

        private void UpdateBalls(float dt)
        {
            bool checkLaunch = _inputHandler.CheckBallLaunchInput();
            
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

                    if (checkLaunch)
                    {
                        ball.IsLaunched = true;
                        _scoreService.StartTimer();
                        
                        // Activate shoot mode if purchased (only on first ball launch)
                        if (_startWithShootMode)
                        {
                            _powerUpManager.ActivateShootMode(startWithShootMode: true);
                            _startWithShootMode = false;
                        }
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

            // Check UFO collisions
            foreach (var ball in _balls)
            {
                if (_ufoManager.CheckCollision(ball, _bricks, out Vector2 ufoPos, out Vector2 targetPos))
                {
                    // UFO was hit - explosion effect
                    _particleSystem.SpawnExplosion(ufoPos, 30, Color.Orange);
                    _particleSystem.SpawnExplosion(ufoPos, 20, Color.Red);
                    _audioService.PlayExplosion();
                    _floatingTextSystem.AddText("STEEL BLOCK!", ufoPos, Color.Gray, 3f);
                    
                    // Lightning bolt from UFO to target brick
                    if (targetPos != Vector2.Zero)
                    {
                        SpawnLightningBolt(ufoPos, targetPos);
                    }
                    
                    break; // Only one ball can hit per frame
                }
            }

            // Handle wall bounces
            if (collisionResult.WallBounce)
            {
                _audioService.PlayWallBounce();
            }

            // Handle paddle bounces
            if (collisionResult.PaddleBounce)
            {
                _audioService.PlayPaddleBounce();
            }

            // Handle brick hits using CollisionHandler
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
                else if (_balls.Count == 1 && _powerUpManager.MultiBallChaosActive)
                {
                    // Deactivate multi-ball chaos when only 1 ball remains
                    _powerUpManager.DeactivateMultiBallChaos();
                    _floatingTextSystem.AddText("CHAOS ENDED", _balls[0].Center, Color.Gray, 2f);
                }
            }
        }

        private void HandleBrickDestruction(int index, Brick brick, bool fromProjectile = false)
        {
            // Steel bricks need special handling
            if (brick.IsSteel)
            {
                bool destroyed = brick.HitSteel();
                
                // Spawn crack particles
                _particleSystem.SpawnExplosion(brick.Center, 5, Color.Gray);
                _audioService.PlayWallBounce(); // Metallic sound
                
                if (destroyed)
                {
                    // Steel brick finally destroyed
                    _bricks.RemoveAt(index);
                    _scoreService.AddBrickScore();
                    _particleSystem.SpawnExplosion(brick.Center, 20, Color.DarkGray);
                    _particleSystem.SpawnExplosion(brick.Center, 15, Color.Silver);
                    _audioService.PlayExplosion();
                    _floatingTextSystem.AddText("STEEL DESTROYED!", brick.Center, Color.Silver, 2f);
                }
                else
                {
                    // Show remaining hits
                    _floatingTextSystem.AddText($"{brick.SteelHitsRemaining} LEFT", brick.Center, Color.Gray, 1f);
                }
                
                return; // Don't process as normal brick
            }
            
            // Delegate to CollisionHandler
            var result = _collisionHandler.HandleBrickDestruction(brick, fromProjectile);
            
            _bricks.RemoveAt(index);
            _scoreService.AddBrickScore();

            // Special brick: randomly activate one of four power-ups (blocked during shoot mode or multi-ball chaos)
            if (result.WasSpecialBrick && !_powerUpManager.CanShoot && !_powerUpManager.MultiBallChaosActive)
            {
                var random = new Random();
                int powerUpType = random.Next(10); // 0-9 for 10% Multi-Ball Chaos chance
                
                if (powerUpType == 9) // 10% chance for Multi-Ball Chaos
                {
                    SpawnMultiBallChaos(brick.Center);
                    _powerUpManager.ActivateMultiBallChaos();
                    _floatingTextSystem.AddText("MULTI-BALL CHAOS!", brick.Center, Color.Magenta, 4f);
                    _audioService.PlayPowerUp();
                    
                    // Massive particle explosion
                    _particleSystem.SpawnExplosion(brick.Center, 40, Color.Magenta);
                    _particleSystem.SpawnExplosion(brick.Center, 30, Color.Yellow);
                    _particleSystem.SpawnExplosion(brick.Center, 30, Color.Cyan);
                    
                    // Play explosion sound
                    _audioService.PlayExplosion();
                }
                else
                {
                    // 90% chance for regular power-ups
                    int regularPowerUp = random.Next(3); // 0, 1, or 2
                    
                    switch (regularPowerUp)
                    {
                        case 0: // Shoot mode
                            _powerUpManager.ActivateShootMode();
                            _floatingTextSystem.AddText("SHOOT MODE!", brick.Center, Color.Yellow, 3f);
                            break;
                            
                        case 1: // Extra ball
                            SpawnExtraBall(brick.Center);
                            _floatingTextSystem.AddText("+BALL", brick.Center, Color.White, 3f);
                            break;
                            
                        case 2: // Big paddle (refresh timer if already active)
                            _powerUpManager.ActivateBigPaddle();
                            _floatingTextSystem.AddText("BIG PADDLE!", brick.Center, Color.Cyan, 3f);
                            break;
                    }
                }
            }
        }

        private void CheckProjectileCollisions()
        {
            // Delegate to CollisionHandler
            var bricksToRemove = _collisionHandler.CheckProjectileCollisions(_projectiles, _bricks);
            
            foreach (int brickIndex in bricksToRemove)
            {
                HandleBrickDestruction(brickIndex, _bricks[brickIndex], fromProjectile: true);
            }
        }

        #endregion

        #region Game Flow & Level Management

        private void ApplyRestartResult(GameRestartResult result)
        {
            _bricks = result.NewBricks;
            _paddle = result.NewPaddle;
            _shopService = result.NewShopService;
            _balls.Clear();
            _balls.Add(result.InitialBall);
            
            // Recreate PowerUpManager with new paddle reference
            _powerUpManager = new PowerUpManager(_paddle, _projectiles, _audioService);
        }

        private void OnAllBallsLost()
        {
            _gameFlowController.OnAllBallsLost(_balls, _paddle, SetupGameOverUI);
            
            if (_scoreService.IsGameOver)
            {
                _audioService.PlayGameOver();
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

            _gameFlowController.OnLevelComplete(SetupShopUI);
            _audioService.PlayLevelComplete();

            // Stop all balls
            foreach (var ball in _balls)
            {
                ball.Velocity = Vector2.Zero;
                ball.IsLaunched = false;
            }
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
            
            // Reset UFO when ball resets (only if manager is initialized)
            _ufoManager?.Reset();
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

        private void SpawnMultiBallChaos(Vector2 position)
        {
            var random = new Random();
            int ballCount = random.Next(4, 7); // 4-6 balls (increased)
            
            // Spawn multiple balls with different angles
            for (int i = 0; i < ballCount; i++)
            {
                float angle = -90f + (i - ballCount / 2f) * 35f; // Spread across angles
                float radians = angle * (float)Math.PI / 180f;
                float speed = 180f + random.Next(-30, 31); // More varied speeds
                
                var ball = new Ball(
                    new Rectangle(
                        (int)position.X - GameConstants.BallSize / 2 + random.Next(-15, 16),
                        (int)position.Y + random.Next(-10, 11),
                        GameConstants.BallSize,
                        GameConstants.BallSize
                    ),
                    new Vector2((float)Math.Cos(radians) * speed, (float)Math.Sin(radians) * speed),
                    true
                );
                _balls.Add(ball);
                
                // Spawn colorful particles for each ball with trails
                Color[] colors = { Color.Red, Color.Yellow, Color.Cyan, Color.Magenta, Color.Lime, Color.Orange };
                Color ballColor = colors[i % colors.Length];
                
                // Multiple particle bursts per ball
                _particleSystem.SpawnExplosion(ball.Center, 20, ballColor);
                _particleSystem.SpawnDustCloud(ball.Center, 15);
                
                // Floating text for each ball
                _floatingTextSystem.AddText($"BALL {i+1}!", ball.Center, ballColor, 1.5f);
            }
            
            // Central mega explosion
            _particleSystem.SpawnExplosion(position, 50, Color.White);
        }

        private void SpawnLightningBolt(Vector2 startPos, Vector2 endPos)
        {
            // Create lightning bolt effect from UFO to target brick
            var random = new Random();
            
            // Calculate direction and distance
            Vector2 direction = endPos - startPos;
            float distance = direction.Length();
            direction.Normalize();
            
            // Create lightning segments with random offsets
            int segments = (int)(distance / 10f); // One segment per 10 pixels
            Vector2 currentPos = startPos;
            
            for (int i = 0; i < segments; i++)
            {
                float progress = i / (float)segments;
                
                // Random offset perpendicular to direction
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                float offset = random.Next(-8, 9);
                
                // Target position with random zigzag
                Vector2 nextPos = startPos + direction * (distance * (i + 1) / segments) + perpendicular * offset;
                
                // Spawn particles along the lightning path
                int particlesPerSegment = 3;
                for (int j = 0; j < particlesPerSegment; j++)
                {
                    Vector2 particlePos = Vector2.Lerp(currentPos, nextPos, j / (float)particlesPerSegment);
                    
                    // Bright cyan/white lightning particles
                    Color lightningColor = random.Next(2) == 0 ? Color.Cyan : Color.White;
                    _particleSystem.SpawnExplosion(particlePos, 2, lightningColor);
                }
                
                currentPos = nextPos;
            }
            
            // Extra bright flash at start and end
            _particleSystem.SpawnExplosion(startPos, 8, Color.White);
            _particleSystem.SpawnExplosion(endPos, 12, Color.Cyan);
            
            // Sound effect for lightning
            _audioService.PlayPowerUp(); // Electric sound
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
                    _drawManager.DrawVictory(_uiManager);
                }
                else if (_scoreService.IsGameOver)
                {
                    _drawManager.DrawGameOver(_uiManager);
                }
                else if (_uiManager.LevelComplete)
                {
                    _drawManager.DrawLevelComplete(_uiManager, _currentShopItems);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGame()
        {
            // UI Area
            _drawManager.DrawUIArea();

            // Paddle
            _drawManager.DrawPaddle(_paddle);

            // Balls
            _drawManager.DrawBalls(_balls);

            // Bricks
            _drawManager.DrawBricks(_bricks);

            // Projectiles
            _drawManager.DrawProjectiles(_projectiles);

            // UFO
            if (_ufoManager.UFOActive)
            {
                _drawManager.DrawUFO(_ufoManager.CurrentUFO);
            }

            // Particles
            _drawManager.DrawParticles();

            // Floating texts
            _drawManager.DrawFloatingTexts();

            // UI
            _drawManager.DrawUI(_gameState.CurrentLevel);

            // Shoot mode indicator
            _drawManager.DrawShootModeIndicator();

            // Continue with rest of shoot mode indicator
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

        #endregion
    }
}
