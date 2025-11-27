using Microsoft.Xna.Framework;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Models;
using AlleywayMonoGame.Services;
using AlleywayMonoGame.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlleywayMonoGame.Systems
{
    /// <summary>
    /// Handles collision consequences and game logic related to destruction
    /// </summary>
    public class CollisionHandler
    {
        private readonly ScoreService _scoreService;
        private readonly ParticleSystem _particleSystem;
        private readonly FloatingTextSystem _floatingTextSystem;
        private readonly AudioService _audioService;

        public CollisionHandler(
            ScoreService scoreService,
            ParticleSystem particleSystem,
            FloatingTextSystem floatingTextSystem,
            AudioService audioService)
        {
            _scoreService = scoreService;
            _particleSystem = particleSystem;
            _floatingTextSystem = floatingTextSystem;
            _audioService = audioService;
        }

        /// <summary>
        /// Handles brick destruction and its consequences
        /// </summary>
        public BrickDestructionResult HandleBrickDestruction(Brick brick, bool fromProjectile)
        {
            var result = new BrickDestructionResult
            {
                WasSpecialBrick = brick.Type == BrickType.Special
            };

            // Calculate brick color for particles
            int brickHeight = 20;
            int row = (brick.Bounds.Y - 50) / (brickHeight + 2);
            Color brickColor = Brick.GetColorForRow(row);

            // Spawn particles
            _particleSystem.SpawnExplosion(brick.Center, 24, brickColor);

            // Play appropriate sound
            if (fromProjectile)
            {
                _audioService.PlayExplosion();
            }
            else
            {
                _audioService.PlayPaddleHit();
            }

            return result;
        }

        /// <summary>
        /// Checks and handles projectile-brick collisions
        /// </summary>
        public List<int> CheckProjectileCollisions(List<Projectile> projectiles, List<Brick> bricks)
        {
            var bricksToRemove = new List<int>();

            for (int p = projectiles.Count - 1; p >= 0; p--)
            {
                var projectile = projectiles[p];
                bool projectileHit = false;

                for (int b = 0; b < bricks.Count; b++)
                {
                    var brick = bricks[b];
                    if (projectile.Bounds.Intersects(brick.Bounds))
                    {
                        bricksToRemove.Add(b);
                        projectileHit = true;
                        break;
                    }
                }

                if (projectileHit)
                {
                    projectiles.RemoveAt(p);
                }
            }

            return bricksToRemove.Distinct().OrderByDescending(i => i).ToList();
        }
    }

    public struct BrickDestructionResult
    {
        public bool WasSpecialBrick;
    }
}
