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

    public abstract void OnEquiping(CharacterBase character);

    public abstract void OnTakingOff(CharacterBase character);
}
