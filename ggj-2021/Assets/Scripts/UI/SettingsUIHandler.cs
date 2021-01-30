using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIHandler : UIPageBase
{
  public GameStateManager.GameStage ReturnState;

  protected override void Awake()
  {
    base.Awake();
    Shown += OnShown;
  }

  private void OnShown()
  {
  }

  void Update()
  {
  }

  void OnBackClicked()
  {
    GameStateManager.Instance.SetGameStage(ReturnState);
  }
}
