using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class BattleNotEnd : AWeaponTalent
{
    public BattleNotEnd(JsonData c, int r) : base(c, r)
    {

    }

    int gainSkillPointTurn = -1;
    float selfEnergy, skilldmgUp;
    public override void OnEquiping(Character character)
    {
        selfEnergy = (float)(double)config["effect"]["selfEnergy"]["value"][refine];
        skilldmgUp = (float)(double)config["effect"]["dmgUp"]["value"][refine];
        character.AddBuff("battleNotEnd", BuffType.Permanent, CommonAttribute.EnergyRecharge, ValueType.InstantNumber, selfEnergy);
        character.afterBurst.Add(new TriggerEvent<Character.TalentUponTarget>("battleNotEndBurstPoint", c => { 
            if(c[0] is Character)
            {
                if (gainSkillPointTurn != BattleManager.Instance.curTurnNumber)
                {
                    BattleManager.Instance.skillPoint.GainPoint(1);
                    gainSkillPointTurn = BattleManager.Instance.curTurnNumber;
                }
            }
        }));
        character.afterSkill.Add(new TriggerEvent<Character.TalentUponTarget>("battleNotEndSkill", c => {
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
        ));
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
        character.afterBurst.RemoveAll(t => t.tag == "battleNotEndBurstPoint");
        character.afterSkill.RemoveAll(t => t.tag == "battleNotEndSkillEnergy");
    }
}
