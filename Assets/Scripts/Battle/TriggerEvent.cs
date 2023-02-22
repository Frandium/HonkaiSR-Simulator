using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEvent<T> : AProgressWithTurn
{
    public T trigger;

    public TriggerEvent(string _tag, int _times = int.MaxValue): base(_tag, _times)
    {
        tag = _tag;
    }

    public TriggerEvent(string _tag, T e, int _times = int.MaxValue): base(_tag, _times)
    {
        tag = _tag;
        trigger = e;
    }
}
