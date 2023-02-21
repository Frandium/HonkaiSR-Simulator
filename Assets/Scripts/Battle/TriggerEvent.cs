using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEvent<T>
{
    int times;
    public T trigger;

    public TriggerEvent(int _times = int.MaxValue)
    {
        times = _times;
    }

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

    public void Zero()
    {
        times = 0;
    }
}
