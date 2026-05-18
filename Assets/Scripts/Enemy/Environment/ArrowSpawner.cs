using UnityEngine;
public class ArrowSpawner : MonoBehaviour
{
  float spawnTime = 5f;
  [SerializeField] GameObject arrowPrefab;
    void Update()
    {
        spawnTime -= Time.deltaTime;
        if(spawnTime <= 0)
        {
            Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            spawnTime = 5f;
        }
    }
}
