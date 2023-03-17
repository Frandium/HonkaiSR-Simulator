using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public abstract class AWeaponTalent : AEquipmentTalent
{
    protected JsonData config;

    public AWeaponTalent(JsonData c)
    {
        config = c;
    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {

    }

    public override void OnEnemyRefresh(Character self, List<Enemy> enemies)
    {

    }
}
