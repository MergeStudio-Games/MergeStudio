using NUnit.Framework;
using UnityEngine;
using MergeStudio.Economy;
namespace MergeStudio.Tests
{
    public sealed class EconomyTests
    {
        [Test] public void SpendingCannotCreateMoney()
        {
            var wallet = new Currency(10); Assert.IsFalse(wallet.TrySpend(11)); Assert.AreEqual(10, wallet.Balance);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => wallet.TrySpend(-1));
            Assert.IsTrue(wallet.TrySpend(10)); Assert.AreEqual(0, wallet.Balance);
        }
        [Test] public void OverflowPreservesBalance()
        {
            var wallet = new Currency(int.MaxValue); Assert.Throws<System.OverflowException>(() => wallet.Add(1)); Assert.AreEqual(int.MaxValue, wallet.Balance);
        }
        [Test] public void EnergyPreservesPartialIntervalAndCaps()
        {
            var energy = new EnergySystem(1, 100, 10, 60); energy.Tick(225);
            Assert.AreEqual(3, energy.Current); Assert.AreEqual(220, energy.Timestamp);
            energy.Tick(10000); Assert.AreEqual(10, energy.Current);
            energy.TrySpend(1, 10000); energy.Tick(10001); Assert.AreEqual(9, energy.Current);
        }
        [Test] public void BackwardClockDoesNotGenerateEnergy()
        {
            var energy = new EnergySystem(1, 100, 10, 60); energy.Tick(90); Assert.AreEqual(1, energy.Current); Assert.AreEqual(90, energy.Timestamp);
        }
        [Test] public void ShopDoesNotChargeWhenFull()
        {
            var config = ScriptableObject.CreateInstance<EconomyConfigSO>();
            try { var wallet = new Currency(30); Assert.IsFalse(new ShopSystem().BuyEnergy(wallet, new EnergySystem(100, 0, 100, 120), config)); Assert.AreEqual(30, wallet.Balance); }
            finally { Object.DestroyImmediate(config); }
        }
        [Test] public void OfflineAdNeverGrantsReward()
        {
            bool rewarded = false, failed = false;
            new AdMediationService().ShowRewarded(() => rewarded = true, () => failed = true);
            Assert.IsFalse(rewarded); Assert.IsTrue(failed);
        }
        [Test] public void ShopChargesOnceAndAddsConfiguredEnergy()
        {
            var config = ScriptableObject.CreateInstance<EconomyConfigSO>();
            try
            {
                var wallet = new Currency(config.EnergyPackPrice);
                var energy = new EnergySystem(0, 0, config.MaxEnergy, config.EnergySeconds);
                var shop = new ShopSystem();
                Assert.IsTrue(shop.BuyEnergy(wallet, energy, config));
                Assert.AreEqual(0, wallet.Balance); Assert.AreEqual(config.EnergyPackAmount, energy.Current);
                Assert.IsFalse(shop.BuyEnergy(wallet, energy, config));
                config.EnergyPackAmount = -1;
                Assert.Throws<System.ArgumentOutOfRangeException>(() => shop.BuyEnergy(wallet, energy, config));
                Assert.AreEqual(0, wallet.Balance);
            }
            finally { Object.DestroyImmediate(config); }
        }
    }
}
