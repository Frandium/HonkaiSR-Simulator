using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public abstract class AEquipmentTalents
{
    protected JsonData config;
    public AEquipmentTalents(JsonData c)
    {
        config = c;
    }

    public abstract void OnEquiping(Character character);

    public abstract void OnTakingOff(Character character);
}
