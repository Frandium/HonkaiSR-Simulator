using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Japard : ACharacterTalents
{
    public Japard(Character c): base(c)
    {

    }
    float atkDmg, skillAtk, skillFreeze, burstDefPer, burstDefIns, talentHp;

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Cryo, atkDmg, DamageType.Attack);
        self.DealDamage(e, Element.Cryo, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Cryo, skillAtk, DamageType.Skill);
        self.DealDamage(e, Element.Cryo, DamageType.Skill, dmg);
        float hit = .65f * (1 + self.GetFinalAttr(self, e, CommonAttribute.EffectHit, DamageType.Skill));
        float resist = 1 - 1 / (1 + e.GetFinalAttr(self, e, CommonAttribute.EffectResist, DamageType.Skill));
        if (Utils.TwoRandom(hit) && !Utils.TwoRandom(resist))
        {
            // �������
            e.AddState(self, new State(StateType.Frozen, 1));
            e.mono?.ShowMessage("����", Color.blue);
            if(e.onTurnStart.Find(t => t.tag == "japardFreeze") == null)
                e.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEndEvent>("japardFreeze", () =>
                {
                    if (e.IsUnderState(StateType.Frozen))
                    {
                        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Cryo, skillFreeze, DamageType.Continue);
                        self.DealDamage(e, Element.Cryo, DamageType.Continue, dmg);
                    }
                }));
        }
        base.SkillEnemyAction(enemies);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        float shield = burstDefPer * self.GetFinalAttr(CommonAttribute.DEF) + burstDefIns;
        foreach(Character c in characters)
        {
            c.GetShield(new Shield("japardBurst", shield, 3));
        }
        base.BurstCharacterAction(characters);
    }

    public override void OnEquipping()
    {
        atkDmg = (float)(double)self.config["atk"]["dmg"]["value"][self.atkLevel];
        skillAtk = (float)(double)self.config["skill"]["atk"]["value"][self.skillLevel];
        skillFreeze = (float)(double)self.config["skill"]["freeze"]["value"][self.skillLevel];
        burstDefPer = (float)(double)self.config["burst"]["defPer"]["value"][self.burstLevel];
        burstDefIns = (int)self.config["burst"]["defIns"]["value"][self.burstLevel];
        talentHp = (float)(double)self.config["talent"]["hp"]["value"][self.burstLevel];
        
        TriggerEvent<Creature.DamageEvent> t = new TriggerEvent<Creature.DamageEvent>("japardTalent");
        t.trigger = (s, v, e, dt) =>
        {
            if (self.hp - v <= 0)
            {
                self.hp = .25f * self.GetFinalAttr(CommonAttribute.MaxHP);
                self.mono.hpLine.fillAmount = self.mono.hpPercentage;
                self.mono?.ShowMessage("����֮��", Color.blue);
                t.Zero();
                return 0;
            }
            return v;
        };
        self.onTakingDamage.Add(t);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        foreach(Character c in characters)
        {
            c.AddBuff("japardMystery", BuffType.Buff, CommonAttribute.DEF, ValueType.InstantNumber, self.GetBaseAttr(CommonAttribute.DEF) * .25f, 2);
        }
    }

}
