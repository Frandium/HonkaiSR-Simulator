using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudPassanger : AArtifactTalent
{
    public CloudPassanger(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("cloudPassanger2", BuffType.Permanent, CommonAttribute.HealBonus, ValueType.InstantNumber, .1f);
    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {
        if(count >= 4)
        {
            BattleManager.Instance.skillPoint.GainPoint(1);
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("cloudPassanger2");
    }
}
