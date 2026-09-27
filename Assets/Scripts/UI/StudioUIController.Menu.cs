using UnityEngine;
using UnityEngine.UI;

namespace MergeStudio.UI
{
    public sealed partial class StudioUIController
    {
        private Sprite _menuPill;
        private GameObject _menuDialog;
        private RectTransform _menuDialogContent;
        private Text _menuDialogTitle;
        private MenuIconGraphic _menuSoundIcon;

        private void BuildMenu()
        {
            RectTransform badge = Panel("Menu Welcome Badge", _safeRoot, Cream, _menuPill);
            SetRect(badge, 0.29f, 0.254f, 0.71f, 0.287f);
            AddShadow(badge.gameObject, new Color(0.04f, 0.18f, 0.17f, 0.28f), new Vector2(0, -5));
            Text welcome = Label("ÜÇÜNÜ EŞLEŞTİR", badge, 29, FontStyle.Bold, DeepTeal);
            Stretch(welcome.rectTransform, 6);
            welcome.raycastTarget = false;

            Button play = MenuPillButton("Home Play", "OYNA", _safeRoot, new Color32(255, 111, 77, 255), () =>
            {
                _inputLocked = true;
                _sceneRequest.RaiseEvent("Game");
            });
            SetRect(play.GetComponent<RectTransform>(), 0.16f, 0.145f, 0.84f, 0.235f);
            Text playLabel = play.GetComponentInChildren<Text>();
            playLabel.fontSize = playLabel.resizeTextMaxSize = 70;
            AddShadow(playLabel.gameObject, new Color(0.43f, 0.15f, 0.06f, 0.65f), new Vector2(0, -4));

            MenuRoundButton("Home Settings", new Vector2(0.89f, 0.945f), MenuIconGraphic.Symbol.Settings,
                () => ShowHomeDialog(true));
            _menuSoundIcon = MenuRoundButton("Home Sound", new Vector2(0.40f, 0.09f), MenuIconGraphic.Symbol.Sound,
                ToggleMenuSound);
            _menuSoundIcon.Muted = !_soundEnabled;
            MenuRoundButton("Home Help", new Vector2(0.60f, 0.09f), MenuIconGraphic.Symbol.Information,
                () => ShowHomeDialog(false));

            RectTransform overlay = Panel("Home Dialog", _safeRoot, new Color(0.015f, 0.10f, 0.11f, 0.86f), null);
            Stretch(overlay);
            _menuDialog = overlay.gameObject;
            RectTransform card = Panel("Home Dialog Card", overlay, Cream, _rounded);
            SetRect(card, 0.085f, 0.25f, 0.915f, 0.75f);
            AddShadow(card.gameObject, new Color(0, 0, 0, 0.25f), new Vector2(0, -12));
            _menuDialogTitle = Label("AYARLAR", card, 48, FontStyle.Bold, DeepTeal);
            SetRect(_menuDialogTitle.rectTransform, 0.08f, 0.79f, 0.92f, 0.96f);
            _menuDialogContent = new GameObject("Home Dialog Content", typeof(RectTransform)).GetComponent<RectTransform>();
            _menuDialogContent.SetParent(card, false);
            SetRect(_menuDialogContent, 0.07f, 0.26f, 0.93f, 0.78f);
            Button close = MenuPillButton("Home Dialog Close", "TAMAM", card, new Color32(255, 111, 77, 255),
                () => _menuDialog.SetActive(false));
            SetRect(close.GetComponent<RectTransform>(), 0.14f, 0.065f, 0.86f, 0.23f);
            _menuDialog.SetActive(false);
        }

        private Button MenuPillButton(string name, string title, Transform parent, Color faceColor,
            UnityEngine.Events.UnityAction action)
        {
            RectTransform rim = Panel(name, parent, new Color32(255, 221, 163, 255), _menuPill);
            AddShadow(rim.gameObject, new Color(0.26f, 0.16f, 0.10f, 0.70f), new Vector2(0, -9));
            RectTransform inset = Panel("Inner Bevel", rim, new Color32(204, 89, 45, 255), _menuPill);
            Stretch(inset, 6);
            RectTransform face = Panel("Button Face", inset, faceColor, _menuPill);
            Stretch(face, 5);
            Image faceImage = face.GetComponent<Image>();
            faceImage.raycastTarget = false;
            RectTransform glint = Panel("Top Highlight", face, new Color(1, 0.95f, 0.78f, 0.30f), _menuPill);
            SetRect(glint, 0.055f, 0.59f, 0.945f, 0.92f);
            glint.GetComponent<Image>().raycastTarget = false;
            Text label = Label(title, face, 40, FontStyle.Bold, Cream);
            Stretch(label.rectTransform, 5);
            label.raycastTarget = false;
            Button button = rim.gameObject.AddComponent<Button>();
            button.targetGraphic = faceImage;
            var colors = button.colors;
            colors.pressedColor = new Color(0.82f, 0.75f, 0.70f);
            colors.highlightedColor = new Color(1, 0.97f, 0.92f);
            colors.fadeDuration = 0.07f;
            button.colors = colors;
            button.onClick.AddListener(() => { if (!_inputLocked) action(); });
            return button;
        }

        private MenuIconGraphic MenuRoundButton(string name, Vector2 position, MenuIconGraphic.Symbol symbol,
            UnityEngine.Events.UnityAction action)
        {
            RectTransform rim = Panel(name, _safeRoot, new Color32(246, 220, 174, 255), _circle);
            rim.anchorMin = rim.anchorMax = position;
            rim.sizeDelta = new Vector2(126, 126);
            AddShadow(rim.gameObject, new Color(0.04f, 0.16f, 0.15f, 0.55f), new Vector2(0, -6));
            RectTransform face = Panel("Round Face", rim, Cream, _circle);
            Stretch(face, 6);
            face.GetComponent<Image>().raycastTarget = false;
            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(MenuIconGraphic));
            iconObject.transform.SetParent(face, false);
            Stretch(iconObject.GetComponent<RectTransform>(), 26);
            var icon = iconObject.GetComponent<MenuIconGraphic>();
            icon.Kind = symbol;
            icon.color = DeepTeal;
            icon.raycastTarget = false;
            Button button = rim.gameObject.AddComponent<Button>();
            button.targetGraphic = face.GetComponent<Image>();
            button.onClick.AddListener(action);
            return icon;
        }

        private void ToggleMenuSound()
        {
            _soundEnabled = !_soundEnabled;
            PlayerPrefs.SetInt("mixo.sound", _soundEnabled ? 1 : 0);
            PlayerPrefs.Save();
            _menuSoundIcon.Muted = !_soundEnabled;
        }

        private void ShowHomeDialog(bool settings)
        {
            foreach (Transform child in _menuDialogContent)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            _menuDialogTitle.text = settings ? "AYARLAR" : "NASIL OYNANIR?";
            if (settings)
            {
                Text soundText = null;
                Button sound = MenuPillButton("Home Sound Setting", _soundEnabled ? "SES: AÇIK" : "SES: KAPALI",
                    _menuDialogContent, DeepTeal, () =>
                    {
                        ToggleMenuSound();
                        soundText.text = _soundEnabled ? "SES: AÇIK" : "SES: KAPALI";
                    });
                soundText = sound.GetComponentInChildren<Text>();
                SetRect(sound.GetComponent<RectTransform>(), 0.03f, 0.56f, 0.97f, 0.92f);
                Text hapticText = null;
                Button haptic = MenuPillButton("Home Haptic Setting", _hapticsEnabled ? "TİTREŞİM: AÇIK" : "TİTREŞİM: KAPALI",
                    _menuDialogContent, DeepTeal, () =>
                    {
                        _hapticsEnabled = !_hapticsEnabled;
                        PlayerPrefs.SetInt("mixo.haptics", _hapticsEnabled ? 1 : 0);
                        PlayerPrefs.Save();
                        hapticText.text = _hapticsEnabled ? "TİTREŞİM: AÇIK" : "TİTREŞİM: KAPALI";
                    });
                hapticText = haptic.GetComponentInChildren<Text>();
                SetRect(haptic.GetComponent<RectTransform>(), 0.03f, 0.10f, 0.97f, 0.46f);
            }
            else
            {
                Text instructions = Label("Aynı üç yemeği seç ve eşleştir.\n\nHaznedeki 7 yuva dolmadan\ntüm yemekleri temizle.\n\nTakılırsan ipucu, geri alma veya\nkarıştırmayı kullan.",
                    _menuDialogContent, 34, FontStyle.Normal, DeepTeal);
                Stretch(instructions.rectTransform, 4);
            }
            _menuDialog.SetActive(true);
        }

        private static Sprite CreateMenuPill()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Mixo Menu Pill" };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f));
                    byte alpha = (byte)Mathf.Clamp((63f - distance) * 255f, 0, 255);
                    byte light = (byte)Mathf.Lerp(202, 255, y / 127f);
                    pixels[y * size + x] = new Color32(light, light, light, alpha);
                }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0,
                SpriteMeshType.FullRect, new Vector4(62, 62, 62, 62));
        }
    }
}
