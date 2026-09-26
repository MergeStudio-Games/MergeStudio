using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using MergeStudio.Core;
using MergeStudio.UI;

namespace MergeStudio.Tests
{
    public sealed class SceneSmokeTests
    {
        [UnityTest]
        public IEnumerator BootMenuAndGameWithoutRuntimeErrors()
        {
            yield return SceneManager.LoadSceneAsync("Init");
            float deadline = Time.realtimeSinceStartup + 30f;
            while (SceneManager.GetActiveScene().name != "MainMenu" && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name, "Init must open MainMenu.");
            yield return null;
            Assert.NotNull(Object.FindAnyObjectByType<StudioUIController>());
            yield return SceneManager.LoadSceneAsync("Game");
            for (int i = 0; i < 30; i++) yield return null;
            var manager = Object.FindAnyObjectByType<GameManager>();
            Assert.NotNull(manager);
            Assert.IsTrue(manager.enabled, "Save initialization must succeed.");
            Assert.NotNull(Object.FindAnyObjectByType<SafeArea>());
            var pause = GameObject.Find("Pause Button").GetComponent<UnityEngine.UI.Button>();
            pause.onClick.Invoke();
            yield return null;
            var overlay = GameObject.Find("Pause Overlay");
            Assert.NotNull(overlay, "Pause opens from the HUD.");
            var resume = System.Linq.Enumerable.First(overlay.GetComponentsInChildren<UnityEngine.UI.Button>(),
                button => button.GetComponentInChildren<UnityEngine.UI.Text>().text == "DEVAM ET");
            resume.onClick.Invoke();
            yield return null;
            Assert.IsFalse(overlay.activeSelf, "Resume closes pause overlay.");
            LogAssert.NoUnexpectedReceived();
            yield return SceneManager.LoadSceneAsync("MainMenu");
        }
    }
}
