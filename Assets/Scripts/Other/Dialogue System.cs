using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform dialogueBox;          
    [SerializeField] private TextMeshProUGUI linePrefab;     
    [SerializeField] private GameObject dialogueAdvanceButton;

    [Header("Dialogue Data")]
    [SerializeField] private CharacterData[] characters;     
    [SerializeField] private DialogueLine[] dialogueLines;   

    [Header("UI Settings")]
    [SerializeField] private float charDelay = 0.005f;        // Reduced delay for snappier dialogue
    [SerializeField] private float moveUpSpeed = 400f;        // Slightly faster movement

    [Header("Controls")]
    [SerializeField] private KeyCode advanceKey = KeyCode.A;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isMovingUp = false;
    private Coroutine typingCoroutine;
    private TextMeshProUGUI currentLine;

    public bool IsDialogueActive => currentLine != null || isTyping;
    public System.Action OnDialogueComplete;

    void Start()
    {
        foreach (var character in characters)
        {
            if (character.characterSprite != null)
                character.characterSprite.SetActive(false);

            if (character.characterAnimator != null)
                character.characterAnimator.SetBool("isDialogue", false);
        }

        if (dialogueAdvanceButton != null)
            dialogueAdvanceButton.SetActive(true);
    }

    void Update()
    {
        if (!IsDialogueActive) return;

        if (Input.GetKeyDown(advanceKey) && !isMovingUp)
        {
            if (isTyping)
                FinishTypingNow(); // Skip to full line instantly
            else
                StartMovingCurrentLine();
        }

        // Move line if flagged
        if (isMovingUp && currentLine != null)
        {
            RectTransform rt = currentLine.rectTransform;
            rt.anchoredPosition += Vector2.up * moveUpSpeed * Time.deltaTime;

            float topY = dialogueBox.GetComponent<RectTransform>().rect.height / 2 + 50f;
            if (rt.anchoredPosition.y > topY)
            {
                Destroy(currentLine.gameObject);
                currentLine = null;
                isMovingUp = false;
                ShowNextLine();
            }
        }
    }

    public void StartDialogue()
    {
        if (dialogueLines.Length == 0) return;

        currentLineIndex = 0;
        ClearCurrentLine();
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines[currentLineIndex];
        typingCoroutine = StartCoroutine(TypeLineRoutine(line));
        currentLineIndex++;
    }

    private IEnumerator TypeLineRoutine(DialogueLine line)
    {
        isTyping = true;
        ClearCurrentLine();

        SetActiveCharacter(line.characterIndex);

        currentLine = Instantiate(linePrefab, dialogueBox, false);
        currentLine.gameObject.SetActive(true);
        currentLine.text = "";
        currentLine.rectTransform.anchoredPosition = Vector2.zero;
        currentLine.rectTransform.localScale = Vector3.one;

        if (line.characterIndex >= 0 && line.characterIndex < characters.Length)
            currentLine.color = characters[line.characterIndex].textColor;

        // Snappier typing: type multiple chars at once
        int charsPerFrame = 2; // You can increase to 3 or 4 if you want even faster
        for (int i = 0; i < line.text.Length; i += charsPerFrame)
        {
            int end = Mathf.Min(i + charsPerFrame, line.text.Length);
            currentLine.text += line.text.Substring(i, end - i);
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
        typingCoroutine = null;

        StopCharacterAnimation(line.characterIndex);
    }

    private void StartMovingCurrentLine()
    {
        isMovingUp = true;
    }

    private void SetActiveCharacter(int characterIndex)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            CharacterData character = characters[i];

            if (character.characterSprite != null)
                character.characterSprite.SetActive(false);

            if (character.characterAnimator != null)
                character.characterAnimator.SetBool("isDialogue", false);
        }

        if (characterIndex >= 0 && characterIndex < characters.Length)
        {
            CharacterData character = characters[characterIndex];

            if (character.characterSprite != null)
                character.characterSprite.SetActive(true);

            if (character.characterAnimator != null)
                character.characterAnimator.SetBool("isDialogue", true);
        }
    }

    private void StopCharacterAnimation(int characterIndex)
    {
        if (characterIndex >= 0 && characterIndex < characters.Length)
        {
            CharacterData character = characters[characterIndex];

            if (character.characterAnimator != null)
                character.characterAnimator.SetBool("isDialogue", false);
        }
    }

    private void FinishTypingNow()
    {
        if (typingCoroutine == null) return;

        StopCoroutine(typingCoroutine);
        DialogueLine line = dialogueLines[Mathf.Clamp(currentLineIndex - 1, 0, dialogueLines.Length - 1)];
        currentLine.text = line.text;

        isTyping = false;
        typingCoroutine = null;

        StopCharacterAnimation(line.characterIndex);
    }

    private void ClearCurrentLine()
    {
        if (currentLine != null)
            Destroy(currentLine.gameObject);
        currentLine = null;
        isMovingUp = false;
    }

    private void EndDialogue()
    {
        foreach (var character in characters)
        {
            if (character.characterSprite != null)
                character.characterSprite.SetActive(false);

            if (character.characterAnimator != null)
                character.characterAnimator.SetBool("isDialogue", false);
        }

        if (dialogueAdvanceButton != null)
            dialogueAdvanceButton.SetActive(true);

        OnDialogueComplete?.Invoke();
    }
}
