namespace AlleywayMonoGame.Services
{
    /// <summary>
    /// Service for managing shop purchases and upgrades.
    /// </summary>
    public class ShopService
    {
        public int BankBalance { get; private set; }
        public float PaddleSpeedMultiplier { get; private set; } = 1.0f;
        public int ExtraBallsPurchased { get; private set; }
        public bool StartWithShootMode { get; set; }

        private const int SpeedUpgradeCost = 25;
        private const int ExtraBallCost = 5;
        private const int ShootModeCost = 15;
        private const float SpeedUpgradeIncrement = 0.03f;

        public void AddMoney(int amount)
        {
            BankBalance += amount;
        }

        public bool CanAfford(ShopItem item)
        {
            return item switch
            {
                ShopItem.SpeedUpgrade => BankBalance >= SpeedUpgradeCost,
                ShopItem.ExtraBall => BankBalance >= ExtraBallCost,
                ShopItem.ShootMode => BankBalance >= ShootModeCost,
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
                    PaddleSpeedMultiplier += SpeedUpgradeIncrement;
                    return true;
                    
                case ShopItem.ExtraBall:
                    BankBalance -= ExtraBallCost;
                    ExtraBallsPurchased++;
                    return true;
                    
                case ShopItem.ShootMode:
                    BankBalance -= ShootModeCost;
                    StartWithShootMode = true;
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
    }

    public enum ShopItem
    {
        SpeedUpgrade,
        ExtraBall,
        ShootMode
    }
}
