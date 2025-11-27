using Microsoft.Xna.Framework;
using AlleywayMonoGame.Models;
using System;
using System.Collections.Generic;

namespace AlleywayMonoGame.Systems
{
    /// <summary>
    /// System responsible for managing particle effects and their lifecycle.
    /// </summary>
    public class ParticleSystem
    {
        private readonly List<Particle> _particles;
        private readonly Random _random;

        public IReadOnlyList<Particle> Particles => _particles;

        public ParticleSystem()
        {
            _particles = new List<Particle>();
            _random = new Random();
        }

        public void Update(float deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                particle.Lifetime -= deltaTime;
                
                if (particle.Lifetime <= 0f)
                {
                    _particles.RemoveAt(i);
                    continue;
                }
                
                particle.Position += particle.Velocity * deltaTime;
                particle.Velocity *= 0.99f; // Damping
                particle.Velocity += new Vector2(0, 200f * deltaTime); // Gravity
            }
        }

        public void SpawnExplosion(Vector2 center, int count, Color baseColor)
        {
            for (int i = 0; i < count; i++)
            {
                double angle = _random.NextDouble() * Math.PI * 2.0;
                float speed = (float)(50 + _random.NextDouble() * 300);
                
                float vr = (float)(0.8 + _random.NextDouble() * 0.4);
                float vg = (float)(0.8 + _random.NextDouble() * 0.4);
                float vb = (float)(0.8 + _random.NextDouble() * 0.4);
                var color = new Color(
                    Math.Min(1f, baseColor.R / 255f * vr),
                    Math.Min(1f, baseColor.G / 255f * vg),
                    Math.Min(1f, baseColor.B / 255f * vb)
                );

                var particle = new Particle
                {
                    Position = center,
                    Velocity = new Vector2(
                        (float)Math.Cos(angle) * speed,
                        (float)Math.Sin(angle) * speed
                    ),
                    MaxLifetime = (float)(0.4 + _random.NextDouble() * 0.9),
                    Lifetime = (float)(0.4 + _random.NextDouble() * 0.9),
                    Size = (float)(2 + _random.NextDouble() * 6),
                    Color = color
                };
                
                _particles.Add(particle);
            }
        }

        public void SpawnDustCloud(Vector2 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (_random.Next(60) + 60) * (float)Math.PI / 180f;
                float speed = 80f + _random.Next(120);
                
                _particles.Add(new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)Math.Cos(angle) * speed,
                        (float)Math.Sin(angle) * speed * 0.5f
                    ),
                    Lifetime = 1.2f + (float)_random.NextDouble() * 0.8f,
                    MaxLifetime = 1.2f + (float)_random.NextDouble() * 0.8f,
                    Size = 5f + _random.Next(7),
                    Color = Color.LightGray
                });
            }
        }

        public void SpawnSmokeTrail(Vector2 position)
        {
            if (_random.Next(100) < 80)
            {
                _particles.Add(new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        _random.Next(-20, 20),
                        30f + _random.Next(40)
                    ),
                    Lifetime = 6.0f,
                    MaxLifetime = 6.0f,
                    Size = 8f + _random.Next(6),
                    Color = Color.White
                });
            }
        }

        public void Clear()
        {
            _particles.Clear();
        }
    }
}
