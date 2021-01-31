using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamInventoryComponent : MonoBehaviour
{
  private ScreamSoundDefinition[] _sourceBottleNotes = new ScreamSoundDefinition[3];
  private ScreamSoundDefinition[] _targetBottleNotes = new ScreamSoundDefinition[3];
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

    var sourceScreamNotes = _sourceBottle.ScreamSounds;
    var targetScreamNotes = _targetBottle.ScreamSounds;

    if (sourceScreamNotes.Count == 3 && targetScreamNotes.Count == 3)
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
    _sourceBottle.FillScream(_sourceBottleNotes);
    _targetBottle.FillScream(_targetBottleNotes);
  }

  public void SetSourceBottleNote(int slotIndex, ScreamSoundDefinition note)
  {
    _sourceBottleNotes[slotIndex] = note;
  }

  public ScreamSoundDefinition GetSourceBottleNote(int slotIndex)
  {
    return _sourceBottleNotes[slotIndex];
  }

  public void SetTargetBottleNote(int slotIndex, ScreamSoundDefinition note)
  {
    _targetBottleNotes[slotIndex] = note;
  }

  public ScreamSoundDefinition GetTargetBottleNote(int slotIndex)
  {
    return _targetBottleNotes[slotIndex];
  }

  public void SwapNotes(int sourceSlotIndex, int targetSlotIndex)
  {
    ScreamSoundDefinition temp = _targetBottleNotes[targetSlotIndex];
    _targetBottleNotes[targetSlotIndex] = _sourceBottleNotes[sourceSlotIndex];
    _sourceBottleNotes[sourceSlotIndex] = temp;
  }
}
