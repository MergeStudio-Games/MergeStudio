using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Localization.Settings;
using MergeStudio.Events;
using MergeStudio.Persistence;
using MergeStudio.Economy;

namespace MergeStudio.UI
{
    // View owns its child widgets. All gameplay communication uses channels.
    public sealed class StudioUIController : MonoBehaviour
    {
        [SerializeField] private bool _menu;
        [SerializeField] private StringEventChannelSO _sceneRequest, _boardChanged;
        [SerializeField] private VoidEventChannelSO _spawnRequest, _orderRequest, _shopRequest;
        [SerializeField] private EconomyConfigSO _economy;
        [SerializeField] private IntEventChannelSO _cellRequest, _goldChanged, _energyChanged;
        private Text _gold, _energy, _instruction, _shop, _soundButton;
        private AudioSource _audio;
        private AudioClip _feedback;
        private bool _muted;
        private float _pulse;
        private GridLayoutGroup _grid;
        private int _columns = 7, _rows = 9, _selected = -1;
        private int _goldValue, _energyValue;
        private SaveData _boardData;
        private bool _localeReady;
        private Transform _content;
        private readonly List<Text> _cells = new List<Text>();
        private readonly Dictionary<Text, string> _localized = new Dictionary<Text, string>();
        private Font _font;
        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.transform.SetParent(transform, false); canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400); scaler.matchWidthOrHeight = 0.5f;
            var safe = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeArea)); safe.transform.SetParent(canvas.transform, false);
            var layout = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup)); layout.transform.SetParent(safe.transform, false);
            var rect = layout.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(32, 32); rect.offsetMax = new Vector2(-32, -32);
            var vertical = layout.GetComponent<VerticalLayoutGroup>(); vertical.spacing = 12; vertical.childControlHeight = true; vertical.childForceExpandHeight = false;
            _content = layout.transform;
            Label(Application.productName, 100);
            if (_menu) Button("play", () => _sceneRequest.RaiseEvent("Game"));
            else
            {
                _muted = PlayerPrefs.GetInt("MergeStudio.Muted", 1) != 0;
                _audio = gameObject.AddComponent<AudioSource>(); _audio.playOnAwake = false; _audio.spatialBlend = 0;
                if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
                var samples = new float[2205];
                for (int i = 0; i < samples.Length; i++)
                    samples[i] = Mathf.Sin(2 * Mathf.PI * 660 * i / 22050f) * 0.12f * Mathf.Sin(Mathf.PI * i / samples.Length);
                _feedback = AudioClip.Create("Board feedback", samples.Length, 1, 22050, false); _feedback.SetData(samples, 0);
                _gold = Label("", 70); _energy = Label("", 70);
                var board = new GameObject("Board", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement)); board.transform.SetParent(_content, false);
                _grid = board.GetComponent<GridLayoutGroup>(); _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                _grid.spacing = new Vector2(6, 6); _grid.childAlignment = TextAnchor.MiddleCenter;
                RebuildBoard(7, 9);
                Button("spawn", () => _spawnRequest.RaiseEvent()); Button("orders", () => _orderRequest.RaiseEvent());
                if (_shopRequest != null && _economy != null)
                    _shop = ButtonText("", () => _shopRequest.RaiseEvent(), _content);
                _instruction = Label("", 100);
                _soundButton = ButtonText("", ToggleSound, _content);
                Button("close", () => _sceneRequest.RaiseEvent("MainMenu"));
            }
            if (FindAnyObjectByType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
        private void OnEnable()
        {
            _goldChanged.OnEventRaised += Gold; _energyChanged.OnEventRaised += Energy; _boardChanged.OnEventRaised += Board;
            LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
        }
        private void OnDisable()
        {
            _goldChanged.OnEventRaised -= Gold; _energyChanged.OnEventRaised -= Energy; _boardChanged.OnEventRaised -= Board;
            LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
        }
        private IEnumerator Start() { yield return LocalizationSettings.InitializationOperation; _localeReady = true; RefreshLocale(); }
        private void LocaleChanged(UnityEngine.Localization.Locale locale) => RefreshLocale();
        private string Localize(string key) => LocalizationSettings.StringDatabase.GetLocalizedString("UI", key);
        private void RefreshLocale()
        {
            if (!_localeReady) return;
            foreach (var pair in _localized) pair.Key.text = Localize(pair.Value);
            Gold(_goldValue); Energy(_energyValue); RefreshInstruction();
            if (_soundButton != null) _soundButton.text = Localize(_muted ? "sound_off" : "sound_on");
        }
        private void ToggleSound()
        {
            _muted = !_muted; PlayerPrefs.SetInt("MergeStudio.Muted", _muted ? 1 : 0); PlayerPrefs.Save(); RefreshLocale();
        }
        private void OnDestroy() { if (_feedback != null) Destroy(_feedback); }
        private void Gold(int value) { _goldValue = value; if (_gold != null && _localeReady) _gold.text = Localize("gold") + ": " + value; }
        private void Energy(int value) { _energyValue = value; if (_energy != null && _localeReady) _energy.text = Localize("energy") + ": " + value; }
        private void RefreshInstruction()
        {
            if (_instruction == null || !_localeReady) return;
            bool complete = _boardData != null && _boardData.CompletedOrders.Count > 0;
            _instruction.text = Localize(complete ? "first_order_complete" : "merge_instruction");
            if (_shop != null && _boardData != null)
            {
                _shop.text = string.Format(Localize("energy_offer"), _economy.EnergyPackAmount, _economy.EnergyPackPrice, _boardData.Diamonds);
                _shop.transform.parent.GetComponent<Button>().interactable = _boardData.Diamonds >= _economy.EnergyPackPrice && _energyValue < _economy.MaxEnergy;
            }
        }
        private void RebuildBoard(int columns, int rows)
        {
            foreach (var cell in _cells) { cell.transform.parent.gameObject.SetActive(false); Destroy(cell.transform.parent.gameObject); }
            _cells.Clear(); _columns = columns; _rows = rows; _grid.constraintCount = columns;
            for (int i = 0; i < columns * rows; i++)
            {
                int index = i;
                var text = ButtonText("\u00b7", () => Tap(index), _grid.transform);
                var input = text.transform.parent.gameObject.AddComponent<BoardCellInput>();
                input.Index = index; input.MoveRequested = Drag;
                text.resizeTextForBestFit = true; text.resizeTextMinSize = 14; text.resizeTextMaxSize = 36;
                _cells.Add(text);
            }
        }
        private void LateUpdate()
        {
            if (_grid == null) return;
            float width = ((RectTransform)_grid.transform).rect.width;
            float availableHeight = Mathf.Max(100, ((RectTransform)_content).rect.height - 900);
            float size = Mathf.Max(1, Mathf.Min((width - 6 * (_columns - 1)) / _columns, (availableHeight - 6 * (_rows - 1)) / _rows));
            _grid.cellSize = new Vector2(size, size);
            _grid.GetComponent<LayoutElement>().preferredHeight = size * _rows + 6 * (_rows - 1);
            _pulse = Mathf.Max(0, _pulse - Time.unscaledDeltaTime * 8);
            _grid.transform.localScale = Vector3.one * (1 + 0.015f * _pulse);
        }
        private void Tap(int index)
        {
            if (_boardData == null) return;
            if (_selected < 0)
            {
                if (index >= _boardData.Board.Count || _boardData.Board[index] == null || _boardData.Board[index].Tier == 0) return;
                _selected = index;
                _cells[index].text = "[" + _cells[index].text + "]";
            }
            else _selected = -1;
            _cellRequest.RaiseEvent(index);
        }
        private void Drag(int from, int to)
        {
            if (_boardData == null || from < 0 || from >= _boardData.Board.Count || _boardData.Board[from] == null || _boardData.Board[from].Tier == 0) return;
            _selected = -1;
            _cellRequest.RaiseEvent(-1); _cellRequest.RaiseEvent(from); _cellRequest.RaiseEvent(to);
        }
        private void Board(string json)
        {
            var data = Serialization.FromJson(json);
            bool changed = false;
            if (_boardData != null)
            {
                changed = data.Board.Count != _boardData.Board.Count;
                for (int i = 0; !changed && i < data.Board.Count; i++)
                    changed = data.Board[i]?.Tier != _boardData.Board[i]?.Tier || data.Board[i]?.ItemId != _boardData.Board[i]?.ItemId;
            }
            _boardData = data; _selected = -1;
            if (_grid == null) return;
            if (changed) { _pulse = 1; if (!_muted) _audio.PlayOneShot(_feedback); }
            if (data.BoardWidth != _columns || data.BoardHeight != _rows) RebuildBoard(data.BoardWidth, data.BoardHeight);
            for (int i = 0; i < _cells.Count; i++)
            {
                var cell = i < data.Board.Count ? data.Board[i] : null;
                int tier = cell == null ? 0 : cell.Tier;
                _cells[i].text = tier == 0 ? "\u00b7" : tier.ToString();
                _cells[i].transform.parent.GetComponent<Image>().color = tier == 0
                    ? new Color(0.12f, 0.18f, 0.23f) : Color.Lerp(new Color(0.30f, 0.36f, 0.20f), new Color(0.52f, 0.24f, 0.12f), tier / 10f);
            }
            RefreshInstruction();
        }
        private Text Label(string value, float height)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Text), typeof(LayoutElement)); go.transform.SetParent(_content, false);
            var text = go.GetComponent<Text>(); text.font = _font; text.fontSize = 42; text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.text = value;
            go.GetComponent<LayoutElement>().preferredHeight = height; return text;
        }
        private void Button(string key, UnityEngine.Events.UnityAction action) => _localized.Add(ButtonText(key, action, _content), key);
        private Text ButtonText(string value, UnityEngine.Events.UnityAction action, Transform parent)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement)); go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.17f, 0.38f, 0.47f); go.GetComponent<LayoutElement>().preferredHeight = 90;
            var button = go.GetComponent<Button>(); button.targetGraphic = go.GetComponent<Image>(); button.onClick.AddListener(action);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text)); label.transform.SetParent(go.transform, false);
            var rect = label.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = label.GetComponent<Text>(); text.font = _font; text.fontSize = 36; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.text = value; text.raycastTarget = false; return text;
        }
    }
}
