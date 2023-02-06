using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kokomi : IBattleTalents
{
    Character self;

    public Kokomi(Character _self)
    {
        self = _self;
    }

    public void AttackCharacterAction(List<Character> characters)
    {
    }

    public void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.ATKDamageCharacter(self, Element.Physical, 62);
        enemies[0].TakeDamage(dmg, self, Element.Physical);
        BattleManager.Instance.skillPoint.GainPoint(self.attackGainPointCount);
    }

    public void BurstCharacterAction(List<Character> characters)
    {
        self.ClearEnergy();
        float heal = DamageCal.MaxHPHeal(self, 20.1f, 1692);
//        characters[0].TakeHeal(heal, self, BattleManager.Instance.NextTurn);
        for(int i = 0; i < characters.Count; ++i)
        {
            characters[i].TakeHeal(heal, self);
        }
    }

    public void BurstEnemyAction(List<Enemy> enemies)
    {
    }

    public void OnDying()
    {
    }

    public void SkillCharacterAction(List<Character> characters)
    {
        float heal = DamageCal.MaxHPHeal(self, 7.5f, 862);
        characters[0].TakeHeal(heal, self);
        characters[0].TakeElementOnly(self, Element.Hydro);
        BattleManager.Instance.skillPoint.ConsumePoint(self.skillConsumePointCount);
        self.ChargeEnergy(18.5f);
    }

    public void SkillEnemyAction(List<Enemy> enemies)
    {
    }

    void IBattleTalents.OnTakingDamage(Creature source, float value, Element element)
    {
        self.ChargeEnergy(10);
    }
}
