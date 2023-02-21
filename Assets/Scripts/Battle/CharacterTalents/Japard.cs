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
            e.onTurnStart.Add("japardFreeze", new TriggerEvent<Creature.TurnStartEndEvent>(() =>
            {
                // 如果被冻结
                float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Cryo, 25, DamageType.Continue);
                self.DealDamage(e, Element.Cryo, DamageType.Continue, dmg);
            }));
        }
        base.SkillEnemyAction(enemies);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {

        base.BurstCharacterAction(characters);
    }

    public override void OnEquipping()
    {
        TriggerEvent<Creature.DamageEvent> t = new TriggerEvent<Creature.DamageEvent>();
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
            return v;
        };
        self.onTakingDamage.Add("japardTalent", t);
    }

}
