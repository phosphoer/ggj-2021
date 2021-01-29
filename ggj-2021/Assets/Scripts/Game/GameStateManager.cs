using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    public enum GameStage
    {
        Invalid,
        MainMenu,
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
    public CameraControllerGame GameCamera;

    private GameStage _gameStage = GameStage.Invalid;
    private GameObject _mainMenuUI = null;
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
                        nextGameStage = GameStage.DayOutro;
                    }
                    else
                    {
                        nextGameStage = GameStage.WinGame;
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
            case GameStage.DayIntro:
                {
                    _dayIntroUI = (GameObject)Instantiate(DaytimeUIPrefab, Vector3.zero, Quaternion.identity);
                    _dayIntroUIHander = _dayIntroUI.GetComponent<DayIntroUIHandler>();
                    _playerSanity.OnStartedDay(CurrentDay);
                    _screamBank.OnStartedDay(CurrentDay);

                    if (MusicDayIntro != null)
                    {
                        AudioManager.Instance.FadeInSound(gameObject, MusicDayIntro, 3.0f);
                    }

                    CameraControllerStack.Instance.PushController(MenuCamera);

                    DayIntroStarted?.Invoke();
                }
                break;
            case GameStage.Daytime:
                {
                    _daytimeUI = (GameObject)Instantiate(DaytimeUIPrefab, Vector3.zero, Quaternion.identity);
                    _playerSanity.OnStartedDay(CurrentDay);

                    CameraControllerStack.Instance.PushController(GameCamera);

                    DaytimeStarted?.Invoke();
                }
                break;
            case GameStage.DayOutro:
                {
                    _dayOutroUI = (GameObject)Instantiate(DaytimeUIPrefab, Vector3.zero, Quaternion.identity);
                    _dayOutroUIHander = _dayIntroUI.GetComponent<DayOutroUIHandler>();
                    _playerSanity.OnCompletedDay();
                    _screamBank.OnCompletedDay();

                    if (MusicDayOutro != null)
                    {
                        AudioManager.Instance.FadeInSound(gameObject, MusicDayOutro, 3.0f);
                    }

                    CameraControllerStack.Instance.PushController(MenuCamera);

                    DayIntroStarted?.Invoke();
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
