using UnityEngine;
using UnityEngine.AI;

public class PathFindObject : MonoBehaviour
{
  public NavMeshBuildSource Source
  {
    get
    {
      NavMeshBuildSource source = default(NavMeshBuildSource);
      source.area = _area;
      source.transform = transform.localToWorldMatrix;
      source.size = Vector3.one;
      source.shape = NavMeshBuildSourceShape.Mesh;
      source.sourceObject = GetComponent<MeshFilter>().sharedMesh;
      return source;
    }
  }

  [SerializeField]
  private int _area = 0;

  private void OnEnable()
  {
    PathFindManager.AddObject(this);
  }

  private void OnDisable()
  {
    PathFindManager.RemoveObject(this);
  }
}