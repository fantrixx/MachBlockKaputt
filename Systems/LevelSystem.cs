using Microsoft.Xna.Framework;
using AlleywayMonoGame.Entities;
using System;
using System.Collections.Generic;

namespace AlleywayMonoGame.Systems
{
    /// <summary>
    /// System for generating brick formations and managing levels.
    /// </summary>
    public class LevelSystem
    {
        private readonly Random _random;
        private readonly int _screenWidth;
        private readonly int _gameAreaTop;

        public LevelSystem(int screenWidth, int gameAreaTop)
        {
            _random = new Random();
            _screenWidth = screenWidth;
            _gameAreaTop = gameAreaTop;
        }

        public LevelData GenerateLevel(int level)
        {
            var data = new LevelData();
            
            int totalLevels = 10;
            int baseRows = 3;
            int baseCols = 6;
            
            int currentLevel = Math.Min(level, totalLevels);
            int currentRows = baseRows + (currentLevel - 1);
            int currentCols = baseCols + (currentLevel - 1);
            
            int brickWidth = _screenWidth / currentCols;
            int brickHeight = Math.Max(10, 20 - currentLevel);
            int brickStartY = _gameAreaTop + 30;

            int pattern = (level - 1) % 4;
            
            switch (pattern)
            {
                case 0: // Full grid
                    GenerateFullGrid(data.Bricks, currentRows, currentCols, brickWidth, brickHeight, brickStartY);
                    break;
                    
                case 1: // Pyramid
                    GeneratePyramid(data.Bricks, currentRows, currentCols, brickWidth, brickHeight, brickStartY);
                    break;
                    
                case 2: // Checkerboard
                    GenerateCheckerboard(data.Bricks, currentRows, currentCols, brickWidth, brickHeight, brickStartY);
                    break;
                    
                case 3: // Gaps
                    GenerateGaps(data.Bricks, currentRows, currentCols, brickWidth, brickHeight, brickStartY);
                    break;
            }

            AssignSpecialBricks(data);
            return data;
        }

        private void GenerateFullGrid(List<Brick> bricks, int rows, int cols, int width, int height, int startY)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    bricks.Add(new Brick(
                        new Rectangle(c * width, startY + r * height, width - 2, height - 2)
                    ));
                }
            }
        }

        private void GeneratePyramid(List<Brick> bricks, int rows, int cols, int width, int height, int startY)
        {
            for (int r = 0; r < rows; r++)
            {
                int rowCols = Math.Max(1, cols - r * 2);
                int startCol = r;
                for (int c = 0; c < rowCols; c++)
                {
                    bricks.Add(new Brick(
                        new Rectangle((startCol + c) * width + 2, startY + r * (height + 2), width - 4, height)
                    ));
                }
            }
        }

        private void GenerateCheckerboard(List<Brick> bricks, int rows, int cols, int width, int height, int startY)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if ((r + c) % 2 == 0)
                    {
                        bricks.Add(new Brick(
                            new Rectangle(c * width + 2, startY + r * (height + 2), width - 4, height)
                        ));
                    }
                }
            }
        }

        private void GenerateGaps(List<Brick> bricks, int rows, int cols, int width, int height, int startY)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (c % 3 != 0)
                    {
                        bricks.Add(new Brick(
                            new Rectangle(c * width + 2, startY + r * (height + 2), width - 4, height)
                        ));
                    }
                }
            }
        }

        private void AssignSpecialBricks(LevelData data)
        {
            int shootCount = Math.Max(1, (data.Bricks.Count * 40) / 100);
            int extraBallCount = Math.Max(1, (data.Bricks.Count * 40) / 100);
            
            HashSet<int> usedIndices = new HashSet<int>();
            
            // Assign shoot power bricks
            int attempts = 0;
            while (data.ShootPowerIndices.Count < shootCount && attempts < shootCount * 3)
            {
                int randomIndex = _random.Next(data.Bricks.Count);
                if (!usedIndices.Contains(randomIndex))
                {
                    data.ShootPowerIndices.Add(randomIndex);
                    data.Bricks[randomIndex].Type = BrickType.ShootPowerUp;
                    usedIndices.Add(randomIndex);
                }
                attempts++;
            }
            
            // Assign extra ball bricks
            attempts = 0;
            while (data.ExtraBallIndices.Count < extraBallCount && attempts < extraBallCount * 3)
            {
                int randomIndex = _random.Next(data.Bricks.Count);
                if (!usedIndices.Contains(randomIndex))
                {
                    data.ExtraBallIndices.Add(randomIndex);
                    data.Bricks[randomIndex].Type = BrickType.ExtraBall;
                    usedIndices.Add(randomIndex);
                }
                attempts++;
            }
        }
    }

    public class LevelData
    {
        public List<Brick> Bricks { get; } = new List<Brick>();
        public List<int> ShootPowerIndices { get; } = new List<int>();
        public List<int> ExtraBallIndices { get; } = new List<int>();
    }
}
