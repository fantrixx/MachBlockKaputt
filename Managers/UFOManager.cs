using System;
using System.Collections.Generic;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Models;
using AlleywayMonoGame.Services;
using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Managers
{
    /// <summary>
    /// Manages UFO spawning and behavior
    /// </summary>
    public class UFOManager
    {
        private readonly UFO _ufo;
        private readonly Random _random;
        private readonly AudioService _audioService;
        private float _spawnTimer;
        private const float MinSpawnInterval = 15f; // Minimum 15 seconds between spawns
        private const float MaxSpawnInterval = 30f; // Maximum 30 seconds
        private const float SpawnChance = 0.5f; // 50% chance
        
        public bool UFOActive => _ufo.IsActive;
        public UFO CurrentUFO => _ufo;

        public UFOManager(int screenWidth, AudioService audioService)
        {
            _ufo = new UFO(screenWidth);
            _random = new Random();
            _audioService = audioService;
            _spawnTimer = _random.Next((int)MinSpawnInterval, (int)MaxSpawnInterval);
        }

        public void Update(float deltaTime)
        {
            if (_ufo.IsActive)
            {
                _ufo.Update(deltaTime);
                
                // Deactivate music when UFO leaves
                if (!_ufo.IsActive)
                {
                    // Music fade out handled elsewhere
                }
            }
            else
            {
                // Spawn timer
                _spawnTimer -= deltaTime;
                
                if (_spawnTimer <= 0f)
                {
                    // Roll for spawn
                    if (_random.NextDouble() < SpawnChance)
                    {
                        SpawnUFO();
                    }
                    
                    // Reset timer
                    _spawnTimer = _random.Next((int)MinSpawnInterval, (int)MaxSpawnInterval);
                }
            }
        }

        private void SpawnUFO()
        {
            _ufo.Spawn(_random);
            // Play UFO mysterious sound
            _audioService.PlayUFO();
        }

        public void ForceSpawn()
        {
            if (!_ufo.IsActive)
            {
                SpawnUFO();
            }
        }

        public bool CheckCollision(Ball ball, List<Brick> bricks, out Vector2 ufoPosition, out Vector2 targetBrickPosition)
        {
            ufoPosition = Vector2.Zero;
            targetBrickPosition = Vector2.Zero;
            
            if (!_ufo.IsActive) return false;
            
            if (ball.Rect.Intersects(_ufo.Bounds))
            {
                // Store UFO position before destroying
                ufoPosition = _ufo.Center.ToVector2();
                
                // UFO hit! Convert random brick to steel
                if (bricks.Count > 0)
                {
                    int randomIndex = _random.Next(bricks.Count);
                    Brick targetBrick = bricks[randomIndex];
                    
                    // Store target brick position
                    targetBrickPosition = targetBrick.Center;
                    
                    // Convert to steel brick
                    targetBrick.ConvertToSteel();
                }
                
                _ufo.Destroy();
                return true;
            }
            
            return false;
        }

        public void Reset()
        {
            if (_ufo.IsActive)
            {
                _ufo.Destroy();
            }
            _spawnTimer = _random.Next((int)MinSpawnInterval, (int)MaxSpawnInterval);
        }
    }
}
