using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaterInteraction : ObjectWaterInteraction
{
    public override WaterInteractionState OnWaterEnter()
    {
        SetRigidbodyValues(inWater: true);
        StartEnemyEscapeIntoWater();
        return WaterInteractionState.SwimmingAway;
    }


    void StartEnemyEscapeIntoWater()
    {
        
    }
}
