using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public virtual void OnBattleStart(CharacterMono c)
    {
        // 加一些 buff，给装备者加。
        // 如果给我方全体加，就找 BattleManager
    }

    public virtual float CalBuffValue(Creature source, Creature target,  CommonAttribute a, DamageType damageType)
    {
        float res = 0;
        foreach(Buff b in buffs)
        {
            res += b.CalBuffValue(source, target, a, damageType);
        }
        return res;
    }


    // 自身能提供的 buff。
    protected List<Buff> buffs = new List<Buff>();
}
