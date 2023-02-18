using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public virtual void OnBattleStart(Character c)
    {
        // 加一些 buff，给装备者加。
        // 如果给我方全体加，就找 BattleManager
    }

    public virtual float CalBuffValue(CreatureBase source, CreatureBase target,  CommonAttribute a)
    {
        float res = 0;
        foreach(ValueBuff b in buffs)
        {
            res += b.CalBuffValue(source, target, a);
        }
        return res;
    }


    // 自身能提供的 buff。
    protected List<ValueBuff> buffs = new List<ValueBuff>();
}
