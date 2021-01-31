using UnityEngine;

public class Buoyancy : MonoBehaviour 
{
  [SerializeField]
  private Rigidbody _rb = null;

  private void FixedUpdate()
  {
    float waterDepth = -Mathf.Min(0, transform.position.y);

    _rb.AddForce(Vector3.up * waterDepth * 2, ForceMode.VelocityChange);
    _rb.AddForce(_rb.velocity * -0.1f * waterDepth, ForceMode.VelocityChange);
  }
}