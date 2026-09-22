using System;
namespace MergeStudio.Gameplay {
    public sealed class MergeBoard {
        private readonly Item[] _cells;
        public int Count => _cells.Length;
        public Item this[int index] => _cells[index];
        public MergeBoard(int width, int height) { if (width < 1 || height < 1 || width > 100 || height > 100) throw new ArgumentOutOfRangeException(); _cells = new Item[width * height]; }
        public bool Place(int index, Item item) { if (item == null || string.IsNullOrEmpty(item.Id) || item.Tier < 1 || _cells[index] != null) return false; _cells[index] = new Item(item.Id, item.Tier); return true; }
        public bool Merge(int from, int to) {
            if (from == to || from < 0 || to < 0 || from >= Count || to >= Count) return false;
            var a = _cells[from]; var b = _cells[to];
            if (a == null || b == null || a.Id != b.Id || a.Tier != b.Tier || b.Tier >= 10) return false;
            _cells[to] = new Item(b.Id, b.Tier + 1); _cells[from] = null; return true;
        }
        public bool MoveOrMerge(int from, int to) {
            if (from == to || from < 0 || to < 0 || from >= Count || to >= Count || _cells[from] == null) return false;
            if (_cells[to] != null) return Merge(from, to);
            _cells[to] = _cells[from]; _cells[from] = null; return true;
        }
        public bool Consume(string id, int tier) { for (int i = 0; i < Count; i++) if (_cells[i]?.Id == id && _cells[i].Tier == tier) { _cells[i] = null; return true; } return false; }
    }
}
