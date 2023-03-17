using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEvent<T> : ACountDownBehaviour
{
    public T trigger;

    public TriggerEvent(string _tag, int turnTimes = int.MaxValue, CountDownType countdownType = CountDownType.Turn, int triggerTimes = int.MaxValue): 
        base(_tag, countdownType, triggerTimes, turnTimes)
    {
        tag = _tag;
    }

    public TriggerEvent(string _tag, T e, int turnTimes = int.MaxValue, CountDownType countdownType = CountDownType.Turn, int triggerTimes = int.MaxValue):
        base(_tag, countdownType,  triggerTimes, turnTimes)
    {
        tag = _tag;
        trigger = e;
    }
}
