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
        public int TotalEarned { get; private set; }
        public int TotalSpent { get; private set; }
        
        private readonly Random _random = new Random();

        private const int SpeedUpgradeCost = 25;
        private const int ExtraBallCost = 5;
        private const int ShootModeCost = 15;
        private const int PaddleSizeCost = 30;
        private const float SpeedUpgradeIncrement = 0.03f;
        private const float PaddleSizeIncrement = 0.04f;

        public void AddMoney(int amount)
        {
            BankBalance += amount;
            TotalEarned += amount;
        }

        public bool CanAfford(ShopItem item)
        {
            return item switch
            {
                ShopItem.SpeedUpgrade => BankBalance >= SpeedUpgradeCost,
                ShopItem.ExtraBall => BankBalance >= ExtraBallCost,
                ShopItem.ShootMode => BankBalance >= ShootModeCost,
                ShopItem.PaddleSize => BankBalance >= PaddleSizeCost,
                _ => false
            };
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
                    return true;
                    
                case ShopItem.PaddleSize:
                    BankBalance -= PaddleSizeCost;
                    TotalSpent += PaddleSizeCost;
                    PaddleSizeMultiplier += PaddleSizeIncrement;
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

        public int CalculateTimeBonus(float gameTime)
        {
            return Math.Max(0, 100 - (int)gameTime);
        }

        public ShopItem[] GetRandomShopItems(int count = 3)
        {
            var allItems = new List<ShopItem>
            {
                ShopItem.SpeedUpgrade,
                ShopItem.ExtraBall,
                ShopItem.ShootMode,
                ShopItem.PaddleSize
            };

            // Shuffle and take first 'count' items
            var shuffled = allItems.OrderBy(x => _random.Next()).ToArray();
            return shuffled.Take(Math.Min(count, allItems.Count)).ToArray();
        }

        public string GetItemName(ShopItem item)
        {
            return item switch
            {
                ShopItem.SpeedUpgrade => "+3% Speed",
                ShopItem.ExtraBall => "Extra Ball",
                ShopItem.ShootMode => "Shoot 6s",
                ShopItem.PaddleSize => "+4% Size",
                _ => "Unknown"
            };
        }
    }

    public enum ShopItem
    {
        SpeedUpgrade,
        ExtraBall,
        ShootMode,
        PaddleSize
    }
}
