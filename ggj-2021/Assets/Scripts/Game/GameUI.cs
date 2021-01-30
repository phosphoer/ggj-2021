using UnityEngine;

public class GameUI : Singleton<GameUI>
{
  public WorldAttachedUI WorldAttachedUI;
  public MainMenuUIHandler MainMenuUI;

  private void Awake()
  {
    Instance = this;
  }
}