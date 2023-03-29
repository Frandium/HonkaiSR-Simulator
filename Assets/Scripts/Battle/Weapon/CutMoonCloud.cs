using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class CutMoonCloud : AWeaponTalent
{
    public CutMoonCloud(JsonData d, int r):base(d, r)
    {

    }

    float atk, crtdmg, echarge;
    public override void OnEquiping(Character character)
    {
        atk = (float)(double)config["effect"]["atk"]["value"][refine];
        crtdmg = (float)(double)config["effect"]["crtdmg"]["value"][refine];
        echarge = (float)(double)config["effect"]["echarge"]["value"][refine];
        character.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEvent>("cutMoonCloudTrigger", () =>
        {
            Effect(BattleManager.Instance.characters);
            return true;
        }));
        character.onDying.Add(new TriggerEvent<Creature.DyingEvent>("cutMoonCloudRemove", () =>
        {
            foreach(Character c in BattleManager.Instance.characters)
            {
                c.RemoveBuff("cutMoonCloudRandom");
            }
        }));
    }

    public override void OnTakingOff(Character character)
    {

    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {
        Effect(characters);
    }

    void Effect(List<Character> characters)
    {
        int i = Random.Range(0, 3);
        foreach(Character c in characters)
        {
            if(i == 0)
            {
                c.AddBuff("cutMoonCloudRandom", BuffType.Buff, CommonAttribute.ATK, ValueType.Percentage, atk, cdtype: CountDownType.Permanent);
            }else if(i == 1)
            {
                c.AddBuff("cutMoonCloudRandom", BuffType.Buff, CommonAttribute.CriticalDamage, ValueType.InstantNumber, crtdmg, cdtype: CountDownType.Permanent);
            }
            else
            {
                c.AddBuff("cutMoonCloudRandom", BuffType.Buff, CommonAttribute.EnergyRecharge, ValueType.InstantNumber, echarge, cdtype: CountDownType.Permanent);
            }
        }
    }
}
