using UnityEngine;

public class TransformOrb : MonoBehaviour
{
    [SerializeField] public float speed = 5f;
    public Transform player;
    [SerializeField] public float CollectionRNG = 0.5f;
    [SerializeField] public int value = 5;
    private bool isFlying = false;
    void Update()
    {
        if (isFlying && player != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
            if (Vector3.Distance(transform.position, player.position) < CollectionRNG)
            {
                Collect();
            }
        }
    }

    public void Activate(Transform target)
    {
        player = target;
        isFlying = true;
    }
    private void Collect()
    {
        TransformationGauge gauge = player.GetComponentInChildren<TransformationGauge>();
        if (gauge != null)
        {
            gauge.AddEnergy(value);
        }
        Destroy(gameObject);
    }
    }

