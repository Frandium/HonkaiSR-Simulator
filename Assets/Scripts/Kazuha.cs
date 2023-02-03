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
    { // 一定要先减抗再 take damage，不然就被反应掉了。
        switch (e.elementState)
        {
            case Element.Cryo:
                e.AddBuff(new ValueBuff(BuffType.Debuff, AttributeType.CryoResist, -.2f, 2));
                break;
            case Element.Hydro:
                e.AddBuff(new ValueBuff(BuffType.Debuff, AttributeType.HydroResist, -.2f, 2));
                break;
            default:
                break;
        }
    }

    public void AttackCharacterAction(List<Character> characters)
    {
    }

    public void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamage(self, Element.Physical, 88.9f);
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
        float dmg = DamageCal.ATKDamage(self, Element.Anemo, 472);
           enemies[0].AddBuff(new ValueBuff(BuffType.Debuff, AttributeType.CryoResist, -.2f, 2));
        for (int i = 0; i < enemies.Count; ++i)
        {
            ReduceResist(enemies[i]);
            enemies[i].TakeDamage(dmg, self, Element.Anemo);
//            enemies[i].AddBuff(new ValueBuff(BuffType.Debuff, AttributeType.CryoResist, -.2f, 2));
        }
        // ReduceResist(enemies[enemies.Count - 1]);
        // enemies[enemies.Count - 1].TakeDamage(dmg, self, Element.Anemo, self.bm.NextTurn);
        //
    }

    public void OnDying()
    {
    }

    public void SkillCharacterAction(List<Character> characters)
    {
    }

    public void SkillEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamage(self, Element.Anemo, 346);
        ReduceResist(enemies[0]);
        enemies[0].TakeDamage(dmg, self, Element.Anemo);
        self.ChargeEnergy(12.5f);
        self.bm.skillPoint.ConsumePoint(self.skillConsumePointCount);
    }

    public void OnTakingDamage(Creature source, float value)
    {
        self.ChargeEnergy(10);
    }

}
