using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleValueBuff
{
    public int attribute { get; protected set; }
    public float value { get; protected set; }
    public ValueType type { get; protected set; }

    public SimpleValueBuff(int a, float v, ValueType t)
    {
        attribute = a;
        value = v;
        type = t;
    }
}

public class Artifact: Equipment
{
    public Artifact(SimpleValueBuff _main, SimpleValueBuff[] _vices)
    {
        buffs = new ValueBuff[_vices.Length + 1];
        buffs[0] = BattleManager.Instance.valueBuffPool.GetOne().Set(_main);
        for(int i = 0; i < _vices.Length; ++i)
        {
            buffs[i + 1] = BattleManager.Instance.valueBuffPool.GetOne().Set(_vices[i]);
        }
    }
}

public class DisplayArtifact
{
    public SimpleValueBuff main;
    public SimpleValueBuff[] vices;

    public DisplayArtifact(SimpleValueBuff m, SimpleValueBuff[] v)
    {
        main = m;
        vices = v;
    }
}
