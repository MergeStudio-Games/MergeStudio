using System;
using System.Collections.Generic;
using UnityEngine;

namespace MergeStudio.Gameplay
{
    public static class FoodCatalog
    {
        [Serializable]
        private sealed class Manifest
        {
            public int TextureWidth;
            public int TextureHeight;
            public Entry[] Entries;
        }

        [Serializable]
        private sealed class Entry
        {
            public string Name;
            public int X, Y, Width, Height;
        }

        private static Sprite[] _sprites;

        public static IReadOnlyList<Sprite> Load()
        {
            if (_sprites != null && _sprites.Length > 0 && _sprites[0] != null) return _sprites;
            const string path = "MixoKitchen/UI/food-atlas-a";
            var texture = Resources.Load<Texture2D>(path);
            var data = Resources.Load<TextAsset>(path + "-layout");
            if (texture == null || data == null) throw new InvalidOperationException("Food atlas or sprite manifest is missing.");
            Manifest manifest = JsonUtility.FromJson<Manifest>(data.text);
            if (manifest == null || manifest.Entries == null || manifest.TextureWidth != texture.width || manifest.TextureHeight != texture.height)
                throw new InvalidOperationException("Food atlas dimensions do not match its sprite manifest.");
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (Entry entry in manifest.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name) || !names.Add(entry.Name) || entry.Width <= 0 || entry.Height <= 0 ||
                    entry.X < 0 || entry.Y < 0 || entry.X + entry.Width > texture.width || entry.Y + entry.Height > texture.height)
                    throw new InvalidOperationException("Invalid food sprite: " + entry.Name);
            }
            _sprites = new Sprite[manifest.Entries.Length];
            for (int i = 0; i < _sprites.Length; i++)
            {
                Entry entry = manifest.Entries[i];
                _sprites[i] = Sprite.Create(texture, new Rect(entry.X, entry.Y, entry.Width, entry.Height),
                    new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
                _sprites[i].name = entry.Name;
            }
            return _sprites;
        }
    }
}
