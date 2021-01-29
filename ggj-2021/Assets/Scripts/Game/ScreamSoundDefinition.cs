using UnityEngine;

[CreateAssetMenu(fileName = "new-scream-sound", menuName = "Scream Sound")]
public class ScreamSoundDefinition : ScriptableObject
{
  public Color Color = Color.white;
  public string Letters;
  public SoundBank Sound;
}