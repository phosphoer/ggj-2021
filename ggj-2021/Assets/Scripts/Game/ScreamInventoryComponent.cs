using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamInventoryComponent : MonoBehaviour
{
  [SerializeField]
  public string[] _sourceBottleNotes = new string[3];
  [SerializeField]
  public string[] _targetBottleNotes = new string[3];

  private ScreamMappingDefinition _screamMappingDefinition;
  public ScreamMappingDefinition ScreamMapping
  {
    get { return _screamMappingDefinition; }
    set { _screamMappingDefinition = value; }
  }

  public void SetSourceBottleNote(int slotIndex, string note)
  {
    _sourceBottleNotes[slotIndex] = note;
  }

  public string GetSourceBottleNote(int slotIndex)
  {
    return _sourceBottleNotes[slotIndex];
  }

  public void SetTargetBottleNote(int slotIndex, string note)
  {
    _targetBottleNotes[slotIndex] = note;
  }

  public string GetTargetBottleNote(int slotIndex)
  {
    return _targetBottleNotes[slotIndex];
  }

  public void SwapNotes(int sourceSlotIndex, int targetSlotIndex)
  {
    string temp = _targetBottleNotes[targetSlotIndex];
    _targetBottleNotes[targetSlotIndex] = _sourceBottleNotes[sourceSlotIndex];
    _sourceBottleNotes[sourceSlotIndex] = temp;
  }
}
