using Scriptable_Objects;
using UnityEngine;

namespace Player
{
    public class PlayerSpawner : MonoBehaviour
    {

        [SerializeField] PlayerInfo info; 
        
        void Awake()
        {
            if (info.UseSpawnPosition)
                transform.position = info.SpawnPosition;
        }
    }
}
