using UnityEngine;
using UnityEngine.Serialization;

public abstract class Promptable : MonoBehaviour
{
    internal Collider promptableCollider;
    public ButtonAction inputAction;
}

[System.Serializable]
public class ButtonAction
{
    [SerializeField] public Button button;
    [SerializeField] public string description;
}