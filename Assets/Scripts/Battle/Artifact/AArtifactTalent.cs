using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AArtifactTalent : AEquipmentTalent
{
    protected int count;

    public AArtifactTalent(int c)
    {
        count = c;
    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {

    }

    public override void OnEnemyRefresh(Character self, List<Enemy> enemies)
    {

    }
}
