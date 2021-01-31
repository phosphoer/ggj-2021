using UnityEngine;
using System.Collections;

public class DialogUI : MonoBehaviour
{
  [SerializeField]
  private DialogItemUI _dialogPrefab = null;

  public void ShowDialog(string text, float duration, Transform speakerTransform, Vector3 worldOffset)
  {
    StartCoroutine(ShowDialogAsync(text, duration, speakerTransform, worldOffset));
  }

  private IEnumerator ShowDialogAsync(string text, float duration, Transform speakerTransform, Vector3 worldOffset)
  {
    RectTransform uiRoot = GameUI.Instance.WorldAttachedUI.ShowItem(speakerTransform, worldOffset);
    DialogItemUI dialogItem = Instantiate(_dialogPrefab, uiRoot);
    dialogItem.Text = text;

    yield return Tween.WaitForTime(duration);

    GameUI.Instance.WorldAttachedUI.HideItem(uiRoot);
  }
}