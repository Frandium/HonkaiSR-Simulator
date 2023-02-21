using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield
{
    float maxHp;
    float hp;
    int times;

    public Shield(float max, int _times)
    {
        maxHp = max;
        hp = maxHp;
        times = _times;
    }

    public float TakeDamage(float v)
    {
        hp -= v;
        return hp;
    }

    public bool CountDown()
    {
        times--;
        return times >= 0;
    }
}
