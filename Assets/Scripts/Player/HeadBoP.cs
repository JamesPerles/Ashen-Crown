using UnityEngine;
public class HeadBoP : MonoBehaviour
{
    [SerializeField] public float bobDistance = 0.5f;
    [SerializeField] public float bobSpeed = 0.5f;
    void Update()
    {
        float zRotation = Mathf.PingPong(Time.time * bobSpeed, bobDistance) - (bobDistance / 2f);
        transform.rotation = Quaternion.Euler(0, 0, zRotation);
    }
}
