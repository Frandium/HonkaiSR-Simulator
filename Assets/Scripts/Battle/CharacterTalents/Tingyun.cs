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
        if (self.constellaLevel >= 5)
        {
            self.BurstLevelUp(2);
            self.ATKLevelUp(1);
        }
        if (self.constellaLevel >= 3)
        {
            self.SkillLevelUp(2);
            self.TalentLevelUp(2);
        }
        atkDmg = (float)(double)self.metaData["atk"]["dmg"]["value"][self.atkLevel];
        skillAtkUp = (float)(double)self.metaData["skill"]["atkUp"]["value"][self.skillLevel];
        skillAtkMax = (float)(double)self.metaData["skill"]["atkMax"]["value"][self.skillLevel];
        skillDmg = (float)(double)self.metaData["skill"]["dmg"]["value"][self.skillLevel];
        burstDmgUp = (float)(double)self.metaData["burst"]["dmgUp"]["value"][self.burstLevel];
        talentAtk = (float)(double)self.metaData["talent"]["atk"]["value"][self.talentLevel];
        base.OnEquipping();
        self.onDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("tingyunTalent", (t, d) =>
        {
            if (curSkill != null) {
                float dmgBase = curSkill.GetFinalAttr(CommonAttribute.ATK) * talentAtk;
                float elebonus = self.GetFinalAttr(self, t, CommonAttribute.PhysicalBonus + (int)Element.Electro, DamageType.All);
                float genebonus = self.GetFinalAttr(self, t, CommonAttribute.GeneralBonus, DamageType.All);
                float overallBonus = 1 + Mathf.Max(0, elebonus + genebonus); // 伤害加成下限 0，无上限
                float dmg = dmgBase * overallBonus;
                bool critical = Utils.TwoRandom(self.GetFinalAttr(self, t, CommonAttribute.CriticalRate, DamageType.All));
                if (critical)
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

                t.TakeDamage(self, new Damage(dmg, Element.Electro, DamageType.All, critical));
            }
            return d;
        }));
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        Damage dmg = Damage.NormalDamage(self, e, CommonAttribute.ATK, Element.Pyro, atkDmg, DamageType.Attack);
        self.DealDamage(e, dmg);
        base.AttackEnemyAction(enemies);
    }

    Character curSkill;
    bool constellation2Triggerd = false;
    public override void SkillCharacterAction(List<Character> characters)
    {
        if (curSkill != null)
        {
            curSkill.RemoveBuff("tingyunSkillATK");
            curSkill.onDealingDamage.RemoveAll(t => t.tag == "tingyunHelp");
            if(self.constellaLevel >= 1)
            {
                curSkill.onBurst.RemoveAll(t => t.tag == "tingyunConstellation1Trigger");
                if(self.constellaLevel >= 2)
                {
                    curSkill.onDealingDamage.RemoveAll(t => t.tag == "tingyunConstellation2");
                    curSkill.onTurnStart.RemoveAll(t => t.tag == "tingyunConstellation2Refresh");
                }
            }
        }
        Character c = characters[0];
        curSkill = c;
        c.AddBuff(Utils.valueBuffPool.GetOne().Set("tingyunSkillATK", BuffType.Buff, CommonAttribute.ATK, (s, t, d) =>
        {
            float res = t.GetBaseAttr(CommonAttribute.ATK) * skillAtkUp;
            res = Mathf.Min(res, self.GetFinalAttr(CommonAttribute.ATK) * skillAtkMax);
            return res;
        }, 3));
        c.onDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("tingyunHelp", (t, d) =>
        {
            Damage dmg = Damage.NormalDamage(c, t, CommonAttribute.ATK, Element.Electro, skillDmg + (self.constellaLevel >= 4? .2f : 0), d.type);
            t.TakeDamage(c, dmg);
            return d;
        }, 3));
        c.mono?.ShowMessage("赐福", Color.red);
        c.mono?.ShowMessage("攻击提升", Color.red);
        c.mono?.ShowMessage("协同伤害", Color.red);
        if (self.constellaLevel >= 1)
        {
            c.onBurst.Add(new TriggerEvent<Character.TalentUponTarget>("tingyunConstellation1Trigger", c =>
            {
                curSkill.AddBuff("tingyunConstellation1SpeedUp", BuffType.Buff, CommonAttribute.Speed, ValueType.Percentage, .2f, 2);
            }));
            if (self.constellaLevel >= 2)
            {
                c.onDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("tingyunConstellation2", (t, d) =>
                {
                    if (t.hp <= 0 && !constellation2Triggerd)
                    {
                        constellation2Triggerd = true;
                        curSkill.ChangeEnergy(10);
                    }
                    return d;
                }));
                c.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEndEvent>("tingyunConstellation2Refresh", () =>
                {
                    constellation2Triggerd = false;
                }));
            }
        }
        base.SkillCharacterAction(characters);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        Character c = characters[0];
        c.ChangeEnergy(self.constellaLevel >= 6? 60: 50);
        c.AddBuff("tingyunBurstBonus", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, burstDmgUp, 3);
        c.mono?.ShowMessage("造成伤害提高", Color.red);
        base.BurstCharacterAction(characters);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        self.ChangeEnergy(50);
    }

}
