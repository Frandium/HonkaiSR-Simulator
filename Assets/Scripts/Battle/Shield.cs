using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : AProgressWithTurn
{
    public float maxHp { get; protected set; }
    public float hp { get; protected set; }

    public Shield(string tag, float max, int _times): base(tag, _times)
    {
        maxHp = max;
        hp = maxHp;
    }

    public float TakeDamage(float v)
    {
        hp -= v;
        return hp;
    }
}
