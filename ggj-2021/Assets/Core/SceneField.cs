// Taken from http://wiki.unity3d.com/index.php/SceneField

using UnityEngine;

[System.Serializable]
public class SceneField : ISerializationCallbackReceiver
{

#if UNITY_EDITOR
  public UnityEditor.SceneAsset sceneAsset;
#endif

#pragma warning disable 414
  [SerializeField, HideInInspector]
  private string sceneName = "";
#pragma warning restore 414


  // Makes it work with the existing Unity methods (LoadLevel/LoadScene)
  public static implicit operator string(SceneField sceneField)
  {
#if UNITY_EDITOR
    return System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GetAssetPath(sceneField.sceneAsset));
#else
    return sceneField.sceneName;
#endif
  }

  public void OnBeforeSerialize()
  {
#if UNITY_EDITOR
    sceneName = this;
#endif
  }
  public void OnAfterDeserialize() { }
}