using UnityEngine;

public class SanityRestoreInWater : MonoBehaviour
{
  public float RestoreAmount = 25;

  private void Update()
  {
    if (transform.position.y < -1)
    {
      GameStateManager.Instance.PlayerSanity.RestoreSanity(RestoreAmount);
      Destroy(gameObject);
    }
  }
}