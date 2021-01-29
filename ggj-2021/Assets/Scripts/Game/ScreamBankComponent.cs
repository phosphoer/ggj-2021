using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamBankComponent : MonoBehaviour
{
    public int[] ScreamCountCalender = { 1, 2, 3, 4, 5 };
    public string[] ScreamNoteCalender = { "AAH", "EEH", "OOH" };

    private string _requireScreamNoteString;
    public string RequireScreamNoteString
    {
        get { return _requireScreamNoteString; }
    }

    private int _requireScreamNoteCount;
    public int RequireScreamNoteCount
    {
        get { return _requireScreamNoteCount; }
    }

    private int _currentScreamNoteCount;
    public int CurrentScreamNoteCount
    {
        get { return _currentScreamNoteCount; }
    }

    public bool HasReachedScreamNoteGoal
    {
        get { return _currentScreamNoteCount >= _requireScreamNoteCount; }
    }

    public void OnStartedDay(int dayIndex)
    {
        _currentScreamNoteCount = 0;
        _requireScreamNoteCount = ScreamCountCalender[dayIndex];
        _requireScreamNoteString = ScreamNoteCalender[dayIndex];
    }

    public void OnCompletedDay()
    {
    }

    public bool DepositScreamNote(string note)
    {
        if (note == RequireScreamNoteString)
        {
            _currentScreamNoteCount++;
            return true;
        }

        return false;
    }
}
