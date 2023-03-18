using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectroBand : AArtifactTalent
{
    public ElectroBand(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("electroBand2", BuffType.Permanent, CommonAttribute.ElectroBonus, ValueType.InstantNumber, .1f);
    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {
        if (count >= 4)
        {
            self.beforeSkill.Add(new TriggerEvent<Character.TalentUponTarget>("electroBand4", t =>
            {
                self.AddBuff("electroBand4ATKUp", BuffType.Buff, CommonAttribute.ATK, ValueType.Percentage, .2f, 1);
            }));
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("electroBand2");
        character.beforeSkill.RemoveAll(t => t.tag == "electroBand4");
    }
}