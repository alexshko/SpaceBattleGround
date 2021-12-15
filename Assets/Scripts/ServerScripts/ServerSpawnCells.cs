using UnityEngine;
using Mirror;

namespace SpaceBattle.Server
{
    public class ServerSpawnCells : NetworkBehaviour
    {
        [Tooltip("Cells that will be spawn. They have to be also in the NetworkManager")]
        public GameObject[] objectsToSpawn;
        [Tooltip("The radius of the world")]
        public float worldRadius = 1000;
        [Tooltip("how long to wait between spawns")]
        public float timeBetweenSpawns = 5;

        //counter of how many spawned items:
        private int numOfSpawnedCells;
        private float timeLastSpawn;

        public override void OnStartServer()
        {
            base.OnStartServer();
            numOfSpawnedCells = 0;
            timeLastSpawn = 0;
            SpawnRandomCell();
        }

        private void Update()
        {
            if (Time.time > timeLastSpawn + timeBetweenSpawns)
            {
                SpawnRandomCell();
            }
        }

        private void SpawnRandomCell()
        {
            Vector3 randomPos = Random.insideUnitSphere * worldRadius;
            //by making modolus division over the number of total objects to spawn, we choose the next item to spawn by round robin:
            GameObject cellToSpawn = objectsToSpawn[numOfSpawnedCells % objectsToSpawn.Length];
            GameObject spawnedCell = Instantiate(cellToSpawn, randomPos, cellToSpawn.transform.rotation);
            NetworkServer.Spawn(spawnedCell);
            timeLastSpawn = Time.time;

            numOfSpawnedCells++;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawSphere(Vector3.zero, worldRadius);
        }
    }
}