using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEvent<T>
{
    int times;
    public T trigger { get; protected set; }
    public TriggerEvent(T e, int _times = int.MaxValue)
    {
        trigger = e;
        times = _times;
    }

    public bool CountDown()
    {
        times--;
        return times <= 0;
    }
}
