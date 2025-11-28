using Microsoft.Xna.Framework;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Models;
using System;
using System.Collections.Generic;

namespace AlleywayMonoGame.Systems
{
    /// <summary>
    /// System responsible for handling all collision detection and resolution.
    /// </summary>
    public class CollisionSystem
    {
        private const float BallSpeedIncrease = 1.05f;
        private const float MaxBallSpeed = 800f;

        public CollisionResult CheckBallCollisions(
            List<Ball> balls,
            Paddle paddle,
            List<Brick> bricks,
            int screenWidth,
            int gameAreaTop,
            int screenHeight)
        {
            var result = new CollisionResult();

            for (int ballIndex = balls.Count - 1; ballIndex >= 0; ballIndex--)
            {
                Ball ball = balls[ballIndex];
                
                if (!ball.IsLaunched)
                    continue;

                // Wall collisions
                if (ball.Rect.X <= 0)
                {
                    ball.Rect = new Rectangle(0, ball.Rect.Y, ball.Rect.Width, ball.Rect.Height);
                    ball.Velocity = new Vector2(-ball.Velocity.X, ball.Velocity.Y);
                    result.WallBounce = true;
                }
                if (ball.Rect.X + ball.Rect.Width >= screenWidth)
                {
                    ball.Rect = new Rectangle(screenWidth - ball.Rect.Width, ball.Rect.Y, ball.Rect.Width, ball.Rect.Height);
                    ball.Velocity = new Vector2(-ball.Velocity.X, ball.Velocity.Y);
                    result.WallBounce = true;
                }
                
                // Top collision
                if (ball.Rect.Y <= gameAreaTop)
                {
                    ball.Rect = new Rectangle(ball.Rect.X, gameAreaTop, ball.Rect.Width, ball.Rect.Height);
                    ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y);
                    result.WallBounce = true;
                }

                // Paddle collision
                if (ball.Rect.Intersects(paddle.Bounds))
                {
                    // Position ball above paddle
                    ball.Rect = new Rectangle(ball.Rect.X, paddle.Y - ball.Rect.Height - 1, ball.Rect.Width, ball.Rect.Height);
                    
                    // Calculate hit position relative to paddle center (-1 to 1)
                    float hitPos = (ball.Rect.Center.X - paddle.Center.X) / (paddle.Width / 2f);
                    hitPos = Math.Clamp(hitPos, -1f, 1f);
                    
                    // Get incoming angle and speed
                    float currentSpeed = ball.Velocity.Length();
                    float baseSpeed = Math.Max(currentSpeed, 300f);
                    
                    // Calculate incoming angle (in degrees, 0° = horizontal, 90° = vertical down)
                    float incomingAngleRad = (float)Math.Atan2(-ball.Velocity.Y, Math.Abs(ball.Velocity.X));
                    float incomingAngleDeg = incomingAngleRad * 180f / (float)Math.PI;
                    
                    // Paddle influence based on hit position
                    // Center = add up to 30° steeper, Edges = add up to 40° flatter
                    float paddleInfluence = -hitPos * hitPos * 40f + 30f; // Parabola: center=30°, edges=-10°
                    
                    // Combine incoming angle with paddle influence
                    float outgoingAngleDeg = Math.Clamp(incomingAngleDeg + paddleInfluence, 25f, 85f);
                    float outgoingAngleRad = outgoingAngleDeg * (float)Math.PI / 180f;
                    
                    // Calculate new velocity with reflected angle
                    float newVelocityY = -baseSpeed * (float)Math.Sin(outgoingAngleRad);
                    float newVelocityX = baseSpeed * (float)Math.Cos(outgoingAngleRad) * Math.Sign(hitPos);
                    
                    // Special case: hit near center, use incoming horizontal direction
                    if (Math.Abs(hitPos) < 0.1f)
                    {
                        newVelocityX = baseSpeed * (float)Math.Cos(outgoingAngleRad) * Math.Sign(ball.Velocity.X != 0 ? ball.Velocity.X : 1);
                    }
                    
                    ball.Velocity = new Vector2(newVelocityX, newVelocityY);
                    result.PaddleHit = true;
                    result.PaddleBounce = true;
                }

                // Ball-to-ball collision
                for (int otherIndex = ballIndex - 1; otherIndex >= 0; otherIndex--)
                {
                    Ball otherBall = balls[otherIndex];
                    if (!otherBall.IsLaunched) continue;
                    
                    Vector2 ball1Center = ball.Center;
                    Vector2 ball2Center = otherBall.Center;
                    float distance = Vector2.Distance(ball1Center, ball2Center);
                    float minDistance = ball.Rect.Width;
                    
                    if (distance < minDistance && distance > 0)
                    {
                        Vector2 normal = Vector2.Normalize(ball1Center - ball2Center);
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
                        
                        Vector2 relativeVelocity = ball.Velocity - otherBall.Velocity;
                        float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);
                        
                        if (velocityAlongNormal > 0)
                        {
                            Vector2 impulse = normal * velocityAlongNormal;
                            ball.Velocity -= impulse;
                            otherBall.Velocity += impulse;
                        }
                    }
                }

                // Brick collisions
                for (int i = bricks.Count - 1; i >= 0; i--)
                {
                    if (ball.Rect.Intersects(bricks[i].Bounds))
                    {
                        result.BricksHit.Add((i, bricks[i]));
                        ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y);
                        IncreaseBallSpeed(ball);
                        break;
                    }
                }

                // Bottom - ball lost
                if (ball.Rect.Y > screenHeight)
                {
                    result.BallsLost.Add(ballIndex);
                }
            }

            return result;
        }

        public List<(int projectileIndex, int brickIndex, Brick brick)> CheckProjectileCollisions(
            List<Projectile> projectiles,
            List<Brick> bricks,
            int gameAreaTop)
        {
            var collisions = new List<(int, int, Brick)>();

            for (int p = projectiles.Count - 1; p >= 0; p--)
            {
                Projectile proj = projectiles[p];
                
                if (proj.IsOffScreen(gameAreaTop))
                {
                    collisions.Add((p, -1, null!)); // Mark for removal
                    continue;
                }
                
                for (int i = bricks.Count - 1; i >= 0; i--)
                {
                    if (proj.Bounds.Intersects(bricks[i].Bounds))
                    {
                        collisions.Add((p, i, bricks[i]));
                        break;
                    }
                }
            }

            return collisions;
        }

        private void IncreaseBallSpeed(Ball ball)
        {
            if (ball.Velocity == Vector2.Zero)
                return;

            float speed = ball.Velocity.Length();
            speed = Math.Min(MaxBallSpeed, speed * BallSpeedIncrease);
            ball.Velocity = Vector2.Normalize(ball.Velocity) * speed;
        }
    }

    public class CollisionResult
    {
        public List<(int index, Brick brick)> BricksHit { get; } = new List<(int, Brick)>();
        public List<int> BallsLost { get; } = new List<int>();
        public bool PaddleHit { get; set; }
        public bool WallBounce { get; set; }
        public bool PaddleBounce { get; set; }
    }
}
