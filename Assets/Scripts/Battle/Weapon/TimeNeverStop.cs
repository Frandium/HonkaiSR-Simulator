using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class TimeNeverStop : AWeaponTalent
{
    public TimeNeverStop(JsonData d, int r):base(d, r)
    {
    }

    float maxhp, heal, rate;
    Character self;
    public override void OnEquiping(Character character)
    {
        self = character;
        maxhp = (float)(double)config["effect"]["maxhp"]["value"][refine];
        heal = (float)(double)config["effect"]["heal"]["value"][refine];
        rate = (float)(double)config["effect"]["rate"]["value"][refine];
        character.AddBuff("timeNeverStopMaxHp", BuffType.Permanent, CommonAttribute.MaxHP, ValueType.Percentage, maxhp);
        character.AddBuff("timeNeverStopHeal", BuffType.Permanent, CommonAttribute.HealBonus, ValueType.InstantNumber, heal);
    }

    float healAmount = 0;
    public override void OnBattleStart(Character self, List<Character> characters)
    {
        self.afterDealingHeal.Add(new TriggerEvent<Creature.HealEvent>("timeNeverStop", (t, h) =>
        {
            healAmount += h.fullValue;
            return h;
        }));

        foreach(Character character in characters)
        {
            character.afterNormalAttack.Add(new TriggerEvent<Character.TalentUponTarget>("timeNeverStopCoAttack", DealDamage));
            character.afterSkill.Add(new TriggerEvent<Character.TalentUponTarget>("timeNeverStopCoAttack", DealDamage));
            character.afterBurst.Add(new TriggerEvent<Character.TalentUponTarget>("timeNeverStopCoAttack", DealDamage));
        }
    }

    void DealDamage(List<Creature> targets)
    {
        Enemy target = targets[Random.Range(0, targets.Count)] as Enemy;
        if (healAmount <= 0 || target == null)
        {
            return;
        }
        float dmg = healAmount * rate;
        healAmount = 0;
        DamageConfig dc = new DamageConfig(DamageType.CoAttack, self.element);
        bool critical = Utils.TwoRandom(self.GetFinalAttr(self, target, CommonAttribute.CriticalRate, dc));
        if (critical)
        {
            dmg *= 1 + self.GetFinalAttr(self, target, CommonAttribute.CriticalDamage, dc);
        }

        float def = target.GetFinalAttr(self, target, CommonAttribute.DEF, dc);
        float defIgnore = self.GetFinalAttr(self, target, CommonAttribute.DEFIgnore, dc);
        def *= (1 - defIgnore);
        float defRate = 1 - def / (def + 200 + self.level * 10);
        float overallResist = 1
        - target.GetFinalAttr(self, target, CommonAttribute.PhysicalResist + (int)self.element, dc)
            - target.GetFinalAttr(self, target, CommonAttribute.GeneralResist, dc);
        if (overallResist < .05f) overallResist = .05f; // 抗性上限 95%，无下限，但 0 以下折半
        if (overallResist > 1) overallResist = 1 + (overallResist - 1) * .5f;
        overallResist -= self.GetFinalAttr(CommonAttribute.PhysicalPenetrate + (int)self.element);
        dmg *= overallResist * defRate * (1 - target.GetFinalAttr(CommonAttribute.DmgDown));
        Damage d = new Damage(dmg, self.element, DamageType.CoAttack, critical);
        target.TakeDamage(self, d);
    }

    public override void OnTakingOff(Character character)
    {
        // write code to remove all buffs added in OnEquiping
        character.RemoveBuff("timeNeverStopMaxHp");
        character.RemoveBuff("timeNeverStopHeal");
    }
}
