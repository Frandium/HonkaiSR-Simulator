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
        self.onDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("seeleAdditionalTurn",
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
                    self.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEndEvent>("seeleAddTurn2",
                        () =>
                        {
                            Debug.Log("希儿额外回合开始");
                            self.AddBuff("seelUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, talentDmgUp, 1);
                            addtionalTurn = true;
                            self.onTurnEnd.Add(new TriggerEvent<Creature.TurnStartEndEvent>("seeleAddTurn3",
                                () =>
                                {
                                    Debug.Log("希儿额外回合结束");
                                    addtionalTurn = false;
                                }, 1)
                            );
                        }, 1));
                }
            }
            return dmg;
        }));
        if (self.config.abilityActivated[0])
        {
            self.AddBuff(Utils.valueBuffPool.GetOne().Set("seeleAbility1TauntDown", BuffType.Permanent, CommonAttribute.Taunt, (s, t, dt) =>
            {
                if (self.hp <= self.GetFinalAttr(CommonAttribute.MaxHP) / 2)
                    return -50;
                return 0;
            }));
        }
        if (self.config.abilityActivated[1])
        {
            self.afterNormalAttack.Add(new TriggerEvent<Character.TalentUponTarget>("talent", e =>
            {
                ability2Activated = true;
                self.mono?.ShowMessage("行动提前", Color.green);
            }));
            self.onTurnEnd.Add(new TriggerEvent<Creature.TurnStartEndEvent>("talent_inner", () =>
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
            self.AddBuff(Utils.valueBuffPool.GetOne().Set("seeleConstellation2CriticalRate", BuffType.Permanent, CommonAttribute.CriticalRate, (s, t, dt)=> {
                if(t.hp <= t.GetFinalAttr(CommonAttribute.MaxHP) / 2)
                {
                    return .15f;
                }
                return 0;
            }));
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
            e.onTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("seeleConstellation6ExtraDamage", (s, d) => {
                Damage dmg = Damage.NormalDamage(self, e, CommonAttribute.ATK, Element.Quantus, burstDmg, DamageType.Burst);
                dmg.value *= .18f;
                e.TakeDamage(self, dmg);
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
