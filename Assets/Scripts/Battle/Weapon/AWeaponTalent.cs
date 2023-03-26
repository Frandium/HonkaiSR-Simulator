using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public abstract class AWeaponTalent : AEquipmentTalent
{
    protected JsonData config;
    protected int refine;

    public AWeaponTalent(JsonData c, int r)
    {
        config = c;
        refine = r;
    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {
        
    }

    public override void OnEnemyRefresh(Character self, List<Enemy> enemies)
    {

    }
}
