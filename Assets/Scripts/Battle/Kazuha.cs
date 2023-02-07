using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kazuha : IBattleTalents
{
    Character self;

    public Kazuha(Character _self)
    {
        self = _self;
    }

    void ReduceResist(Enemy e)
    { 
        // 一定要先减抗再 take damage，不然就被反应掉了。
        e.AddBuff(new ValueBuff(BuffType.Debuff, ValueType.InstantNumber, (int)CommonAttribute.AnemoResist + (int)e.elementState , -.2f, 2));
    }

    public void AttackCharacterAction(List<Character> characters)
    {
    }

    public void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Physical, 88.9f);
        enemies[0].TakeDamage(dmg, self, Element.Physical);
        BattleManager.Instance.skillPoint.GainPoint(self.attackGainPointCount);
        self.ChargeEnergy(2.5f);
    }

    public void BurstCharacterAction(List<Character> characters)
    {
    }

    public void BurstEnemyAction(List<Enemy> enemies)
    {
        self.ClearEnergy();
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Anemo, 472);
        for (int i = 0; i < enemies.Count; ++i)
        {
            ReduceResist(enemies[i]);
            enemies[i].TakeDamage(dmg, self, Element.Anemo);
        }
    }

    public void OnDying()
    {
    }

    public void SkillCharacterAction(List<Character> characters)
    {
    }

    public void SkillEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Anemo, 346);
        ReduceResist(enemies[0]);
        enemies[0].TakeDamage(dmg, self, Element.Anemo);
        self.ChargeEnergy(12.5f);
        BattleManager.Instance.skillPoint.ConsumePoint(self.skillConsumePointCount);
    }

    public void OnTakingDamage(Creature source, float value, Element element)
    {
        self.ChargeEnergy(10);
    }

}
