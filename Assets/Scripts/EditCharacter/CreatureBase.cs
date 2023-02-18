using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBase
{
    public string dbname = "test";
    public string disname = "²âÊÔ½ÇÉ«";

    public float[] attrs = new float[(int)CommonAttribute.Count];
    protected int level = 60;
    public List<ValueBuff> buffs = new List<ValueBuff>();

    public CreatureBase() { }


    public virtual float GetBaseAttr(CommonAttribute attr)
    {
        return attrs[(int)attr];
    }

    public virtual float GetFinalAttr(CommonAttribute attr)
    {
        float res = attrs[(int)attr];
        foreach(ValueBuff buff in buffs)
        {
            res += buff.CalBuffValue(this, this, attr);
        }
        return res;
    }
}
