using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ButtonMappings : MonoBehaviour
{
    public static ButtonMappings instance;

    public InputType currentInputType = InputType.Xbox;
    
    public Sprite xboxInteract;
    public Sprite psInteract;
    public Sprite keyboardInteract;
    
    public List<ButtonMapping> mappings;

    void OnEnable() => instance = this;

    public ButtonMapping GetButtonMapping(Button button) => 
        mappings.First(b => b.button == button);

    public Sprite GetSpriteForButton(Button button) => 
        GetButtonMapping(button).GetSpriteAccordingToInputType();
}

public enum InputType
{
    Keyboard, Xbox, Ps
}