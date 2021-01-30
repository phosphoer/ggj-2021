using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayOutroUIHandler : UIPageBase
{
  public float ShowDuration = 3;
  public float _timer;

  [SerializeField]
  private Text _titleTextField = null;

  public bool IsComplete()
  {
    return _timer <= 0;
  }

  protected override void Awake()
  {
    base.Awake();
    _timer = ShowDuration;
  }

  void Start()
  {
    if (_titleTextField != null)
    {
      int day = GameStateManager.Instance.CurrentDay + 1;
      switch (day)
      {
        case 1:
          _titleTextField.text = "End of the 1st day";
          break;
        case 2:
          _titleTextField.text = "End of the 2nd day";
          break;
        case 3:
          _titleTextField.text = "End of the 3rd day";
          break;
        default:
          _titleTextField.text = string.Format("End of the {0}th day");
          break;
      }
    }
  }

  void Update()
  {
    _timer -= Time.deltaTime;
  }
}
