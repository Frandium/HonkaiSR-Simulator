using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class Tingyun : ACharacterTalents
{
    public Tingyun(Character c):base(c){

    }

    float atkDmg, skillAtkUp, skillAtkMax, skillDmg, burstDmgUp, talentAtk;

    public override void OnEquipping()
    {
        atkDmg = (float)(double)self.metaData["atk"]["dmg"]["value"][self.atkLevel];
        skillAtkUp = (float)(double)self.metaData["skill"]["atkUp"]["value"][self.atkLevel];
        skillAtkMax = (float)(double)self.metaData["skill"]["atkMax"]["value"][self.atkLevel];
        skillDmg = (float)(double)self.metaData["skill"]["dmg"]["value"][self.atkLevel];
        burstDmgUp = (float)(double)self.metaData["burst"]["dmgUp"]["value"][self.atkLevel];
        talentAtk = (float)(double)self.metaData["talent"]["atk"]["value"][self.atkLevel];
        base.OnEquipping();
        self.onDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("tingyunTalent", (t, v, e, d) =>
        {
            if (curSkill != null) {
                float dmgBase = curSkill.GetFinalAttr(CommonAttribute.ATK) * 30 / 100.0f;
                float elebonus = self.GetFinalAttr(self, t, CommonAttribute.PhysicalBonus + (int)Element.Electro, DamageType.All);
                float genebonus = self.GetFinalAttr(self, t, CommonAttribute.GeneralBonus, DamageType.All);
                float overallBonus = 1 + Mathf.Max(0, elebonus + genebonus); // 伤害加成下限 0，无上限
                float dmg = dmgBase * overallBonus;
                if (Random.Range(0, 1000) < self.GetFinalAttr(self, t, CommonAttribute.CriticalRate, DamageType.All) * 1000)
                {
                    dmg *= self.GetFinalAttr(self, t, CommonAttribute.CriticalDamage, DamageType.All);
                }

                float def = t.GetFinalAttr(self, t, CommonAttribute.DEF, DamageType.All);
                float defRate = 1 - def / (def + 2000);
                float overallResist = 1
                    - t.GetFinalAttr(self, t, CommonAttribute.PhysicalResist + (int)Element.Electro, DamageType.All)
                    - t.GetFinalAttr(self, t, CommonAttribute.GeneralResist, DamageType.All);
                if (overallResist < .05f) overallResist = .05f; // 抗性上限 95%，无下限，但 0 以下折半
                if (overallResist > 1) overallResist = 1 + (overallResist - 1) * .5f;
                dmg *= overallResist * defRate;

                t.TakeDamage(self, dmg, Element.Electro, DamageType.All);
            }
            return v;
        }));
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Electro, 0.5f, DamageType.Attack);
        self.DealDamage(e, Element.Electro, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    Character curSkill;

    public override void SkillCharacterAction(List<Character> characters)
    {
        if(curSkill != null)
        {
            curSkill.RemoveBuff("tingyunSkillATK");
            curSkill.onDealingDamage.RemoveAll(t => t.tag == "tingyunHelp");
        }
        Character c = characters[0];
        c.AddBuff(Utils.valueBuffPool.GetOne().Set("tingyunSkillATK", BuffType.Buff, CommonAttribute.ATK, 3, (s, t, d) =>
        {
            float res = t.GetBaseAttr(CommonAttribute.ATK) * .25f;
            res = Mathf.Min(res, self.GetFinalAttr(CommonAttribute.ATK) * .15f);
            return res;
        }));
        c.onDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("tingyunHelp", (t, v, e, d) =>
        {
            float dmg = DamageCal.NormalDamage(c, t, CommonAttribute.ATK, Element.Electro, 0.2f, d);
            t.TakeDamage(c, dmg, Element.Electro, d);
            return v;
        }, 3));
        curSkill = c;
        c.mono?.ShowMessage("赐福", Color.red);
        c.mono?.ShowMessage("攻击提升", Color.red);
        c.mono?.ShowMessage("协同伤害", Color.red);
        base.SkillCharacterAction(characters);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        Character c = characters[0];
        c.ChangeEnergy(50);
        c.AddBuff("tingyunBurstBonus", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .2f, 3);
        c.mono?.ShowMessage("造成伤害提高", Color.red);
        base.BurstCharacterAction(characters);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        self.ChangeEnergy(50);
    }

}
