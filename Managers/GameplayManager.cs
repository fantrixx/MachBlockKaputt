using Microsoft.Xna.Framework;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Models;
using AlleywayMonoGame.Services;
using AlleywayMonoGame.Systems;
using AlleywayMonoGame.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlleywayMonoGame.Managers
{
    /// <summary>
    /// Manages core gameplay logic: balls, collisions, and game flow
    /// </summary>
    public class GameplayManager
    {
        private readonly List<Ball> _balls;
        private readonly List<Brick> _bricks;
        private readonly List<Projectile> _projectiles;
        private readonly Paddle _paddle;
        
        private readonly CollisionSystem _collisionSystem;
        private readonly ParticleSystem _particleSystem;
        private readonly FloatingTextSystem _floatingTextSystem;
        private readonly AudioService _audioService;
        private readonly PowerUpManager _powerUpManager;
        private readonly ScoreService _scoreService;

        public IReadOnlyList<Ball> Balls => _balls;
        public IReadOnlyList<Brick> Bricks => _bricks;
        public IReadOnlyList<Projectile> Projectiles => _projectiles;

        public GameplayManager(
            List<Ball> balls,
            List<Brick> bricks,
            List<Projectile> projectiles,
            Paddle paddle,
            CollisionSystem collisionSystem,
            ParticleSystem particleSystem,
            FloatingTextSystem floatingTextSystem,
            AudioService audioService,
            PowerUpManager powerUpManager,
            ScoreService scoreService)
        {
            _balls = balls;
            _bricks = bricks;
            _projectiles = projectiles;
            _paddle = paddle;
            _collisionSystem = collisionSystem;
            _particleSystem = particleSystem;
            _floatingTextSystem = floatingTextSystem;
            _audioService = audioService;
            _powerUpManager = powerUpManager;
            _scoreService = scoreService;
        }

        public void UpdateBalls(float deltaTime, int screenWidth, int gameAreaTop, int screenHeight, out Action<int, Brick> onBrickHit, out Action onAllBallsLost)
        {
            onBrickHit = null!;
            onAllBallsLost = null!;

            // Move unlaunched balls with paddle
            foreach (var ball in _balls.Where(b => !b.IsLaunched))
            {
                ball.Rect = new Rectangle(
                    _paddle.X + _paddle.Width / 2 - ball.Rect.Width / 2,
                    _paddle.Y - ball.Rect.Height - 1,
                    ball.Rect.Width,
                    ball.Rect.Height
                );
            }

            // Move launched balls
            foreach (var ball in _balls.Where(b => b.IsLaunched))
            {
                ball.Rect = new Rectangle(
                    ball.Rect.X + (int)(ball.Velocity.X * deltaTime),
                    ball.Rect.Y + (int)(ball.Velocity.Y * deltaTime),
                    ball.Rect.Width,
                    ball.Rect.Height
                );
            }

            // Check collisions
            var collisionResult = _collisionSystem.CheckBallCollisions(
                _balls,
                _paddle,
                _bricks,
                screenWidth,
                gameAreaTop,
                screenHeight
            );

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

            // Handle brick hits
            foreach (var (index, brick) in collisionResult.BricksHit.OrderByDescending(b => b.index))
            {
                onBrickHit += (idx, brk) => { };
                onBrickHit?.Invoke(index, brick);
            }

            // Handle lost balls
            foreach (int ballIndex in collisionResult.BallsLost.OrderByDescending(i => i))
            {
                _balls.RemoveAt(ballIndex);

                if (_balls.Count == 0)
                {
                    onAllBallsLost += () => { };
                    onAllBallsLost?.Invoke();
                }
                else if (_balls.Count == 1 && _powerUpManager.MultiBallChaosActive)
                {
                    _powerUpManager.DeactivateMultiBallChaos();
                    _floatingTextSystem.AddText("CHAOS ENDED", _balls[0].Center, Color.Gray, 2f);
                }
            }
        }

        public void SpawnExtraBall(Vector2 position)
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

        public void SpawnMultiBallChaos(Vector2 position)
        {
            var random = new Random();
            int ballCount = random.Next(4, 7);
            
            for (int i = 0; i < ballCount; i++)
            {
                float angle = -90f + (i - ballCount / 2f) * 35f;
                float radians = angle * (float)Math.PI / 180f;
                float speed = random.Next(180, 251);
                
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
                
                Color[] colors = { Color.Red, Color.Yellow, Color.Cyan, Color.Magenta, Color.Lime, Color.Orange };
                Color ballColor = colors[i % colors.Length];
                
                _particleSystem.SpawnExplosion(ball.Center, 20, ballColor);
                _particleSystem.SpawnDustCloud(ball.Center, 15);
                
                _floatingTextSystem.AddText($"BALL {i+1}!", ball.Center, ballColor, 1.5f);
            }
            
            _particleSystem.SpawnExplosion(position, 50, Color.White);
        }

        public void SpawnLightningBolt(Vector2 startPos, Vector2 endPos)
        {
            var random = new Random();
            
            Vector2 direction = endPos - startPos;
            float distance = direction.Length();
            direction.Normalize();
            
            int segments = (int)(distance / 10f);
            Vector2 currentPos = startPos;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                float offset = random.Next(-8, 9);
                
                Vector2 nextPos = startPos + direction * (distance * (i + 1) / segments) + perpendicular * offset;
                
                int particlesPerSegment = 3;
                for (int j = 0; j < particlesPerSegment; j++)
                {
                    Vector2 particlePos = Vector2.Lerp(currentPos, nextPos, j / (float)particlesPerSegment);
                    
                    Color lightningColor = random.Next(2) == 0 ? Color.Cyan : Color.White;
                    _particleSystem.SpawnExplosion(particlePos, 2, lightningColor);
                }
                
                currentPos = nextPos;
            }
            
            _particleSystem.SpawnExplosion(startPos, 8, Color.White);
            _particleSystem.SpawnExplosion(endPos, 12, Color.Cyan);
            
            _audioService.PlayPowerUp();
        }

        public void ResetBall()
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
    }
}
