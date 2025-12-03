using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CutsceneController : MonoBehaviour
{
    [Header("Timeline & Dialogue")]
    public PlayableDirector timeline;
    public DialogueManager dialogueManager;
    public GameObject dialogueBox;

    [Header("Player Settings")]
    public GameObject player;
    public Sprite cutsceneSprite;

    [Header("Camera Settings")]
    public CinemachineCamera cinemachineCamera;
    public Transform playerTarget;
    public Transform cutsceneTarget;

    private SpriteRenderer playerRenderer;
    private Sprite originalSprite;
    private MonoBehaviour[] playerComponents;
    private Animator playerAnimator;

    void Start()
    {
        // Cache original player sprite
        playerRenderer = player.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
            originalSprite = playerRenderer.sprite;

        // Cache animator
        playerAnimator = player.GetComponent<Animator>();

        // Disable dialogue box during cutscene
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        // Disable player components (but keep GameObject active)
        DisablePlayerComponents();

        // Change to cutscene sprite
        if (playerRenderer != null && cutsceneSprite != null)
            playerRenderer.sprite = cutsceneSprite;

        // Focus camera on cutscene target
        if (cinemachineCamera != null && cutsceneTarget != null)
        {
            cinemachineCamera.Follow = cutsceneTarget;
            cinemachineCamera.LookAt = cutsceneTarget;
        }

        // Start Timeline
        if (timeline != null)
        {
            timeline.Play();
            timeline.stopped += OnTimelineComplete;
        }
    }

    private void DisablePlayerComponents()
    {
        // Get all components except Transform and SpriteRenderer
        playerComponents = player.GetComponents<MonoBehaviour>();
        foreach (var component in playerComponents)
        {
            if (component != null && component != this)
            {
                component.enabled = false;
            }
        }

        // Disable Animator
        if (playerAnimator != null)
            playerAnimator.enabled = false;

        // Disable Rigidbody2D if present
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = false;

        // Disable Collider2D if present
        Collider2D col = player.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    private void EnablePlayerComponents()
    {
        // Re-enable all components
        foreach (var component in playerComponents)
        {
            if (component != null)
            {
                component.enabled = true;
            }
        }

        // Re-enable Animator
        if (playerAnimator != null)
            playerAnimator.enabled = true;

        // Re-enable Rigidbody2D if present
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = true;

        // Re-enable Collider2D if present
        Collider2D col = player.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;
    }

    // Called when timeline finishes
    private void OnTimelineComplete(PlayableDirector director)
    {
        // Restore camera to follow player
        if (cinemachineCamera != null && playerTarget != null)
        {
            cinemachineCamera.Follow = playerTarget;
            cinemachineCamera.LookAt = playerTarget;
        }

        // Enable dialogue box
        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        // Start dialogue
        dialogueManager.RegisterDialogueComplete(OnDialogueComplete);
         dialogueManager.StartDialogue();
    }

    private void OnDialogueComplete()
    {
        // Disable dialogue box
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        // Restore original sprite
        if (playerRenderer != null && originalSprite != null)
            playerRenderer.sprite = originalSprite;

        // Re-enable player components
        EnablePlayerComponents();
    }

    private void OnDestroy()
    {
        // Unsubscribe from timeline event
        if (timeline != null)
            timeline.stopped -= OnTimelineComplete;
    }
}