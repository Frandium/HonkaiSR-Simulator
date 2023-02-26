using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bronya : ACharacterTalents
{
    public Bronya(Character _self): base(_self)
    {

    }
    bool talent_activated = false;
    float atkdmg;
    float skilldmgUp;
    float burstAtkUp, burstCrtDmgPct, burstCrtDmgIns;
    float locationUp;
    public override void OnEquipping()
    {
        atkdmg = (float)(double)self.config["atk"]["dmg"]["value"][self.atkLevel];
        skilldmgUp = (float)(double)self.config["skill"]["dmgUp"]["value"][self.skillLevel];
        burstAtkUp = (float)(double)self.config["burst"]["atkUp"]["value"][self.skillLevel];
        burstCrtDmgIns = (float)(double)self.config["burst"]["crtDmgIns"]["value"][self.burstLevel];
        burstCrtDmgPct = (float)(double)self.config["burst"]["crtDmgPct"]["value"][self.burstLevel];
        locationUp = (float)(double)self.config["talent"]["location"]["value"][self.talentLevel];
        self.onNormalAttack.Add("talent", e =>
        {
            talent_activated = true;
            self.mono?.ShowMessage("行动提前", Color.green);
        });
        self.onTurnEnd.Add(new TriggerEvent<Creature.TurnStartEndEvent>("talent_inner", () =>
        {
            if (talent_activated)
            {
                self.ChangePercentageLocation(locationUp);
                talent_activated = false;
            }
        }));
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.NormalDamage(self, enemies[0], CommonAttribute.ATK, Element.Anemo, atkdmg, DamageType.Attack);
        self.DealDamage(enemies[0], Element.Anemo, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillCharacterAction(List<Character> characters)
    {
        Character c = characters[0];
        c.ChangePercentageLocation(1);
        c.AddBuff("bronyaSkill", BuffType.Buff, CommonAttribute.GeneralBonus,  ValueType.InstantNumber, skilldmgUp, 1);
        c.mono?.ShowMessage("行动提前", Color.black);
        Buff toRemove = c.buffs.Find(b => b.buffType == BuffType.Debuff);
        c.buffs.Remove(toRemove);
        base.SkillCharacterAction(characters);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        foreach(Character c in characters)
        {
            c.AddBuff("bronyaBurstATK", BuffType.Buff, CommonAttribute.ATK, ValueType.Percentage, burstAtkUp, 2);
            c.AddBuff(Utils.valueBuffPool.GetOne().Set("bronyaBurstCrtDmg", BuffType.Buff, CommonAttribute.CriticalDamage, 2, (s, t, _) =>
            {
                return burstCrtDmgIns + self.GetBaseAttr(CommonAttribute.CriticalDamage) * burstCrtDmgPct;
            }));
            c.mono?.ShowMessage("攻击提升", Color.green);
            c.mono?.ShowMessage("暴击伤害提升", Color.green);
        }
        base.BurstCharacterAction(characters);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        foreach(Character c in characters)
        {
            c.AddBuff("bronyaMystery", BuffType.Permanent, CommonAttribute.ATK, ValueType.Percentage, .15f, 2);
        }
    }
}
