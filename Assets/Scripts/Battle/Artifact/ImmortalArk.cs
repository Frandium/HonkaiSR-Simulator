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
            c.AddBuff(Utils.valueBuffPool.GetOne().Set("immortalArk4_" + self.dbname, BuffType.Buff, CommonAttribute.ATK, (s, t, d) => { 
                if(self.GetFinalAttr(CommonAttribute.Speed) >= 120)
                {
                    return .08f * c.GetBaseAttr(CommonAttribute.ATK);
                }
                return 0;
            }));
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("immortalArk2");
    }
}
