using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayIntroUIHandler : UIPageBase
{
  [SerializeField]
  private Text _titleTextField = null;

  public float ShowDuration = 3;
  public float _timer;

  public bool IsComplete()
  {
    return _timer <= 0;
  }

  protected override void Awake()
  {
    base.Awake();
    _timer = ShowDuration;
    Shown += OnShown;
  }

  private void OnShown()
  {
    if (_titleTextField != null)
    {
      int day = GameStateManager.Instance.CurrentDay + 1;
      if (day < GameStateManager.Instance.TotalDays)
      {
        switch (day)
        {
          case 1:
            _titleTextField.text = "Start of the 1st day";
            break;
          case 2:
            _titleTextField.text = "Start of the 2nd day";
            break;
          case 3:
            _titleTextField.text = "Start of the 3rd day";
            break;
          default:
            _titleTextField.text = string.Format("Start of the {0}th day");
            break;
        }
      }
      else
      {
        _titleTextField.text = "Start of the FINAL day";
      }
    }
  }

  void Update()
  {
    _timer -= Time.deltaTime;
  }
}
