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
    float selfEnergy, skilldmgUp;
    public override void OnEquiping(Character character)
    {
        selfEnergy = (float)(double)config["effect"]["selfEnergy"]["value"][refine];
        skilldmgUp = (float)(double)config["effect"]["dmgUp"]["value"][refine];
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
        character.onSkill.Add("battleNotEndSkill", c => {
            foreach(Character cha in BattleManager.Instance.characters)
            {
                Character thisTurn = cha;
                cha.onTurnStart.Add(
                    new TriggerEvent<Creature.TurnStartEndEvent>("battleNotEndSkillStart", () => {
                        thisTurn.AddBuff("battleNotEndSkillDmgUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, skilldmgUp, 1);
                        RemoveUnusedSkillBuff(thisTurn);
                    })
                );
            }
        }
        );
    }

    void RemoveUnusedSkillBuff(Character c)
    {
        foreach(Character cha in BattleManager.Instance.characters)
        {
            if(cha!= c)
            {
                cha.onTurnStart.RemoveAll(t => t.tag == "battleNotEndSkillStart");
            }
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.buffs.RemoveAll( b => b.tag == "battleNotEnd");
        character.onBurst.Remove("battleNotEndBurstPoint");
        character.onSkill.Remove("battleNotEndSkillEnergy");
    }
}
