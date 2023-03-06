using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleValueBuff
{
    public CommonAttribute attribute { get; protected set; }
    public float value { get; protected set; }
    public ValueType type { get; protected set; }

    public SimpleValueBuff(CommonAttribute a, float v, ValueType t)
    {
        attribute = a;
        value = v;
        type = t;
    }
}

public class Artifact: Equipment
{
    public Artifact(PhraseConfig _main, List<PhraseConfig> _vices)
    {
        buffs.Add(Utils.valueBuffPool.GetOne().Set(_main));
        for(int i = 0; i < _vices.Count; ++i)
        {
            buffs.Add(Utils.valueBuffPool.GetOne().Set(_vices[i]));
        }
    }
}

public class DisplayArtifact
{
    public SimpleValueBuff main;
    public List<SimpleValueBuff> vices;

    public DisplayArtifact(SimpleValueBuff m, List<SimpleValueBuff> v)
    {
        main = m;
        vices = v;
    }
}
