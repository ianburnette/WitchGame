using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectWaterInteraction : MonoBehaviour, IWaterInteraction
{
    [SerializeField] float offset;
    [SerializeField] float resetTime;
    [SerializeField] float sinkSpeed, sinkForce;
    [SerializeField] float waterEntranceSpeedDamping;
    [SerializeField] float waterDrag, waterAngularDrag, baseDrag, baseAngularDrag;
    [SerializeField] Pickup pickup;
    
    public float Offset => offset;
    public float SubmergeDepth { get; }
    public float Height { get; set; }
    public Rigidbody rb { get; set; }
    
    void OnEnable() => rb = GetComponent<Rigidbody>();
    
    public virtual WaterInteractionState OnWaterEnter()
    {
        SetRigidbodyValues(inWater: true);
        var waterInteractionState = GetWaterInteractionState(pickup.Weight);
        if (waterInteractionState == WaterInteractionState.Sinking)
            StartCoroutine(ResetObjectInWater());
        return waterInteractionState;  
    }

    internal void SetRigidbodyValues(bool inWater)
    {
        rb.drag = inWater ? waterDrag : baseDrag;
        rb.angularDrag = inWater ? waterAngularDrag : baseAngularDrag;
        rb.velocity = inWater ? rb.velocity / waterEntranceSpeedDamping : rb.velocity;
        sinkForce = inWater ? sinkForce : 0;
    }

    WaterInteractionState GetWaterInteractionState(ObjectWeight pickupWeight) => 
        pickup.Weight == ObjectWeight.Heavy ? WaterInteractionState.Sinking : WaterInteractionState.Floating;

    void Update() => Height = transform.position.y;

    IEnumerator ResetObjectInWater()
    {
        yield return new WaitForSeconds(resetTime);
        OnWaterExit();
        pickup.Reset();
    }
    
    public void FloatForce(Vector3 force, ForceMode forceMode) => rb.AddForce(force,forceMode);

    public void SinkForce(Vector3 force, ForceMode forceMode)
    {
        rb.AddForce(force - (Vector3.down * sinkForce), forceMode);
        sinkForce -= sinkSpeed * Time.deltaTime;
    }

    public void OnWaterExit() => SetRigidbodyValues(inWater: false);
}
