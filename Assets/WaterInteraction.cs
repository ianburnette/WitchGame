using UnityEngine;

public interface IWaterInteraction
{
    float Offset { get; }
    float SubmergeDepth { get; }
    float Height { get; set; }
    Rigidbody rb { get; set; }

    WaterInteractionState OnWaterEnter();
    void FloatForce(Vector3 force, ForceMode forceMode);
    void SinkForce(Vector3 force, ForceMode forceMode);
    void OnWaterExit();
}

public enum WaterInteractionState
{
    Floating,
    Sinking
}
