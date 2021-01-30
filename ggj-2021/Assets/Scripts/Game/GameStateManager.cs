using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
  public enum GameStage
  {
    Invalid,
    MainMenu,
    Settings,
    DayIntro,
    Daytime,
    DayOutro,
    WinGame,
    LoseGame,
  }

  public static event System.Action DayIntroStarted;
  public static event System.Action DaytimeStarted;
  public static event System.Action DayOutroStarted;

  public GameStage CurrentStage => _gameStage;

  public GameStage EditorDefaultStage = GameStage.Daytime;
  public GameObject MainMenuUIPrefab;
  public GameObject SettingsMenuUIPrefab;
  public GameObject DayIntroUIPrefab;
  public GameObject DaytimeUIPrefab;
  public GameObject DayOutroUIPrefab;
  public GameObject WinGameUIPrefab;
  public GameObject LoseGameUIPrefab;

  public SoundBank MusicMenuLoop;
  public SoundBank MusicDayIntro;
  public SoundBank MusicDayOutro;
  public SoundBank WinAlert;
  public SoundBank LoseAlert;
  public CameraControllerBase MenuCamera;
  public CameraControllerBase GameCamera;

  private GameStage _gameStage = GameStage.Invalid;
  private GameObject _mainMenuUI = null;
  private GameObject _settingsMenuUI = null;
  private GameObject _dayIntroUI = null;
  private GameObject _daytimeUI = null;
  private GameObject _dayOutroUI = null;
  private GameObject _winGameUI = null;
  private GameObject _loseGameUI = null;

  private DayIntroUIHandler _dayIntroUIHander = null;
  private DayOutroUIHandler _dayOutroUIHander = null;

  [SerializeField]
  private SanityComponent _playerSanity = null;
  public SanityComponent PlayerSanity
  {
    get { return _playerSanity; }
  }

  [SerializeField]
  private ScreamBankComponent _screamBank = null;
  public ScreamBankComponent ScreamBank
  {
    get { return _screamBank; }
  }

  public int TotalDays = 4;

  [SerializeField]
  private int _currentDay = 0;
  public int CurrentDay
  {
    get { return _currentDay; }
  }

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
      case GameStage.Settings:
        break;
      case GameStage.DayIntro:
        if (_dayIntroUIHander.IsComplete())
        {
          nextGameStage = GameStage.Daytime;
        }
        break;
      case GameStage.Daytime:
        if (!_playerSanity.HasSanityRemaining)
        {
          nextGameStage = GameStage.LoseGame;
        }
        else if (_screamBank != null && _screamBank.HasReachedScreamNoteGoal)
        {
          if (_currentDay + 1 >= TotalDays)
          {
            nextGameStage = GameStage.WinGame;
          }
          else
          {
            nextGameStage = GameStage.DayOutro;
          }
        }
        break;
      case GameStage.DayOutro:
        if (_dayOutroUIHander.IsComplete())
        {
          nextGameStage = GameStage.DayIntro;
        }
        break;
      case GameStage.WinGame:
        break;
      case GameStage.LoseGame:
        break;
    }

    SetGameStage(nextGameStage);
  }

  public void NewGame()
  {
    _currentDay = 0;
    SetGameStage(GameStage.DayIntro);
  }

  public void Settings()
  {
    SetGameStage(GameStage.Settings);
  }

  public void SetGameStage(GameStage newGameStage)
  {
    if (newGameStage != _gameStage)
    {
      OnExitStage(_gameStage, newGameStage);
      OnEnterStage(newGameStage);
      _gameStage = newGameStage;
    }
  }

  public void OnExitStage(GameStage oldGameStage, GameStage newGameStage)
  {
    switch (oldGameStage)
    {
      case GameStage.MainMenu:
        {
          if (MusicMenuLoop != null && newGameStage != GameStage.Settings)
          {
            AudioManager.Instance.FadeOutSound(gameObject, MusicMenuLoop, 0.5f);
          }

          CameraControllerStack.Instance.PopController(MenuCamera);

          Destroy(_mainMenuUI);
          _mainMenuUI = null;
        }
        break;
      case GameStage.Settings:
        {
          CameraControllerStack.Instance.PopController(MenuCamera);

          Destroy(_settingsMenuUI);
          _settingsMenuUI = null;
        }
        break;
      case GameStage.DayIntro:
        {
          if (MusicDayIntro != null)
          {
            AudioManager.Instance.FadeOutSound(gameObject, MusicDayIntro, 1.0f);
          }

          CameraControllerStack.Instance.PopController(MenuCamera);

          Destroy(_dayIntroUI);
          _dayIntroUI = null;
          _dayIntroUIHander = null;
        }
        break;
      case GameStage.Daytime:
        {
          _playerSanity.OnCompletedDay();
          _screamBank.OnCompletedDay();

          CameraControllerStack.Instance.PopController(GameCamera);

          Destroy(_daytimeUI);
          _daytimeUI = null;
        }
        break;
      case GameStage.DayOutro:
        {
          if (MusicDayOutro != null)
          {
            AudioManager.Instance.FadeOutSound(gameObject, MusicDayIntro, 1.0f);
          }

          CameraControllerStack.Instance.PopController(MenuCamera);

          // Move on to the next day!
          _currentDay++;

          Destroy(_dayOutroUI);
          _dayOutroUI = null;
          _dayOutroUIHander = null;
        }
        break;
      case GameStage.WinGame:
        {
          CameraControllerStack.Instance.PopController(MenuCamera);

          Destroy(_winGameUI);
          _winGameUI = null;
        }
        break;
      case GameStage.LoseGame:
        {
          CameraControllerStack.Instance.PopController(MenuCamera);

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
          CameraControllerStack.Instance.PushController(MenuCamera);

          if (MusicMenuLoop != null)
          {
            AudioManager.Instance.FadeInSound(gameObject, MusicMenuLoop, 3.0f);
          }
        }
        break;
      case GameStage.Settings:
        {
          _settingsMenuUI = (GameObject)Instantiate(SettingsMenuUIPrefab, Vector3.zero, Quaternion.identity);
          CameraControllerStack.Instance.PushController(MenuCamera);
        }
        break;
      case GameStage.DayIntro:
        {
          _dayIntroUI = (GameObject)Instantiate(DayIntroUIPrefab, Vector3.zero, Quaternion.identity);
          _dayIntroUIHander = _dayIntroUI.GetComponent<DayIntroUIHandler>();
          CameraControllerStack.Instance.PushController(MenuCamera);

          if (MusicDayIntro != null)
          {
            AudioManager.Instance.FadeInSound(gameObject, MusicDayIntro, 3.0f);
          }

          DayIntroStarted?.Invoke();
        }
        break;
      case GameStage.Daytime:
        {
          _daytimeUI = (GameObject)Instantiate(DaytimeUIPrefab, Vector3.zero, Quaternion.identity);
          _playerSanity.OnStartedDay(CurrentDay);
          _screamBank.OnStartedDay(CurrentDay);
          CameraControllerStack.Instance.PushController(GameCamera);

          DaytimeStarted?.Invoke();
        }
        break;
      case GameStage.DayOutro:
        {
          _dayOutroUI = (GameObject)Instantiate(DayOutroUIPrefab, Vector3.zero, Quaternion.identity);
          _dayOutroUIHander = _dayOutroUI.GetComponent<DayOutroUIHandler>();
          CameraControllerStack.Instance.PushController(MenuCamera);

          if (MusicDayOutro != null)
          {
            AudioManager.Instance.FadeInSound(gameObject, MusicDayOutro, 3.0f);
          }

          DayOutroStarted?.Invoke();
        }
        break;
      case GameStage.WinGame:
        {
          _winGameUI = (GameObject)Instantiate(WinGameUIPrefab, Vector3.zero, Quaternion.identity);
          CameraControllerStack.Instance.PushController(MenuCamera);

          if (WinAlert != null)
          {
            AudioManager.Instance.PlaySound(WinAlert);
          }
        }
        break;
      case GameStage.LoseGame:
        {
          _loseGameUI = (GameObject)Instantiate(LoseGameUIPrefab, Vector3.zero, Quaternion.identity);
          CameraControllerStack.Instance.PushController(MenuCamera);

          if (LoseAlert != null)
          {
            AudioManager.Instance.PlaySound(LoseAlert);
          }
        }
        break;
    }
  }
}
