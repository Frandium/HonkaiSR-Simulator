using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ACharacterTalents: ACommomBattleTalents
{
    protected Character self;

    public ACharacterTalents(Character _self)
    {
        self = _self;
    }

    public virtual void AttackCharacterAction(List<Character> characters)
    {
        BattleManager.Instance.skillPoint.GainPoint(self.attackGainPointCount);
        self.ChargeEnergy(self.attackGainEnergy);
    }

    public virtual void AttackEnemyAction(List<Enemy> enemies)
    {
        BattleManager.Instance.skillPoint.GainPoint(self.attackGainPointCount);
        self.ChargeEnergy(self.attackGainEnergy);
    }

    public virtual void SkillCharacterAction(List<Character> characters)
    {
        BattleManager.Instance.skillPoint.ConsumePoint(self.skillConsumePointCount);
        self.ChargeEnergy(self.skillGainEnergy);
    }

    public virtual void SkillEnemyAction(List<Enemy> enemies)
    {
        BattleManager.Instance.skillPoint.ConsumePoint(self.skillConsumePointCount);
        self.ChargeEnergy(self.skillGainEnergy);
    }

    public virtual void BurstCharacterAction(List<Character> characters)
    {
        self.ClearEnergy();
    }

    public virtual void BurstEnemyAction(List<Enemy> enemies)
    {
        self.ClearEnergy();
    }

    public override void OnTakingDamage(Creature source, float value, Element element, DamageType type)
    {
        self.ChargeEnergy(self.takeDmgGainEnergy);
        base.OnTakingDamage(source, value, element, type);
    }
}
