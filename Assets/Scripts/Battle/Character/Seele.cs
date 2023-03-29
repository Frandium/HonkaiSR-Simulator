using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seele : ACharacterTalents
{
    public Seele(Character _self) : base(_self)
    {

    }

    bool addtionalTurn = false;
    float atkDmg, skillDmg, burstDmg, talentDmgUp;
    bool ability2Activated = false;

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
        skillDmg = (float)(double)self.metaData["skill"]["atk"]["value"][self.atkLevel];
        burstDmg = (float)(double)self.metaData["burst"]["atk"]["value"][self.atkLevel];
        talentDmgUp = (float)(double)self.metaData["talent"]["dmgUp"]["value"][self.atkLevel];
        self.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("seeleAdditionalTurn",
            (target, dmg) =>
        {
            if (target.hp <= 0)
            {
                if (self.config.constellaLevel >= 4)
                    self.ChangeEnergy(15);
                if (!addtionalTurn)
                {
                    BattleManager.Instance.runway.InsertAdditionalTurn(self);
                    self.mono.ShowMessage("额外回合", Color.cyan);
                    self.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEvent>("seeleAddTurn2",
                        () =>
                        {
                            Debug.Log("希儿额外回合开始");
                            self.AddBuff("seelUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, talentDmgUp, 1);
                            addtionalTurn = true;
                            self.onTurnEnd.Add(new TriggerEvent<Creature.TurnEndEvent>("seeleAddTurn3",
                                () =>
                                {
                                    Debug.Log("希儿额外回合结束");
                                    addtionalTurn = false;
                                }, 1)
                            );
                            return true;
                        }, 1));
                }
            }
            return dmg;
        }));
        if (self.config.abilityActivated[0])
        {
            self.AddBuff("seeleAbility1TauntDown", BuffType.Permanent, CommonAttribute.Taunt, ValueType.InstantNumber, -50, (s, t, dt) =>
            {
                return self.hp <= self.GetFinalAttr(CommonAttribute.MaxHP) / 2;
            });
        }
        if (self.config.abilityActivated[1])
        {
            self.afterNormalAttack.Add(new TriggerEvent<Character.TalentUponTarget>("talent", e =>
            {
                ability2Activated = true;
                self.mono?.ShowMessage("行动提前", Color.green);
            }));
            self.onTurnEnd.Add(new TriggerEvent<Creature.TurnEndEvent>("talent_inner", () =>
            {
                if (ability2Activated)
                {
                    self.ChangePercentageLocation(.2f);
                    ability2Activated = false;
                }
            }));
        }
        if(self.config.constellaLevel >= 2)
        {
            self.AddBuff("seeleConstellation2CriticalRate", BuffType.Permanent, CommonAttribute.CriticalRate, ValueType.InstantNumber, .15f,
                (s, t, dt) => { return t.hp <= t.GetFinalAttr(CommonAttribute.MaxHP) / 2; });
        }
    }
    

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        Damage dmg = Damage.NormalDamage(self, e, CommonAttribute.ATK, Element.Quantus, atkDmg, DamageType.Attack);
        self.DealDamage(e, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        Damage dmg = Damage.NormalDamage(self, e, CommonAttribute.ATK, Element.Quantus, skillDmg, DamageType.Skill);
        self.DealDamage(e, dmg);
        self.AddBuff("seeleSkillSpeed", BuffType.Buff, CommonAttribute.Speed, ValueType.Percentage, .25f, 3, maxStack: self.constellaLevel >= 1 ? 2 : 1);
        self.mono?.ShowMessage("速度提升", Color.blue);
        base.SkillEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        SeeleUp();
        Damage dmg = Damage.NormalDamage(self, e, CommonAttribute.ATK, Element.Quantus, burstDmg, DamageType.Burst);
        self.DealDamage(e, dmg);
        base.BurstEnemyAction(enemies);
        if (self.constellaLevel >= 6)
        {
            e.beforeTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("seeleConstellation6ExtraDamage", (s, d) => {
                if (d.type != DamageType.CoAttack)
                {
                    Damage dmg = Damage.NormalDamage(self, e, CommonAttribute.ATK, Element.Quantus, burstDmg, DamageType.Burst);
                    dmg.fullValue *= .18f;
                    dmg.type = DamageType.CoAttack;
                    e.TakeDamage(self, dmg);
                }
                return d;
            }, 1));
        }
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        SeeleUp();
    }

    void SeeleUp()
    {
        self.AddBuff("seeleUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, talentDmgUp, 1);
        if (self.config.abilityActivated[2])
        {
            self.AddBuff("seeleUpQuantusPenetrate", BuffType.Buff, CommonAttribute.QuantusPenetrate, ValueType.InstantNumber, .2f, 1);
        }
    }
}
