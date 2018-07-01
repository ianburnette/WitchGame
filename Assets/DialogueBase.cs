using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.UI;

class DialogueBase : MonoBehaviour {
    public static DialogueBase instance;

    [SerializeField] Transform camera;
    [SerializeField] Transform dialogueBubble;
    [SerializeField] TextMeshProUGUI textField;
    [SerializeField] float heightOffset;
    [SerializeField] Vector3 bubbleSize;
    [SerializeField] float bubbleGrowSpeed, bubbleShrinkSpeed;

    public bool align;

    void OnEnable() => instance = this;

    public void ShowBubble(Vector3 position, string dialogue) {
        dialogueBubble.localScale = Vector3.zero;
        dialogueBubble.position = position + Vector3.up * heightOffset;
        textField.text = dialogue;
        dialogueBubble.DOScale(bubbleSize, bubbleGrowSpeed);
        align = true;
    }

    public void HideBubble() {
        dialogueBubble.DOScale(Vector3.zero, bubbleShrinkSpeed);
        align = false;
    }

    void Update() {
        if (align)
            dialogueBubble.LookAt(camera);
    }
}
