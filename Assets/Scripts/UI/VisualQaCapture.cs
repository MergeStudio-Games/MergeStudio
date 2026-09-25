using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            var runner = new GameObject("Visual QA Capture", typeof(VisualQaCapture));
            DontDestroyOnLoad(runner);
        }

        private IEnumerator Start()
        {
            if (SceneManager.GetActiveScene().name != "Game")
            {
                SceneManager.LoadScene("Game");
                while (SceneManager.GetActiveScene().name != "Game") yield return null;
            }
            for (int i = 0; i < 20; i++) yield return null;
            yield return new WaitForEndOfFrame();

            string output = ReadArgument(OutputArgument);
            if (string.IsNullOrWhiteSpace(output)) output = Path.Combine(Application.dataPath, "..", "Build", "VisualQA", "MixoKitchen-Game.png");
            output = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var capture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            capture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            capture.Apply();
            File.WriteAllBytes(output, capture.EncodeToPNG());
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
