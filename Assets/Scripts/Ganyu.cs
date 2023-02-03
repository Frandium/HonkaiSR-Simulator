using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ganyu : IBattleTalents
{
    Character self;

    public Ganyu(Character _self)
    {
        self = _self;
    }

    public void AttackCharacterAction(List<Character> characters)
    {
    }

    public void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamage(self, Element.Physical, 89.9f);
        enemies[0].TakeDamage(dmg, self, Element.Physical);
        self.bm.skillPoint.GainPoint(self.attackGainPointCount);
        self.ChargeEnergy(2.5f);
    }

    public void BurstCharacterAction(List<Character> characters)
    {
    }

    public void BurstEnemyAction(List<Enemy> enemies)
    {
        self.ClearEnergy();
        float dmg = DamageCal.ATKDamage(self, Element.Cryo, 126 * 4);
        for (int i = 0; i < enemies.Count; ++ i)
        {
            enemies[i].TakeDamage(dmg, self, Element.Cryo);
        }
//        enemies[enemies.Count - 1].TakeDamage(dmg, self, Element.Cryo, self.bm.NextTurn);
    }

    public void OnDying()
    {
    }

    public void SkillCharacterAction(List<Character> characters)
    {

    }

    public void SkillEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamage(self, Element.Cryo, 238);
        enemies[0].TakeDamage(dmg, self, Element.Cryo);
        float heal = DamageCal.MaxHPHeal(self, 2.5f, 428);
        self.TakeHeal(heal, self, null);
        self.bm.skillPoint.ConsumePoint(self.skillConsumePointCount);
        self.ChargeEnergy(10);
    }

    void IBattleTalents.OnTakingDamage(Creature source, float value)
    {
        self.ChargeEnergy(10);
    }
}
