using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
  public enum GameStage
  {
    Invalid,
    MainMenu,
    Game,
    WinGame,
    LoseGame,
  }

  public static event System.Action GameStarted;

  public GameStage CurrentStage => _gameStage;

  public GameStage EditorDefaultStage = GameStage.Game;
  public GameObject MainMenuUIPrefab;
  public GameObject GameUIPrefab;
  public GameObject WinGameUIPrefab;
  public GameObject LoseGameUIPrefab;

  public SoundBank MusicMenuLoop;
  public SoundBank MusicGameLoop;
  public SoundBank WinAlert;
  public SoundBank LoseAlert;
  public CameraControllerBase MenuCamera;
  public CameraControllerBase GameCamera;

  private GameStage _gameStage = GameStage.Invalid;
  private GameObject _mainMenuUI = null;
  private GameObject _gameUI = null;
  private GameObject _winGameUI = null;
  private GameObject _loseGameUI = null;

  private void Awake()
  {
    GameStateManager.Instance = this;
  }

  private void Start()
  {
    // Base camera controller
    CameraControllerStack.Instance.PushController(MenuCamera);

    GameStage InitialStage = GameStage.MainMenu;
#if UNITY_EDITOR
    InitialStage = EditorDefaultStage;
#endif
    SetGameStage(InitialStage);
  }

  private void Update()
  {
    GameStage nextGameStage = _gameStage;

    switch (_gameStage)
    {
      case GameStage.MainMenu:
        break;
      case GameStage.Game:
        break;
      case GameStage.WinGame:
        break;
      case GameStage.LoseGame:
        break;
    }

    SetGameStage(nextGameStage);
  }

  public void SetGameStage(GameStage newGameStage)
  {
    if (newGameStage != _gameStage)
    {
      OnExitStage(_gameStage);
      OnEnterStage(newGameStage);
      _gameStage = newGameStage;
    }
  }

  public void OnExitStage(GameStage oldGameStage)
  {
    switch (oldGameStage)
    {
      case GameStage.MainMenu:
        {
          if (MusicMenuLoop != null)
          {
            AudioManager.Instance.FadeOutSound(gameObject, MusicMenuLoop, 0.5f);
          }

          Destroy(_mainMenuUI);
          _mainMenuUI = null;
        }
        break;
      case GameStage.Game:
        {
          if (MusicGameLoop != null)
          {
            AudioManager.Instance.FadeOutSound(gameObject, MusicGameLoop, 1.0f);
          }

          CameraControllerStack.Instance.PopController(GameCamera);

          Destroy(_gameUI);
          _gameUI = null;
        }
        break;
      case GameStage.WinGame:
        {
          Destroy(_winGameUI);
          _winGameUI = null;
        }
        break;
      case GameStage.LoseGame:
        {
          Destroy(_loseGameUI);
          _loseGameUI = null;
        }
        break;
    }
  }

  public void OnEnterStage(GameStage newGameStage)
  {
    switch (newGameStage)
    {
      case GameStage.MainMenu:
        {
          _mainMenuUI = (GameObject)Instantiate(MainMenuUIPrefab, Vector3.zero, Quaternion.identity);

          if (MusicMenuLoop != null)
          {
            AudioManager.Instance.FadeInSound(gameObject, MusicMenuLoop, 3.0f);
          }
        }
        break;
      case GameStage.Game:
        {
          if (MusicGameLoop != null)
          {
            AudioManager.Instance.FadeInSound(gameObject, MusicGameLoop, 1.0f);
          }

          _gameUI = (GameObject)Instantiate(GameUIPrefab, Vector3.zero, Quaternion.identity);

          CameraControllerStack.Instance.PushController(GameCamera);

          GameStarted?.Invoke();
        }
        break;
      case GameStage.WinGame:
        {
          if (WinAlert != null)
          {
            AudioManager.Instance.PlaySound(WinAlert);
          }

          _winGameUI = (GameObject)Instantiate(WinGameUIPrefab, Vector3.zero, Quaternion.identity);
        }
        break;
      case GameStage.LoseGame:
        {
          if (LoseAlert != null)
          {
            AudioManager.Instance.PlaySound(LoseAlert);
          }

          _loseGameUI = (GameObject)Instantiate(LoseGameUIPrefab, Vector3.zero, Quaternion.identity);
        }
        break;
    }
  }
}
