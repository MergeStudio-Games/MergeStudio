using UnityEngine;
namespace MergeStudio.UI {
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour {
        private Rect _last; private Vector2Int _size;
        private void OnEnable() => Apply();
        public void Refresh() => Apply();
        private void Update() { if (_last != Screen.safeArea || _size.x != Screen.width || _size.y != Screen.height) Apply(); }
        public static Rect Normalize(Rect area, int width, int height) => new Rect(area.x / width, area.y / height, area.width / width, area.height / height);
        private void Apply() {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            _last = Screen.safeArea; _size = new Vector2Int(Screen.width, Screen.height);
            var normalized = Normalize(_last, _size.x, _size.y); var rect = (RectTransform)transform;
            rect.anchorMin = normalized.min; rect.anchorMax = normalized.max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
