using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
  public event System.Action HoldStart;
  public event System.Action HoldEnd;

  public bool IsHoldingObject => _holdableObject != null;
  public HoldableObject HeldObject => _holdableObject;

  [SerializeField]
  private Transform _objectHoldRoot = null;

  private HoldableObject _holdableObject;

  public void HoldObject(HoldableObject holdableObject)
  {
    _holdableObject = holdableObject;

    _holdableObject.StartHold();
    _holdableObject.transform.parent = _objectHoldRoot;
    _holdableObject.transform.localPosition = Vector3.zero;
    _holdableObject.transform.localRotation = Quaternion.identity;

    HoldStart?.Invoke();
  }

  public void DropObject()
  {
    _holdableObject.StopHold();
    _holdableObject = null;

    HoldEnd?.Invoke();
  }
}