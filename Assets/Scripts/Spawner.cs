using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private Obstacle ObstaclePrefab;

    [SerializeField]
    private Coin CoinPrefab;

    [SerializeField]
    private Vector2 SpawnDelay;

    [SerializeField]
    private float LaneWidth = 2.5f;

    [SerializeField]
    private float SpawnHeight = 0f;

    [SerializeField]
    [Range(0f, 1f)]
    private float CoinSpawnChance = 0.5f;

    [SerializeField]
    private float SpawnForwardDistance = 34f;

    [SerializeField]
    private string[] ObstacleResourcePaths = new string[]
    {
        "Art/Obstacles/ObstacleCrate",
        "Art/Obstacles/ObstacleGrille",
        "Art/Obstacles/ObstacleBarrier"
    };

    private float _nextSpawn;
    private Coin _runtimeCoinPrefab;
    private GameObject[] _obstacleVisuals;
    private Transform _playerTransform;

    private void Start()
    {
        _obstacleVisuals = LoadObstacleVisuals();
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        if (CoinPrefab == null)
        {
            _runtimeCoinPrefab = BuildRuntimeCoinPrefab();
        }
    }

    private void Update()
    {
        if (Time.time > _nextSpawn)
        {
            SpawnLaneItem();

            _nextSpawn = Time.time + Random.Range(SpawnDelay.x, SpawnDelay.y);
        }
    }

    private void SpawnLaneItem()
    {
        int lane = Mathf.Clamp(Random.Range(-1, 2), -1, 1);
        float spawnZWorld = _playerTransform != null
            ? _playerTransform.position.z + SpawnForwardDistance
            : transform.position.z + SpawnForwardDistance;
        Vector3 spawnPosition = GetLaneWorldPosition(lane, spawnZWorld);

        Coin coinPrefab = CoinPrefab != null ? CoinPrefab : _runtimeCoinPrefab;
        bool spawnCoin = coinPrefab != null && Random.value <= CoinSpawnChance;
        if (spawnCoin)
        {
            Vector3 coinPos = spawnPosition + Vector3.up * 1f;
            Coin coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
            coin.gameObject.SetActive(true);
            return;
        }

        if (ObstaclePrefab != null)
        {
            Obstacle obstacle = Instantiate(ObstaclePrefab, spawnPosition, Quaternion.identity);
            Renderer obstacleRenderer = obstacle.GetComponent<Renderer>();
            if (obstacleRenderer != null)
            {
                RunnerVisuals.Paint(obstacleRenderer, new Color(0.55f, 0.2f, 0.18f));
            }

            AttachRandomObstacleVisual(obstacle.gameObject);
        }
    }

    /// <summary>Centre monde de la voie, aligné sur l'axe X du spawner (origine des lanes).</summary>
    private Vector3 GetLaneWorldPosition(int laneIndex, float worldZ)
    {
        laneIndex = Mathf.Clamp(laneIndex, -1, 1);
        float laneCenterX = transform.position.x + laneIndex * LaneWidth;
        float y = transform.position.y + SpawnHeight;
        return new Vector3(laneCenterX, y, worldZ);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 center = transform.position + new Vector3(0f, SpawnHeight, 0f);
        Vector3 size = new Vector3(LaneWidth * 2f, 1f, 1f);
        Gizmos.DrawWireCube(center, size);
    }

    private Coin BuildRuntimeCoinPrefab()
    {
        GameObject coinObject = new GameObject("RuntimeCoinPrefab");
        coinObject.name = "RuntimeCoinPrefab";
        SphereCollider collider = coinObject.AddComponent<SphereCollider>();
        collider.radius = 0.55f;
        collider.isTrigger = true;

        Coin coin = coinObject.AddComponent<Coin>();
        return coin;
    }

    private GameObject[] LoadObstacleVisuals()
    {
        System.Collections.Generic.List<GameObject> visuals = new System.Collections.Generic.List<GameObject>();
        foreach (string path in ObstacleResourcePaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            GameObject loaded = Resources.Load<GameObject>(path);
            if (loaded != null)
            {
                visuals.Add(loaded);
            }
        }

        return visuals.ToArray();
    }

    private void AttachRandomObstacleVisual(GameObject obstacleRoot)
    {
        if (_obstacleVisuals == null || _obstacleVisuals.Length == 0 || obstacleRoot == null)
        {
            return;
        }

        GameObject visualPrefab = _obstacleVisuals[Random.Range(0, _obstacleVisuals.Length)];
        GameObject visual = Instantiate(visualPrefab, obstacleRoot.transform);
        visual.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;
        RunnerVisuals.PaintChildren(visual, new Color(0.85f, 0.85f, 0.88f));
        RunnerVisuals.ResizeHeight(visual.transform, 1.9f);

        // Hide the old primitive renderer if obstacle prefab had one.
        Renderer baseRenderer = obstacleRoot.GetComponent<Renderer>();
        Renderer[] visualRenderers = visual.GetComponentsInChildren<Renderer>(true);
        if (baseRenderer != null && visualRenderers.Length > 0)
        {
            baseRenderer.enabled = false;
        }
    }
}
