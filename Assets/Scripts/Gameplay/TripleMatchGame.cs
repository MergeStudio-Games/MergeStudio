using System;
using System.Collections.Generic;
using System.Linq;

namespace MergeStudio.Gameplay
{
    public enum TripleMatchState
    {
        Playing,
        Won,
        Lost
    }

    [Serializable]
    public sealed class TripleTileSnapshot
    {
        public int Index;
        public string ItemId;
        public float X;
        public float Y;
        public int Layer;
        public bool Active;
        public bool Highlighted;
    }

    [Serializable]
    public sealed class TripleMatchSnapshot
    {
        public int Level;
        public int Score;
        public int Combo;
        public int SecondsRemaining;
        public int TotalTiles;
        public int ClearedTiles;
        public int UndoRemaining;
        public int HintRemaining;
        public int ShuffleRemaining;
        public string State;
        public List<TripleTileSnapshot> Tiles = new List<TripleTileSnapshot>();
        public List<string> Tray = new List<string>();
    }

    public readonly struct TripleMatchLevel
    {
        public TripleMatchLevel(int number, int typeCount, int tripleCount, int seconds, int layers)
        {
            Number = number;
            TypeCount = typeCount;
            TripleCount = tripleCount;
            Seconds = seconds;
            Layers = layers;
        }

        public int Number { get; }
        public int TypeCount { get; }
        public int TripleCount { get; }
        public int Seconds { get; }
        public int Layers { get; }

        public static TripleMatchLevel For(int level)
        {
            level = Math.Max(1, Math.Min(100, level));
            int types = Math.Min(34, 6 + (level - 1) / 4);
            int triples = Math.Min(60, 8 + (level - 1) / 2);
            int layers = Math.Min(6, 2 + (level - 1) / 20);
            int seconds = level <= 5 ? 0 : Math.Max(95, 215 - level);
            return new TripleMatchLevel(level, types, triples, seconds, layers);
        }
    }

    public sealed class TripleMatchGame
    {
        private sealed class Tile
        {
            public int Index;
            public string ItemId;
            public float X;
            public float Y;
            public int Layer;
            public bool Active = true;
        }

        private sealed class Turn
        {
            public bool[] Active;
            public List<string> Tray;
            public int Score;
            public int Combo;
        }

        private readonly List<Tile> _tiles = new List<Tile>();
        private readonly List<string> _tray = new List<string>();
        private readonly Stack<Turn> _history = new Stack<Turn>();
        private readonly Random _random;
        private readonly int _totalTiles;
        private int _hintedIndex = -1;
        private float _remaining;

        public TripleMatchGame(int level, IReadOnlyList<string> itemIds)
        {
            if (itemIds == null || itemIds.Count < 6) throw new ArgumentException("At least six item types are required.", nameof(itemIds));
            Level = TripleMatchLevel.For(level);
            _random = new Random(0x4D49584F ^ Level.Number * 7919);
            _remaining = Level.Seconds;
            UndoRemaining = 3;
            HintRemaining = 3;
            ShuffleRemaining = 3;

            var available = itemIds.OrderBy(value => value, StringComparer.Ordinal).Take(Level.TypeCount).ToArray();
            var sequence = new List<string>(Level.TripleCount * 3);
            for (int i = 0; i < Level.TripleCount; i++)
            {
                string item = available[i % available.Length];
                sequence.Add(item);
                sequence.Add(item);
                sequence.Add(item);
            }
            Shuffle(sequence);

            int count = sequence.Count;
            for (int i = 0; i < count; i++)
            {
                int layer = i % Level.Layers;
                double angle = i * 2.399963229728653 + _random.NextDouble() * 0.38;
                double radius = Math.Sqrt((i + 0.5) / count) * 0.43;
                float x = Clamp01(0.5f + (float)(Math.Cos(angle) * radius) + Jitter(0.035f));
                float y = Clamp01(0.5f + (float)(Math.Sin(angle) * radius * 0.86) + Jitter(0.035f));
                _tiles.Add(new Tile { Index = i, ItemId = sequence[i], X = x, Y = y, Layer = layer });
            }
            _totalTiles = count;
        }

        public TripleMatchLevel Level { get; }
        public TripleMatchState State { get; private set; } = TripleMatchState.Playing;
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int UndoRemaining { get; private set; }
        public int HintRemaining { get; private set; }
        public int ShuffleRemaining { get; private set; }
        public IReadOnlyList<string> Tray => _tray;

        public bool Select(int index)
        {
            if (State != TripleMatchState.Playing || index < 0 || index >= _tiles.Count || !_tiles[index].Active) return false;
            SaveTurn();
            _hintedIndex = -1;
            var tile = _tiles[index];
            tile.Active = false;
            int insertAt = _tray.FindLastIndex(value => value == tile.ItemId);
            if (insertAt < 0) _tray.Add(tile.ItemId);
            else _tray.Insert(insertAt + 1, tile.ItemId);

            int matches = _tray.Count(value => value == tile.ItemId);
            if (matches >= 3)
            {
                for (int i = _tray.Count - 1, removed = 0; i >= 0 && removed < 3; i--)
                {
                    if (_tray[i] != tile.ItemId) continue;
                    _tray.RemoveAt(i);
                    removed++;
                }
                Combo++;
                Score += 100 + Math.Min(10, Combo) * 20;
            }
            else Combo = 0;

            if (_tiles.All(value => !value.Active)) State = TripleMatchState.Won;
            else if (_tray.Count >= 7) State = TripleMatchState.Lost;
            return true;
        }

        public bool Undo()
        {
            if (State != TripleMatchState.Playing || UndoRemaining <= 0 || _history.Count == 0) return false;
            Turn turn = _history.Pop();
            for (int i = 0; i < _tiles.Count; i++) _tiles[i].Active = turn.Active[i];
            _tray.Clear();
            _tray.AddRange(turn.Tray);
            Score = turn.Score;
            Combo = turn.Combo;
            _hintedIndex = -1;
            UndoRemaining--;
            return true;
        }

        public bool Hint()
        {
            if (State != TripleMatchState.Playing || HintRemaining <= 0) return false;
            string wanted = _tray.GroupBy(value => value).OrderByDescending(group => group.Count()).Select(group => group.Key).FirstOrDefault();
            Tile tile = _tiles.FirstOrDefault(value => value.Active && (wanted == null || value.ItemId == wanted));
            if (tile == null) return false;
            _hintedIndex = tile.Index;
            HintRemaining--;
            return true;
        }

        public bool ShuffleActive()
        {
            if (State != TripleMatchState.Playing || ShuffleRemaining <= 0) return false;
            var positions = _tiles.Where(value => value.Active).Select(value => (value.X, value.Y, value.Layer)).ToList();
            Shuffle(positions);
            int position = 0;
            foreach (Tile tile in _tiles.Where(value => value.Active))
            {
                (tile.X, tile.Y, tile.Layer) = positions[position++];
            }
            _hintedIndex = -1;
            ShuffleRemaining--;
            return true;
        }

        public bool Tick(float seconds)
        {
            if (State != TripleMatchState.Playing || Level.Seconds <= 0 || seconds <= 0) return false;
            int previous = (int)Math.Ceiling(_remaining);
            _remaining = Math.Max(0, _remaining - seconds);
            if (_remaining <= 0) State = TripleMatchState.Lost;
            return previous != (int)Math.Ceiling(_remaining) || State == TripleMatchState.Lost;
        }

        public TripleMatchSnapshot Snapshot()
        {
            var snapshot = new TripleMatchSnapshot
            {
                Level = Level.Number,
                Score = Score,
                Combo = Combo,
                SecondsRemaining = Level.Seconds <= 0 ? -1 : (int)Math.Ceiling(_remaining),
                TotalTiles = _totalTiles,
                ClearedTiles = _tiles.Count(value => !value.Active),
                UndoRemaining = UndoRemaining,
                HintRemaining = HintRemaining,
                ShuffleRemaining = ShuffleRemaining,
                State = State.ToString(),
                Tray = new List<string>(_tray)
            };
            foreach (Tile tile in _tiles)
            {
                snapshot.Tiles.Add(new TripleTileSnapshot
                {
                    Index = tile.Index,
                    ItemId = tile.ItemId,
                    X = tile.X,
                    Y = tile.Y,
                    Layer = tile.Layer,
                    Active = tile.Active,
                    Highlighted = tile.Index == _hintedIndex
                });
            }
            return snapshot;
        }

        private void SaveTurn()
        {
            _history.Push(new Turn
            {
                Active = _tiles.Select(value => value.Active).ToArray(),
                Tray = new List<string>(_tray),
                Score = Score,
                Combo = Combo
            });
            while (_history.Count > 12)
            {
                var turns = _history.Reverse().Skip(1).Reverse().ToArray();
                _history.Clear();
                foreach (Turn turn in turns) _history.Push(turn);
            }
        }

        private float Jitter(float amount) => (float)(_random.NextDouble() * amount * 2 - amount);
        private static float Clamp01(float value) => Math.Max(0.04f, Math.Min(0.96f, value));

        private void Shuffle<T>(IList<T> values)
        {
            for (int i = values.Count - 1; i > 0; i--)
            {
                int swap = _random.Next(i + 1);
                (values[i], values[swap]) = (values[swap], values[i]);
            }
        }
    }
}
