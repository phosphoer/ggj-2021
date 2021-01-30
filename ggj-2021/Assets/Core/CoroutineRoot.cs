using UnityEngine;

public class CoroutineRoot : MonoBehaviour
{
  public static CoroutineRoot Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = new GameObject("coroutine-root").AddComponent<CoroutineRoot>();
        DontDestroyOnLoad(_instance.gameObject);
      }

      return _instance;
    }
  }

  private static CoroutineRoot _instance = null;
}