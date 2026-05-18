using UnityEngine;
public class TransformOrb : MonoBehaviour
{
    [SerializeField] public float speed = 5f;
    [SerializeField] public float CollectionRNG = 0.5f;
    [SerializeField] public int value = 5;
    [SerializeField] string playerTag = "Player";
    bool isFlying = false;
    void Update()
    {
        if (!isFlying)
            return;
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj == null)
            return;
        Transform player = playerObj.transform;
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, player.position) < CollectionRNG)
        {
            Collect();
        }
    }
    public void Activate()
    {
        isFlying = true;
    }
    void Collect()
{
    TransformationGauge gauge = FindFirstObjectByType<TransformationGauge>();
    if (gauge != null)
    {
        gauge.AddEnergy(value);
    }
    Destroy(gameObject);
}
}