using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
  public static int InstanceCount => _instances.Count;
  public static IReadOnlyList<Interactable> Instances => _instances;

  public event System.Action InteractionTriggered;

  public float InteractionRadius
  {
    get { return _interactionRadius; }
    set { _interactionRadius = value; }
  }

  public bool RequiresLineOfSight => _requiresLineOfSight;

  [SerializeField]
  private float _interactionRadius = 3.0f;

  [SerializeField]
  private bool _requiresLineOfSight = false;

  [SerializeField]
  private GameObject _interactPromptUIPrefab = null;

  private RectTransform _uiRoot;


  private static List<Interactable> _instances = new List<Interactable>();

  public static Interactable GetInstance(int instanceIndex)
  {
    return _instances[instanceIndex];
  }

  private void OnEnable()
  {
    _instances.Add(this);
  }

  private void OnDisable()
  {
    _instances.Remove(this);

    HidePrompt();
  }

  public void TriggerInteraction()
  {
    InteractionTriggered?.Invoke();
    HidePrompt();
  }

  public void ShowPrompt()
  {
    _uiRoot = GameUI.Instance.WorldAttachedUI.ShowItem(transform, Vector3.up);
    Instantiate(_interactPromptUIPrefab, _uiRoot);
  }

  public void HidePrompt()
  {
    GameUI.Instance.WorldAttachedUI.HideItem(_uiRoot);
  }
}