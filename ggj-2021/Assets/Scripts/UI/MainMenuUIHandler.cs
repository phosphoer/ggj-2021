using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuUIHandler : MonoBehaviour
{
  EventSystem eventSystem;

  void OnEnable()
  {
    eventSystem = EventSystem.current;
  }

  public void OnNewGameClicked()
  {
    GameStateManager.Instance.NewGame();
  }

  public void OnSettingsClicked()
  {
    GameStateManager.Instance.Settings();
  }

  public void OnQuitGameClicked()
  {
    //If we are running in a standalone build of the game
#if UNITY_STANDALONE
    //Quit the application
    Application.Quit();
#endif

    //If we are running in the editor
#if UNITY_EDITOR
    //Stop playing the scene
    UnityEditor.EditorApplication.isPlaying = false;
#endif
  }
}
