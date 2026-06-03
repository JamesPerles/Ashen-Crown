using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
public class PlayerTransformation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TransformationGauge transformationGauge;
    [SerializeField] GameObject normalForm;
    [SerializeField] GameObject transformedForm;
    [SerializeField] float transformAnimationDuration = 1f;
    [SerializeField] float revertAnimationDuration = 1f;
    [SerializeField] float drainRate = 1f;
    PlayerHealth normalHealth;
    PlayerHealth transformedHealth;
    Animator normalAnimator;
    Animator transformedAnimator;
    CinemachineCamera vcam;
    Vector3 hiddenPosition = new Vector3(-999f, -999f, 0f);
    [HideInInspector] public bool isTransformed = false;
    [HideInInspector] public bool isTransforming = false;
    bool isReverting = false;
    void Start()
    {
        normalHealth = normalForm.GetComponent<PlayerHealth>();
        normalAnimator = normalForm.GetComponent<Animator>();
        transformedHealth = transformedForm.GetComponent<PlayerHealth>();
        transformedAnimator = transformedForm.GetComponent<Animator>();
        vcam = FindFirstObjectByType<CinemachineCamera>();
        transformedForm.SetActive(false);
    }
    void Update()
    {
        if (!isTransformed && !isTransforming && transformationGauge.IsFull() &&
            Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(TransformRoutine());
        }
        if (isTransformed && !isReverting)
        {
            transformationGauge.AddEnergy(-drainRate * Time.deltaTime);

            if (transformationGauge.currentEnergy <= 0f)
            {
                StartCoroutine(RevertRoutine());
            }
        }
    }
    IEnumerator TransformRoutine()
    {
        isTransforming = true;
        PlayerController playerController = normalForm.GetComponent<PlayerController>();
        Rigidbody2D normalRb = normalForm.GetComponent<Rigidbody2D>();
        if (normalRb != null)
        {
            normalRb.linearVelocity = Vector2.zero;
            normalRb.angularVelocity = 0f;
        }
        if (playerController != null)
            playerController.enabled = false;
        if (normalAnimator != null)
            normalAnimator.SetTrigger("isTransforming");
        yield return new WaitForSeconds(transformAnimationDuration);
        if (playerController != null)
            playerController.enabled = true;
        if (normalHealth != null && transformedHealth != null)
            transformedHealth.currentHP = normalHealth.currentHP;
        transformedForm.transform.position = normalForm.transform.position;
        transformedForm.transform.rotation = normalForm.transform.rotation;
        Rigidbody2D transformedRb = transformedForm.GetComponent<Rigidbody2D>();
        if (transformedRb != null)
        {
            transformedRb.linearVelocity = Vector2.zero;
            transformedRb.angularVelocity = 0f;
        }
        normalForm.tag = "Untagged";
        transformedForm.tag = "Player";
        if (normalRb != null)
        {
            normalRb.linearVelocity = Vector2.zero;
            normalRb.bodyType = RigidbodyType2D.Kinematic;
        }
        normalForm.GetComponent<Collider2D>().enabled = false;
        normalForm.GetComponent<SpriteRenderer>().enabled = false;
        normalForm.transform.position = hiddenPosition;
        transformedForm.SetActive(true);
        if (vcam != null)
        {
            vcam.Follow = transformedForm.transform;
            vcam.LookAt = transformedForm.transform;
        }
        isTransformed = true;
        isTransforming = false;
    }
    IEnumerator RevertRoutine()
    {
        isReverting = true;
        PlayerController transformedController = transformedForm.GetComponent<PlayerController>();
        Rigidbody2D transformedRb = transformedForm.GetComponent<Rigidbody2D>();
        if (transformedRb != null)
        {
            transformedRb.linearVelocity = Vector2.zero;
            transformedRb.angularVelocity = 0f;
        }
        if (transformedController != null)
            transformedController.enabled = false;
        if (transformedAnimator != null)
            transformedAnimator.SetBool("isReverting", true);
        if (normalAnimator != null)
            normalAnimator.SetBool("isReverting", true);
        yield return new WaitForSeconds(revertAnimationDuration);
        if (transformedController != null)
            transformedController.enabled = true;
        if (transformedAnimator != null)
            transformedAnimator.SetBool("isReverting", false);
        if (normalAnimator != null)
            normalAnimator.SetBool("isReverting", false);
        if (normalHealth != null && transformedHealth != null)
            normalHealth.currentHP = transformedHealth.currentHP;
        normalForm.transform.position = transformedForm.transform.position;
        normalForm.transform.rotation = transformedForm.transform.rotation;
        Rigidbody2D rb = normalForm.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        normalForm.GetComponent<Collider2D>().enabled = true;
        normalForm.GetComponent<SpriteRenderer>().enabled = true;
        transformedForm.tag = "Untagged";
        normalForm.tag = "Player";
        transformedForm.SetActive(false);
        if (vcam != null)
        {
            vcam.Follow = normalForm.transform;
            vcam.LookAt = normalForm.transform;
        }
        isTransformed = false;
        isReverting = false;
        yield return null;
    }
}