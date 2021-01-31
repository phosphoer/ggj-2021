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
  [SerializeField]
  private RectTransform _sanityBarRectTransform = null;

  [SerializeField]
  private Image _screamBankImage = null;

  [SerializeField]
  private Text _dayLabel = null;

  protected override void Awake()
  {
    base.Awake();
    Shown += OnShown;
  }

  private void OnShown()
  {
    _dayLabel.text = string.Format("Day {0}", GameStateManager.Instance.CurrentDay + 1);
  }

  void Update()
  {
    RefreshSanityBar();
    RefreshScreamBankCircle();
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
