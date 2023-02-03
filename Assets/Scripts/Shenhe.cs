using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shenhe : IBattleTalents
{
    Character self;

    public Shenhe(Character _self)
    {
        self = _self;
    }

    public void AttackCharacterAction(List<Character> characters)
    {
    }

    public void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamage(self, Element.Physical, 35.2f);
        enemies[0].TakeDamage(dmg, self, Element.Physical);
        self.ChargeEnergy(4);
        self.bm.skillPoint.GainPoint(self.attackGainPointCount);
    }

    public void BurstCharacterAction(List<Character> characters)
    {
    }

    public void BurstEnemyAction(List<Enemy> enemies)
    {
        self.ClearEnergy();
        float dmg = DamageCal.ATKDamage(self, Element.Cryo, 80.5f);
        for (int i = 0; i < enemies.Count; ++i)
        {
            enemies[i].TakeDamage(dmg, self, Element.Cryo);
            enemies[i].AddBuff(new ValueBuff(BuffType.Debuff, AttributeType.CryoResist, -.15f, 1));
            enemies[i].AddBuff(new ValueBuff(BuffType.Debuff, AttributeType.PhysicalResist, -.15f, 1));
        }
        // enemies[0].TakeDamage(dmg, self, Element.Cryo, self.bm.NextTurn);
        // enemies[0].AddBuff(new ValueBuff(BuffType.Debuff, AttributeType.CryoResist, -.15f, 1));
        // enemies[0].AddBuff(new ValueBuff(BuffType.Debuff, AttributeType.PhysicalResist, -.15f, 1));
    }

    public void OnDying()
    {
    }

    public void SkillCharacterAction(List<Character> characters)
    {
        self.ChargeEnergy(10);
        characters[0].AddBuff(new ValueBuff(BuffType.Buff, AttributeType.CryoBonus, .2f, 2));
        characters[0].ChargeEnergy(10);
        if (characters[0].element == Element.Cryo)
        {
            characters[0].ChargeEnergy(5);
        }
        self.bm.skillPoint.ConsumePoint(1);
    }

    public void SkillEnemyAction(List<Enemy> enemies)
    {
    }

    void IBattleTalents.OnTakingDamage(Creature source, float value)
    {
        self.ChargeEnergy(10);
    }
}
