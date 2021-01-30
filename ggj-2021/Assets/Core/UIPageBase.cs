using System.Collections;
using UnityEngine;

public class UIPageBase : MonoBehaviour
{
  public event System.Action Shown;
  public event System.Action Hidden;

  public bool IsVisible => _isVisible;

  public bool IsAnyPartAnimating
  {
    get
    {
      bool animating = false;
      foreach (UIHydrate anim in _hydrateOnShow)
        animating |= anim.IsAnimating;

      return animating;
    }
  }

  public bool ShowOnStart = false;
  public bool ShowCursor = false;

  [SerializeField]
  private UIHydrate[] _hydrateOnShow = null;

  private CanvasGroup _canvasGroup = null;
  private bool _isVisible = false;
  private int _dehydrateRefCount = 0;
  private int _fadeStack = 0;
  private Coroutine _fadeRoutine;

  // Not using default parameter here just make [ContextMenu] work 
  [ContextMenu("Show")]
  public void Show()
  {
    Show(true);
  }

  public IEnumerator ShowAsync()
  {
    Show();
    yield return WaitForCloseAsync();
  }

  public IEnumerator WaitForCloseAsync()
  {
    while (IsVisible)
      yield return null;
  }

  public void Show(bool playAnim)
  {
    if (!_isVisible)
    {
      _isVisible = true;
      gameObject.SetActive(true);

      if (playAnim)
      {
        foreach (UIHydrate hydrate in _hydrateOnShow)
        {
          hydrate.Hydrate();
        }
      }

      Shown?.Invoke();

      if (ShowCursor)
      {
        CanvasCursor.PushVisible();

        if (PlayerCharacterController.Instance != null)
          PlayerCharacterController.Instance.PushDisableControls();
      }
    }

    EnsureFadeRoutine();
  }

  [ContextMenu("Hide")]
  public void Hide()
  {
    Hide(true);
  }

  public void Hide(bool playAnim)
  {
    if (_isVisible)
    {
      _isVisible = false;

      _dehydrateRefCount = _hydrateOnShow.Length;
      if (playAnim)
      {
        foreach (UIHydrate hydrate in _hydrateOnShow)
        {
          hydrate.Dehydrate(OnDehydrateComplete);
        }
      }

      if (!playAnim || _hydrateOnShow.Length == 0)
      {
        _dehydrateRefCount = 0;
        OnDehydrateComplete();
      }

      Hidden?.Invoke();

      if (ShowCursor)
      {
        CanvasCursor.PopVisible();

        if (PlayerCharacterController.Instance != null)
          PlayerCharacterController.Instance.PopDisableControls();
      }
    }
  }

  public void HideAfterTime(float inSeconds)
  {
    StartCoroutine(HideAfterTimeAsync(inSeconds));
  }

  public void Toggle()
  {
    if (_isVisible)
    {
      Hide();
    }
    else
    {
      Show();
    }
  }

  public void PushFadeStack(bool instantFade = false)
  {
    _fadeStack += 1;
    EnsureFadeRoutine();

    if (instantFade)
    {
      _canvasGroup.alpha = 0;
    }
  }

  public void PopFadeStack()
  {
    _fadeStack -= 1;
    EnsureFadeRoutine();
  }

  protected virtual void Awake()
  {
    if (ShowOnStart)
      Show();
    else
      Hide(playAnim: false);
  }

  protected virtual void OnValidate()
  {
    for (int i = 0; _hydrateOnShow != null && i < _hydrateOnShow.Length; ++i)
    {
      if (_hydrateOnShow[i] == null)
        Debug.LogWarning($"{name} contains null entries in Hydrate On Show!");
    }
  }

  private void OnDehydrateComplete()
  {
    --_dehydrateRefCount;
    if (_dehydrateRefCount <= 0)
    {
      gameObject.SetActive(false);
      _fadeRoutine = null;
    }
  }

  private void EnsureFadeRoutine()
  {
    if (_fadeRoutine == null)
    {
      _fadeRoutine = StartCoroutine(FadeAsync());
    }
  }

  private IEnumerator HideAfterTimeAsync(float duration)
  {
    for (float timer = 0; timer < duration; timer += Time.unscaledDeltaTime)
    {
      yield return null;
    }

    Hide();
  }

  private IEnumerator FadeAsync()
  {
    if (_canvasGroup == null)
    {
      _canvasGroup = gameObject.AddComponent<CanvasGroup>();
      _canvasGroup.alpha = 1;
    }

    float targetAlpha = _fadeStack > 0 ? 0 : 1;
    while (_canvasGroup.alpha != targetAlpha)
    {
      _canvasGroup.alpha = Mathfx.Damp(_canvasGroup.alpha, targetAlpha, 0.25f, Time.unscaledDeltaTime * 3);
      yield return null;
    }

    _fadeRoutine = null;
  }

#if UNITY_EDITOR
  [ContextMenu("Gather Hydrate-ables")]
  private void GatherHydrates()
  {
    UnityEditor.Undo.RecordObject(this, "Gather Hydrate-ables");
    _hydrateOnShow = GetComponentsInChildren<UIHydrate>(includeInactive: true);
  }
#endif
}