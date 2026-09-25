using System;
using UnityEngine;
using MergeStudio.Economy;
using MergeStudio.Events;
using MergeStudio.Gameplay;
using MergeStudio.Persistence;

namespace MergeStudio.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private EconomyConfigSO _economy;
        [SerializeField] private OrderConfigSO _order;
        [SerializeField] private VoidEventChannelSO _spawnRequest, _orderRequest, _shopRequest;
        [SerializeField] private IntEventChannelSO _cellRequest, _goldGained, _goldChanged, _energyChanged;
        [SerializeField] private StringEventChannelSO _boardChanged, _orderFulfilled;
        private SaveSystem _save;
        private SaveData _data;
        private MergeBoard _board;
        private Currency _gold;
        private Currency _diamonds;
        private EnergySystem _energy;
        private OrderSystem _orders;
        private int _selected = -1;
        private float _nextSave;
        private bool _ready;

        private void Awake()
        {
            _save = new SaveSystem(Application.persistentDataPath);
            try { _data = _save.Load(); }
            catch (Exception error) { Debug.LogError("Save could not be recovered. Existing files preserved: " + error.Message); enabled = false; return; }
            _board = new MergeBoard(_data.BoardWidth, _data.BoardHeight);
            for (int i = 0; i < _data.Board.Count; i++)
            {
                var cell = _data.Board[i];
                if (cell != null && cell.Tier > 0) _board.Place(i, new Item(cell.ItemId, cell.Tier));
            }
            _gold = new Currency(_data.Gold);
            _diamonds = new Currency(_data.Diamonds);
            _energy = new EnergySystem(_data.Energy, _data.EnergyTimestamp == 0 ? Now : _data.EnergyTimestamp, _economy.MaxEnergy, _economy.EnergySeconds);
            _orders = new OrderSystem(_data.CompletedOrders, _goldGained, _orderFulfilled);
            _ready = true;
        }
        private static long Now => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        private void OnEnable()
        {
            if (!_ready) return;
            _spawnRequest.OnEventRaised += Spawn;
            _orderRequest.OnEventRaised += Fulfill;
            if (_shopRequest != null) _shopRequest.OnEventRaised += BuyEnergy;
            _cellRequest.OnEventRaised += Select;
            _goldGained.OnEventRaised += AddGold;
            _orderFulfilled.OnEventRaised += CompleteOrder;
        }
        private void Start() { if (_ready) Publish(); }
        private void OnDisable()
        {
            if (!_ready) return;
            _spawnRequest.OnEventRaised -= Spawn;
            _orderRequest.OnEventRaised -= Fulfill;
            if (_shopRequest != null) _shopRequest.OnEventRaised -= BuyEnergy;
            _cellRequest.OnEventRaised -= Select;
            _goldGained.OnEventRaised -= AddGold;
            _orderFulfilled.OnEventRaised -= CompleteOrder;
        }
        private void Update()
        {
            if (!_ready) return;
            int previous = _energy.Current; _energy.Tick(Now);
            if (previous != _energy.Current) _energyChanged.RaiseEvent(_energy.Current);
            if (Time.unscaledTime >= _nextSave) { Persist(); _nextSave = Time.unscaledTime + 30; }
        }
        private void Spawn()
        {
            for (int i = 0; i < _board.Count; i++)
            {
                if (_board[i] != null) continue;
                if (!_energy.TrySpend(_economy.SpawnCost, Now)) return;
                _board.Place(i, new Item(_order.ItemId, UnityEngine.Random.value < _economy.BonusDropRate ? 2 : 1));
                Publish(); Persist(); return;
            }
        }
        private void Select(int index)
        {
            // UI uses -1 to cancel a pending tap before an explicit drag pair.
            if (index == -1) { _selected = -1; return; }
            if (index < 0 || index >= _board.Count) return;
            if (_selected < 0) { if (_board[index] != null) _selected = index; }
            else { _board.MoveOrMerge(_selected, index); _selected = -1; Publish(); Persist(); }
        }
        private void Fulfill()
        {
            if ((long)_gold.Balance + _order.GoldReward > int.MaxValue) return;
            if (_orders.Fulfill(_order, _board)) { Publish(); Persist(); }
        }
        private void BuyEnergy()
        {
            _energy.Tick(Now);
            if (new ShopSystem().BuyEnergy(_diamonds, _energy, _economy)) { Publish(); Persist(); }
        }
        private void AddGold(int amount) { _gold.Add(amount); _goldChanged.RaiseEvent(_gold.Balance); }
        private void CompleteOrder(string id) { if (!_data.CompletedOrders.Contains(id)) _data.CompletedOrders.Add(id); }
        private void Capture()
        {
            _data.Gold = _gold.Balance; _data.Diamonds = _diamonds.Balance; _data.Energy = _energy.Current; _data.EnergyTimestamp = _energy.Timestamp;
            _data.Board.Clear();
            for (int i = 0; i < _board.Count; i++) { var item = _board[i]; _data.Board.Add(new BoardCell { ItemId = item?.Id, Tier = item?.Tier ?? 0 }); }
        }
        private void Publish()
        {
            _selected = -1;
            Capture(); _goldChanged.RaiseEvent(_gold.Balance); _energyChanged.RaiseEvent(_energy.Current);
            _boardChanged.RaiseEvent(Serialization.ToJson(_data));
        }
        private void Persist()
        {
            if (!_ready) return;
            try { Capture(); _save.Save(_data); }
            catch (Exception error) { Debug.LogError("Save failed: " + error.Message); }
        }
        private void OnApplicationPause(bool paused) { if (paused) Persist(); }
        private void OnApplicationQuit() => Persist();
    }
}
