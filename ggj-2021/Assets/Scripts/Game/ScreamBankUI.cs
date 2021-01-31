using UnityEngine;
using System.Collections.Generic;

public class ScreamBankUI : MonoBehaviour
{
  [SerializeField]
  private Transform _listLayoutRoot = null;

  [SerializeField]
  private ScreamItemUI _screamItemPrefab = null;

  private List<ScreamItemUI> _screamItems = new List<ScreamItemUI>();

  public void SetRequests(IReadOnlyList<ScreamSoundDefinition> requestList)
  {
    for (int i = 0; i < _screamItems.Count; ++i)
    {
      Destroy(_screamItems[i].gameObject);
    }

    _screamItems.Clear();

    for (int i = 0; i < requestList.Count; ++i)
    {
      ScreamItemUI screamItemUI = Instantiate(_screamItemPrefab, _listLayoutRoot);
      screamItemUI.ScreamSound = requestList[i];
      _screamItems.Add(screamItemUI);
    }
  }

  private void Start()
  {
    GameStateManager.Instance.ScreamBank.RemainingScreamsChanged += OnRemainingScreamsChanged;
    OnRemainingScreamsChanged();
  }

  private void OnDestroy()
  {
    GameStateManager.Instance.ScreamBank.RemainingScreamsChanged -= OnRemainingScreamsChanged;
  }

  private void OnRemainingScreamsChanged()
  {
    SetRequests(GameStateManager.Instance.ScreamBank.RemainingRequests);
  }
}