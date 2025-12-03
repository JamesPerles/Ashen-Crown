using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 6)]
    public string text;
    public int characterIndex;
}