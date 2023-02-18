using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class BattleNotEnd : AEquipmentTalents
{
    int refine;
    public BattleNotEnd(JsonData c, int _refine) : base(c)
    {
        refine = _refine;
    }

    public override void OnEquiping(CharacterBase character)
    {
        character.buffs.Add("battleNotEnd", Utils.valueBuffPool.GetOne().Set(
            new SimpleValueBuff(CommonAttribute.EnergyRecharge, (float)(double)config["selfEnergy"][refine], ValueType.InstantNumber)));
        character.onBurst.Add("battleNotEndBurstPoint", c => { 
            if(c is CharacterBase)
            {
                BattleManager.Instance.skillPoint.GainPoint(1);
            }
        });
        character.onSkill.Add("battleNotEndSkillEnergy", c => {
            if(c is CharacterBase)
            {
                CharacterBase cha = c as CharacterBase;
                if(cha != character)
                {
                    cha.ChangeEnergy((float)(double)config["burstEnergy"][refine]);
                }
            }
        }
        );
    }

    public override void OnTakingOff(CharacterBase character)
    {
        character.buffs.Remove("battleNotEnd");
        character.onBurst.Remove("battleNotEndSkillPoint");
        character.onSkill.Remove("battleNotEndSkillEnergy");
    }
}
