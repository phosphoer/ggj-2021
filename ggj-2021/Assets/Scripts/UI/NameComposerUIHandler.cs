using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameComposerUIHandler : UIPageBase
{
  [SerializeField]
  private Text[] _nameLabels;

  [SerializeField]
  private RMF_RadialMenu _radialMenu = null;

  private List<string> _fullName = new List<string>();

  protected override void Awake()
  {
    base.Awake();
    Shown += OnShown;
  }

  private void OnShown()
  {
    _fullName = new List<string>();
    RefreshNameButtonLabels();
  }

  void Update()
  {

  }

  void RefreshNameButtonLabels()
  {
    for (int labelIdx = 0; labelIdx < _nameLabels.Length; ++labelIdx)
    {
      string nameLabel = labelIdx < _fullName.Count ? _fullName[labelIdx] : "";

      _nameLabels[labelIdx].text = nameLabel;
    }
  }

  public void OnNameButtonClicked(Button button)
  {
    int targetLabelIndex = _fullName.Count;

    if (targetLabelIndex < _nameLabels.Length)
    {
      Text childLabel = button.GetComponentInChildren<Text>();
      string newNote = childLabel.text;

      _fullName.Add(newNote);
      RefreshNameButtonLabels();
    }
  }

  public void OnNameTrackButtonClicked(Button button)
  {
    int trackIndex = -1;
    for (int trackLabelIndex = 0; trackLabelIndex < _nameLabels.Length; ++trackLabelIndex)
    {
      Text screamSongLabel = _nameLabels[trackLabelIndex];
      Button parentButton = screamSongLabel.gameObject.GetComponentInParent<Button>();

      if (parentButton == button)
      {
        trackIndex = trackLabelIndex;
      }
    }

    if (trackIndex != -1)
    {
      _fullName.RemoveAt(trackIndex);
      RefreshNameButtonLabels();
    }
  }
}
