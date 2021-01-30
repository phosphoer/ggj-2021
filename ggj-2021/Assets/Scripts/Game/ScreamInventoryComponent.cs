using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamInventoryComponent : MonoBehaviour
{
  private Dictionary<string, int> _screamNoteCounts = new Dictionary<string, int>();

  public bool IsEmpty
  {
    get { return _screamNoteCounts.Count > 0; }
  }

  public int GetScreamNoteCount(string screamNote)
  {
    return _screamNoteCounts.ContainsKey(screamNote) ? _screamNoteCounts[screamNote] : 0;
  }

  public void AddScreamNote(string screamNote, int count)
  {
    if (_screamNoteCounts.ContainsKey(screamNote))
    {
      _screamNoteCounts[screamNote] = _screamNoteCounts[screamNote] + count;
    }
    else
    {
      _screamNoteCounts.Add(screamNote, count);
    }
  }

  public bool SubtractScreamNote(string screamNote, int count)
  {
    if (_screamNoteCounts.ContainsKey(screamNote) && _screamNoteCounts[screamNote] >= count)
    {
      _screamNoteCounts[screamNote] -= count;
      return true;
    }

    return false;
  }

  void Start()
  {

  }

  void Update()
  {

  }
}
