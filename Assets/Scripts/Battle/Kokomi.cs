using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kokomi : ACharacterTalents
{
    public Kokomi(Character _self): base(_self)
    {

    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.DamageCharacter(self, enemies[0], CommonAttribute.ATK, Element.Hydro, 62);
        BattleManager.Instance.DealDamage(self, enemies[0], Element.Hydro, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        float heal = DamageCal.Heal(self, characters[0], CommonAttribute.MaxHP, 20.1f, 1692);
        for(int i = 0; i < characters.Count; ++i)
        {
            characters[i].TakeHeal(heal, self);
        }
        base.BurstCharacterAction(characters);
    }

    public override void SkillCharacterAction(List<Character> characters)
    {
        foreach(Character c in characters)
        {
            float heal = DamageCal.Heal(self, c, CommonAttribute.MaxHP, 7.5f, 862);
            c.TakeHeal(heal, self);
            c.TakeElementOnly(self, Element.Hydro);
        }
        base.SkillCharacterAction(characters);
    }
}
