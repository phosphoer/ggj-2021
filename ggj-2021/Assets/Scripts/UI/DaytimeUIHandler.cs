using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct TutorialLine
{
  public SoundBank TutorialAudio;
  public string TutorialText;
  public float Duration;
}

public class DaytimeUIHandler : UIPageBase
{
  public List<TutorialLine> TutorialLines;
  private int _tutorialLineIndex = 0;
  private float _tutorialLineTimer = 0;
  private bool _isRunningTutorial = false;

  [SerializeField]
  private RectTransform _sanityBarRectTransform = null;

  [SerializeField]
  private Image _screamBankImage = null;

  [SerializeField]
  private Text _dayLabel = null;

  [SerializeField]
  private Text _tutorialTextField = null;

  protected override void Awake()
  {
    base.Awake();
    Shown += OnShown;
  }

  private void OnShown()
  {
    // Only show the tutorial on the first day
    if (GameStateManager.Instance.CurrentDay == 0)
    {
      _isRunningTutorial = true;
      FireCurrentTutorialLine();
    }

    if (_tutorialTextField != null)
    {
      _tutorialTextField.text = string.Format("Day {0}", GameStateManager.Instance.CurrentDay + 1);
    }
  }

  void Update()
  {
    if (_isRunningTutorial)
    {
      UpdateTutorialLineTimer();
    }

    RefreshSanityBar();
    RefreshScreamBankCircle();
  }

  void UpdateTutorialLineTimer()
  {
    if (_tutorialLineIndex < TutorialLines.Count)
    {
      TutorialLine tutorialLine = TutorialLines[_tutorialLineIndex];

      if (_tutorialLineTimer >= tutorialLine.Duration)
      {
        _tutorialLineTimer = 0;
        _tutorialLineIndex++;
        FireCurrentTutorialLine();
      }
      else
      {
        _tutorialLineTimer += Time.deltaTime;
      }
    }
  }

  void FireCurrentTutorialLine()
  {
    if (_tutorialLineIndex < TutorialLines.Count)
    {
      TutorialLine tutorialLine = TutorialLines[_tutorialLineIndex];

      if (tutorialLine.TutorialAudio != null)
      {
        AudioManager.Instance.PlaySound(tutorialLine.TutorialAudio);
      }

      if (_tutorialTextField != null)
      {
        _tutorialTextField.text = tutorialLine.TutorialText;
      }
    }
    else
    {
      if (_tutorialTextField != null)
      {
        _tutorialTextField.text = "";
      }
    }
  }

  void RefreshSanityBar()
  {
    _sanityBarRectTransform.transform.localScale = new Vector3(GameStateManager.Instance.PlayerSanity.SanityFraction, 1, 1);
  }

  void RefreshScreamBankCircle()
  {
    ScreamBankComponent bankComponent = GameStateManager.Instance.ScreamBank;

    if (bankComponent.TotalScreamNoteCount > 0)
    {
      _screamBankImage.fillAmount = 1.0f - (bankComponent.RemainingScreamNoteCount / (float)bankComponent.TotalScreamNoteCount);
    }
  }
}
