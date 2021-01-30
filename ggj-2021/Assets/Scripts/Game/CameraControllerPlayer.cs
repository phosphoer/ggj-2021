using UnityEngine;

public class CameraControllerPlayer : CameraControllerBase
{
  public Vector3 CameraWorldOffset = new Vector3(5, 10, -5);
  public Vector3 CameraLookOffset = Vector3.zero;

  private void LateUpdate()
  {
    var playerController = PlayerCharacterController.Instance;
    Vector3 playerPos = playerController.transform.position;
    Vector3 cameraPos = playerPos + CameraWorldOffset;
    Vector3 cameraLook = playerPos + CameraLookOffset;

    transform.position = cameraPos;
    transform.rotation = Quaternion.LookRotation(cameraLook - cameraPos);
  }
}