using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamInventoryComponent : MonoBehaviour
{
  [SerializeField]
  public string[] _sourceBottleNotes = new string[3];
  [SerializeField]
  public string[] _targetBottleNotes = new string[3];

  private ScreamContainer _sourceBottle;
  private ScreamContainer _targetBottle;

  private ScreamMappingDefinition _screamMappingDefinition;
  public ScreamMappingDefinition ScreamMapping
  {
    get { return _screamMappingDefinition; }
    set { _screamMappingDefinition = value; }
  }

  public void StartMixingBottles(ScreamContainer heldBottle, ScreamContainer groundBottle)
  {
    Debug.Log($"Started mixing bottles {heldBottle.name} and {groundBottle.name}");
    ScreamInventoryComponent inventoryComponent = GameUI.Instance.ScreamComposerUI.ScreamInventory;

    _sourceBottle = heldBottle;
    _targetBottle = groundBottle;

    string[] sourceScreamNotes = _sourceBottle.GetScreamNotes();
    string[] targetScreamNotes = _targetBottle.GetScreamNotes();

    if (sourceScreamNotes.Length == 3 && targetScreamNotes.Length == 3)
    {
      for (int slotIndex = 0; slotIndex < 3; ++slotIndex)
      {
        inventoryComponent.SetSourceBottleNote(slotIndex, sourceScreamNotes[slotIndex]);
        inventoryComponent.SetTargetBottleNote(slotIndex, targetScreamNotes[slotIndex]);
      }
    }
  }

  public void FinishMixingBottles()
  {
    string sourceScreamString = string.Join(" ", _sourceBottleNotes);
    string targetScreamString = string.Join(" ", _targetBottleNotes);

    _sourceBottle.FillScream(sourceScreamString);
    _targetBottle.FillScream(targetScreamString);
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
