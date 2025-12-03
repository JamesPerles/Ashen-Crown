using UnityEngine;

public class DialogueManager : MonoBehaviour
{
 [SerializeField] private DialogueSystem dialogueSystem;

    public bool IsDialogueActive => dialogueSystem != null && dialogueSystem.IsDialogueActive;

   public void StartDialogue()
   {
    if (dialogueSystem != null)
           dialogueSystem.StartDialogue();
    }

    public void RegisterDialogueComplete(System.Action onComplete)
    {
        if (dialogueSystem != null)
            dialogueSystem.OnDialogueComplete += onComplete;
    }
}
