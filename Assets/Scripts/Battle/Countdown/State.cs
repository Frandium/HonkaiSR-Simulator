using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State: ACountDownBehaviour
{
    public StateType state { get; protected set; }
    public delegate void OnStateRemove();
    public OnStateRemove onremove { get; set; }
    public State(StateType _s, int times, OnStateRemove onremove = null): base("state", CountDownType.Turn, int.MaxValue, times)
    {
        state = _s;
        this.onremove = onremove;
    }
}
