using UnityEngine;

public class GameUI : Singleton<GameUI>
{
  public WorldAttachedUI WorldAttachedUI;
  public MainMenuUIHandler MainMenuUI;
  public DayIntroUIHandler DayIntroUI;
  public DayOutroUIHandler DayOutroUI;
  public DaytimeUIHandler DaytimeUI;
  public ScreamComposerUIHandler ScreamComposerUI;
  public LoseGameUIHandler LoseGameUI;
  public WinGameUIHandler WinGameUI;
  public SettingsUIHandler SettingsUI;

  private void Awake()
  {
    Instance = this;
  }
}