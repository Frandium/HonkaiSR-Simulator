using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shenhe : ACharacterTalents
{
    public Shenhe(Character _self): base(_self)
    {

    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Physical, 35.2f);
        BattleManager.Instance.DealDamage(self, enemies[0], Element.Physical, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Cryo, 80.5f);
        for (int i = 0; i < enemies.Count; ++i)
        {
            BattleManager.Instance.DealDamage(self, enemies[i], Element.Cryo, DamageType.Burst, dmg);
            enemies[i].AddBuff(BattleManager.Instance.valueBuffPool.GetOne().Set(BuffType.Debuff, ValueType.InstantNumber, (int)CommonAttribute.CryoResist, -.15f, 1));
            enemies[i].AddBuff(BattleManager.Instance.valueBuffPool.GetOne().Set(BuffType.Debuff, ValueType.InstantNumber, (int)CommonAttribute.PhysicalResist, -.15f, 1));
        }
        base.BurstEnemyAction(enemies);
    }

    public override void SkillCharacterAction(List<Character> characters)
    {
        characters[0].AddBuff(BattleManager.Instance.valueBuffPool.GetOne().Set(BuffType.Buff, ValueType.InstantNumber, (int)CharacterAttribute.CryoBonus, .2f, 2));
        characters[0].ChargeEnergy(10);
        if (characters[0].element == Element.Cryo)
        {
            characters[0].ChargeEnergy(5);
        }
        base.SkillCharacterAction(characters);
    }
}
