using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScreamCalendarDay
{
  public ScreamMappingDefinition PossibleScreamRequests = null;
  public int MainScreamCount = 1;
  public int CommonScreamCount = 3;
}

public class ScreamBankComponent : MonoBehaviour
{
  public event System.Action RemainingScreamsChanged;

  public int TotalScreamNoteCount => _currentDayRequests.Count;
  public int RemainingScreamNoteCount => _remainingRequests.Count;
  public bool HasReachedScreamNoteGoal => _remainingRequests.Count == 0;
  public IReadOnlyList<ScreamSoundDefinition> RemainingRequests => _remainingRequests;

  [SerializeField]
  private ScreamCalendarDay[] _calendarDays = null;

  private List<ScreamSoundDefinition> _currentDayRequests = new List<ScreamSoundDefinition>();
  private List<ScreamSoundDefinition> _remainingRequests = new List<ScreamSoundDefinition>();

  public void OnStartedDay(int dayIndex)
  {
    ScreamCalendarDay calendarDay = _calendarDays[dayIndex];

    _currentDayRequests.Clear();
    _remainingRequests.Clear();

    for (int i = 0; i < calendarDay.MainScreamCount; ++i)
      _currentDayRequests.Add(calendarDay.PossibleScreamRequests.Screams[0]);

    for (int i = 0; i < calendarDay.CommonScreamCount; ++i)
    {
      int randomIndex = Random.Range(0, calendarDay.PossibleScreamRequests.Screams.Count);
      _currentDayRequests.Add(calendarDay.PossibleScreamRequests.Screams[randomIndex]);
    }

    _remainingRequests.AddRange(_currentDayRequests);

    RemainingScreamsChanged?.Invoke();
  }

  public void OnCompletedDay()
  {

  }
}
