using UnityEngine;

public class PlaySoundEventOnStart : MonoBehaviour
{
  public SoundBank SoundBank;
  public float FadeInTime;

  private void Start()
  {
    if (FadeInTime > 0)
    {
      StartCoroutine(AudioManager.QueueFadeInSoundRoutine(gameObject, SoundBank, 1.0f, FadeInTime));
    }
    else
    {
      StartCoroutine(AudioManager.QueuePlaySoundRoutine(gameObject, SoundBank));
    }
  }
}