using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State: AProgressWithTurn
{
    public StateType state { get; protected set; }
    public State(StateType _s, int times): base("state", times)
    {
        state = _s;
    }
}
