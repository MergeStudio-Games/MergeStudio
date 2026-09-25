using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MergeStudio.UI
{
    // Widget callbacks stay inside the owning view; it publishes gameplay channels.
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BoardCellInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        public int Index { get; set; }
        public Action<int, int> MoveRequested { get; set; }
        private CanvasGroup _group;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0.55f;
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnDrop(PointerEventData eventData)
        {
            var source = eventData.pointerDrag == null ? null : eventData.pointerDrag.GetComponent<BoardCellInput>();
            if (source == null || source.transform.parent != transform.parent || source.Index == Index) return;
            MoveRequested?.Invoke(source.Index, Index);
        }

        public void OnEndDrag(PointerEventData eventData) { if (_group != null) _group.alpha = 1f; }
        private void OnDisable() { if (_group != null) _group.alpha = 1f; }
    }
}
