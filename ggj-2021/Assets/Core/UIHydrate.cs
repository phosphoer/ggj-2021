using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIHydrate : MonoBehaviour
{
  public event System.Action Dehydrated;

  public bool IsAnimating => _currentRoutine != null;
  public bool IsHydrated => _isHydrated;

  public bool HydrateOnEnable = false;
  public bool StartDehydrated = false;

  [SerializeField]
  private Transform _targetTransform = null;

  [SerializeField]
  private bool _useNormalizedScale = true;

  [SerializeField]
  private bool _enableRandomDelay = true;

  private Coroutine _currentRoutine;
  private Coroutine _showTimedRoutine;
  private Vector3 _startScale;
  private List<IEnumerator> _childEnumerators;
  private bool _isHydrated;

  private const float kHydrateTime = 0.75f;
  private const float kDehydrateTime = 0.3f;

  public Coroutine ShowTimed(float duration)
  {
    if (_showTimedRoutine != null)
    {
      CoroutineRoot.Instance.StopCoroutine(_showTimedRoutine);
      _showTimedRoutine = null;
    }

    _showTimedRoutine = CoroutineRoot.Instance.StartCoroutine(ShowTimedAsync(duration));
    return _showTimedRoutine;
  }

  public Coroutine Hydrate()
  {
    // Debug.Log($"Calling hydrate on {name}", gameObject);
    _isHydrated = true;

    if (_currentRoutine != null)
    {
      // Debug.Log($"Stopping existing routine on {name}", gameObject);
      CoroutineRoot.Instance.StopCoroutine(_currentRoutine);
      _currentRoutine = null;
    }

    gameObject.SetActive(true);
    _currentRoutine = CoroutineRoot.Instance.StartCoroutine(HydrateRoutine());
    return _currentRoutine;
  }

  public Coroutine Dehydrate(System.Action finishCallback = null)
  {
    // Debug.Log($"Calling dehydrate on {name}", gameObject);
    _isHydrated = false;

    if (_currentRoutine != null)
    {
      // Debug.Log($"Stopping existing routine on {name}", gameObject);
      CoroutineRoot.Instance.StopCoroutine(_currentRoutine);
      _currentRoutine = null;
    }

    _currentRoutine = CoroutineRoot.Instance.StartCoroutine(DehydrateRoutine(finishCallback));
    return _currentRoutine;
  }

  private IEnumerator HydrateRoutine()
  {
    if (_targetTransform == null)
    {
      _targetTransform = transform;
    }

    // Wait for global assets lol
    while (GameGlobals.Instance == null)
    {
      _targetTransform.localScale = Vector3.zero;
      yield return null;
    }

    Vector3 startScale = _startScale * GameGlobals.Instance.UIHydrateCurve.Evaluate(0);
    Vector3 endScale = _startScale * GameGlobals.Instance.UIHydrateCurve.Evaluate(1);
    _targetTransform.localScale = startScale;

    if (_enableRandomDelay)
    {
      float waitTime = Random.Range(0, 0.3f);
      for (float time = 0; time < waitTime; time += Time.unscaledDeltaTime)
        yield return null;
    }

    for (float time = 0; time < kHydrateTime && _targetTransform != null; time += Time.unscaledDeltaTime)
    {
      float t = time / kHydrateTime;
      float tCurve = GameGlobals.Instance.UIHydrateCurve.Evaluate(t);
      _targetTransform.localScale = _startScale * tCurve;
      yield return null;
    }

    if (_targetTransform != null)
      _targetTransform.localScale = endScale;

    _currentRoutine = null;
  }

  private IEnumerator DehydrateRoutine(System.Action finishCallback = null)
  {
    Vector3 startScale = _startScale * GameGlobals.Instance.UIDehydrateCurve.Evaluate(0);
    Vector3 endScale = _startScale * GameGlobals.Instance.UIDehydrateCurve.Evaluate(1);
    _targetTransform.localScale = startScale;

    if (_enableRandomDelay)
    {
      float waitTime = Random.Range(0, 0.3f);
      for (float time = 0; time < waitTime; time += Time.unscaledDeltaTime)
        yield return null;
    }

    for (float time = 0; time < kDehydrateTime && _targetTransform != null; time += Time.unscaledDeltaTime)
    {
      float t = time / kDehydrateTime;
      float tCurve = GameGlobals.Instance.UIDehydrateCurve.Evaluate(t);
      _targetTransform.localScale = _startScale * tCurve;
      yield return null;
    }

    if (_targetTransform != null)
      _targetTransform.localScale = endScale;

    gameObject.SetActive(false);

    _currentRoutine = null;
    finishCallback?.Invoke();
    Dehydrated?.Invoke();
  }

  private void Awake()
  {
    if (_targetTransform == null)
      _targetTransform = transform;

    _startScale = _useNormalizedScale ? Vector3.one : _targetTransform.localScale;

    if (StartDehydrated)
    {
      _targetTransform.localScale = Vector3.zero;
      gameObject.SetActive(false);
      _isHydrated = false;
    }
    else
    {
      _isHydrated = true;
    }
  }

  private void OnEnable()
  {
    if (HydrateOnEnable)
    {
      Hydrate();
    }
  }

  private void OnDisable()
  {
    _isHydrated = false;
  }

  private IEnumerator ShowTimedAsync(float duration)
  {
    yield return Hydrate();

    for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
    {
      yield return null;
    }

    yield return Dehydrate();
  }
}