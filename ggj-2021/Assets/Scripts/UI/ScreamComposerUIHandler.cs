using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreamComposerUIHandler : UIPageBase
{
  public Color UnselectedColor = Color.black;
  public Color SelectedColor = Color.red;

  [SerializeField]
  private ScreamInventoryComponent _screamInventory;

  [SerializeField]
  private Text[] _sourceScreamSongLabels; // left side

  [SerializeField]
  private Text[] _targetScreamSongLabels; // right side

  private int _selectedSourceSlotIndex = -1;
  private int _selectedTargetSlotIndex = -1;

  protected override void Awake()
  {
    base.Awake();
    Shown += OnShown;
  }

  private void OnShown()
  {
    RefreshSongButtonLabels();
  }

  void Update()
  {

  }

  void RefreshSongButtonLabels()
  {
    for (int slotIdx = 0; slotIdx < 3; ++slotIdx)
    {
      _sourceScreamSongLabels[slotIdx].text = _screamInventory.GetSourceBottleNote(slotIdx);
      _targetScreamSongLabels[slotIdx].text = _screamInventory.GetTargetBottleNote(slotIdx);
    }
  }

  private int FindSlotIndexForButton(Button button, Text[] labels)
  {
    for (int slotIndex = 0; slotIndex < labels.Length; ++slotIndex)
    {
      Text label = labels[slotIndex];
      if (label.gameObject.GetComponentInParent<Button>() == button)
      {
        return slotIndex;
      }
    }

    return -1;
  }

  public void OnSourceNoteButtonClicked(Button button)
  {
    int sourceSlotIndex = FindSlotIndexForButton(button, _sourceScreamSongLabels);

    // Has a slot in the source been selected?
    if (_selectedSourceSlotIndex != -1)
    {
      // Is it a new slot
      if (_selectedSourceSlotIndex != sourceSlotIndex)
      {
        // Deselect the old slot
        _sourceScreamSongLabels[_selectedSourceSlotIndex].color = UnselectedColor;
        // Select the new slot
        _sourceScreamSongLabels[sourceSlotIndex].color = SelectedColor;
        _selectedSourceSlotIndex = sourceSlotIndex;
      }
      else
      {
        // Do nothing
      }
    }
    else
    {
      // Is there at selected slot in the target bottle?
      if (_selectedTargetSlotIndex != -1)
      {
        // Swap notes
        _screamInventory.SwapNotes(sourceSlotIndex, _selectedTargetSlotIndex);
        RefreshSongButtonLabels();

        // Deselect the target slot
        _sourceScreamSongLabels[_selectedTargetSlotIndex].color = UnselectedColor;
        _selectedTargetSlotIndex = -1;
      }
      else
      {
        // Select slot
        _sourceScreamSongLabels[sourceSlotIndex].color = SelectedColor;
        _selectedSourceSlotIndex = sourceSlotIndex;
      }
    }
  }

  public void OnTargetNoteButtonClicked(Button button)
  {
    int targetSlotIndex = FindSlotIndexForButton(button, _targetScreamSongLabels);

    // Has a slot in the target been selected?
    if (_selectedTargetSlotIndex != -1)
    {
      // Is it a new slot
      if (_selectedTargetSlotIndex != targetSlotIndex)
      {
        // Deselect the old slot
        _targetScreamSongLabels[_selectedTargetSlotIndex].color = UnselectedColor;
        // Select the new slot
        _targetScreamSongLabels[targetSlotIndex].color = SelectedColor;
        _selectedTargetSlotIndex = targetSlotIndex;
      }
      else
      {
        // Do nothing
      }
    }
    else
    {
      // Is there at selected slot in the target bottle?
      if (_selectedSourceSlotIndex != -1)
      {
        // Swap notes
        _screamInventory.SwapNotes(_selectedSourceSlotIndex, targetSlotIndex);
        RefreshSongButtonLabels();

        // Deselect the source slot
        _targetScreamSongLabels[_selectedSourceSlotIndex].color = UnselectedColor;
        _selectedSourceSlotIndex = -1;
      }
      else
      {
        // Select slot
        _targetScreamSongLabels[targetSlotIndex].color = SelectedColor;
        _selectedTargetSlotIndex = targetSlotIndex;
      }
    }
  }
}
