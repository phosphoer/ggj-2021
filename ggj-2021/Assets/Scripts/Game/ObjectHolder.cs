using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
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
  }

  public void DropObject()
  {
    _holdableObject.StopHold();
    _holdableObject = null;
  }
}