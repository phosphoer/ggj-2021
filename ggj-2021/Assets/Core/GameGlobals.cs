using UnityEngine;

public class GameGlobals : Singleton<GameGlobals>
{
  public AnimationCurve UIHydrateCurve;
  public AnimationCurve UIDehydrateCurve;

  private void Awake()
  {
    Instance = this;
  }
}