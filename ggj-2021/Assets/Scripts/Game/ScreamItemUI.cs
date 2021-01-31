using UnityEngine;
using UnityEngine.UI;

public class ScreamItemUI : MonoBehaviour
{
  public ScreamSoundDefinition ScreamSound
  {
    get { return _screamSound; }
    set
    {
      if (_screamSound != value)
      {
        _screamSound = value;
        RefreshUI();
      }
    }
  }

  [SerializeField]
  private Text _text = null;

  private ScreamSoundDefinition _screamSound = null;

  private void RefreshUI()
  {
    _text.text = _screamSound.Letters.ToUpper();
    _text.color = _screamSound.Color;
  }
}