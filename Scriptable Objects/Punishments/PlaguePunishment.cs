using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scriptable_Objects.Punishments
{
    [CreateAssetMenu(menuName = nameof(Punishment) + "/" + nameof(PlaguePunishment))]
    public class PlaguePunishment : Punishment
    {
        [SerializeField] GameObject poisonCloud;
        [SerializeField] int amount;
        [SerializeField] PathFinderDatabase database;

        List<GameObject> _clouds = new List<GameObject>();

        protected override void OnBegin()
        {
            for (int i = 0; i < amount; i++)
            {
                _clouds.Add(Instantiate(poisonCloud, GetRandom(), Quaternion.identity));
            }
        }

        Vector2 GetRandom()
        {
            return new Vector2(Random.Range(database.MinX, database.MaxX), Random.Range(database.MinY, database.MaxY));
        }

        protected override void OnEnd()
        {
            foreach (GameObject cloud in _clouds)
            {
                Destroy(cloud);
            }

            _clouds = new List<GameObject>(amount);
        }
    }
}
