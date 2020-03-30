using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public GameObject objPrefab;

    public Transform spawnPoint;

    public void SpawnObj()
    {
        Instantiate(objPrefab, spawnPoint.position, Quaternion.identity);
    }
}
