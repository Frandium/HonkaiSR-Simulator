using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorThief : AArtifactTalent
{
    public MeteorThief(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("meteorThief2", BuffType.Permanent, CommonAttribute.BreakBonus, ValueType.InstantNumber, .2f);
        
        if (count < 4)
            return;
        character.AddBuff("meteorThief4", BuffType.Permanent, CommonAttribute.BreakBonus, ValueType.InstantNumber, .2f);
    }


    public override void OnEnemyRefresh(Character self, List<Enemy> enemies)
    {
        foreach (Enemy e in enemies)
        {
            e.onBreak.Add(new TriggerEvent<Enemy.OnBreak>("meteorThief4Enegy", (s, d) =>
            {
                if (s == self)
                {
                    self.ChangeEnergy(3);
                }
            }));
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("meteorThief2");
        character.RemoveBuff("meteorThief4");
    }
}
