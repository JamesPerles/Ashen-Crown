using UnityEngine;
using Unity.Cinemachine;

public class PlayerTransformation : MonoBehaviour
{
    [SerializeField] private TransformationGauge transformationGauge;
    [SerializeField] private GameObject transformedPrefab;
    [SerializeField] private float transformationDuration = 10f;
    [SerializeField] private float transformAnimationDuration = 1f;

    private PlayerHealth playerHealth;
    private Animator animator;
    private bool isTransformed = false;
    private bool isTransforming = false;
    private CinemachineCamera vcam;

    [System.Obsolete]
    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();
        
        // Find the Cinemachine camera in the scene
        vcam = FindObjectOfType<CinemachineCamera>();
    }

    void Update()
    {
        // Prevent multiple transformation attempts
        if (!isTransformed && !isTransforming && transformationGauge.IsFull() && Input.GetKeyDown(KeyCode.X))
        {
            PlayTransformAnimation();
        }
    }

    private void PlayTransformAnimation()
    {
        isTransforming = true;
        
        if (animator != null)
        {
            animator.SetTrigger("isTransforming");
        }

        Invoke(nameof(TransformPlayer), transformAnimationDuration);
    }

    void TransformPlayer()
    {
        isTransformed = true;

        float currentHP = playerHealth.currentHP;
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        // Spawn transformed version
        GameObject newForm = Instantiate(transformedPrefab, pos, rot);
        PlayerHealth newHealth = newForm.GetComponent<PlayerHealth>();
        
        // Set HP immediately - the updated PlayerHealth script won't overwrite it
        newHealth.currentHP = currentHP;

        // Update Cinemachine to follow new player
        if (vcam != null)
        {
            vcam.Follow = newForm.transform;
            vcam.LookAt = newForm.transform;
        }

        // Disable current player (this will hide its health bar)
        gameObject.SetActive(false);

        // Revert after time
       // StartCoroutine(RevertAfterTime(newForm));
    }

    System.Collections.IEnumerator RevertAfterTime(GameObject newForm)
    {
        yield return new WaitForSeconds(transformationDuration);

        // Get final HP and position from transformed form
        PlayerHealth transformedHealth = newForm.GetComponent<PlayerHealth>();
        float finalHP = transformedHealth.currentHP;
        Vector3 pos = newForm.transform.position;
        Quaternion rot = newForm.transform.rotation;

        // Update Cinemachine to follow original player again
        if (vcam != null)
        {
            vcam.Follow = transform;
            vcam.LookAt = transform;
        }

        // Revert to base form
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rot;
        playerHealth.currentHP = finalHP; // Set HP before activating
        gameObject.SetActive(true);

        Destroy(newForm);

        isTransformed = false;
        isTransforming = false;
    }
}