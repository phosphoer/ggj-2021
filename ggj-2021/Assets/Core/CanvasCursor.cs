using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CanvasCursor : MonoBehaviour
{
  public static bool IsVisible { get; private set; }
  public static Vector3 CursorDelta => _lastMouseDelta;
  public static Vector3 CursorWorldPos => _cursorRect.position;


  [SerializeField]
  private float _autoHideTime = 10.0f;

  private static Canvas _canvas;
  private static RectTransform _canvasRect;
  private static RectTransform _cursorRect;
  private static Image _cursorImage;

  private static float _targetScale = 1;
  private static float _autoHideTimer = 0;
  private static bool _autoHidden = false;
  private static int _visibleStack = 0;
  private static Vector3 _lastMousePos;
  private static Vector3 _lastMouseDelta;

  public static void PushVisible()
  {
    _visibleStack += 1;
  }

  public static void PopVisible()
  {
    _visibleStack -= 1;
    if (_visibleStack < 0)
    {
      Debug.LogWarning($"Cursor visible stack went below zero to {_visibleStack}");
      _visibleStack = 0;
    }
  }

  private void Awake()
  {
    _canvas = GetComponentInParent<Canvas>();
    _canvasRect = _canvas.GetComponent<RectTransform>();
    _cursorRect = GetComponent<RectTransform>();
    _cursorImage = GetComponent<Image>();

    Cursor.visible = false;
    _autoHideTimer = _autoHideTime;
    _lastMousePos = Input.mousePosition;
    _visibleStack = 0;
  }

  private void Update()
  {
    IsVisible = !_autoHidden && _visibleStack > 0;
    _cursorImage.enabled = IsVisible;
    Cursor.visible = false;

    Cursor.lockState = _visibleStack > 0 ? CursorLockMode.None : CursorLockMode.Locked;

    // Convert real cursor coords to canvas 
    Vector3 cursorNormalized = Input.mousePosition;
    cursorNormalized.x /= Screen.width;
    cursorNormalized.y /= Screen.height;
    cursorNormalized.z = 0;

    _lastMouseDelta = Input.mousePosition - _lastMousePos;
    _lastMousePos = Input.mousePosition;

    Vector3 cursorPos = cursorNormalized;
    cursorPos.x *= _canvasRect.rect.width;
    cursorPos.y *= _canvasRect.rect.height;

    // Debug.Log($"Input mouse pos {Input.mousePosition.x}x{Input.mousePosition.y}");
    // Debug.Log($"Screen size {Screen.width}x{Screen.height}");
    // Debug.Log($"Canvas rect {_canvasRect.rect.width}x{_canvasRect.rect.height}");
    // Debug.Log($"Normalized pos {cursorNormalized.x}x{cursorNormalized.y}");
    // Debug.Log($"Cursor pos {cursorPos.x}x{cursorPos.y}");

    // Auto hide when mouse doesn't move 
    _autoHidden = _autoHideTimer >= _autoHideTime;
    if (Mathf.Approximately(CursorDelta.x, 0) &&
        Mathf.Approximately(CursorDelta.y, 0))
    {
      _autoHideTimer += Time.unscaledDeltaTime;
    }
    else
    {
      _autoHideTimer = 0;
    }

    // Position cursor icon
    _cursorRect.anchoredPosition = cursorPos;

    // Click animation
    if (Input.GetKey(KeyCode.Mouse0))
    {
      _targetScale = 1.5f;
    }
    else
    {
      _targetScale = 1;
    }

    _cursorRect.localScale = Mathfx.Damp(_cursorRect.localScale, Vector3.one * _targetScale, 0.25f, Time.unscaledDeltaTime * 20.0f);
  }
}