using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ACharacterTalents
{
    protected CharacterBase self;

    public ACharacterTalents(CharacterBase _self)
    {
        self = _self;
    }

    public virtual void OnEquipping()
    {

    }

    public virtual void AttackCharacterAction(List<CharacterBase> characters)
    {
        BattleManager.Instance.skillPoint.GainPoint(self.attackGainPointCount);
        self.ChangeEnergy(self.attackGainEnergy);
    }

    public virtual void AttackEnemyAction(List<EnemyBase> enemies)
    {
        BattleManager.Instance.skillPoint.GainPoint(self.attackGainPointCount);
        self.ChangeEnergy(self.attackGainEnergy);
    }

    public virtual void SkillCharacterAction(List<CharacterBase> characters)
    {
        BattleManager.Instance.skillPoint.ConsumePoint(self.skillConsumePointCount);
        self.ChangeEnergy(self.skillGainEnergy);
    }

    public virtual void SkillEnemyAction(List<EnemyBase> enemies)
    {
        BattleManager.Instance.skillPoint.ConsumePoint(self.skillConsumePointCount);
        self.ChangeEnergy(self.skillGainEnergy);
    }

    public virtual void BurstCharacterAction(List<CharacterBase> characters)
    {
        self.ChangeEnergy(-100);
    }

    public virtual void BurstEnemyAction(List<EnemyBase> enemies)
    {
        self.ChangeEnergy(-100);
    }

    public virtual void Mystery(List<CharacterBase> characters, List<EnemyBase> enemies)
    {

    }
}
