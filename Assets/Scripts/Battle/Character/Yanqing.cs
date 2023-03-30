using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

public class Yanqing : ACharacterTalents
{
    public Yanqing(Character c) : base(c) { }

    float atkDmg, skillAtk, burstCrtRate, burstCrtDmg, burstAtk, talentCrtRate, talentCrtDmg, talentAtk1, talentAtk2;
    int burstTurn;
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
        atkDmg = (float)(double)self.metaData["atk"]["dmg"]["value"][self.atkLevel];
        skillAtk = (float)(double)self.metaData["skill"]["skillAtk"]["value"][self.skillLevel];
        burstCrtRate = (float)(double)self.metaData["burst"]["burstCrtRate"]["value"][self.burstLevel];
        burstCrtDmg = (float)(double)self.metaData["burst"]["burstCrtDmg"]["value"][self.burstLevel];
        burstTurn = (int)self.metaData["burst"]["burstTurn"]["value"][self.burstLevel];
        burstAtk = (float)(double)self.metaData["burst"]["burstAtk"]["value"][self.burstLevel];
        talentCrtRate = (float)(double)self.metaData["talent"]["talentCrtRate"]["value"][self.talentLevel];
        talentCrtDmg = (float)(double)self.metaData["talent"]["talentCrtDmg"]["value"][self.talentLevel];
        talentAtk1 = (float)(double)self.metaData["talent"]["talentAtk1"]["value"][self.talentLevel];
        talentAtk2 = (float)(double)self.metaData["talent"]["talentAtk2"]["value"][self.talentLevel];

        self.AddBuff("yanqingTalentTaunt", BuffType.Permanent, CommonAttribute.Taunt, ValueType.InstantNumber, -50,
                       (s, t, d) => { return s.buffs.Find(b => b.tag == "yanqingZhijianlianxin") != null; });
        self.AddBuff("yanqingTalentCrtRate", BuffType.Permanent, CommonAttribute.CriticalRate, ValueType.InstantNumber, talentCrtRate, 
            (s, t, d) => { return s.buffs.Find(b => b.tag == "yanqingZhijianlianxin") != null; });
        self.AddBuff("yanqingTalentCrtDmg", BuffType.Permanent, CommonAttribute.CriticalDamage, ValueType.InstantNumber, talentCrtDmg,
            (s, t, d) => { return s.buffs.Find(b => b.tag == "yanqingZhijianlianxin") != null; });
        var talentTrigger = new TriggerEvent<Character.TalentUponTarget>("talentAdditional", targets =>
        {
            if (!Utils.TwoRandom(.4f)) return;
            foreach (Enemy target in targets)
            {
                if (target == null)
                    continue;
                Damage d = Damage.NormalDamage(self, target, CommonAttribute.ATK, talentAtk1, new DamageConfig(DamageType.Additional, Element.Cryo));
                self.DealDamage(target, d);

                self.TestAndAddEffect(1, target, () =>
                {
                    target.AddState(self, new State(StateType.Frozen, 1));
                    target.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEvent>("yanqingTalentCryoDmg", () =>
                    {
                        Damage d = Damage.NormalDamage(self, target, CommonAttribute.ATK, talentAtk2, new DamageConfig(DamageType.Continue, Element.Cryo));
                        self.DealDamage(target, d);
                        return true;
                    }));
                });
            }
        });
        self.afterNormalAttack.Add(talentTrigger);
        self.afterSkill.Add(talentTrigger);
        self.afterBurst.Add(talentTrigger);
        self.afterTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("yanqingTalentRemove", (s, d) =>
        {
            self.RemoveBuff("yanqingZhijianlianxin", true);
            return d;
        }));
        if (self.config.abilityActivated[0])
        {
            self.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("yanqingAbility1", (t, d) =>
            {
                if(t is Enemy && (t as Enemy).weakPoint.Contains(Element.Cryo))
                {
                    Damage dmg = Damage.NormalDamage(self, t, CommonAttribute.ATK, .3f, new DamageConfig(DamageType.CoAttack, Element.Cryo));
                    self.DealDamage(t, dmg);
                }
                return d;
            }));
        }
        if (self.config.abilityActivated[1])
        {
            self.AddBuff("yanqingAbility2", BuffType.Permanent, CommonAttribute.EffectResist, ValueType.InstantNumber, .2f,
                (s, t, d) => { return s.buffs.Find(b => b.tag == "yanqingZhijianlianxin") != null; });
        }
        if (self.config.abilityActivated[2])
        {
            self.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("yanqingAbility3", (t, d) =>
            {
                if (d.isCritical)
                {
                    self.AddBuff("yanqingAbility3Speed", BuffType.Buff, CommonAttribute.Speed, ValueType.Percentage, .1f, 2);
                }
                return d;
            }));
        }

        if(self.constellaLevel >= 1)
        {
            self.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("yanqingC1CoAttack", (t, d) => {
                if (t.IsUnderState(StateType.Frozen))
                {
                    Damage dmg = Damage.NormalDamage(self, t, CommonAttribute.ATK, 1, new DamageConfig(DamageType.CoAttack, Element.Cryo));
                    self.DealDamage(t, dmg);
                    t.RemoveState(StateType.Frozen);
                }
                return d;
            }));
        }
        if(self.constellaLevel >= 2)
        {
            self.AddBuff("yanqingC2EnergyCharge", BuffType.Permanent, CommonAttribute.EnergyRecharge, ValueType.InstantNumber, .12f,
                (s, t, d) => { return s.buffs.Find(b => b.tag == "yanqingZhijianlianxin") != null; });
        }
        if(self.constellaLevel >= 4)
        {
            self.AddBuff("yanqingC4Cryo", BuffType.Permanent, CommonAttribute.CryoPenetrate, ValueType.InstantNumber, .2f);
        }
        if(self.constellaLevel >= 6)
        {
            self.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("yanqingC6", (t, d) => {
                if (t.hp < 0)
                {
                    Buff cr = self.buffs.Find(b => b.tag == "yanqingBurstCrtRate");
                    Buff cd = self.buffs.Find(b => b.tag == "yanqingBurstCrtDmg");
                    cr?.ChangeTurnTime(1);
                    cd?.ChangeTurnTime(1);
                }
                return d;
            }));
        }
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, atkDmg, new DamageConfig(DamageType.Attack, Element.Cryo));
        self.DealDamage(e, d);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, skillAtk, new DamageConfig(DamageType.Skill, Element.Cryo));
        self.DealDamage(e, d);
        self.AddBuff("yanqingZhijianlianxin", BuffType.Buff, CommonAttribute.Count, ValueType.Count, 0, 2);
        base.SkillEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        self.AddBuff("yanqingBurstCrtRate", BuffType.Buff, CommonAttribute.CriticalRate, ValueType.InstantNumber, burstCrtRate, 1);
        if(self.buffs.Find(b => b.tag == "yanqingZhijianlianxin") != null)
        {
            self.AddBuff("yanqingBurstCrtDmg", BuffType.Buff, CommonAttribute.CriticalDamage, ValueType.InstantNumber, burstCrtDmg, burstTurn);
        }
        Enemy e = enemies[0];
        Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, burstAtk, new DamageConfig(DamageType.Burst, Element.Cryo));
        self.DealDamage(e, d);
        base.BurstEnemyAction(enemies);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        self.AddBuff("yanqingMystery", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .3f,
            (s, t, d) => { return t.hp > t.GetFinalAttr(CommonAttribute.MaxHP) * .3f; }, 3);
    }
}
