using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Japard : ACharacterTalents
{
    public Japard(Character c): base(c)
    {

    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Cryo, 50, DamageType.Attack);
        self.DealDamage(e, Element.Cryo, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Cryo, 100, DamageType.Skill);
        self.DealDamage(e, Element.Cryo, DamageType.Skill, dmg);
        float p = .6f * (1 + self.GetFinalAttr(self, e, CommonAttribute.EffectHit, DamageType.Skill)) / (1 + e.GetFinalAttr(self, e, CommonAttribute.EffectResist, DamageType.Skill));
        if (Utils.TwoRandom(p))
        {
            // 冻结敌人
            e.AddState(self, new State(StateType.Frozen, 1));
            e.mono?.ShowMessage("冻结", Color.blue);
            e.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEndEvent>("japardFreeze", () =>
            {
                if (e.IsUnderState(StateType.Frozen))
                {
                    float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Cryo, 25, DamageType.Continue);
                    self.DealDamage(e, Element.Cryo, DamageType.Continue, dmg);
                }
            }));
        }
        base.SkillEnemyAction(enemies);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        float shield = .36f * self.GetFinalAttr(CommonAttribute.DEF) + 120;
        foreach(Character c in characters)
        {
            c.GetShield(new Shield("japardBurst", shield, 3));
        }
        base.BurstCharacterAction(characters);
    }

    public override void OnEquipping()
    {
        TriggerEvent<Creature.DamageEvent> t = new TriggerEvent<Creature.DamageEvent>("japardTalent");
        t.trigger = (s, v, e, dt) =>
        {
            if (self.hp - v <= 0)
            {
                self.hp = .25f * self.GetFinalAttr(CommonAttribute.MaxHP);
                self.mono.hpLine.fillAmount = self.mono.hpPercentage;
                self.mono?.ShowMessage("不屈之身", Color.blue);
                t.Zero();
                return 0;
            }
            return 0;
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
