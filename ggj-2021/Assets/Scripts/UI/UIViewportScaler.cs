using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class UIViewportScaler : MonoBehaviour
{
  [SerializeField]
  private Vector2 _referenceResolution = new Vector2(1280, 800);

  private Vector2 _lastScale;
  private CanvasScaler _canvasScaler;

  private void Start()
  {
    _canvasScaler = GetComponent<CanvasScaler>();
  }

  private void Update()
  {
    if (_canvasScaler != null)
    {
      float scaleX = Screen.width / _referenceResolution.x;
      float scaleY = Screen.height / _referenceResolution.y;
      if (scaleX == 0 || scaleY == 0 || (scaleX == _lastScale.x && scaleY == _lastScale.y))
        return;

      _canvasScaler.scaleFactor = Mathf.Min(scaleX, scaleY);
      _lastScale = new Vector2(scaleX, scaleY);
    }
  }

  [ContextMenu("Apply Scale")]
  private void ApplyScale()
  {
    Debug.LogFormat("Applying scale for screen res {0}x{1}", Screen.width, Screen.height);
    if (_canvasScaler == null)
    {
      _canvasScaler = GetComponent<CanvasScaler>();
    }

    Update();
  }
}
