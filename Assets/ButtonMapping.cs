using System;
using UnityEngine;

[Serializable]
public class ButtonMapping
{
    [SerializeField] public Button button;
    [SerializeField] Sprite xboxButton, psButton, keyboardButton;

    public ButtonMapping(Button button, Sprite xbox, Sprite ps, Sprite keyboard)
    {
        this.button = button;
        xboxButton = xbox;
        psButton = ps;
        keyboardButton = keyboard;
    }

    public Sprite GetSpriteAccordingToInputType()
    {
        switch (ButtonMappings.instance.currentInputType)
        {
            case InputType.Keyboard:
                return keyboardButton;
            case InputType.Xbox:
                return xboxButton;
            case InputType.Ps:
                return psButton;
            default:
                throw new ArgumentOutOfRangeException("Button mapping not found for that input type: " + 
                                                      ButtonMappings.instance.currentInputType);
        }
    }
}
