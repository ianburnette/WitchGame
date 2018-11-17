using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class EnemyWaterInteraction : MonoBehaviour, IWaterInteraction
{
    public float Offset { get; }
    public float SubmergeDepth { get; }
    public float Height { get; set; }
    public Rigidbody rb { get; set; }

    public Rigidbody rigid;
    [SerializeField] float waterDragAmt, waterAngularDragAmt;
    [SerializeField] float waterEntranceSpeedDamping;
    [SerializeField] float outOfWaterResetTime;
    [SerializeField] Enemy enemy;
    [SerializeField] EnemyStateMachine stateMachine;
    public bool inWater;
    

    public WaterInteractionState OnWaterEnter()
    {
        inWater = true;
        enemy.SetInitialPosition();
        stateMachine.MyState = EnemyState.Wander;
        SetRigidbodyValues();
        return WaterInteractionState.SwimmingAway;
    }

    void SetRigidbodyValues()
    {
        rigid.drag = waterDragAmt;
        rigid.angularDrag = waterAngularDragAmt;
        rigid.useGravity = false;
        rigid.velocity = inWater ? rigid.velocity / waterEntranceSpeedDamping : rigid.velocity;
    }

    public void FloatForce(Vector3 force, ForceMode forceMode)
    {
        throw new System.NotImplementedException();
    }

    public void SinkForce(Vector3 force, ForceMode forceMode)
    {
    }

    public void OnWaterExit()
    {
        rigid.useGravity = true;
        Invoke(nameof(SetRigidbodyValues), outOfWaterResetTime);
    }

}
