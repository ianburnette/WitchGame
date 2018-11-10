using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaterInteraction : ObjectWaterInteraction
{

    [SerializeField] CharacterMotor motor;
    public override WaterInteractionState OnWaterEnter()
    {
        SetRigidbodyValues(inWater: true);
        StartEnemyEscapeIntoWater();
        return WaterInteractionState.SwimmingAway;
    }

    void Update()
    {
            
    }

    void StartEnemyEscapeIntoWater()
    {
        
    }
}
