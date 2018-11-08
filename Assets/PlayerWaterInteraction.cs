using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaterInteraction : MonoBehaviour, IWaterInteraction
{
    [SerializeField] float offset;
    public float Offset => offset;
    public float SubmergeDepth { get; }
    public float Height { get; set; }
    public Rigidbody rb { get; set; }

    [SerializeField] private PlayerMoveBase movementBase;

    void OnEnable() => rb = GetComponent<Rigidbody>();

    public WaterInteractionState OnWaterEnter()
    {
        movementBase.movementStateMachine.GetInWater();
        if (movementBase.playerAbilities.underwaterUnlocked)
        {
            // TODO: handle this when we get to it
        }
        return WaterInteractionState.Floating;
    }

    void Update()
    {
        Height = transform.position.y;
    }
    
    public void FloatForce(Vector3 force, ForceMode forceMode)
    {
        rb.AddForce(force,forceMode);
    }

    public void SinkForce(Vector3 force, ForceMode forceMode)
    {
        //TODO account for sinking behavior here when we get to it, if that makes sense
    }

    public void OnWaterExit() => movementBase.movementStateMachine.GetOutOfWater();
}
