using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AProgressWithTurn
{
    public string tag { get; protected set; }
    public int times { get; protected set; }

    public AProgressWithTurn(string _s, int _t)
    {
        tag = _s;
        times = _t;
    }

    public virtual bool CountDown()
    {
        --times;
        return times <= 0;
    }

    public void Zero()
    {
        times = 0;
    }
}
