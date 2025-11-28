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
                {
                    // Pulse effect during Multi-Ball Chaos mode
                    if (_powerUpManager.MultiBallChaosActive && _balls.Count > 1)
                    {
                        // Pulsating scale and glow
                        float pulseTimer = (float)_scoreService.GameTimer * 8f; // Fast pulse
                        float pulseScale = 1f + (float)Math.Sin(pulseTimer) * 0.15f; // 0.85 to 1.15
                        
                        // Calculate pulsing size
                        int pulseSize = (int)(ball.Rect.Width * pulseScale);
                        int pulseOffset = (ball.Rect.Width - pulseSize) / 2;
                        
                        Rectangle pulseRect = new Rectangle(
                            ball.Rect.X + pulseOffset,
                            ball.Rect.Y + pulseOffset,
                            pulseSize,
                            pulseSize
                        );
                        
                        // Colorful glow effect
                        float glowIntensity = (float)Math.Sin(pulseTimer) * 0.5f + 0.5f; // 0 to 1
                        Color[] chaosColors = { Color.Magenta, Color.Cyan, Color.Yellow, Color.Lime };
                        int ballIndex = _balls.IndexOf(ball);
                        Color glowColor = chaosColors[ballIndex % chaosColors.Length];
                        
                        // Draw glow halo
                        int haloSize = pulseSize + (int)(6 * glowIntensity);
                        Rectangle haloRect = new Rectangle(
                            ball.Rect.X + (ball.Rect.Width - haloSize) / 2,
                            ball.Rect.Y + (ball.Rect.Width - haloSize) / 2,
                            haloSize,
                            haloSize
                        );
                        _spriteBatch.Draw(_ballTexture, haloRect, glowColor * (0.3f * glowIntensity));
                        
                        // Draw pulsing ball
                        _spriteBatch.Draw(_ballTexture, pulseRect, Color.White);
                    }
                    else
                    {
                        // Normal ball rendering
                        _spriteBatch.Draw(_ballTexture, ball.Rect, Color.Silver);
                    }
                }
            }

            // Bricks
            DrawBricks();

            // Projectiles
            foreach (var proj in _projectiles)
            {
                _spriteBatch.Draw(_whitePixel, proj.Bounds, Color.Yellow);
            }

            // UFO
            if (_ufoManager.UFOActive)
            {
                DrawUFO(_ufoManager.CurrentUFO);
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

                bool isSpecial = brick.Type == BrickType.Special && !_powerUpManager.CanShoot && !_powerUpManager.MultiBallChaosActive;

                // Steel bricks - special rendering with cracks
                if (brick.IsSteel)
                {
                    DrawSteelBrick(brick);
                    continue;
                }

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

        private void DrawUFO(UFO ufo)
        {
            // Pixel-art flying saucer
            Rectangle bounds = ufo.Bounds;
            int centerX = bounds.X + bounds.Width / 2;
            int centerY = bounds.Y + bounds.Height / 2;

            // Dome (top part)
            int domeHeight = bounds.Height / 3;
            Color domeColor = new Color(150, 150, 200);
            _spriteBatch.Draw(_whitePixel, new Rectangle(centerX - 12, bounds.Y, 24, domeHeight), domeColor);
            
            // Dome window
            _spriteBatch.Draw(_whitePixel, new Rectangle(centerX - 6, bounds.Y + 2, 12, domeHeight - 4), Color.Cyan * 0.7f);

            // Main body (saucer disc)
            Color bodyColor = new Color(100, 100, 120);
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X, centerY - 4, bounds.Width, 8), bodyColor);
            
            // Body highlight
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 2, centerY - 3, bounds.Width - 4, 2), new Color(180, 180, 200));
            
            // Body shadow
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 2, centerY + 1, bounds.Width - 4, 2), new Color(60, 60, 80));

            // Lights (blinking)
            float lightFlicker = (float)Math.Sin(_scoreService.GameTimer * 10) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(Color.Red, Color.Yellow, lightFlicker);
            
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.X + 8, centerY, 3, 3), lightColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(bounds.Right - 11, centerY, 3, 3), lightColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(centerX - 1, centerY, 3, 3), Color.Green * lightFlicker);

            // Bottom beam effect (subtle)
            if (lightFlicker > 0.7f)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(centerX - 2, centerY + 4, 4, 10), Color.Cyan * 0.3f);
            }
        }

        private void DrawSteelBrick(Brick brick)
        {
            // Steel gray color
            Color steelColor = new Color(100, 100, 110);
            _spriteBatch.Draw(_whitePixel, brick.Bounds, steelColor);

            // Metallic sheen
            Color sheenColor = new Color(180, 180, 190);
            _spriteBatch.Draw(_whitePixel, 
                new Rectangle(brick.Bounds.X + 2, brick.Bounds.Y + 2, brick.Bounds.Width - 4, 3), 
                sheenColor);

            // Rivets (steel look)
            Color rivetColor = new Color(70, 70, 80);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 3, brick.Bounds.Y + 3, 2, 2), rivetColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.Right - 5, brick.Bounds.Y + 3, 2, 2), rivetColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 3, brick.Bounds.Bottom - 5, 2, 2), rivetColor);
            _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.Right - 5, brick.Bounds.Bottom - 5, 2, 2), rivetColor);

            // Progressive cracks based on hits remaining
            Color crackColor = new Color(40, 40, 50);
            int hits = brick.SteelHitsRemaining;

            // Stage 1: 4 hits left - small crack
            if (hits <= 4)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 8, brick.Bounds.Y + 4, 1, 6), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 8, brick.Bounds.Y + 4, 6, 1), crackColor);
            }

            // Stage 2: 3 hits left - crack extends
            if (hits <= 3)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 14, brick.Bounds.Y + 7, 1, 8), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 14, brick.Bounds.Y + 7, 10, 1), crackColor);
            }

            // Stage 3: 2 hits left - more cracks
            if (hits <= 2)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 20, brick.Bounds.Y + 3, 1, 12), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 6, brick.Bounds.Y + 12, 12, 1), crackColor);
                
                // Darken brick
                _spriteBatch.Draw(_whitePixel, brick.Bounds, Color.Black * 0.2f);
            }

            // Stage 4: 1 hit left - heavily damaged
            if (hits <= 1)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 4, brick.Bounds.Y + 8, 1, 8), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 24, brick.Bounds.Y + 10, 1, 6), crackColor);
                _spriteBatch.Draw(_whitePixel, new Rectangle(brick.Bounds.X + 10, brick.Bounds.Y + 15, 15, 1), crackColor);
                
                // Heavy darkening
                _spriteBatch.Draw(_whitePixel, brick.Bounds, Color.Black * 0.4f);
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
