using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeVVK : AArtifactTalent
{
    public LifeVVK(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("lifeVVK2", BuffType.Permanent, CommonAttribute.EnergyRecharge, ValueType.InstantNumber, .05f);
    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {
        if(self.GetFinalAttr(CommonAttribute.Speed) >= 145){
            self.ChangePercentageLocation(.5f);
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("lifeVVK2");
    }
}
