using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seele : ACharacterTalents
{
    public Seele(Character _self) : base(_self)
    {

    }

    bool addtionalTurn = false;

    public override void OnEquipping()
    {
        self.onDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("seeleAdditionalTurn",
            (target, v, e, t) =>
        {
            if (!addtionalTurn && target.hp <= 0)
            {
                BattleManager.Instance.runway.InsertAdditionalTurn(self);
                self.mono.ShowMessage("额外回合", Color.cyan);
                self.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEndEvent>("seeleAddTurn2",
                    () =>
                    {
                        Debug.Log("希儿额外回合开始");
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
            return v;
        }));
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Quantus, 50, DamageType.Attack);
        self.DealDamage(e, Element.Quantus, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Quantus, 110, DamageType.Skill);
        self.DealDamage(e, Element.Quantus, DamageType.Skill, dmg);
        self.AddBuff("seeleSkillSpeed", BuffType.Buff, CommonAttribute.Speed, ValueType.Percentage, .25f, 2);
        base.SkillEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        Enemy e = enemies[0];
        self.AddBuff("seelUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .4f, 1);
        float dmg = DamageCal.NormalDamage(self, e, CommonAttribute.ATK, Element.Quantus, 240, DamageType.Burst);
        self.DealDamage(e, Element.Quantus, DamageType.Burst, dmg);
        base.BurstEnemyAction(enemies);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        self.AddBuff("seelUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .4f, 1);
    }
}
