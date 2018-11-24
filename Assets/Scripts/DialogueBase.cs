using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

class DialogueBase : MonoBehaviour {
    public static DialogueBase instance;

    [SerializeField] Transform camera;
    [SerializeField] Transform player;
    [SerializeField] Transform dialogueBubble;
    [SerializeField] Transform dialogueFocus;
    [SerializeField] Transform npc;
    [SerializeField] TextMeshProUGUI textField;
    [SerializeField] float heightOffset;
    [SerializeField] Vector3 bubbleSize;
    [SerializeField] float bubbleGrowSpeed, bubbleShrinkSpeed;
    
    [FormerlySerializedAs("currentDialogues")]
    [Header("Current")]
    [SerializeField] NpcDialogue currentDialogue;
    [SerializeField] int currentDialogueIndex;
    [SerializeField] bool inRange;
    
    [Header("Camera Control")]
    [SerializeField] CinemachineMixingCamera mixingCamera;
    [SerializeField] float transitionTime;
    [SerializeField] float normalCameraWeight;
    [SerializeField] float dialogueCameraWeight;
    [SerializeField] GoEaseType thisEaseType;
    
    public bool InDialogue { get; set; }
    
    //[Header("Player Input Suppression")]
    //[SerializeField] PlayerMoveBase

    public bool align;

    public float NormalCameraWeight
    {
        get { return normalCameraWeight; }
        set { normalCameraWeight = value; }
    }

    public float DialogueCameraWeight
    {
        get { return dialogueCameraWeight; }
        set { dialogueCameraWeight = value; }
    }

    public bool InRange
    {
        get { return inRange; }
        set { inRange = value; }
    }

    public NpcDialogue CurrentDialogue
    {
        get { return currentDialogue; }
        set => currentDialogue = value;
    }

    void OnEnable() => instance = this;

    public void ShowBubble()
    {
        if (currentDialogue == null) return;
        InDialogue = true;
        dialogueBubble.localScale = Vector3.zero;
        dialogueBubble.position = currentDialogue.MyCollider.bounds.max + Vector3.up * heightOffset;
        currentDialogueIndex = 0;
        ShowDialogue();
        dialogueBubble.DOScale(bubbleSize, bubbleGrowSpeed);
        align = true;
        TransitionTo(dialogueCamera: true);
    }

    public void ShowNextDialogue()
    {
        if (CurrentDialogue == null) return;
        if (currentDialogueIndex < CurrentDialogue.Dialogues.Count-1)
        {
            currentDialogueIndex++;
            ShowDialogue();
        }
        else
            HideBubble();
    }

    void ShowDialogue() => textField.text = CurrentDialogue.Dialogues[currentDialogueIndex];

    void TransitionTo(bool dialogueCamera)
    {
        var config = new GoTweenConfig()
            .floatProp(dialogueCamera ? "DialogueCameraWeight" : "NormalCameraWeight", 1)
            .floatProp(dialogueCamera ? "NormalCameraWeight" : "DialogueCameraWeight", 0);
        config.easeType = thisEaseType;
        
        Go.to(this, transitionTime, config);
    }

    public void HideBubble()
    {
        InDialogue = false;
        dialogueBubble.DOScale(Vector3.zero, bubbleShrinkSpeed);
        align = false;
        currentDialogueIndex = 0;
        currentDialogue = null;
        TransitionTo(dialogueCamera: false);
    }

    void Update() {
        mixingCamera.m_Weight0 = normalCameraWeight;
        mixingCamera.m_Weight1 = dialogueCameraWeight;
        if (!align || CurrentDialogue==null) return;
        dialogueBubble.LookAt(camera);
        dialogueFocus.position = (player.position + dialogueBubble.position + CurrentDialogue.transform.position) / 3f;
    }
}
