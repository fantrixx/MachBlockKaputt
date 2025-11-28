using System.Collections.Generic;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Services;

namespace AlleywayMonoGame.Managers
{
    /// <summary>
    /// Manages power-ups and special abilities.
    /// </summary>
    public class PowerUpManager
    {
        private readonly Paddle _paddle;
        private readonly List<Projectile> _projectiles;
        private readonly AudioService _audioService;

        // Shoot mode state
        public bool CanShoot { get; set; }
        public float ShootPowerTimer { get; set; }
        public float CannonExtension { get; set; }
        
        // Big paddle state
        public bool BigPaddleActive { get; set; }
        public float BigPaddleTimer { get; set; }
        
        // Multi-Ball Chaos state
        public bool MultiBallChaosActive { get; set; }
        
        public float FlickerTimer { get; set; }

        public PowerUpManager(Paddle paddle, List<Projectile> projectiles, AudioService audioService)
        {
            _paddle = paddle;
            _projectiles = projectiles;
            _audioService = audioService;
        }

        public void ActivateShootMode(bool startWithShootMode = false)
        {
            if (!CanShoot && !BigPaddleActive)
            {
                CanShoot = true;
                ShootPowerTimer = startWithShootMode ? 6f : 6f;
                CannonExtension = 0f;
                _audioService.PlayPowerUp();
            }
        }

        public void ActivateBigPaddle()
        {
            if (!BigPaddleActive)
            {
                BigPaddleActive = true;
                BigPaddleTimer = 10f;
                _paddle.Enlarge();
                _audioService.PlayPaddleEnlarge();
            }
            else
            {
                // Refresh timer if already active
                BigPaddleTimer = 10f;
                _audioService.PlayPowerUp();
            }
        }

        public void UpdateShootMode(float deltaTime)
        {
            if (CanShoot)
            {
                ShootPowerTimer -= deltaTime;
                if (ShootPowerTimer <= 0)
                {
                    DeactivateShootMode();
                }
                else
                {
                    CannonExtension = Math.Min(1f, CannonExtension + deltaTime * 3f);
                }
            }
            else if (CannonExtension > 0f)
            {
                // Retract cannon smoothly when shoot mode ends
                CannonExtension = Math.Max(0f, CannonExtension - deltaTime * 4f);
            }
        }

        public void UpdateBigPaddle(float deltaTime)
        {
            if (BigPaddleActive)
            {
                BigPaddleTimer -= deltaTime;
                if (BigPaddleTimer <= 0)
                {
                    DeactivateBigPaddle();
                }
            }
        }

        public void DeactivateShootMode()
        {
            CanShoot = false;
            ShootPowerTimer = 0f;
            CannonExtension = 0f;
            _projectiles.Clear();
        }

        public void DeactivateBigPaddle()
        {
            BigPaddleActive = false;
            BigPaddleTimer = 0f;
            _paddle.Shrink();
            _audioService.PlayPaddleShrink();
        }

        public void ActivateMultiBallChaos()
        {
            MultiBallChaosActive = true;
        }

        public void DeactivateMultiBallChaos()
        {
            MultiBallChaosActive = false;
        }

        public void ResetAll()
        {
            DeactivateShootMode();
            DeactivateBigPaddle();
            DeactivateMultiBallChaos();
        }
    }
}
