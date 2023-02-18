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
        float dmg = DamageCal.DamageCharacter(self, enemies[0], CommonAttribute.ATK, Element.Physical, 89.9f);
        self.DealDamage(enemies[0], Element.Physical, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        self.ClearEnergy();
        float dmg = DamageCal.DamageCharacter(self, enemies[0], CommonAttribute.ATK, Element.Cryo, 126 * 4);
        for (int i = 0; i < enemies.Count; ++ i)
        {
            self.DealDamage(enemies[i], Element.Cryo, DamageType.Burst, dmg);
        }
        base.BurstEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.DamageCharacter(self, enemies[0], CommonAttribute.ATK, Element.Cryo, 238);
        self.DealDamage(enemies[0], Element.Cryo, DamageType.Skill, dmg);
        //self.AddBuff(valueBuffPool.GetOne().Set(BuffType.Buff, CommonAttribute.Taunt, 2, c => -20));
        base.SkillEnemyAction(enemies);
    }
}
