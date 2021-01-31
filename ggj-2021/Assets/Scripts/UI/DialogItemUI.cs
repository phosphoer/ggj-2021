using UnityEngine;
using UnityEngine.UI;

public class DialogItemUI : MonoBehaviour
{
  public string Text
  {
    get { return _dialogText.text; }
    set
    {
      _dialogText.text = value;
    }
  }

  [SerializeField]
  private Text _dialogText = null;
}