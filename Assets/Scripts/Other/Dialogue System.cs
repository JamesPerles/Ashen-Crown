using UnityEngine;
using TMPro;
using System.Collections;
public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Transform dialogueBox;
    [SerializeField] TextMeshProUGUI linePrefab;
    [SerializeField] GameObject dialogueAdvanceButton;
    [SerializeField] float charDelay = 0.05f;
    [Header("Dialogue Data")]
    [SerializeField] CharacterData[] characters;
    [SerializeField] DialogueLine[] dialogueLines;
    [SerializeField] KeyCode advanceKey = KeyCode.A;
    int currentLineIndex = 0;
    bool isTyping = false;
    Coroutine typingCoroutine;
    TextMeshProUGUI currentLine;
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

        if (Input.GetKeyDown(advanceKey))
        {
            if (isTyping)
                FinishTypingNow();
            else
                AdvanceLine();
        }

        SkipDialogue();
    }

    public void StartDialogue()
    {
        if (dialogueLines.Length == 0) return;
        currentLineIndex = 0;
        ClearCurrentLine();
        ShowNextLine();
    }
    void ShowNextLine()
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
    IEnumerator TypeLineRoutine(DialogueLine line)
    {
        isTyping = true;
        SetActiveCharacter(line.characterIndex);
        TextMeshProUGUI newLine = Instantiate(linePrefab, dialogueBox, false);
        newLine.gameObject.SetActive(true);
        newLine.text = "";
        newLine.rectTransform.anchoredPosition = Vector2.zero;
        newLine.rectTransform.localScale = Vector3.one;
        if (line.characterIndex >= 0 && line.characterIndex < characters.Length)
            newLine.color = characters[line.characterIndex].textColor;
        currentLine = newLine;
        int charsPerFrame = 2;
        for (int i = 0; i < line.text.Length; i += charsPerFrame)
        {
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift);
            float delay = shiftHeld ? charDelay / 2f : charDelay;
            int end = Mathf.Min(i + charsPerFrame, line.text.Length);
            currentLine.text += line.text.Substring(i, end - i);
            yield return new WaitForSeconds(delay);
        }
        isTyping = false;
        typingCoroutine = null;
        StopCharacterAnimation(line.characterIndex);
    }
    void AdvanceLine()
    {
        if (currentLine != null)
        {
            currentLine.gameObject.SetActive(false);
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
        ShowNextLine();
    }
    void SetActiveCharacter(int characterIndex)
    {
        foreach (var character in characters)
        {
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

    void StopCharacterAnimation(int characterIndex)
    {
        if (characterIndex >= 0 && characterIndex < characters.Length)
        {
            CharacterData character = characters[characterIndex];
            if (character.characterAnimator != null)
                character.characterAnimator.SetBool("isDialogue", false);
        }
    }
    void FinishTypingNow()
    {
        if (typingCoroutine == null) return;
        StopCoroutine(typingCoroutine);
        DialogueLine line = dialogueLines[Mathf.Clamp(currentLineIndex - 1, 0, dialogueLines.Length - 1)];
        currentLine.text = line.text;
        isTyping = false;
        typingCoroutine = null;
        StopCharacterAnimation(line.characterIndex);
    }
    void ClearCurrentLine()
    {
        if (currentLine != null)
        {
            currentLine.gameObject.SetActive(false);
            Destroy(currentLine.gameObject);
        }
        currentLine = null;
    }
    void EndDialogue()
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
    void SkipDialogue()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            EndDialogue();
    }
}