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
        self.ChangeEnergy(self.attackGainEnergy);
    }

    public virtual void AttackEnemyAction(List<Enemy> enemies)
    {
        BattleManager.Instance.skillPoint.GainPoint(self.attackGainPointCount);
        self.ChangeEnergy(self.attackGainEnergy);
    }

    public virtual void SkillCharacterAction(List<Character> characters)
    {
        BattleManager.Instance.skillPoint.ConsumePoint(self.skillConsumePointCount);
        self.ChangeEnergy(self.skillGainEnergy);
    }

    public virtual void SkillEnemyAction(List<Enemy> enemies)
    {
        BattleManager.Instance.skillPoint.ConsumePoint(self.skillConsumePointCount);
        self.ChangeEnergy(self.skillGainEnergy);
    }

    public virtual void BurstCharacterAction(List<Character> characters)
    {
        self.ChangeEnergy(-1000);
    }

    public virtual void BurstEnemyAction(List<Enemy> enemies)
    {
        self.ChangeEnergy(-1000);
    }

    public virtual void Mystery(List<Character> characters, List<Enemy> enemies)
    {

    }

    public virtual void OnEnemyRefresh(List<Enemy> enemies)
    {

    }

    public virtual void OnBattleStart(List<Character> characters)
    {

    }

    public virtual Summon Summon()
    {
        return null;
    } 

}
