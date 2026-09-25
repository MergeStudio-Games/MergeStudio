using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using MergeStudio.UI;

namespace MergeStudio.Tests
{
    public sealed class BoardCellInputTests
    {
        [UnityTest]
        public IEnumerator DragRoutesSourceAndTargetOnlyWithinItsBoard()
        {
            var root = new GameObject("Board");
            var other = new GameObject("OtherBoard");
            var eventSystem = Object.FindAnyObjectByType<EventSystem>();
            bool ownsEventSystem = eventSystem == null;
            if (ownsEventSystem) eventSystem = new GameObject("TestEvents", typeof(EventSystem)).GetComponent<EventSystem>();
            try
            {
                var source = new GameObject("Source", typeof(BoardCellInput)).GetComponent<BoardCellInput>();
                var target = new GameObject("Target", typeof(BoardCellInput)).GetComponent<BoardCellInput>();
                source.transform.SetParent(root.transform); target.transform.SetParent(root.transform);
                source.Index = 2; target.Index = 5;
                int calls = 0;
                target.MoveRequested = (from, to) => { Assert.AreEqual(2, from); Assert.AreEqual(5, to); calls++; };
                var pointer = new PointerEventData(eventSystem) { pointerDrag = source.gameObject };
                source.OnBeginDrag(pointer); target.OnDrop(pointer); source.OnEndDrag(pointer);
                Assert.AreEqual(1, calls); Assert.AreEqual(1f, source.GetComponent<CanvasGroup>().alpha);
                source.transform.SetParent(other.transform); target.OnDrop(pointer);
                Assert.AreEqual(1, calls);
            }
            finally { Object.Destroy(root); Object.Destroy(other); if (ownsEventSystem) Object.Destroy(eventSystem.gameObject); }
            yield return null;
        }
    }
}
