using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        class SpawnInfo
        {
            public GameObject enemy;
            [Range(0,1)] public float probability;
        }

        [SerializeField] List<SpawnInfo> spawns;
        [SerializeField] int maxEnemies;
        [SerializeField, Tooltip("Seconds to wait before it can spawn another enemy")] float spawnRate;
        [SerializeField] float rateRandomness = 1;
        [SerializeField] float startDelay = 5f;
        [SerializeField, Tooltip("How much to change for start (-+)")] float startRandom = 1f;
        [SerializeField] Transform[] positions = new Transform[3];
        
        int _enemiesCount;
        int _nextPosition = -1;
        float[] _probabilityTable;
        bool _isSpawning;

        
        
        void Start()
        {
            _probabilityTable = new float[spawns.Count];
            
            int i = 0;
            
            foreach (SpawnInfo spawnInfo in spawns)
            {
                if (i > 0)
                    _probabilityTable[i] = spawnInfo.probability + _probabilityTable[i - 1];
                else
                    _probabilityTable[i] = spawnInfo.probability;
                
                i++;
            }

            StartCoroutine(SpawnUnits());
        }

        IEnumerator SpawnUnits()
        {
            _isSpawning = true;

            yield return new WaitForSeconds(Random.Range(startDelay - startRandom, startDelay + startRandom));
            
            while (_enemiesCount < maxEnemies)
            {
                GameObject enemy = Instantiate(GetRandomEnemy(), GetNextPosition(), Quaternion.identity, transform);
                enemy.GetComponent<Enemy>().OnKilled += OnEnemyKilled;
                _enemiesCount++;

                yield return new WaitForSeconds(Random.Range(spawnRate - rateRandomness, spawnRate + rateRandomness));
            }

            _isSpawning = false;
        }

        void OnEnemyKilled(Enemy enemy)
        {
            _enemiesCount--;
            enemy.OnKilled -= OnEnemyKilled;

            if (!_isSpawning)
                StartCoroutine(SpawnUnits());
        }

        GameObject GetRandomEnemy()
        {
            float random = Random.value;
            for (int index = 0; index < _probabilityTable.Length; index++)
            {
                float probability = _probabilityTable[index];
                if (random < probability)
                    return spawns[index].enemy;
            }

            Debug.LogError("Error when calculating the enemy to spawn from probabilities");
            return spawns[0].enemy;
        }

        Vector3 GetNextPosition()
        {
            _nextPosition = ++_nextPosition % positions.Length;

            return positions[_nextPosition].position;
        }

        void OnValidate()
        {
            float total = spawns.Sum(spawnInfo => spawnInfo.probability);

            if (Math.Abs(total - 1) > Mathf.Epsilon)
                Debug.LogError("Probability not 1");
            
            if (positions.Length < maxEnemies)
                Debug.LogWarning("The number of available positions is smaller than the possible enemies to spawn. This means that enemies may spawn on top of each other");
        }
    }
}
