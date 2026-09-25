using System;
using System.Linq;
using UnityEngine;
using MergeStudio.Events;
using MergeStudio.Gameplay;
using MergeStudio.Persistence;

namespace MergeStudio.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private const int RestartCommand = -100;
        private const int ContinueCommand = -101;

        [SerializeField] private VoidEventChannelSO _spawnRequest, _orderRequest, _shopRequest;
        [SerializeField] private IntEventChannelSO _cellRequest, _goldChanged, _energyChanged;
        [SerializeField] private StringEventChannelSO _boardChanged;

        private SaveSystem _save;
        private SaveData _data;
        private TripleMatchGame _game;
        private string[] _itemIds;
        private bool _ready;
        private float _nextSave;

        private void Awake()
        {
            _save = new SaveSystem(Application.persistentDataPath);
            try { _data = _save.Load(); }
            catch (Exception error)
            {
                Debug.LogError("Save could not be recovered. Existing files preserved: " + error.Message);
                enabled = false;
                return;
            }

            _data.PlayerLevel = Mathf.Clamp(_data.PlayerLevel, 1, 100);
            _itemIds = Resources.LoadAll<Sprite>("MixoKitchen/PremiumFood")
                .Select(sprite => sprite.name)
                .Distinct()
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (_itemIds.Length < 6)
            {
                Debug.LogError("Mixo Kitchen requires at least six imported food sprites.");
                enabled = false;
                return;
            }
            StartLevel(_data.PlayerLevel);
            _ready = true;
        }

        private void OnEnable()
        {
            if (!_ready) return;
            _cellRequest.OnEventRaised += HandleCommand;
            _spawnRequest.OnEventRaised += Shuffle;
            _orderRequest.OnEventRaised += Undo;
            if (_shopRequest != null) _shopRequest.OnEventRaised += Hint;
        }

        private void Start()
        {
            if (_ready) Publish();
        }

        private void OnDisable()
        {
            if (!_ready) return;
            _cellRequest.OnEventRaised -= HandleCommand;
            _spawnRequest.OnEventRaised -= Shuffle;
            _orderRequest.OnEventRaised -= Undo;
            if (_shopRequest != null) _shopRequest.OnEventRaised -= Hint;
        }

        private void Update()
        {
            if (!_ready) return;
            if (_game.Tick(Time.unscaledDeltaTime)) Publish();
            if (Time.unscaledTime < _nextSave) return;
            Persist();
            _nextSave = Time.unscaledTime + 30;
        }

        private void HandleCommand(int command)
        {
            bool changed;
            if (command == RestartCommand)
            {
                StartLevel(_data.PlayerLevel);
                changed = true;
            }
            else if (command == ContinueCommand && _game.State == TripleMatchState.Won)
            {
                _data.PlayerLevel = Mathf.Min(100, _data.PlayerLevel + 1);
                _data.Gold = Math.Min(int.MaxValue, _data.Gold + 50 + _game.Level.Number * 5);
                StartLevel(_data.PlayerLevel);
                changed = true;
            }
            else changed = _game.Select(command);

            if (!changed) return;
            Publish();
            Persist();
        }

        private void Shuffle()
        {
            if (_game.ShuffleActive()) Publish();
        }

        private void Undo()
        {
            if (_game.Undo()) Publish();
        }

        private void Hint()
        {
            if (_game.Hint()) Publish();
        }

        private void StartLevel(int level)
        {
            _game = new TripleMatchGame(level, _itemIds);
        }

        private void Publish()
        {
            TripleMatchSnapshot snapshot = _game.Snapshot();
            _goldChanged.RaiseEvent(_data.Gold);
            _energyChanged.RaiseEvent(snapshot.SecondsRemaining);
            _boardChanged.RaiseEvent(JsonUtility.ToJson(snapshot));
        }

        private void Persist()
        {
            if (!_ready) return;
            try { _save.Save(_data); }
            catch (Exception error) { Debug.LogError("Save failed: " + error.Message); }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Persist();
        }

        private void OnApplicationQuit() => Persist();
    }
}
