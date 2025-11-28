using Microsoft.Xna.Framework;

namespace AlleywayMonoGame.Services
{
    /// <summary>
    /// Service for managing shop purchases and upgrades.
    /// </summary>
    public class ShopService
    {
        public int BankBalance { get; private set; }
        public float PaddleSpeedMultiplier { get; private set; } = 1.0f;
        public float PaddleSizeMultiplier { get; private set; } = 1.0f;
        public int ExtraBallsPurchased { get; private set; }
        public bool StartWithShootMode { get; set; }
        public bool HasShield { get; set; }
        public bool ShieldBreaking { get; set; }
        public float ShieldBreakTimer { get; set; }
        public int TotalEarned { get; private set; }
        public int TotalSpent { get; private set; }
        
        private readonly Random _random = new Random();
        private readonly HashSet<ShopItem> _purchasedOneTimeItems = new HashSet<ShopItem>();

        private const int SpeedUpgradeCost = 25;
        private const int ExtraBallCost = 5;
        private const int ShootModeCost = 15;
        private const int PaddleSizeCost = 40;
        private const int ShieldCost = 30;
        private const int RerollCost = 5;
        private const float SpeedUpgradeIncrement = 0.03f;
        private const float PaddleSizeIncrement = 0.04f;

        public void AddMoney(int amount)
        {
            BankBalance += amount;
            TotalEarned += amount;
        }

        public bool CanAfford(ShopItem item)
        {
            // One-time items can't be purchased again
            if (IsOneTimeItem(item) && _purchasedOneTimeItems.Contains(item))
                return false;

            return item switch
            {
                ShopItem.SpeedUpgrade => BankBalance >= SpeedUpgradeCost,
                ShopItem.ExtraBall => BankBalance >= ExtraBallCost,
                ShopItem.ShootMode => BankBalance >= ShootModeCost,
                ShopItem.PaddleSize => BankBalance >= PaddleSizeCost,
                ShopItem.Shield => BankBalance >= ShieldCost,
                _ => false
            };
        }

        public bool IsOneTimeItem(ShopItem item)
        {
            return item == ShopItem.ShootMode || item == ShopItem.Shield;
        }

        public bool IsPurchased(ShopItem item)
        {
            return _purchasedOneTimeItems.Contains(item);
        }

        public bool CanAffordReroll()
        {
            return BankBalance >= RerollCost;
        }

        public bool Reroll()
        {
            if (!CanAffordReroll())
                return false;

            BankBalance -= RerollCost;
            TotalSpent += RerollCost;
            return true;
        }

        public bool Purchase(ShopItem item)
        {
            if (!CanAfford(item))
                return false;

            switch (item)
            {
                case ShopItem.SpeedUpgrade:
                    BankBalance -= SpeedUpgradeCost;
                    TotalSpent += SpeedUpgradeCost;
                    PaddleSpeedMultiplier += SpeedUpgradeIncrement;
                    return true;
                    
                case ShopItem.ExtraBall:
                    BankBalance -= ExtraBallCost;
                    TotalSpent += ExtraBallCost;
                    ExtraBallsPurchased++;
                    return true;
                    
                case ShopItem.ShootMode:
                    BankBalance -= ShootModeCost;
                    TotalSpent += ShootModeCost;
                    StartWithShootMode = true;
                    _purchasedOneTimeItems.Add(ShopItem.ShootMode);
                    return true;
                    
                case ShopItem.PaddleSize:
                    BankBalance -= PaddleSizeCost;
                    TotalSpent += PaddleSizeCost;
                    PaddleSizeMultiplier += PaddleSizeIncrement;
                    return true;
                    
                case ShopItem.Shield:
                    BankBalance -= ShieldCost;
                    TotalSpent += ShieldCost;
                    HasShield = true;
                    _purchasedOneTimeItems.Add(ShopItem.Shield);
                    return true;
                    
                default:
                    return false;
            }
        }

        public int GetCost(ShopItem item)
        {
            return item switch
            {
                ShopItem.SpeedUpgrade => SpeedUpgradeCost,
                ShopItem.ExtraBall => ExtraBallCost,
                ShopItem.ShootMode => ShootModeCost,
                ShopItem.PaddleSize => PaddleSizeCost,
                ShopItem.Shield => ShieldCost,
                _ => 0
            };
        }

        public void ResetExtraBalls()
        {
            ExtraBallsPurchased = 0;
        }

        public void ResetShootMode()
        {
            StartWithShootMode = false;
        }

        public void UseShield()
        {
            HasShield = false;
            ShieldBreaking = true;
            ShieldBreakTimer = 2.0f; // 2 seconds animation
            _purchasedOneTimeItems.Remove(ShopItem.Shield); // Can be bought again after use
        }

        public void UpdateShieldBreak(float deltaTime)
        {
            if (ShieldBreaking)
            {
                ShieldBreakTimer -= deltaTime;
                if (ShieldBreakTimer <= 0)
                {
                    ShieldBreaking = false;
                    ShieldBreakTimer = 0;
                }
            }
        }

        public int CalculateTimeBonus(float gameTime)
        {
            return Math.Max(0, 100 - (int)gameTime);
        }

        public ShopItem[] GetRandomShopItems(int count = 3)
        {
            var allItems = new List<ShopItem>
            {
                ShopItem.SpeedUpgrade,
                ShopItem.ShootMode,
                ShopItem.PaddleSize,
                ShopItem.Shield
            };

            // Filter out already purchased one-time items (ShootMode, Shield)
            var availableItems = allItems.Where(item => 
                !_purchasedOneTimeItems.Contains(item)
            ).ToList();

            // Shuffle and take first 'count' items
            var shuffled = availableItems.OrderBy(x => _random.Next()).ToArray();
            return shuffled.Take(Math.Min(count, availableItems.Count)).ToArray();
        }

        public string GetItemName(ShopItem item)
        {
            return item switch
            {
                ShopItem.SpeedUpgrade => "+3% Speed",
                ShopItem.ExtraBall => "Extra Ball",
                ShopItem.ShootMode => "Shoot 6s",
                ShopItem.PaddleSize => "+4% Size",
                ShopItem.Shield => "Shield",
                _ => "Unknown"
            };
        }

        public string GetItemDescription(ShopItem item)
        {
            return item switch
            {
                ShopItem.SpeedUpgrade => "Increases paddle\nmovement speed by 3%",
                ShopItem.ExtraBall => "Adds one extra ball\nat level start",
                ShopItem.ShootMode => "Start next level\nwith shoot mode active",
                ShopItem.PaddleSize => "Increases paddle\nwidth by 4%",
                ShopItem.Shield => "Protects you from\nlosing one life",
                _ => "Unknown item"
            };
        }

        public string GetItemIcon(ShopItem item)
        {
            return item switch
            {
                ShopItem.SpeedUpgrade => ">",
                ShopItem.ExtraBall => "O",
                ShopItem.ShootMode => "^",
                ShopItem.PaddleSize => "=",
                _ => "?"
            };
        }

        public Color GetItemColor(ShopItem item)
        {
            return item switch
            {
                ShopItem.SpeedUpgrade => new Color(100, 200, 255), // Blue - Movement
                ShopItem.ExtraBall => new Color(255, 215, 0),       // Gold - Valuable
                ShopItem.ShootMode => new Color(255, 100, 100),     // Red - Combat
                ShopItem.PaddleSize => new Color(150, 255, 150),    // Green - Size
                ShopItem.Shield => new Color(200, 150, 255),        // Purple - Protection
                _ => Color.Gray
            };
        }
    }

    public enum ShopItem
    {
        SpeedUpgrade,
        ExtraBall,
        ShootMode,
        PaddleSize,
        Shield
    }
}
