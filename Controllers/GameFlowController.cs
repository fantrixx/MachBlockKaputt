using Microsoft.Xna.Framework;
using AlleywayMonoGame.Core;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Models;
using AlleywayMonoGame.Services;
using AlleywayMonoGame.Systems;
using AlleywayMonoGame.Managers;
using System;
using System.Collections.Generic;

namespace AlleywayMonoGame.Controllers
{
    /// <summary>
    /// Manages game flow, level progression, and game state transitions
    /// </summary>
    public class GameFlowController
    {
        private readonly ScoreService _scoreService;
        private readonly ShopService _shopService;
        private readonly GameStateManager _gameState;
        private readonly LevelSystem _levelSystem;
        private readonly UIManager _uiManager;
        private readonly PowerUpManager _powerUpManager;
        private readonly ParticleSystem _particleSystem;
        private readonly FloatingTextSystem _floatingTextSystem;
        private readonly AudioService _audioService;

        public GameFlowController(
            ScoreService scoreService,
            ShopService shopService,
            GameStateManager gameState,
            LevelSystem levelSystem,
            UIManager uiManager,
            PowerUpManager powerUpManager,
            ParticleSystem particleSystem,
            FloatingTextSystem floatingTextSystem,
            AudioService audioService)
        {
            _scoreService = scoreService;
            _shopService = shopService;
            _gameState = gameState;
            _levelSystem = levelSystem;
            _uiManager = uiManager;
            _powerUpManager = powerUpManager;
            _particleSystem = particleSystem;
            _floatingTextSystem = floatingTextSystem;
            _audioService = audioService;
        }

        /// <summary>
        /// Handles level completion and transitions
        /// </summary>
        public void OnLevelComplete(Action setupShopUI)
        {
            _gameState.SetLevelComplete();
            _uiManager.LevelComplete = true;
            _uiManager.LevelCompleteTimeBonus = _shopService.CalculateTimeBonus(_scoreService.GameTimer);
            setupShopUI();
        }

        /// <summary>
        /// Advances to the next level or triggers victory
        /// </summary>
        public LevelTransitionResult AdvanceToNextLevel(
            List<Ball> balls,
            List<Projectile> projectiles,
            ref int extraBallsPurchased,
            ref bool startWithShootMode,
            Action setupVictoryUI)
        {
            var result = new LevelTransitionResult();

            // Check for victory
            if (_gameState.CurrentLevel >= 10)
            {
                _gameState.SetVictory();
                setupVictoryUI();
                result.IsVictory = true;
                return result;
            }

            _gameState.NextLevel();
            _uiManager.LevelComplete = false;
            _uiManager.ResetLevelComplete(); // Reset animation state for next level
            _scoreService.ResetTimer();

            // Clear active elements
            projectiles.Clear();
            _particleSystem.Clear();
            _floatingTextSystem.Clear();
            _powerUpManager.CanShoot = false;
            _powerUpManager.ShootPowerTimer = 0f;
            _powerUpManager.CannonExtension = 0f;
            _powerUpManager.BigPaddleActive = false;
            _powerUpManager.BigPaddleTimer = 0f;

            // Generate level
            var levelData = _levelSystem.GenerateLevel(_gameState.CurrentLevel);
            result.NewBricks = levelData.Bricks;

            // Create new paddle
            result.NewPaddle = new Paddle(
                GameConstants.ScreenWidth / 2 - GameConstants.PaddleWidth / 2,
                GameConstants.GameAreaBottom - GameConstants.PaddleBottomMargin,
                GameConstants.PaddleWidth,
                GameConstants.PaddleHeight,
                GameConstants.PaddleSpeed,
                GameConstants.ScreenWidth
            );
            result.NewPaddle.SpeedMultiplier = _shopService.PaddleSpeedMultiplier;
            result.NewPaddle.ApplyPermanentSizeIncrease(_shopService.PaddleSizeMultiplier);

            // Reset balls
            balls.Clear();
            balls.Add(CreateInitialBall(result.NewPaddle, _gameState.CurrentLevel));

            // Spawn extra balls
            for (int i = 0; i < extraBallsPurchased; i++)
            {
                balls.Add(CreateExtraBall(result.NewPaddle, i, _gameState.CurrentLevel));
            }
            extraBallsPurchased = 0;

            // Activate shoot mode immediately if purchased from shop
            if (_shopService.StartWithShootMode)
            {
                startWithShootMode = true;
                // Activate shoot mode immediately so it shows in the info bar
                _powerUpManager.CanShoot = true;
                _powerUpManager.ShootPowerTimer = 7f; // Give it the full duration
                _powerUpManager.CannonExtension = 0f; // Will extend when ball is launched
            }

            _scoreService.StartTimer();
            result.IsVictory = false;
            return result;
        }

        /// <summary>
        /// Restarts the entire game
        /// </summary>
        public GameRestartResult RestartGame()
        {
            _scoreService.Reset();
            _gameState.Reset();
            _uiManager.ResetLevelComplete();
            _powerUpManager.ResetAll();
            _particleSystem.Clear();
            _floatingTextSystem.Clear();

            var levelData = _levelSystem.GenerateLevel(_gameState.CurrentLevel);
            
            var paddle = new Paddle(
                GameConstants.ScreenWidth / 2 - GameConstants.PaddleWidth / 2,
                GameConstants.GameAreaBottom - GameConstants.PaddleBottomMargin,
                GameConstants.PaddleWidth,
                GameConstants.PaddleHeight,
                GameConstants.PaddleSpeed,
                GameConstants.ScreenWidth
            );

            return new GameRestartResult
            {
                NewBricks = levelData.Bricks,
                NewPaddle = paddle,
                InitialBall = CreateInitialBall(paddle, 1),
                NewShopService = new ShopService()
            };
        }

        /// <summary>
        /// Handles all balls lost
        /// </summary>
        public void OnAllBallsLost(List<Ball> balls, Paddle paddle, ShopService shopService, Action setupGameOverUI, Action onShieldUsed)
        {
            bool shieldAbsorbed = _scoreService.LoseLife(shopService);

            if (shieldAbsorbed)
            {
                // Shield absorbed the damage - just reset ball
                balls.Clear();
                balls.Add(CreateInitialBall(paddle, _gameState.CurrentLevel));
                onShieldUsed();
            }
            else if (_scoreService.Lives > 0)
            {
                // Life lost but game continues
                balls.Clear();
                balls.Add(CreateInitialBall(paddle, _gameState.CurrentLevel));
            }
            else
            {
                // Game Over
                _gameState.SetGameOver();
                setupGameOverUI();
            }
        }

        /// <summary>
        /// Processes a shop purchase
        /// </summary>
        public PurchaseResult ProcessPurchase(
            ShopItem item,
            Paddle paddle,
            ref int extraBallsPurchased,
            ref bool startWithShootMode,
            int buttonIndex)
        {
            var result = new PurchaseResult();
            
            if (!_shopService.CanAfford(item))
            {
                result.Success = false;
                return result;
            }

            _shopService.Purchase(item);
            result.Success = true;
            result.Cost = _shopService.GetCost(item);

            // Apply the purchase
            switch (item)
            {
                case ShopItem.SpeedUpgrade:
                    paddle.SpeedMultiplier = _shopService.PaddleSpeedMultiplier;
                    break;
                case ShopItem.ExtraBall:
                    extraBallsPurchased++;
                    break;
                case ShopItem.ShootMode:
                    startWithShootMode = true;
                    break;
                case ShopItem.PaddleSize:
                    paddle.ApplyPermanentSizeIncrease(_shopService.PaddleSizeMultiplier);
                    break;
            }

            _audioService.PlayCashRegister();

            // Setup purchase animation
            result.AnimationX = _uiManager.ShopButtons[buttonIndex].X - 150;
            result.AnimationY = 40 + 3 * 50;

            return result;
        }

        private Ball CreateInitialBall(Paddle paddle, int level)
        {
            // Gentle speed increase: Level 1 = 150, Level 5 = 165, Level 10 = 185
            // Formula: 150 + (level - 1) * 4 = +4 pixels/second per level
            float baseSpeed = 150f + (level - 1) * 4f;
            
            return new Ball(
                new Rectangle(
                    paddle.Center.X - GameConstants.BallSize / 2,
                    paddle.Y - GameConstants.BallSize - GameConstants.BallPaddleGap,
                    GameConstants.BallSize,
                    GameConstants.BallSize
                ),
                new Vector2(baseSpeed, -baseSpeed),
                false
            );
        }

        private Ball CreateExtraBall(Paddle paddle, int index, int level)
        {
            float angle = -90f + (index + 1) * 30f;
            float radians = angle * (float)Math.PI / 180f;
            
            // Same speed progression as initial ball
            float baseSpeed = 150f + (level - 1) * 4f;

            return new Ball(
                new Rectangle(
                    GameConstants.ScreenWidth / 2 - GameConstants.BallSize / 2,
                    paddle.Y - GameConstants.BallSize - GameConstants.BallPaddleGap,
                    GameConstants.BallSize,
                    GameConstants.BallSize
                ),
                new Vector2((float)Math.Cos(radians) * baseSpeed, (float)Math.Sin(radians) * baseSpeed),
                true
            );
        }
    }

    public struct LevelTransitionResult
    {
        public bool IsVictory;
        public List<Brick> NewBricks;
        public Paddle NewPaddle;
    }

    public struct GameRestartResult
    {
        public List<Brick> NewBricks;
        public Paddle NewPaddle;
        public Ball InitialBall;
        public ShopService NewShopService;
    }

    public struct PurchaseResult
    {
        public bool Success;
        public int Cost;
        public float AnimationX;
        public float AnimationY;
    }
}
