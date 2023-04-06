using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clara : ACharacterTalents
{
    public Clara(Character c) : base(c)
    {

    }

    float attackAtk, skillAtk1, skillAtk2, burstDmgDown, burstRate, talentAtk;
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
        burstDmgDown = (float)(double)self.metaData["burst"]["burstDmgDown"]["value"][self.burstLevel];
        burstRate = (float)(double)self.metaData["burst"]["burstRate"]["value"][self.burstLevel];
        talentAtk = (float)(double)self.metaData["talent"]["talentAtk"]["value"][self.talentLevel];

        self.AddBuff("claraTalentDmgDown", BuffType.Permanent, CommonAttribute.DmgDown, ValueType.InstantNumber, .15f);

        DamageConfig dc = new DamageConfig(DamageType.All, Element.Physical);

        self.beforeTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("claraAbility1", (s, d) =>
        {
            if (Utils.TwoRandom(.35f))
            {
                Buff toremove = self.buffs.Find(b => b.buffType == BuffType.Debuff);
                self.RemoveBuff(toremove);
            }
            return d;
        }));
        self.afterTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("claraRevenge", (s, d) =>
        {
            s.AddBuff("claraRevenge", BuffType.Debuff, CommonAttribute.Count, null, null);
            float rate = talentAtk;
            if(isRevengeEmpowered > 0)
            {
                rate += burstRate;
            }
            if (self.config.abilityActivated[2])
            {
                self.AddBuff("claraAbility3", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .3f);
            }
            Damage dmg = Damage.NormalDamage(self, s, CommonAttribute.ATK, rate, dc);
            self.DealDamage(s, dmg);
            if (isRevengeEmpowered > 0)
            {
                isRevengeEmpowered--;
                int idx = BattleManager.Instance.enemies.FindIndex(e => e == s);
                if (idx - 1 >= 0)
                {
                    Enemy e = BattleManager.Instance.enemies[idx - 1];
                    Damage dmg2 = Damage.NormalDamage(self, e, CommonAttribute.ATK, rate * .5f, dc);
                    self.DealDamage(e, dmg2);
                }
                if (idx + 1 < BattleManager.Instance.enemies.Count)
                {
                    Enemy e = BattleManager.Instance.enemies[idx + 1];
                    Damage dmg2 = Damage.NormalDamage(self, e, CommonAttribute.ATK, rate * .5f, dc);
                    self.DealDamage(e, dmg2);
                }
            }
            if (self.config.abilityActivated[2])
            {
                self.RemoveBuff("claraAbility3");
            }
            if(self.constellaLevel >= 4)
            {
                self.AddBuff("claraC4DmgDown", BuffType.Buff, CommonAttribute.DmgDown, ValueType.InstantNumber, .3f);
            }
            return d;
        }));

        if (self.config.abilityActivated[1])
        {
            self.AddBuff("claraAbiltiy2", BuffType.Permanent, CommonAttribute.EffectResist, ValueType.InstantNumber, .35f, (s, t, dc) =>
            {
                return dc.state == StateType.Restricted || 
                dc.state == StateType.Frozen || 
                dc.state == StateType.Entangle;
            });
        }

        if (self.constellaLevel >= 4)
        {
            self.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEvent>("claraC4RemoveBuff", () =>
            {
                self.RemoveBuff("claraC4DmgDown");
                return true;
            }));
        }
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, attackAtk, new DamageConfig(DamageType.Attack, Element.Physical));
        self.DealDamage(e, d);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        DamageConfig dc = new DamageConfig(DamageType.Skill, Element.Physical);
        foreach(Enemy e in enemies)
        {
            bool isRevenge = e.buffs.Find(b => b.tag == "claraRevenge") != null;
            Damage d = Damage.NormalDamage(self, e, CommonAttribute.ATK, skillAtk1 + (isRevenge ? skillAtk2 : 0), dc);
            self.DealDamage(e, d);
            if (self.constellaLevel < 1)
            {
                e.RemoveBuff("claraRevenge");
            }
        }
        base.SkillEnemyAction(enemies);
    }

    int isRevengeEmpowered = 0;
    public override void BurstCharacterAction(List<Character> characters)
    {
        self.AddBuff("claraBurstDmgDown", BuffType.Buff, CommonAttribute.DmgDown, ValueType.InstantNumber, burstDmgDown, null, 3);
        self.AddBuff("claraBurstTaunt", BuffType.Buff, CommonAttribute.Taunt, ValueType.InstantNumber, 400, null, 3);
        isRevengeEmpowered = self.constellaLevel >= 6 ? 3 : 2;
        if(self.constellaLevel >= 2)
        {
            self.AddBuff("claraBurstAtk", BuffType.Buff, CommonAttribute.ATK, ValueType.Percentage, .3f, null, 3);
        }
        self.mono?.ShowMessage("减伤", CreatureMono.PhysicalColor);
        base.BurstCharacterAction(characters);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        self.AddBuff("claraMystery", BuffType.Buff, CommonAttribute.Taunt, ValueType.InstantNumber, 200, null, 2);
    }

    public override void OnBattleStart(List<Character> characters)
    {
        DamageConfig dc = new DamageConfig(DamageType.All, Element.Physical);
        foreach (var character in characters)
        {
            if (character == self)
                continue;

            character.afterTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("claraRevenge", (s, d) =>
            {
                if (self.config.abilityActivated[2])
                    self.AddBuff("claraAbility3", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .3f);
                
                if (self.constellaLevel >= 6 && Utils.TwoRandom(.5f))
                {
                    // 6 命，友军受到攻击后也有 50% 概率反击
                    s.AddBuff("claraRevenge", BuffType.Debuff, CommonAttribute.Count, null, null);
                    float rate = talentAtk;
                    Damage dmg = Damage.NormalDamage(self, s, CommonAttribute.ATK, rate, dc);
                    self.DealDamage(s, dmg);
                }
                else if (isRevengeEmpowered > 0) 
                { 
                    // 非 6 命，只有强化反击时才反击
                    isRevengeEmpowered--;
                    float rate = talentAtk + burstRate;
                    Damage dmg = Damage.NormalDamage(self, s, CommonAttribute.ATK, rate, dc);
                    self.DealDamage(s, dmg);
                    int idx = BattleManager.Instance.enemies.FindIndex(e => e == s);
                    if (idx - 1 >= 0)
                    {
                        Enemy e = BattleManager.Instance.enemies[idx - 1];
                        Damage dmg2 = Damage.NormalDamage(self, e, CommonAttribute.ATK, rate * .5f, dc);
                        self.DealDamage(e, dmg2);
                    }
                    if (idx + 1 < BattleManager.Instance.enemies.Count)
                    {
                        Enemy e = BattleManager.Instance.enemies[idx + 1];
                        Damage dmg2 = Damage.NormalDamage(self, e, CommonAttribute.ATK, rate * .5f, dc);
                        self.DealDamage(e, dmg2);
                    }
                }

                if (self.config.abilityActivated[2])
                    self.RemoveBuff("claraAbility3");
                return d;
            }));

        }
    }

}