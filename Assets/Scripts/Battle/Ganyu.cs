using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ganyu : ACharacterTalents
{
    public Ganyu(Character _self) : base(_self)
    {

    }


    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Physical, 89.9f);
        BattleManager.Instance.DealDamage(self, enemies[0], Element.Physical, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        self.ClearEnergy();
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Cryo, 126 * 4);
        for (int i = 0; i < enemies.Count; ++ i)
        {
            BattleManager.Instance.DealDamage(self, enemies[i], Element.Cryo, DamageType.Burst, dmg);
        }
        base.BurstEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Cryo, 238);
        BattleManager.Instance.DealDamage(self, enemies[0], Element.Cryo, DamageType.Skill, dmg);
        self.AddBuff(BattleManager.Instance.valueBuffPool.GetOne().Set(BuffType.Buff, ValueType.InstantNumber, (int)CharacterAttribute.Taunt, -20, 2));
        base.SkillEnemyAction(enemies);
    }
}
