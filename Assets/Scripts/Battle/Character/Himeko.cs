using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

public class Himeko : ACharacterTalents
{
    public Himeko(Character character) : base(character) { }

    float attackAtk, skillAtk1, skillAtk2, burstAtk, talentAtk;
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
        skillAtk1 = (float)(double)self.metaData["skill"]["skillAtk1"]["value"][self.skillLevel];
        skillAtk2 = (float)(double)self.metaData["skill"]["skillAtk2"]["value"][self.skillLevel];
        burstAtk = (float)(double)self.metaData["burst"]["burstAtk"]["value"][self.burstLevel];
        talentAtk = (float)(double)self.metaData["talent"]["talentAtk"]["value"][self.talentLevel];

        if (self.config.abilityActivated[1])
        {
            self.AddBuff("himekoAbility2", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .2f,
                (s, t, dc) =>
                {
                    return dc.type == DamageType.Skill && t.IsUnderState(StateType.Burning);
                });
        }
        if (self.config.abilityActivated[2]) {
            self.AddBuff("himekoAbility3", BuffType.Permanent, CommonAttribute.CriticalRate, ValueType.InstantNumber, .15f,
                (s, t, dc) =>
                {
                    return self.hp >= .8f * self.GetFinalAttr(CommonAttribute.MaxHP);
                });
        }
        if(self.constellaLevel >= 2)
        {
            self.AddBuff("himekoC2DmgUp", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .15f,
                (s, t, dc) =>
                {
                    return t.hp < .5f * t.GetFinalAttr(CommonAttribute.MaxHP);
                });
        }
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, attackAtk, new DamageConfig(DamageType.Attack, Element.Pyro));
        self.DealDamage(e, d);
        if (self.config.abilityActivated[0] &&
            Utils.TwoRandom((1 + self.GetFinalAttr(self, e, CommonAttribute.EffectHit, DamageConfig.defaultDC)) * .8f))
        {
            e.AddState(self, new State(StateType.Burning, 3, () =>
            {
                e.onTurnStart.RemoveAll(s => s.tag == "himekoBurning");
            }));
            if (e.onTurnStart.Find(t => t.tag == "himekoBurning") == null)
                e.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEvent>("himekoBurning", () =>
                {
                    if (e.IsUnderState(StateType.Burning))
                    {
                        Damage dmg = Damage.NormalDamage(self, e, CommonAttribute.ATK, 3000f, new DamageConfig(DamageType.Continue, Element.Pyro));
                        self.DealDamage(e, dmg);
                    }
                    return true;
                }));
        }
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        Enemy mainTarget = enemies[0];
        float beforeWeakHp = mainTarget.weakHp;
        Damage d = Damage.NormalDamage(self, mainTarget, CommonAttribute.ATK, skillAtk1, new DamageConfig(DamageType.Skill, Element.Pyro));
        self.DealDamage(mainTarget, d);
        if(beforeWeakHp > 0 && mainTarget.weakHp <= 0)
        {
            talentCharge++;
        }
        foreach (Enemy e in enemies)
        {
            if (e == mainTarget) continue;
            beforeWeakHp = e.weakHp;
            Damage d2 = Damage.NormalDamage(self, e, CommonAttribute.ATK, skillAtk2, new DamageConfig(DamageType.Skill, Element.Pyro));
            self.DealDamage(e, d2);
            if (beforeWeakHp > 0 && mainTarget.weakHp <= 0)
            {
                talentCharge++;
            }
        }
        base.SkillEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {

        foreach (Enemy e in enemies)
        {   
            Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, burstAtk, new DamageConfig(DamageType.Burst, Element.Pyro));
            self.DealDamage(e, d);
            if(e.hp < 0)
            {
                self.ChangeEnergy(10);
            }
        }
        if(self.constellaLevel >= 6)
        {
            for(int i = 0; i < 2; i++)
            {
                int j = Random.Range(0, enemies.Count);
                Enemy e = enemies[j];
                Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, burstAtk * .4f, new DamageConfig(DamageType.Burst, Element.Pyro));
                self.DealDamage(e, d);
                if(e.hp < 0)
                {
                    self.ChangeEnergy(10);
                }
            }
        }
        base.BurstEnemyAction(enemies);
    }

    int talentCharge = 1;
    public override void OnEnemyRefresh(List<Enemy> enemies)
    {
        foreach (Enemy e in enemies)
        {
            e.onBreak.Add(new TriggerEvent<Enemy.OnBreak>("himekoTalent", (s, d) =>
            {
                talentCharge++;
            }));
        }
        base.OnEnemyRefresh(enemies);
    }

    public override void OnBattleStart(List<Character> characters)
    {
        TriggerEvent<Character.TalentUponTarget> talentTrigger = new TriggerEvent<Character.TalentUponTarget>("himekoTalentAdditional", (targets) => {
            if (talentCharge >= 3)
            {
                talentCharge = 0;
                foreach (Enemy e in BattleManager.Instance.enemies) {
                    Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, talentAtk, new DamageConfig(DamageType.Additional, Element.Pyro));
                    self.DealDamage(e, d);
                }
                if(self.constellaLevel >= 1)
                {
                    self.AddBuff("himekoC1Speed", BuffType.Buff, CommonAttribute.Speed, ValueType.Percentage, .3f, 2);
                }
            }
        });
        foreach (Character c in characters)
        {
            c.afterNormalAttack.Add(talentTrigger);
            c.afterSkill.Add(talentTrigger);
            c.afterBurst.Add(talentTrigger);
        }
        base.OnBattleStart(characters);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        foreach(Enemy e in enemies)
        {
            if(Utils.TwoRandom(self.GetFinalAttr(CommonAttribute.EffectHit) * 1))
            {
                float resist = 1 - 1 / (1 + e.GetFinalAttr(self, e, CommonAttribute.EffectResist, DamageConfig.defaultDC));
                if (Utils.TwoRandom(resist))
                {
                    e.mono?.ShowMessage("µÖ¿¹", Color.red);
                }
                else
                {
                    e.AddBuff("himekoMystery", BuffType.Debuff, CommonAttribute.DmgUp, ValueType.InstantNumber, .1f, (s, t, d) =>
                    {
                        return d.element == Element.Pyro;
                    });
                }
            }
        }
    }

}
