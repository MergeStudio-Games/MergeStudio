using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MergeStudio.UI
{
    public sealed class VisualQaCapture : MonoBehaviour
    {
        private const string EnableArgument = "-mixo-visual-qa";
        private const string OutputArgument = "-mixo-output";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartCapture()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            if (Array.IndexOf(arguments, EnableArgument) < 0 || FindAnyObjectByType<VisualQaCapture>() != null) return;
            Application.runInBackground = true;
            var runner = new GameObject("Visual QA Capture", typeof(VisualQaCapture));
            DontDestroyOnLoad(runner);
        }

        private IEnumerator Start()
        {
            string scene = ReadArgument("-mixo-scene") ?? "Game";
            // Let the normal bootstrap finish before selecting the requested QA scene.
            while (SceneManager.GetActiveScene().name == "Init") yield return null;
            if (SceneManager.GetActiveScene().name != scene)
            {
                yield return SceneManager.LoadSceneAsync(scene);
            }
            for (int i = 0; i < 20; i++) yield return null;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-mixo-result") >= 0)
            {
                var foods = GameObject.Find("Tiles").GetComponentsInChildren<Button>()
                    .OrderBy(button => button.transform.Find("Food").GetComponent<Image>().sprite.name).ToArray();
                foreach (Button food in foods)
                {
                    if (food != null) food.onClick.Invoke();
                    yield return new WaitForSecondsRealtime(0.32f);
                }
            }
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-mixo-pause") >= 0)
            {
                GameObject.Find("Pause Button").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                yield return null;
            }
            yield return new WaitForEndOfFrame();

            string output = ReadArgument(OutputArgument);
            if (string.IsNullOrWhiteSpace(output)) output = Path.Combine(Application.dataPath, "..", "Build", "VisualQA", "MixoKitchen-Game.png");
            output = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            // Explicit camera rendering also works when the QA player window is hidden.
            // This path is enabled only by the QA command-line flag.
            var cameraObject = new GameObject("QA Capture Camera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.transform.position = new Vector3(0, 0, -100);
            var target = new RenderTexture(Screen.width, Screen.height, 24);
            camera.targetTexture = target;
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (!canvas.isRootCanvas) continue;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 10;
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var capture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            capture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            capture.Apply();
            File.WriteAllBytes(output, capture.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            target.Release();
            Destroy(target);
            Destroy(cameraObject);
            Destroy(capture);
            for (int i = 0; i < 5; i++) yield return null;
            Application.Quit(File.Exists(output) ? 0 : 2);
        }

        private static string ReadArgument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(arguments, name);
            return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
        }
    }
}
