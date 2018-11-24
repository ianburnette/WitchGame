using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class InputPrompt : MonoBehaviour
{
    public static InputPrompt instance;
    [SerializeField] Promptable prompt;
    [SerializeField] ScaleWithCameraDistance camScaler;
    [SerializeField] float scaleUpSpeed, scaleDownSpeed;
    bool prompting;
    [SerializeField] GoEaseType thisEaseType;
    [SerializeField] float yOffset;
    
    [Header("Visuals")]
    [SerializeField] TextMeshProUGUI promptText;
    [SerializeField] Image promptImage;

    void OnEnable() => instance = this;

    public Promptable Prompt
    {
        get => prompt;
        set
        {
            prompt = value;
            prompting = value != null;
            if (value)
            {
                promptText.text = prompt.inputAction.description;
                promptImage.sprite = ButtonMappings.instance.GetSpriteForButton(prompt.inputAction.button);
            }
            var config = new GoTweenConfig().floatProp("ScaleOverride", prompting ? 1 : 0);
            config.easeType = thisEaseType;
            Go.to(camScaler, prompting ? scaleUpSpeed : scaleDownSpeed, config);
        }
    }


    void Update()
    {
        if (!prompting) return;
        var position = prompt.transform.position;
        transform.position = new Vector3(
            position.x,
            prompt.promptableCollider.bounds.max.y + yOffset, 
            position.z);
    }
}
