using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AEnemyTalents: ACommomBattleTalents
{
    protected EnemyBase self;
    public AEnemyTalents(EnemyBase _self)
    {
        self = _self;
    }
    public virtual void MyTurn()
    {

    }
}
