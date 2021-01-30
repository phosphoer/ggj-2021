using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PathFindManager : Singleton<PathFindManager>
{
  public bool IsBuilt => _isBuilt;

  [SerializeField]
  private LayerMask _obstacleMask = Physics.DefaultRaycastLayers;

  [SerializeField]
  private bool _buildOnStart = true;

  [SerializeField]
  private Vector3 _minBounds = new Vector3(-100, -100, -100);

  [SerializeField]
  private Vector3 _maxBounds = new Vector3(100, 100, 100);

  private bool _isBuilt;
  private Bounds _worldBounds;
  private NavMeshData _navMesh;
  private NavMeshPath _path;

  private static List<NavMeshBuildSource> _navMeshBuildSources = new List<NavMeshBuildSource>();
  private static List<NavMeshBuildMarkup> _navMeshMarkups = new List<NavMeshBuildMarkup>();
  private static List<PathFindObject> _pathFindObjects = new List<PathFindObject>();

  public static void AddObject(PathFindObject pathFindObject)
  {
    _pathFindObjects.Add(pathFindObject);
  }

  public static void RemoveObject(PathFindObject pathFindObject)
  {
    _pathFindObjects.Remove(pathFindObject);
  }

  public void GenerateGrid(Vector3 minCorner, Vector3 maxCorner)
  {
    _worldBounds.min = minCorner;
    _worldBounds.max = maxCorner;

    // Find all nav mesh object sources in the scene
    NavMeshBuilder.CollectSources(_worldBounds, _obstacleMask, NavMeshCollectGeometry.PhysicsColliders, 1, _navMeshMarkups, _navMeshBuildSources);
    for (int i = 0; i < _pathFindObjects.Count; ++i)
    {
      _navMeshBuildSources.Add(_pathFindObjects[i].Source);
    }

    // Add a source to represent the 'ground' plane
    // NavMeshBuildSource groundSource = default(NavMeshBuildSource);
    // groundSource.area = 0;
    // groundSource.shape = NavMeshBuildSourceShape.Box;
    // groundSource.size = _worldBounds.size.WithY(0.1f);
    // groundSource.transform = Matrix4x4.identity * Matrix4x4.Translate(_worldBounds.center.WithY(0));
    // _navMeshBuildSources.Add(groundSource);

    // Build settings for default agent
    NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);

    // Create or update the nav mesh
    if (_navMesh == null)
    {
      _navMesh = NavMeshBuilder.BuildNavMeshData(buildSettings, _navMeshBuildSources, _worldBounds, Vector3.zero, Quaternion.identity);
      NavMesh.AddNavMeshData(_navMesh);
    }
    else
    {
      NavMeshBuilder.UpdateNavMeshData(_navMesh, buildSettings, _navMeshBuildSources, _worldBounds);
    }

    _isBuilt = true;
  }

  public void RebuildGrid(bool skipIfNotBuilt = false)
  {
    if (!_isBuilt)
    {
      if (skipIfNotBuilt)
        return;

      Debug.LogError("Attempt to rebuild grid without ever being initialized");
      return;
    }

    Debug.Log("Rebuilding pathfind grid");
    GenerateGrid(_worldBounds.min, _worldBounds.max);
  }

  public bool TryGetTraversablePoint(Vector3 worldPoint, out Vector3 pointOnNavMesh, float maxDistance = 1.0f)
  {
    NavMeshHit navMeshHit;
    if (NavMesh.SamplePosition(worldPoint, out navMeshHit, maxDistance, NavMesh.AllAreas))
    {
      pointOnNavMesh = navMeshHit.position;
      return true;
    }

    pointOnNavMesh = worldPoint;
    return false;
  }

  public bool IsPointTraversable(Vector3 worldPoint)
  {
    if (_worldBounds.Contains(worldPoint))
    {
      NavMeshHit navMeshHit;
      if (NavMesh.SamplePosition(worldPoint, out navMeshHit, 3f, NavMesh.AllAreas))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    return true;
  }

  public bool CalculatePathToPoint(Vector3 fromPoint, Vector3 toPoint, List<Vector3> outPath)
  {
    outPath.Clear();
    if (!IsPointTraversable(fromPoint))
    {
      NavMeshHit navMeshHit;
      if (NavMesh.SamplePosition(fromPoint, out navMeshHit, 10f, NavMesh.AllAreas))
      {
        fromPoint = navMeshHit.position;
        outPath.Add(fromPoint);
      }
    }

    if (NavMesh.CalculatePath(fromPoint, toPoint, NavMesh.AllAreas, _path))
    {
      outPath.AddRange(_path.corners);
      return true;
    }

    return false;
  }

  private void Awake()
  {
    Instance = this;
    _path = new NavMeshPath();
  }

  private void Start()
  {
    if (_buildOnStart)
    {
      GenerateGrid(_minBounds, _maxBounds);
    }
  }
}