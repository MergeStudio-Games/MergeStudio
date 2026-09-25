namespace MergeStudio.Economy {
    public sealed class ShopSystem {
        public bool BuyEnergy(Currency diamonds, EnergySystem energy, EconomyConfigSO config) {
            if (config.EnergyPackPrice < 1 || config.EnergyPackAmount < 1) throw new System.ArgumentOutOfRangeException(nameof(config));
            if (energy.Current >= config.MaxEnergy || !diamonds.TrySpend(config.EnergyPackPrice)) return false;
            energy.Add(config.EnergyPackAmount); return true;
        }
    }
}
