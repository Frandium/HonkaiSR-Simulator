using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unreplaceable : AWeaponTalent
{
    public Unreplaceable(JsonData data, int r):base(data, r)
    {

    }

    float atk, hp, dmg;
    public override void OnEquiping(Character self)
    {
        atk = (float)(double)config["effect"]["atk"]["value"][refine];
        hp = (float)(double)config["effect"]["hp"]["value"][refine];
        dmg = (float)(double)config["effect"]["hp"]["value"][refine];

        self.AddBuff("unreplaceableAtk", BuffType.Permanent, CommonAttribute.ATK, ValueType.Percentage, atk);
        self.afterTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("unreplaceableTrigger", (s, d) =>
        {
            Heal h = Heal.NormalHeal(self, self, CommonAttribute.ATK, hp);
            self.DealHeal(self, h);
            self.AddBuff("unreplaceableDmgUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, dmg, null, 1);
            return d;
        }));
        self.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("unreplaceableTrigger", (t, d) =>
        {
            if (t.hp < 0)
            {
                Heal h = Heal.NormalHeal(self, self, CommonAttribute.ATK, hp);
                self.DealHeal(self, h);
                self.AddBuff("unreplaceableDmgUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, dmg, null, 2);
            }
            return d;
        }));
    }

    public override void OnTakingOff(Character self)
    {
        self.RemoveBuff("unreplaceableAtk");
        self.afterTakingDamage.RemoveAll(t => t.tag == "unreplaceableTrigger");
        self.afterDealingDamage.RemoveAll(t => t.tag == "unreplaceableTrigger");
    }
}
