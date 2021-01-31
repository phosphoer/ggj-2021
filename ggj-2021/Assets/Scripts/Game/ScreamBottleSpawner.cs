using UnityEngine;
using System.Collections.Generic;

public class ScreamBottleSpawner : MonoBehaviour
{
  [SerializeField]
  private Transform[] _spawnPoints = null;

  [SerializeField]
  private RangedFloat _spawnInterval = new RangedFloat(30, 45);

  [SerializeField]
  private ScreamContainer _screamBottlePrefab = null;

  [SerializeField]
  private ScreamMappingDefinition _bottlePossibleScreams = null;

  private float _spawnTimer;
  private List<ScreamContainer> _spawnedContainers = new List<ScreamContainer>();

  private void Update()
  {
    _spawnTimer -= Time.deltaTime;
    if (_spawnTimer <= 0)
    {
      _spawnTimer = _spawnInterval.RandomValue;

      Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
      ScreamContainer screamContainer = Instantiate(_screamBottlePrefab, spawnPoint.position, Quaternion.identity);
      screamContainer.transform.position += (Random.insideUnitSphere * 10).WithY(0);

      for (int i = 0; i < 3; ++i)
      {
        ScreamSoundDefinition chosenSound = _bottlePossibleScreams.Screams[Random.Range(0, _bottlePossibleScreams.Screams.Count)];
        screamContainer.AddScream(chosenSound);
      }

      _spawnedContainers.Add(screamContainer);
    }
  }

  private void FixedUpdate()
  {
    for (int i = 0; i < _spawnedContainers.Count; ++i)
    {
      ScreamContainer bottle = _spawnedContainers[i];
      Vector3 playerPos = PlayerCharacterController.Instance.transform.position;
      Vector3 toPlayer = playerPos - bottle.transform.position;
      bottle.Holdable.Rigidbody.AddForce(toPlayer.normalized * 0.01f, ForceMode.VelocityChange);

      if (bottle.transform.position.y > 1)
      {
        _spawnedContainers.Remove(bottle);
      }
    }
  }
}