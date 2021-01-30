using UnityEngine;

public class GameUI : Singleton<GameUI>
{
  public WorldAttachedUI WorldAttachedUI;

  private void Awake()
  {
    Instance = this;
  }
}