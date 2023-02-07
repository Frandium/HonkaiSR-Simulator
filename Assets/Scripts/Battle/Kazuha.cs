using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kazuha : ACharacterTalents
{    public Kazuha(Character _self): base(_self)
    {

    }

    public override void OnDealingDamage(Creature target, float value, Element element, DamageType type)
    {
        if (element != Element.Anemo)
            return;
        if(target.elementState == Element.Hydro || target.elementState == Element.Pyro || target.elementState == Element.Cryo || target.elementState == Element.Electro)
        {
            target.AddBuff(new ValueBuff(BuffType.Debuff, ValueType.InstantNumber, (int)CommonAttribute.AnemoResist + (int)target.elementState, -.2f, 2));
        }
        base.OnDealingDamage(target, value, element, type);
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Physical, 88.9f);
        BattleManager.Instance.DealDamage(self, enemies[0], Element.Physical, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Anemo, 472);
        for (int i = 0; i < enemies.Count; ++i)
        {
            BattleManager.Instance.DealDamage(self, enemies[i], Element.Anemo, DamageType.Burst, dmg);
        }
        base.BurstEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Anemo, 346);
        BattleManager.Instance.DealDamage(self, enemies[0], Element.Anemo, DamageType.Skill, dmg);
        base.SkillEnemyAction(enemies);
    }
}
