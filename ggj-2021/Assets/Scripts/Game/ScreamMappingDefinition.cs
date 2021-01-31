using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new-scream-mapping", menuName = "Scream Mapping")]
public class ScreamMappingDefinition : ScriptableObject
{
  public IReadOnlyList<ScreamSoundDefinition> Screams => _screams;

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