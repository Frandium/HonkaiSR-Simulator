using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Welt : ACharacterTalents
{
    public Welt(Character c): base(c) { }

    float attackAtk, skillAtk, skillProb, burstAtk, burstBack, talentAtk;
    public override void OnEquipping()
    {
        if (self.constellaLevel >= 5)
        {
            self.SkillLevelUp(2);
            self.ATKLevelUp(1);
        }
        if (self.constellaLevel >= 3)
        {
            self.BurstLevelUp(2);
            self.TalentLevelUp(2);
        }

        attackAtk = (float)(double)self.metaData["atk"]["attackAtk"]["value"][self.atkLevel];
        skillAtk = (float)(double)self.metaData["skill"]["skillAtk"]["value"][self.skillLevel];
        skillProb = (float)(double)self.metaData["skill"]["skillProb"]["value"][self.skillLevel];
        burstAtk = (float)(double)self.metaData["burst"]["burstAtk"]["value"][self.burstLevel];
        burstBack = (float)(double)self.metaData["burst"]["burstBack"]["value"][self.burstLevel];
        talentAtk = (float)(double)self.metaData["talent"]["talentAtk"]["value"][self.talentLevel];

        self.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("weltTalent", (t, d) =>
        {
            if (t.GetFinalAttr(CommonAttribute.Speed) - t.GetBaseAttr(CommonAttribute.Speed) > 0)
            {
                Damage dmg = Damage.NormalDamage(self, t, CommonAttribute.ATK, talentAtk, new DamageConfig(DamageType.CoAttack, Element.Imaginary));
                self.DealDamage(t, dmg);
                if (self.constellaLevel >= 2)
                {
                    self.ChangeEnergy(3);
                }
            }
            return d;
        }));

        if (self.config.abilityActivated[0])
        {
            self.AddBuff("weltAbility1", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .25f, (s, t, d) =>
            {
                return d.type == DamageType.Burst;
            });
        }

        if (self.config.abilityActivated[1])
        {
            self.afterBurst.Add(new TriggerEvent<Character.TalentUponTarget>("weltAbility2", (targets) =>
            {
                self.ChangeEnergy(10);
            }));
        }

        if (self.config.abilityActivated[2])
        {
            self.AddBuff("weltAbility3", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .2f, (s, t, d) =>
            {
                return t is Enemy && (t as Enemy).weakHp <= 0;
            });
        }
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        DamageConfig dc = new DamageConfig(DamageType.Attack, Element.Imaginary);
        Enemy c = enemies[0];
        Damage d = Damage.NormalDamage(self, c, CommonAttribute.ATK, attackAtk, dc);
        self.DealDamage(c, d);
        if (chargedAttack > 0)
        {
            --chargedAttack;
            Damage d2 = Damage.NormalDamage(self, c, CommonAttribute.ATK, attackAtk * .5f, dc);
            d2.type = DamageType.CoAttack;
            self.DealDamage(c, d2);
        }
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        DamageConfig dc = new DamageConfig(DamageType.Skill, Element.Imaginary);
        for (int ii = 0; ii < (self.constellaLevel >= 6 ? 4 : 3); ii++)
        {
            Enemy enemy = enemies[Random.Range(0, enemies.Count)];
            Damage d = Damage.NormalDamage(self, enemy, CommonAttribute.ATK, skillAtk,dc );
            self.DealDamage(enemy, d);
            self.TestAndAddEffect(skillProb, enemy, () =>
            {
                enemy.AddBuff("weltSkillSpeedDown", BuffType.Debuff, CommonAttribute.Speed, ValueType.Percentage, .1f, 2);
            }, dc);
        }
        if (chargedAttack > 0)
        {
            --chargedAttack;
            Enemy enemy = enemies[Random.Range(0, enemies.Count)];
            Damage d2 = Damage.NormalDamage(self, enemy, CommonAttribute.ATK, skillAtk * .8f, dc);
            d2.type = DamageType.CoAttack;
            self.DealDamage(enemy, d2);
            self.TestAndAddEffect(skillProb, enemy, () =>
            {
                enemy.AddBuff("weltSkillSpeedDown", BuffType.Debuff, CommonAttribute.Speed, ValueType.Percentage, .1f, 2);
            }, dc);
        }
        base.SkillEnemyAction(enemies);
    }

    int chargedAttack = 0;
    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        DamageConfig dc = new DamageConfig(DamageType.Burst, Element.Imaginary, StateType.Restricted);
        foreach (Enemy enemy in enemies)
        {
            Damage d = Damage.NormalDamage(self, enemy, CommonAttribute.ATK, burstAtk, dc);
            self.DealDamage(enemy, d);
            self.TestAndAddEffect(1, enemy, () =>
            {
                enemy.AddRestricted(self, 1, .32f, .1f);
            }, dc);
            if (self.constellaLevel >= 4)
            {
                enemy.AddBuff("weltC4DmgUp", BuffType.Debuff, CommonAttribute.DmgUp, ValueType.InstantNumber, .12f, null, 2);
            }
        }
        if(self.constellaLevel>=1)
            chargedAttack = 2;
        base.BurstEnemyAction(enemies);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        DamageConfig dc = new DamageConfig(DamageType.CoAttack, Element.Imaginary, StateType.Restricted);
        foreach(Enemy e in enemies) {
            self.TestAndAddEffect(1, e, () =>
            {
                e.AddRestricted(self, 1, .2f, .1f);
            }, dc);
        }
    }

}
