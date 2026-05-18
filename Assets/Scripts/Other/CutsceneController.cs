using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CutsceneController : MonoBehaviour
{
    [Header("Timeline & Dialogue")]
    public PlayableDirector timeline;
    public DialogueSystem dialogueSystem;
    public GameObject dialogueBox;

    [Header("Player Settings")]
    public GameObject player;
    public Sprite cutsceneSprite;

    [Header("Camera Settings")]
    public CinemachineCamera cinemachineCamera;
    public Transform playerTarget;
    public Transform cutsceneTarget;
    SpriteRenderer playerRenderer;
    Sprite originalSprite;
    MonoBehaviour[] playerComponents;
    Animator playerAnimator;

    void Start()
    {
        playerRenderer = player.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
            originalSprite = playerRenderer.sprite;
        playerAnimator = player.GetComponent<Animator>();

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        DisablePlayer();

        if (playerRenderer != null && cutsceneSprite != null)
            playerRenderer.sprite = cutsceneSprite;

        if (cinemachineCamera != null && cutsceneTarget != null)
        {
            cinemachineCamera.Follow = cutsceneTarget;
            cinemachineCamera.LookAt = cutsceneTarget;
        }

        if (timeline != null)
        {
            timeline.Play();
            timeline.stopped += OnTimelineComplete;
        }
    }
    void DisablePlayer()
    {
        playerComponents = player.GetComponents<MonoBehaviour>();
        foreach (var component in playerComponents)
        {
            if (component != null && component != this)
            {
                component.enabled = false;
            }
        }

        if (playerAnimator != null)
            playerAnimator.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = false;

        Collider2D col = player.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }
    void EnablePlayer()
    {
        foreach (var component in playerComponents)
        {
            if (component != null)
            {
                component.enabled = true;
            }
        }

        if (playerAnimator != null)
            playerAnimator.enabled = true;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = true;

        Collider2D col = player.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;
    }
    void OnTimelineComplete(PlayableDirector director)
    {
        if (cinemachineCamera != null && playerTarget != null)
        {
            cinemachineCamera.Follow = playerTarget;
            cinemachineCamera.LookAt = playerTarget;
        }

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        dialogueSystem.OnDialogueComplete += OnDialogueComplete;
         dialogueSystem.StartDialogue();
    }
    void OnDialogueComplete()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (playerRenderer != null && originalSprite != null)
            playerRenderer.sprite = originalSprite;
        EnablePlayer();
    }
     void OnDestroy()
    {
        if (timeline != null)
            timeline.stopped -= OnTimelineComplete;
    }
}