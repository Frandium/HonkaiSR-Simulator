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
    float selfEnergy, burstEnergy;
    public override void OnEquiping(Character character)
    {
        selfEnergy = (float)(double)config["effect"]["selfEnergy"]["value"][refine];
        burstEnergy = (float)(double)config["effect"]["burstEnergy"]["value"][refine];
        character.AddBuff("battleNotEnd", BuffType.Permanent, CommonAttribute.EnergyRecharge, ValueType.InstantNumber, selfEnergy);
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
                    cha.ChangeEnergy(burstEnergy);
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
