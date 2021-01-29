using UnityEngine;

[CreateAssetMenu(fileName = "new-scream-sound", menuName = "Scream Sound")]
public class ScreamSoundDefinition : ScriptableObject
{
  public string Letters;
  public SoundBank Sound;
}