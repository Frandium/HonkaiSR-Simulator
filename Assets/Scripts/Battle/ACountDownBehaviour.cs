using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CountDownType
{
    Turn = 0,
    Trigger = 1,
    All = 2,
    Permanent = 3,
    Count
}

public abstract class ACountDownBehaviour
{
    public string tag { get; protected set; }
    public int _turnTimes { get; protected set; }
    public int _triggerTimes { get; protected set; }
    public CountDownType ctype { get; protected set; }

    public ACountDownBehaviour(string _s, CountDownType _ctype, int trigger, int turn)
    {
        tag = _s;
        _turnTimes = trigger;
        _turnTimes = turn;
        ctype = _ctype;
    }

    public virtual bool CountDown(CountDownType t)
    {
        if (ctype != CountDownType.All && t != ctype)
            return false;
        if(t == CountDownType.Trigger)
        {
            --_triggerTimes;
            return _triggerTimes <= 0;
        }
        --_turnTimes;
        return _turnTimes <= 0;
    }

    public void Zero()
    {
        _triggerTimes = 0;
        _turnTimes = 0;
    }

    public void ChangeTurnTime(int offset)
    {
        _turnTimes += offset;
    }

    public void ChangeTriggerTime(int offset)
    {
        _triggerTimes += offset;
    }
}
