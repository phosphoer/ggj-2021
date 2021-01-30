using UnityEngine;

public class ScreamContainer : MonoBehaviour
{
  [SerializeField]
  private ScreamController _screamController = null;

  [SerializeField]
  private string _screamString = "eeh ooh aah";

  public void FillScream(string inputScream)
  {
    _screamString = inputScream;
  }

  public void ReleaseScream()
  {
    if (!string.IsNullOrEmpty(_screamString))
    {
      _screamController.StartScream(_screamString, loopScream: false, volumeScale: 1);
      _screamString = null;
    }
  }

  private void Start()
  {
    if (!string.IsNullOrEmpty(_screamString))
    {
      _screamController.StartScream(_screamString, loopScream: true, volumeScale: 0.25f);
    }
  }
}