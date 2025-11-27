using Microsoft.Xna.Framework;
using AlleywayMonoGame.Models;
using System;
using System.Collections.Generic;

namespace AlleywayMonoGame.Systems
{
    /// <summary>
    /// System for managing floating text animations.
    /// </summary>
    public class FloatingTextSystem
    {
        private readonly List<FloatingText> _floatingTexts;

        public IReadOnlyList<FloatingText> FloatingTexts => _floatingTexts;

        public FloatingTextSystem()
        {
            _floatingTexts = new List<FloatingText>();
        }

        public void Update(float deltaTime)
        {
            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var text = _floatingTexts[i];
                text.Lifetime -= deltaTime;
                
                if (text.Lifetime <= 0f)
                {
                    _floatingTexts.RemoveAt(i);
                    continue;
                }
                
                text.Position = new Vector2(text.Position.X, text.Position.Y - 30f * deltaTime);
            }
        }

        public void AddText(string text, Vector2 position, Color color, float lifetime = 3f)
        {
            _floatingTexts.Add(new FloatingText
            {
                Text = text,
                Position = position,
                Lifetime = lifetime,
                MaxLifetime = lifetime,
                Color = color
            });
        }

        public void Clear()
        {
            _floatingTexts.Clear();
        }
    }
}
