using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmortalArk : AArtifactTalent
{
    public ImmortalArk(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("immortalArk2", BuffType.Permanent, CommonAttribute.MaxHP, ValueType.Percentage, .12f);
    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {
        foreach(Character c in characters)
        {
            c.AddBuff("immortalArk4_" + self.dbname, BuffType.Permanent, CommonAttribute.ATK, ValueType.Percentage, .08f,
                (s, t, d) => { return self.GetFinalAttr(CommonAttribute.Speed) >= 120; });
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("immortalArk2");
    }
}
