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

  public SoundBank MusicMenuLoop;
  public SoundBank MusicHigh;
  public SoundBank MusicMid;
  public SoundBank MusicLow;
  public SoundBank WinAlert;
  public SoundBank LoseAlert;
  public CameraControllerBase MenuCamera;

  private GameStage _gameStage = GameStage.Invalid;

  private DayIntroUIHandler _dayIntroUIHander = null;
  private DayOutroUIHandler _dayOutroUIHander = null;

  // Cross Day Stats
  private int _bottlesPickedUpCount = 0;
  public int BottlesPickedUpCount
  {
    get { return _bottlesPickedUpCount; }
    set { _bottlesPickedUpCount = value; }
  }

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

  private AudioManager.AudioInstance _musicLowInstance;
  private AudioManager.AudioInstance _musicMidInstance;
  private AudioManager.AudioInstance _musicHighInstance;

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
        if (_playerSanity.EnemyPursuitCount > 0)
        {
          _musicHighInstance.AudioSource.volume = Mathfx.Damp(_musicHighInstance.AudioSource.volume, MusicHigh.VolumeScale, 0.25f, Time.deltaTime);
          _musicMidInstance.AudioSource.volume = Mathfx.Damp(_musicMidInstance.AudioSource.volume, 0f, 0.25f, Time.deltaTime);
          _musicLowInstance.AudioSource.volume = Mathfx.Damp(_musicLowInstance.AudioSource.volume, 0f, 0.25f, Time.deltaTime);
        }
        else if (PlayerCharacterController.Instance.IsSneaking)
        {
          _musicHighInstance.AudioSource.volume = Mathfx.Damp(_musicHighInstance.AudioSource.volume, 0f, 0.25f, Time.deltaTime);
          _musicMidInstance.AudioSource.volume = Mathfx.Damp(_musicMidInstance.AudioSource.volume, 0f, 0.25f, Time.deltaTime);
          _musicLowInstance.AudioSource.volume = Mathfx.Damp(_musicLowInstance.AudioSource.volume, MusicLow.VolumeScale, 0.25f, Time.deltaTime);
        }
        else
        {
          _musicHighInstance.AudioSource.volume = Mathfx.Damp(_musicHighInstance.AudioSource.volume, 0f, 0.25f, Time.deltaTime);
          _musicMidInstance.AudioSource.volume = Mathfx.Damp(_musicMidInstance.AudioSource.volume, MusicMid.VolumeScale, 0.25f, Time.deltaTime);
          _musicLowInstance.AudioSource.volume = Mathfx.Damp(_musicLowInstance.AudioSource.volume, 0f, 0.25f, Time.deltaTime);
        }

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
            AudioManager.Instance.FadeOutSound(gameObject, MusicMenuLoop, 3f);
          }

          CameraControllerStack.Instance.PopController(MenuCamera);

          GameUI.Instance.MainMenuUI.Hide();
        }
        break;
      case GameStage.Settings:
        {
          CameraControllerStack.Instance.PopController(MenuCamera);

          GameUI.Instance.SettingsUI.Hide();
        }
        break;
      case GameStage.DayIntro:
        {
          CameraControllerStack.Instance.PopController(MenuCamera);

          GameUI.Instance.DayIntroUI.Hide();
          _dayIntroUIHander = null;
        }
        break;
      case GameStage.Daytime:
        {
          _playerSanity.OnCompletedDay();
          _screamBank.OnCompletedDay();

          GameUI.Instance.DaytimeUI.Hide();
        }
        break;
      case GameStage.DayOutro:
        {
          CameraControllerStack.Instance.PopController(MenuCamera);

          // Move on to the next day!
          _currentDay++;

          GameUI.Instance.DayOutroUI.Hide();
          _dayOutroUIHander = null;
        }
        break;
      case GameStage.WinGame:
        {
          CameraControllerStack.Instance.PopController(MenuCamera);

          GameUI.Instance.WinGameUI.Hide();
        }
        break;
      case GameStage.LoseGame:
        {
          CameraControllerStack.Instance.PopController(MenuCamera);

          GameUI.Instance.LoseGameUI.Hide();
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
          GameUI.Instance.MainMenuUI.Show();

          if (MusicMenuLoop != null)
          {
            AudioManager.Instance.FadeInSound(gameObject, MusicMenuLoop, 3.0f);
          }

          ResetGameStats();
        }
        break;
      case GameStage.Settings:
        {
          GameUI.Instance.SettingsUI.Show();
          CameraControllerStack.Instance.PushController(MenuCamera);
        }
        break;
      case GameStage.DayIntro:
        {
          PlayerCharacterController.Instance.Respawn();

          GameUI.Instance.DayIntroUI.Show();
          _dayIntroUIHander = GameUI.Instance.DayIntroUI.GetComponent<DayIntroUIHandler>();
          CameraControllerStack.Instance.PushController(MenuCamera);

          _musicLowInstance = AudioManager.Instance.PlaySound(gameObject, MusicLow, 0);
          _musicMidInstance = AudioManager.Instance.PlaySound(gameObject, MusicMid, 0);
          _musicHighInstance = AudioManager.Instance.PlaySound(gameObject, MusicHigh, 0);

          DayIntroStarted?.Invoke();
        }
        break;
      case GameStage.Daytime:
        {
          GameUI.Instance.DaytimeUI.Show();
          _playerSanity.OnStartedDay(CurrentDay);
          _screamBank.OnStartedDay(CurrentDay);

          if (_currentDay == 0)
          {
            GameUI.Instance.DialogUI.ShowDialog("Looks like I crashed my boat again...time to explore!", 5, PlayerCharacterController.Instance.transform, Vector3.up * 3);
          }
          else if (_currentDay == 1)
          {
            GameUI.Instance.DialogUI.ShowDialog("Oh boy another day!", 5, PlayerCharacterController.Instance.transform, Vector3.up * 3);
          }
          else if (_currentDay == 2)
          {
            GameUI.Instance.DialogUI.ShowDialog("I feel like this might be my last day here!", 5, PlayerCharacterController.Instance.transform, Vector3.up * 3);
          }

          DaytimeStarted?.Invoke();
        }
        break;
      case GameStage.DayOutro:
        {
          GameUI.Instance.DayOutroUI.Show();
          _dayOutroUIHander = GameUI.Instance.DayOutroUI.GetComponent<DayOutroUIHandler>();
          CameraControllerStack.Instance.PushController(MenuCamera);

          AudioManager.Instance.FadeOutSound(gameObject, MusicLow, 3);
          AudioManager.Instance.FadeOutSound(gameObject, MusicMid, 3);
          AudioManager.Instance.FadeOutSound(gameObject, MusicHigh, 3);

          DayOutroStarted?.Invoke();
        }
        break;
      case GameStage.WinGame:
        {
          GameUI.Instance.WinGameUI.Show();
          CameraControllerStack.Instance.PushController(MenuCamera);

          if (WinAlert != null)
          {
            AudioManager.Instance.PlaySound(WinAlert);
          }
        }
        break;
      case GameStage.LoseGame:
        {
          GameUI.Instance.LoseGameUI.Show();
          CameraControllerStack.Instance.PushController(MenuCamera);

          if (LoseAlert != null)
          {
            AudioManager.Instance.PlaySound(LoseAlert);
          }
        }
        break;
    }
  }

  void ResetGameStats()
  {
    _bottlesPickedUpCount = 0;
  }
}
