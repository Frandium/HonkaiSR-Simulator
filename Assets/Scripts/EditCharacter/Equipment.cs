using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{ 
    // 自身能提供的 buff。
    protected List<Buff> buffs = new List<Buff>();

    public virtual float CalBuffValue(Creature source, Creature target,  CommonAttribute a, DamageType damageType, bool forView)
    {
        float res = 0;
        for(int i = buffs.Count - 1; i>=0; --i)
        {
            float b = buffs[i].CalBuffValue(source, target, a, damageType);
            if (!forView && b > 0)
            {
                if (buffs[i].CountDown(CountDownType.Trigger))
                    buffs.RemoveAt(i);
            }
            res += b;
        }
        return res;
    }
}
