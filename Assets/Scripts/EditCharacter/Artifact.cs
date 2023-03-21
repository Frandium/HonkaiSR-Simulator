using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
