using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
  [SerializeField]
  private AICharacterController _monsterPrefab = null;

  [SerializeField]
  private RangedFloat _respawnInterval = new RangedFloat(15, 30);

  private AICharacterController _spawnedMonster = null;
  private float _respawnTimer = 0;

  private void Start()
  {
    _respawnTimer = _respawnInterval.RandomValue * 0.5f;
  }

  private void Update()
  {
    if (GameStateManager.Instance.CurrentStage == GameStateManager.GameStage.Daytime)
    {
      if (_spawnedMonster == null || _spawnedMonster.AIAnimator.IsDead)
      {
        _respawnTimer -= Time.deltaTime;
        if (_respawnTimer <= 0)
        {
          _respawnTimer = _respawnInterval.RandomValue;

          Vector3 spawnPos = transform.position;
          PathFindManager.Instance.TryGetTraversablePoint(spawnPos, out spawnPos, 10);
          _spawnedMonster = Instantiate(_monsterPrefab, spawnPos, transform.rotation);
        }
      }
    }
  }
}