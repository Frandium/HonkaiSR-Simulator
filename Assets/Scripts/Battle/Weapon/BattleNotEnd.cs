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

    int gainSkillPointTurn = -1;
    public override void OnEquiping(Character character)
    {
        character.AddBuff("battleNotEnd", BuffType.Permanent, CommonAttribute.EnergyRecharge, ValueType.InstantNumber, (float)(double)config["selfEnergy"][refine]);
        character.onBurst.Add("battleNotEndBurstPoint", c => { 
            if(c is Character)
            {
                if (gainSkillPointTurn != BattleManager.Instance.curTurnNumber)
                {
                    BattleManager.Instance.skillPoint.GainPoint(1);
                    gainSkillPointTurn = BattleManager.Instance.curTurnNumber;
                }
            }
        });
        character.onSkill.Add("battleNotEndSkillEnergy", c => {
            if(c is Character)
            {
                Character cha = c as Character;
                if(cha != character)
                {
                    cha.ChangeEnergy((float)(double)config["burstEnergy"][refine]);
                }
            }
        }
        );
    }

    public override void OnTakingOff(Character character)
    {
        character.buffs.RemoveAll( b => b.tag == "battleNotEnd");
        character.onBurst.Remove("battleNotEndSkillPoint");
        character.onSkill.Remove("battleNotEndSkillEnergy");
    }
}
