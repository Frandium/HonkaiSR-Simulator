using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class VictoryMoment : AEquipmentTalents
{
    int refine;
    public VictoryMoment(JsonData c, int r) : base(c)
    {
        refine = r;
    }
    float def1, effHit, def2;
    public override void OnEquiping(Character character)
    {
        def1 = (float)(double)config["effect"]["def1"]["value"][refine];
        effHit = (float)(double)config["effect"]["effHit"]["value"][refine];
        def2 = (float)(double)config["effect"]["def2"]["value"][refine];
        character.AddBuff("victoryMomentDEF", BuffType.Permanent, CommonAttribute.DEF, ValueType.Percentage, def1);
        character.AddBuff("victoryMomentEffectHit", BuffType.Permanent, CommonAttribute.EffectHit, ValueType.InstantNumber, effHit);
        character.AddBuff("victoryMomentTaunt", BuffType.Permanent, CommonAttribute.Taunt, ValueType.InstantNumber, 100);

        character.onTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("victoryMomentDEF2", (s, d) =>
        {
            character.AddBuff("victoryMomentDEF2", BuffType.Buff, CommonAttribute.DEF, ValueType.Percentage, def2, 1);
            return d;
        }));
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("victoryMomentDEF");
        character.RemoveBuff("victoryMomentEffectHit");
        character.RemoveBuff("victoryMomentTaunt");
        character.onTakingDamage.RemoveAll(r => r.tag == "victoryMomentDEF2");
    }
}
