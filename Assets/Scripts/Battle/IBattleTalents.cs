using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattleTalents
{
    public void AttackCharacterAction(List<Character> characters);

    public void AttackEnemyAction(List<Enemy> enemies);

    public void SkillCharacterAction(List<Character> characters);

    public void SkillEnemyAction(List<Enemy> enemies);

    public void BurstCharacterAction(List<Character> characters);

    public void BurstEnemyAction(List<Enemy> enemies);

    public void OnTakingDamage(Creature source, float value);

    public void OnDying();
}
