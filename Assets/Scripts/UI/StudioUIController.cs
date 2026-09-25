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
        private static readonly Color DeepTeal = new Color32(14, 47, 52, 255);
        private static readonly Color Gold = new Color32(222, 169, 88, 255);
        private static readonly Color Stone = new Color32(248, 231, 203, 255);

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
        private Sprite _circle;
        private Sprite _background;
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
            _font = Resources.Load<Font>("MixoKitchen/Fonts/Nunito-Variable") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _rounded = CreateRoundedSprite();
            _circle = CreateCircleSprite();
            _background = Resources.Load<Sprite>("MixoKitchen/UI/kitchen-background-v2");
            foreach (Sprite sprite in Resources.LoadAll<Sprite>("MixoKitchen/PremiumFood")) _sprites[sprite.name] = sprite;
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

            Image background = CreateImage("Premium Kitchen Background", canvasObject.transform, Color.white);
            Stretch(background.rectTransform);
            background.sprite = _background;
            background.preserveAspect = false;

            Image topShade = CreateImage("Top Shade", canvasObject.transform, new Color(0.02f, 0.09f, 0.10f, 0.28f));
            SetRect(topShade.rectTransform, 0, 0.77f, 1, 1);
            topShade.raycastTarget = false;

            var safe = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeArea));
            safe.transform.SetParent(canvasObject.transform, false);
            _safeRoot = safe.GetComponent<RectTransform>();
            Stretch(_safeRoot);
        }

        private void BuildMenu()
        {
            Text brand = Label("MIXO", _safeRoot, 126, FontStyle.Bold, Color.white);
            SetRect(brand.rectTransform, 0.08f, 0.78f, 0.92f, 0.92f);
            AddShadow(brand.gameObject, new Color(0, 0, 0, 0.45f), new Vector2(0, -8));
            Text kitchen = Label("K I T C H E N", _safeRoot, 39, FontStyle.Bold, Gold);
            SetRect(kitchen.rectTransform, 0.08f, 0.735f, 0.92f, 0.81f);
            Text tagline = Label("EŞLEŞTİR  •  SERVİS ET  •  USTALAŞ", _safeRoot, 24, FontStyle.Bold, Stone);
            SetRect(tagline.rectTransform, 0.08f, 0.69f, 0.92f, 0.75f);

            string[] heroes = { "burger", "pizza", "birthday-cake", "avocado", "fries" };
            for (int i = 0; i < heroes.Length; i++)
            {
                var card = Panel("Hero Food", _safeRoot, new Color(1, 0.96f, 0.86f, 0.96f), _circle);
                float x = 0.16f + i * 0.17f;
                SetRect(card, x - 0.075f, 0.43f + (i % 2) * 0.045f, x + 0.075f, 0.58f + (i % 2) * 0.045f);
                card.localRotation = Quaternion.Euler(0, 0, -10 + i * 5);
                AddShadow(card.gameObject, new Color(0.02f, 0.08f, 0.08f, 0.42f), new Vector2(0, -12));
                Image icon = CreateImage("Food", card, Color.white);
                Stretch(icon.rectTransform, 18);
                icon.preserveAspect = true;
                if (_sprites.TryGetValue(heroes[i], out Sprite sprite)) icon.sprite = sprite;
                _floaters.Add(card);
            }

            Button play = Button("OYNA", _safeRoot, DeepTeal, () => _sceneRequest.RaiseEvent("Game"), out Text playText);
            SetRect(play.GetComponent<RectTransform>(), 0.15f, 0.19f, 0.85f, 0.29f);
            playText.fontSize = 52;
            playText.color = new Color32(255, 231, 177, 255);
            AddShadow(play.gameObject, new Color(0.01f, 0.04f, 0.04f, 0.48f), new Vector2(0, -12));

            Text note = Label("100 özgün mutfak bölümü seni bekliyor", _safeRoot, 27, FontStyle.Bold, DeepTeal);
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

            RectTransform header = Panel("Header", _safeRoot, new Color(0.035f, 0.15f, 0.16f, 0.96f), _rounded);
            SetRect(header, 0.045f, 0.89f, 0.955f, 0.975f);
            AddShadow(header.gameObject, new Color(0.01f, 0.03f, 0.03f, 0.55f), new Vector2(0, -9));
            _levelText = Label("BÖLÜM 1", header, 31, FontStyle.Bold, Stone);
            SetRect(_levelText.rectTransform, 0.04f, 0.48f, 0.42f, 0.92f);
            _levelText.alignment = TextAnchor.MiddleLeft;
            _scoreText = Label("0", header, 30, FontStyle.Bold, Gold);
            SetRect(_scoreText.rectTransform, 0.70f, 0.48f, 0.94f, 0.92f);
            _scoreText.alignment = TextAnchor.MiddleRight;
            _timerText = Label("∞", header, 27, FontStyle.Bold, Color.white);
            SetRect(_timerText.rectTransform, 0.43f, 0.48f, 0.69f, 0.92f);
            RectTransform progress = Panel("Progress", header, new Color(1, 1, 1, 0.14f), _rounded);
            SetRect(progress, 0.04f, 0.12f, 0.96f, 0.30f);
            _progressFill = CreateImage("Fill", progress, Gold);
            RectTransform fill = _progressFill.rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.pivot = new Vector2(0, 0.5f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;

            _boardArea = Panel("Food Pile", _safeRoot, new Color(1f, 0.96f, 0.88f, 0.08f), _rounded);
            SetRect(_boardArea, 0.045f, 0.315f, 0.955f, 0.845f);
            _tileLayer = new GameObject("Tiles", typeof(RectTransform)).GetComponent<RectTransform>();
            _tileLayer.SetParent(_boardArea, false);
            Stretch(_tileLayer, 25);
            _effectsLayer = new GameObject("Effects", typeof(RectTransform)).GetComponent<RectTransform>();
            _effectsLayer.SetParent(_safeRoot, false);
            Stretch(_effectsLayer);

            RectTransform instructionPill = Panel("Instruction", _boardArea, new Color(0.035f, 0.15f, 0.16f, 0.90f), _rounded);
            SetRect(instructionPill, 0.19f, 0.015f, 0.81f, 0.085f);
            _statusText = Label("Aynı üç lezzeti eşleştir", instructionPill, 23, FontStyle.Bold, Stone);
            Stretch(_statusText.rectTransform, 4);

            RectTransform tray = Panel("Tray", _safeRoot, new Color(0.025f, 0.12f, 0.13f, 0.98f), _rounded);
            SetRect(tray, 0.045f, 0.17f, 0.955f, 0.295f);
            AddShadow(tray.gameObject, new Color(0.01f, 0.03f, 0.03f, 0.60f), new Vector2(0, -11));
            var trayLayout = tray.gameObject.AddComponent<HorizontalLayoutGroup>();
            trayLayout.padding = new RectOffset(16, 16, 16, 16);
            trayLayout.spacing = 9;
            trayLayout.childAlignment = TextAnchor.MiddleCenter;
            trayLayout.childControlWidth = trayLayout.childControlHeight = true;
            trayLayout.childForceExpandWidth = trayLayout.childForceExpandHeight = true;
            for (int i = 0; i < 7; i++)
            {
                RectTransform slot = Panel("Slot " + (i + 1), tray, new Color(1, 0.91f, 0.70f, 0.12f), _rounded);
                var element = slot.gameObject.AddComponent<LayoutElement>();
                element.flexibleWidth = element.flexibleHeight = 1;
                Image icon = CreateImage("Food", slot, Color.white);
                Stretch(icon.rectTransform, 5);
                icon.preserveAspect = true;
                icon.enabled = false;
                _trayImages.Add(icon);
            }

            RectTransform tools = new GameObject("Boosters", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            tools.SetParent(_safeRoot, false);
            SetRect(tools, 0.045f, 0.045f, 0.955f, 0.145f);
            var toolsLayout = tools.GetComponent<HorizontalLayoutGroup>();
            toolsLayout.spacing = 14;
            toolsLayout.childAlignment = TextAnchor.MiddleCenter;
            toolsLayout.childControlWidth = toolsLayout.childControlHeight = true;
            toolsLayout.childForceExpandWidth = toolsLayout.childForceExpandHeight = true;
            AddToolButton("↶", "GERİ", new Color32(169, 80, 67, 255), tools, () => _orderRequest.RaiseEvent(), out _undoCount);
            AddToolButton("✦", "İPUCU", new Color32(186, 132, 54, 255), tools, () => _shopRequest.RaiseEvent(), out _hintCount);
            AddToolButton("↻", "KARIŞTIR", new Color32(35, 116, 121, 255), tools, () => _spawnRequest.RaiseEvent(), out _shuffleCount);

            BuildResultOverlay();
        }

        private void AddToolButton(string icon, string title, Color color, Transform parent, UnityEngine.Events.UnityAction action, out Text count)
        {
            Button button = Button(icon + "  " + title, parent, color, action, out Text label);
            button.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            label.fontSize = 24;
            count = Label("3", button.transform, 21, FontStyle.Bold, Color.white);
            count.alignment = TextAnchor.LowerRight;
            SetRect(count.rectTransform, 0.72f, 0.04f, 0.94f, 0.40f);
            AddShadow(button.gameObject, new Color(0.01f, 0.03f, 0.03f, 0.45f), new Vector2(0, -7));
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
                Handheld.Vibrate();
                StartCoroutine(Burst());
            }
        }

        private void RenderHeader()
        {
            _levelText.text = "BÖLÜM " + _snapshot.Level + " / 100";
            _scoreText.text = "★ " + _snapshot.Score;
            _timerText.text = _snapshot.SecondsRemaining < 0 ? "RAHAT MOD" : FormatTime(_snapshot.SecondsRemaining);
            _timerText.color = _snapshot.SecondsRemaining >= 0 && _snapshot.SecondsRemaining < 30 ? Coral : Color.white;
            float progress = _snapshot.TotalTiles <= 0 ? 0 : (float)_snapshot.ClearedTiles / _snapshot.TotalTiles;
            _progressFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(progress), 1);
            _statusText.text = _snapshot.Combo > 1 ? "MUHTEŞEM SERİ  ×" + _snapshot.Combo : "Aynı üç lezzeti eşleştir";
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
            float size = Mathf.Lerp(148, 92, Mathf.InverseLerp(24, 180, activeCount));
            foreach (TripleTileSnapshot tile in _snapshot.Tiles.Where(value => value.Active).OrderBy(value => value.Layer).ThenBy(value => value.Index))
            {
                if (!_tileButtons.TryGetValue(tile.Index, out Button button))
                {
                    int index = tile.Index;
                    button = Button(string.Empty, _tileLayer, new Color(1f, 0.94f, 0.80f, 0.94f), () => TapTile(index), out Text label);
                    button.GetComponent<Image>().sprite = _circle;
                    button.GetComponent<Image>().type = Image.Type.Simple;
                    label.gameObject.SetActive(false);
                    AddShadow(button.gameObject, new Color(0.02f, 0.09f, 0.09f, 0.38f), new Vector2(0, -7));
                    Image food = CreateImage("Food", button.transform, Color.white);
                    Stretch(food.rectTransform, 3);
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
                rect.localRotation = Quaternion.Euler(0, 0, ((tile.Index * 37) % 13) - 6);
                button.GetComponent<Image>().color = tile.Highlighted ? new Color32(255, 211, 98, 255) : new Color(1f, 0.94f, 0.80f, 0.94f);
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
                image.transform.parent.GetComponent<Image>().color = occupied ? new Color(1f, 0.94f, 0.80f, 0.96f) : new Color(1f, 0.91f, 0.70f, 0.12f);
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
            RectTransform sourceRect = (RectTransform)tile;
            int slotIndex = Mathf.Clamp(_snapshot.Tray.Count, 0, _trayImages.Count - 1);
            RectTransform targetRect = _trayImages[slotIndex].rectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _effectsLayer, RectTransformUtility.WorldToScreenPoint(null, sourceRect.position), null, out Vector2 start);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _effectsLayer, RectTransformUtility.WorldToScreenPoint(null, targetRect.position), null, out Vector2 end);

            Image sourceFood = tile.Find("Food")?.GetComponent<Image>();
            Image flyer = CreateImage("Flying Food", _effectsLayer, Color.white);
            flyer.sprite = sourceFood != null ? sourceFood.sprite : null;
            flyer.preserveAspect = true;
            flyer.raycastTarget = false;
            RectTransform flyerRect = flyer.rectTransform;
            flyerRect.anchorMin = flyerRect.anchorMax = new Vector2(0.5f, 0.5f);
            flyerRect.sizeDelta = sourceRect.sizeDelta;
            flyerRect.anchoredPosition = start;
            flyerRect.SetAsLastSibling();

            const float duration = 0.24f;
            for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(time / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                flyerRect.anchoredPosition = Vector2.LerpUnclamped(start, end, eased) + Vector2.up * Mathf.Sin(t * Mathf.PI) * 95f;
                flyerRect.localScale = Vector3.one * Mathf.Lerp(1f, 0.52f, eased);
                flyerRect.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 18f, eased));
                tile.localScale = Vector3.one * Mathf.Lerp(1f, 0.82f, eased);
                yield return null;
            }
            Destroy(flyer.gameObject);
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

        private static Sprite CreateCircleSprite()
        {
            const int size = 96;
            const float radius = 45f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Mixo Porcelain Disc" };
            var pixels = new Color32[size * size];
            Vector2 center = Vector2.one * (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((radius + 1f - distance) * 255), 0, 255);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
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
            if (_circle != null)
            {
                Texture2D texture = _circle.texture;
                Destroy(_circle);
                if (texture != null) Destroy(texture);
            }
        }
    }
}
