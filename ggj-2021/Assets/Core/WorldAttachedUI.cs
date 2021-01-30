using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WorldAttachedUI : UIPageBase
{
  [SerializeField]
  private RectTransform _itemPrefab = null;

  [SerializeField]
  private RectTransform _rootCanvas = null;

  private List<UIObject> _uiObjects = new List<UIObject>();
  private List<UIObject> _objectPool = new List<UIObject>();
  private Coroutine _updateRoutine;

#if UNITY_EDITOR
  private int _currentId;
#endif

  [System.Serializable]
  private class UIObject
  {
    public RectTransform UI;
    public Transform WorldAnchor;
    public Vector3 WorldOffset;
    public bool IsShown;
  }

  public RectTransform ShowItem(Transform attachedTransform, Vector3 worldOffset)
  {
    UIObject obj = null;
    if (_objectPool.Count > 0)
    {
      obj = _objectPool[_objectPool.Count - 1];
      _objectPool.RemoveAt(_objectPool.Count - 1);
    }
    else
    {
      obj = new UIObject();
    }

    obj.UI = Instantiate(_itemPrefab, transform);
    obj.WorldAnchor = attachedTransform;
    obj.WorldOffset = worldOffset;
    obj.IsShown = true;

    // Naming is convenient for debug logging 
#if UNITY_EDITOR
    obj.UI.name = $"world-locked-ui-{_currentId}";
    _currentId += 1;
#endif

    _uiObjects.Add(obj);
    // Debug.Log($"Showing ui item {obj.UI.name} from {attachedTransform.name}", obj.UI.gameObject);

    Show();

    // Start our update routine if we didn't have one going
    if (_updateRoutine == null)
    {
      _updateRoutine = StartCoroutine(UpdateAsync());
    }

    return obj.UI;
  }

  public void HideItem(RectTransform ui)
  {
    for (int i = 0; i < _uiObjects.Count; ++i)
    {
      // Find the UI object and hide it
      UIObject uiObj = _uiObjects[i];
      if (ReferenceEquals(ui, uiObj.UI) && uiObj.IsShown)
      {
        uiObj.IsShown = false;

        // Either dehydrate or flat out remove the object based on whether it has a hydrate
        UIHydrate hydrate = ui.GetComponent<UIHydrate>();
        if (hydrate != null)
        {
          // Debug.Log($"Hiding ui item {ui.name} from {uiObj.WorldAnchor.name}", ui.gameObject);
          hydrate.Dehydrate(() =>
          {
            RemoveUI(ui);
          });
        }
        else
        {
          RemoveUI(ui);
        }

        return;
      }
    }
  }

  private void RemoveUI(RectTransform ui)
  {
    if (ui != null)
    {
      // Find the UI object in our list and remove it
      for (int i = 0; i < _uiObjects.Count; ++i)
      {
        if (ReferenceEquals(_uiObjects[i].UI, ui))
        {
          UIObject obj = _uiObjects[i];
          obj.UI = null;
          _objectPool.Add(obj);
          _uiObjects.RemoveAt(i);
          break;
        }
      }

      Destroy(ui.gameObject);
    }

    // We can hide this entire screen if there are no more UI objects
    if (_uiObjects.Count == 0)
    {
      _updateRoutine = null;
      Hide();
    }
  }

  protected override void Awake()
  {
    base.Awake();
    Shown += OnShown;
    Hidden += OnHidden;
  }

  private void OnShown()
  {

  }

  private void OnHidden()
  {

  }

  private IEnumerator UpdateAsync()
  {
    var waitForLateUpdate = new WaitForEndOfFrame();

    while (_uiObjects.Count > 0)
    {
      for (int i = 0; i < _uiObjects.Count; ++i)
      {
        UIObject uiObject = _uiObjects[i];
        if (uiObject.WorldAnchor != null)
        {
          // Position the ui at the screenspace position of the object 
          Vector3 worldPos = uiObject.WorldAnchor.position + uiObject.WorldOffset;
          Vector3 canvasPos = Mathfx.WorldToCanvasPosition(_rootCanvas, Camera.main, worldPos, allowOffscreen: true);
          Vector2 anchorPos = canvasPos;
          RectTransform talkBubbleTransform = uiObject.UI.transform as RectTransform;
          talkBubbleTransform.anchoredPosition = anchorPos;

          // While the UI is shown, control its visiblity by distance
          if (uiObject.IsShown)
          {
            float distToCam = Vector3.Distance(Camera.main.transform.position, uiObject.WorldAnchor.position);
            uiObject.UI.gameObject.SetActive(distToCam < 200 && canvasPos.z > 0);
          }
        }
      }

      yield return waitForLateUpdate;
    }

    _updateRoutine = null;
  }
}