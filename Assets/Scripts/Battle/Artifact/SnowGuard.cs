using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowGuard : AArtifactTalent
{
    public SnowGuard(int c) : base(c)
    {

    }

    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("snowGuard2", BuffType.Permanent, CommonAttribute.DmgDown, ValueType.InstantNumber, .08f);
        if (count < 4)
            return;
        character.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEvent>("snowGuard4", () =>
        {
            if(character.hp < character.GetFinalAttr(CommonAttribute.MaxHP) * .5f)
            {
                character.TakeHeal(character, new Heal(.08f * character.GetFinalAttr(CommonAttribute.MaxHP)));
                character.ChangeEnergy(5);
            }
            return true;
        }));
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("snowGuard2");
        character.onTurnStart.RemoveAll(t => t.tag == "snowGuard4");
    }
}