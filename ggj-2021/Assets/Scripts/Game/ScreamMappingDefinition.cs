using UnityEngine;

[CreateAssetMenu(fileName = "new-scream-mapping", menuName = "Scream Mapping")]
public class ScreamMappingDefinition : ScriptableObject
{
  [SerializeField]
  private ScreamSoundDefinition[] _screams = null;

  public ScreamSoundDefinition GetScream(string screamLetters)
  {
    for (int i = 0; i < _screams.Length; ++i)
    {
      if (_screams[i].Letters == screamLetters)
        return _screams[i];
    }

    return null;
  }
}