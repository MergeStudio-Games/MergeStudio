using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using MergeStudio.Events;
using MergeStudio.Gameplay;

namespace MergeStudio.UI
{
    public sealed class StudioUIController : MonoBehaviour
    {
        private static readonly Color Navy = new Color32(25, 35, 59, 255);
        private static readonly Color Cream = new Color32(255, 248, 228, 255);
        private static readonly Color Coral = new Color32(255, 105, 92, 255);
        private static readonly Color Orange = new Color32(255, 174, 66, 255);
        private static readonly Color Mint = new Color32(82, 203, 173, 255);
        private static readonly Color Sky = new Color32(92, 174, 255, 255);

        [SerializeField] private bool _menu;
        [SerializeField] private StringEventChannelSO _sceneRequest, _boardChanged;
        [SerializeField] private VoidEventChannelSO _spawnRequest, _orderRequest, _shopRequest;
        [SerializeField] private IntEventChannelSO _cellRequest, _goldChanged, _energyChanged;

        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private readonly Dictionary<int, Button> _tileButtons = new Dictionary<int, Button>();
        private readonly List<Image> _trayImages = new List<Image>();
        private readonly List<RectTransform> _floaters = new List<RectTransform>();
        private Font _font;
        private Sprite _rounded;
        private RectTransform _safeRoot;
        private RectTransform _boardArea;
        private RectTransform _tileLayer;
        private RectTransform _effectsLayer;
        private Image _progressFill;
        private Text _levelText;
        private Text _scoreText;
        private Text _timerText;
        private Text _statusText;
        private Text _undoCount;
        private Text _hintCount;
        private Text _shuffleCount;
        private GameObject _resultOverlay;
        private Text _resultTitle;
        private Text _resultSubtitle;
        private Text _resultButtonText;
        private AudioSource _audio;
        private AudioClip _tapSound;
        private AudioClip _matchSound;
        private TripleMatchSnapshot _snapshot;
        private int _previousCleared;
        private bool _inputLocked;
        private float _time;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _rounded = CreateRoundedSprite();
            foreach (Sprite sprite in Resources.LoadAll<Sprite>("MixoKitchen/Food")) _sprites[sprite.name] = sprite;
            BuildCanvas();
            if (_menu) BuildMenu();
            else BuildGame();
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private void OnEnable()
        {
            if (_menu) return;
            if (_boardChanged != null) _boardChanged.OnEventRaised += BoardChanged;
            if (_goldChanged != null) _goldChanged.OnEventRaised += GoldChanged;
        }

        private void OnDisable()
        {
            if (_menu) return;
            if (_boardChanged != null) _boardChanged.OnEventRaised -= BoardChanged;
            if (_goldChanged != null) _goldChanged.OnEventRaised -= GoldChanged;
        }

        private void Update()
        {
            _time += Time.unscaledDeltaTime;
            for (int i = 0; i < _floaters.Count; i++)
            {
                if (_floaters[i] == null) continue;
                float wave = Mathf.Sin(_time * (0.75f + i * 0.08f) + i) * 10f;
                _floaters[i].localRotation = Quaternion.Euler(0, 0, wave * 0.35f);
                _floaters[i].localScale = Vector3.one * (1f + Mathf.Sin(_time * 1.2f + i) * 0.035f);
            }
            if (_snapshot == null) return;
            foreach (TripleTileSnapshot tile in _snapshot.Tiles)
            {
                if (!tile.Highlighted || !_tileButtons.TryGetValue(tile.Index, out Button button)) continue;
                button.transform.localScale = Vector3.one * (1f + Mathf.Sin(_time * 9f) * 0.08f);
            }
        }

        private void BuildCanvas()
        {
            var canvasObject = new GameObject("Mixo Kitchen Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Image background = CreateImage("Background", canvasObject.transform, new Color32(255, 193, 93, 255));
            Stretch(background.rectTransform);
            AddBand(canvasObject.transform, new Color32(255, 118, 94, 255), 0.72f, 1f, -8f);
            AddBand(canvasObject.transform, new Color32(255, 220, 125, 255), 0.42f, 0.75f, 5f);
            AddBand(canvasObject.transform, new Color32(110, 214, 190, 255), 0f, 0.44f, -4f);

            var safe = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeArea));
            safe.transform.SetParent(canvasObject.transform, false);
            _safeRoot = safe.GetComponent<RectTransform>();
            Stretch(_safeRoot);
        }

        private void AddBand(Transform parent, Color color, float minY, float maxY, float angle)
        {
            Image band = CreateImage("Background Band", parent, color);
            RectTransform rect = band.rectTransform;
            rect.anchorMin = new Vector2(-0.08f, minY);
            rect.anchorMax = new Vector2(1.08f, maxY);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            band.raycastTarget = false;
        }

        private void BuildMenu()
        {
            Text brand = Label("MIXO", _safeRoot, 118, FontStyle.Bold, Color.white);
            SetRect(brand.rectTransform, 0.08f, 0.78f, 0.92f, 0.92f);
            Text kitchen = Label("KITCHEN", _safeRoot, 58, FontStyle.Bold, Navy);
            SetRect(kitchen.rectTransform, 0.08f, 0.73f, 0.92f, 0.81f);
            Text tagline = Label("EŞLEŞTİR  •  PİŞİR  •  KEŞFET", _safeRoot, 26, FontStyle.Bold, new Color(1, 1, 1, 0.92f));
            SetRect(tagline.rectTransform, 0.08f, 0.68f, 0.92f, 0.74f);

            string[] heroes = { "burger-cheese-double", "pizza", "cake-birthday", "avocado", "fries" };
            for (int i = 0; i < heroes.Length; i++)
            {
                var card = Panel("Hero Food", _safeRoot, new Color(1, 1, 1, 0.94f), _rounded);
                float x = 0.16f + i * 0.17f;
                SetRect(card, x - 0.075f, 0.43f + (i % 2) * 0.045f, x + 0.075f, 0.58f + (i % 2) * 0.045f);
                card.localRotation = Quaternion.Euler(0, 0, -10 + i * 5);
                AddShadow(card.gameObject, new Color(0.18f, 0.1f, 0.08f, 0.23f), new Vector2(0, -10));
                Image icon = CreateImage("Food", card, Color.white);
                Stretch(icon.rectTransform, 18);
                icon.preserveAspect = true;
                if (_sprites.TryGetValue(heroes[i], out Sprite sprite)) icon.sprite = sprite;
                _floaters.Add(card);
            }

            Button play = Button("OYNA", _safeRoot, Coral, () => _sceneRequest.RaiseEvent("Game"), out Text playText);
            SetRect(play.GetComponent<RectTransform>(), 0.15f, 0.19f, 0.85f, 0.29f);
            playText.fontSize = 52;
            AddShadow(play.gameObject, new Color(0.35f, 0.08f, 0.06f, 0.30f), new Vector2(0, -12));

            Text note = Label("100 renkli bölüm seni bekliyor", _safeRoot, 27, FontStyle.Bold, Navy);
            SetRect(note.rectTransform, 0.1f, 0.12f, 0.9f, 0.18f);
        }

        private void BuildGame()
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0;
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            _tapSound = Tone("Tap", 720, 0.07f, 0.09f);
            _matchSound = Tone("Match", 980, 0.22f, 0.14f);

            RectTransform header = Panel("Header", _safeRoot, new Color(1, 1, 1, 0.94f), _rounded);
            SetRect(header, 0.035f, 0.875f, 0.965f, 0.975f);
            AddShadow(header.gameObject, new Color(0.12f, 0.08f, 0.1f, 0.22f), new Vector2(0, -8));
            _levelText = Label("BÖLÜM 1", header, 34, FontStyle.Bold, Navy);
            SetRect(_levelText.rectTransform, 0.04f, 0.48f, 0.42f, 0.92f);
            _levelText.alignment = TextAnchor.MiddleLeft;
            _scoreText = Label("0", header, 30, FontStyle.Bold, Coral);
            SetRect(_scoreText.rectTransform, 0.70f, 0.48f, 0.94f, 0.92f);
            _scoreText.alignment = TextAnchor.MiddleRight;
            _timerText = Label("∞", header, 29, FontStyle.Bold, Navy);
            SetRect(_timerText.rectTransform, 0.43f, 0.48f, 0.69f, 0.92f);
            RectTransform progress = Panel("Progress", header, new Color32(228, 232, 234, 255), _rounded);
            SetRect(progress, 0.04f, 0.16f, 0.96f, 0.38f);
            _progressFill = CreateImage("Fill", progress, Mint);
            RectTransform fill = _progressFill.rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.pivot = new Vector2(0, 0.5f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;

            _boardArea = Panel("Food Pile", _safeRoot, new Color(1, 0.985f, 0.93f, 0.95f), _rounded);
            SetRect(_boardArea, 0.035f, 0.285f, 0.965f, 0.855f);
            AddShadow(_boardArea.gameObject, new Color(0.12f, 0.08f, 0.08f, 0.2f), new Vector2(0, -10));
            _tileLayer = new GameObject("Tiles", typeof(RectTransform)).GetComponent<RectTransform>();
            _tileLayer.SetParent(_boardArea, false);
            Stretch(_tileLayer, 25);
            _effectsLayer = new GameObject("Effects", typeof(RectTransform)).GetComponent<RectTransform>();
            _effectsLayer.SetParent(_safeRoot, false);
            Stretch(_effectsLayer);

            _statusText = Label("Aynı üç yemeği eşleştir", _boardArea, 25, FontStyle.Bold, new Color32(105, 94, 89, 255));
            SetRect(_statusText.rectTransform, 0.1f, 0.015f, 0.9f, 0.08f);

            RectTransform tray = Panel("Tray", _safeRoot, Navy, _rounded);
            SetRect(tray, 0.035f, 0.155f, 0.965f, 0.265f);
            AddShadow(tray.gameObject, new Color(0.08f, 0.06f, 0.12f, 0.32f), new Vector2(0, -10));
            var trayLayout = tray.gameObject.AddComponent<HorizontalLayoutGroup>();
            trayLayout.padding = new RectOffset(18, 18, 18, 18);
            trayLayout.spacing = 10;
            trayLayout.childAlignment = TextAnchor.MiddleCenter;
            trayLayout.childControlWidth = trayLayout.childControlHeight = true;
            trayLayout.childForceExpandWidth = trayLayout.childForceExpandHeight = true;
            for (int i = 0; i < 7; i++)
            {
                RectTransform slot = Panel("Slot " + (i + 1), tray, new Color(1, 1, 1, 0.13f), _rounded);
                var element = slot.gameObject.AddComponent<LayoutElement>();
                element.flexibleWidth = element.flexibleHeight = 1;
                Image icon = CreateImage("Food", slot, Color.white);
                Stretch(icon.rectTransform, 11);
                icon.preserveAspect = true;
                icon.enabled = false;
                _trayImages.Add(icon);
            }

            RectTransform tools = new GameObject("Boosters", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            tools.SetParent(_safeRoot, false);
            SetRect(tools, 0.035f, 0.035f, 0.965f, 0.135f);
            var toolsLayout = tools.GetComponent<HorizontalLayoutGroup>();
            toolsLayout.spacing = 18;
            toolsLayout.childAlignment = TextAnchor.MiddleCenter;
            toolsLayout.childControlWidth = toolsLayout.childControlHeight = true;
            toolsLayout.childForceExpandWidth = toolsLayout.childForceExpandHeight = true;
            AddToolButton("↶", "GERİ", Coral, tools, () => _orderRequest.RaiseEvent(), out _undoCount);
            AddToolButton("★", "İPUCU", Orange, tools, () => _shopRequest.RaiseEvent(), out _hintCount);
            AddToolButton("↻", "KARIŞTIR", Sky, tools, () => _spawnRequest.RaiseEvent(), out _shuffleCount);

            BuildResultOverlay();
        }

        private void AddToolButton(string icon, string title, Color color, Transform parent, UnityEngine.Events.UnityAction action, out Text count)
        {
            Button button = Button(icon + "  " + title, parent, color, action, out Text label);
            button.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            label.fontSize = 26;
            count = Label("3", button.transform, 21, FontStyle.Bold, Color.white);
            count.alignment = TextAnchor.LowerRight;
            SetRect(count.rectTransform, 0.72f, 0.04f, 0.94f, 0.40f);
            AddShadow(button.gameObject, new Color(0.1f, 0.05f, 0.05f, 0.2f), new Vector2(0, -6));
        }

        private void BuildResultOverlay()
        {
            _resultOverlay = new GameObject("Result Overlay", typeof(RectTransform), typeof(Image));
            _resultOverlay.transform.SetParent(_safeRoot, false);
            Stretch((RectTransform)_resultOverlay.transform);
            _resultOverlay.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.72f);
            RectTransform card = Panel("Result Card", _resultOverlay.transform, Cream, _rounded);
            SetRect(card, 0.10f, 0.31f, 0.90f, 0.70f);
            AddShadow(card.gameObject, new Color(0.02f, 0.02f, 0.05f, 0.38f), new Vector2(0, -14));
            _resultTitle = Label("HARİKA!", card, 62, FontStyle.Bold, Coral);
            SetRect(_resultTitle.rectTransform, 0.08f, 0.65f, 0.92f, 0.88f);
            _resultSubtitle = Label("Bütün yemekleri eşleştirdin", card, 30, FontStyle.Bold, Navy);
            SetRect(_resultSubtitle.rectTransform, 0.08f, 0.43f, 0.92f, 0.66f);
            Button action = Button("DEVAM ET", card, Mint, ResultAction, out _resultButtonText);
            SetRect(action.GetComponent<RectTransform>(), 0.12f, 0.12f, 0.88f, 0.34f);
            _resultOverlay.SetActive(false);
        }

        private void ResultAction()
        {
            if (_snapshot == null) return;
            _cellRequest.RaiseEvent(_snapshot.State == TripleMatchState.Won.ToString() ? -101 : -100);
        }

        private void BoardChanged(string json)
        {
            TripleMatchSnapshot next = JsonUtility.FromJson<TripleMatchSnapshot>(json);
            if (next == null) return;
            bool matched = _snapshot != null && next.ClearedTiles - _previousCleared >= 3 && next.Tray.Count < _snapshot.Tray.Count;
            _snapshot = next;
            _previousCleared = next.ClearedTiles;
            RenderHeader();
            RenderTiles();
            RenderTray();
            RenderResult();
            if (matched)
            {
                _audio.PlayOneShot(_matchSound);
                StartCoroutine(Burst());
            }
        }

        private void RenderHeader()
        {
            _levelText.text = "BÖLÜM " + _snapshot.Level + " / 100";
            _scoreText.text = "★ " + _snapshot.Score;
            _timerText.text = _snapshot.SecondsRemaining < 0 ? "RAHAT MOD" : FormatTime(_snapshot.SecondsRemaining);
            _timerText.color = _snapshot.SecondsRemaining >= 0 && _snapshot.SecondsRemaining < 30 ? Coral : Navy;
            float progress = _snapshot.TotalTiles <= 0 ? 0 : (float)_snapshot.ClearedTiles / _snapshot.TotalTiles;
            _progressFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(progress), 1);
            _statusText.text = _snapshot.Combo > 1 ? "MUHTEŞEM SERİ  x" + _snapshot.Combo : "Aynı üç yemeği eşleştir";
            _undoCount.text = _snapshot.UndoRemaining.ToString();
            _hintCount.text = _snapshot.HintRemaining.ToString();
            _shuffleCount.text = _snapshot.ShuffleRemaining.ToString();
        }

        private void RenderTiles()
        {
            var active = new HashSet<int>(_snapshot.Tiles.Where(tile => tile.Active).Select(tile => tile.Index));
            foreach (var pair in _tileButtons.ToArray())
            {
                if (active.Contains(pair.Key)) continue;
                Destroy(pair.Value.gameObject);
                _tileButtons.Remove(pair.Key);
            }

            int activeCount = active.Count;
            float size = Mathf.Lerp(126, 82, Mathf.InverseLerp(24, 180, activeCount));
            foreach (TripleTileSnapshot tile in _snapshot.Tiles.Where(value => value.Active).OrderBy(value => value.Layer).ThenBy(value => value.Index))
            {
                if (!_tileButtons.TryGetValue(tile.Index, out Button button))
                {
                    int index = tile.Index;
                    button = Button(string.Empty, _tileLayer, new Color(1, 1, 1, 0.97f), () => TapTile(index), out Text label);
                    label.gameObject.SetActive(false);
                    AddShadow(button.gameObject, new Color(0.15f, 0.09f, 0.08f, 0.28f), new Vector2(0, -6));
                    Image food = CreateImage("Food", button.transform, Color.white);
                    Stretch(food.rectTransform, 8);
                    food.preserveAspect = true;
                    food.raycastTarget = false;
                    if (_sprites.TryGetValue(tile.ItemId, out Sprite sprite)) food.sprite = sprite;
                    _tileButtons[index] = button;
                }
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.08f + tile.X * 0.84f, 0.10f + tile.Y * 0.82f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(size, size);
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.Euler(0, 0, ((tile.Index * 37) % 19) - 9);
                button.GetComponent<Image>().color = tile.Highlighted ? new Color32(255, 241, 140, 255) : new Color(1, 1, 1, 0.97f);
                rect.SetAsLastSibling();
            }
        }

        private void RenderTray()
        {
            for (int i = 0; i < _trayImages.Count; i++)
            {
                Image image = _trayImages[i];
                bool occupied = i < _snapshot.Tray.Count;
                image.enabled = occupied;
                if (occupied && _sprites.TryGetValue(_snapshot.Tray[i], out Sprite sprite)) image.sprite = sprite;
                image.transform.parent.GetComponent<Image>().color = occupied ? new Color(1, 1, 1, 0.94f) : new Color(1, 1, 1, 0.13f);
            }
        }

        private void RenderResult()
        {
            bool finished = _snapshot.State != TripleMatchState.Playing.ToString();
            _resultOverlay.SetActive(finished);
            if (!finished) return;
            bool won = _snapshot.State == TripleMatchState.Won.ToString();
            _resultTitle.text = won ? "HARİKA!" : "HAZNE DOLDU";
            _resultTitle.color = won ? Mint : Coral;
            _resultSubtitle.text = won
                ? "Bölüm " + _snapshot.Level + " tamamlandı\n+Skor: " + _snapshot.Score
                : "Yemekleri farklı sırayla eşleştir";
            _resultButtonText.text = won && _snapshot.Level < 100 ? "SONRAKİ BÖLÜM" : "TEKRAR DENE";
        }

        private void GoldChanged(int value)
        {
            // Currency remains in the save model and is awarded between levels.
        }

        private void TapTile(int index)
        {
            if (_inputLocked || _snapshot == null || _snapshot.State != TripleMatchState.Playing.ToString()) return;
            if (!_tileButtons.TryGetValue(index, out Button button)) return;
            StartCoroutine(AnimateTap(button.transform, index));
        }

        private IEnumerator AnimateTap(Transform tile, int index)
        {
            _inputLocked = true;
            _audio.PlayOneShot(_tapSound);
            Vector3 start = tile.localScale;
            for (float time = 0; time < 0.11f; time += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(time / 0.11f);
                tile.localScale = start * Mathf.Lerp(1, 0.72f, t);
                yield return null;
            }
            _cellRequest.RaiseEvent(index);
            _inputLocked = false;
        }

        private IEnumerator Burst()
        {
            var particles = new List<Image>();
            for (int i = 0; i < 16; i++)
            {
                Image particle = CreateImage("Match Spark", _effectsLayer, i % 3 == 0 ? Coral : i % 3 == 1 ? Orange : Mint);
                particle.sprite = _rounded;
                RectTransform rect = particle.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.23f);
                rect.sizeDelta = new Vector2(22, 22);
                particles.Add(particle);
            }
            for (float time = 0; time < 0.55f; time += Time.unscaledDeltaTime)
            {
                float t = time / 0.55f;
                for (int i = 0; i < particles.Count; i++)
                {
                    float angle = i * Mathf.PI * 2 / particles.Count;
                    particles[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 240 * t;
                    particles[i].color = new Color(particles[i].color.r, particles[i].color.g, particles[i].color.b, 1 - t);
                    particles[i].rectTransform.localScale = Vector3.one * (1.35f - t);
                }
                yield return null;
            }
            foreach (Image particle in particles) Destroy(particle.gameObject);
        }

        private Button Button(string text, Transform parent, Color color, UnityEngine.Events.UnityAction action, out Text label)
        {
            var go = new GameObject(string.IsNullOrEmpty(text) ? "Food Tile" : text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = _rounded;
            image.type = Image.Type.Sliced;
            image.color = color;
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1.05f, 1.05f, 1.05f),
                pressedColor = new Color(0.88f, 0.88f, 0.88f),
                selectedColor = Color.white,
                disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f),
                colorMultiplier = 1,
                fadeDuration = 0.08f
            };
            button.onClick.AddListener(action);
            label = Label(text, go.transform, 34, FontStyle.Bold, Color.white);
            Stretch(label.rectTransform, 8);
            label.raycastTarget = false;
            return button;
        }

        private Text Label(string value, Transform parent, int size, FontStyle style, Color color)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = value;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(16, size / 2);
            text.resizeTextMaxSize = size;
            return text;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private RectTransform Panel(string name, Transform parent, Color color, Sprite sprite)
        {
            Image image = CreateImage(name, parent, color);
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            return image.rectTransform;
        }

        private static void AddShadow(GameObject target, Color color, Vector2 distance)
        {
            var shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void Stretch(RectTransform rect, float padding = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        private static void SetRect(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static string FormatTime(int seconds)
        {
            seconds = Mathf.Max(0, seconds);
            return string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
        }

        private static AudioClip Tone(string name, float frequency, float duration, float volume)
        {
            const int sampleRate = 22050;
            int length = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float envelope = Mathf.Sin(Mathf.PI * i / length);
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * volume * envelope;
            }
            AudioClip clip = AudioClip.Create(name, length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static Sprite CreateRoundedSprite()
        {
            const int size = 64;
            const float radius = 14;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Mixo Rounded UI" };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(0, radius - Mathf.Min(x, size - 1 - x));
                    float dy = Mathf.Max(0, radius - Mathf.Min(y, size - 1 - y));
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((radius + 0.5f - distance) * 255), 0, 255);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(16, 16, 16, 16));
        }

        private void OnDestroy()
        {
            if (_tapSound != null) Destroy(_tapSound);
            if (_matchSound != null) Destroy(_matchSound);
            if (_rounded != null)
            {
                Texture2D texture = _rounded.texture;
                Destroy(_rounded);
                if (texture != null) Destroy(texture);
            }
        }
    }
}
