using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class Jingyuan : ACharacterTalents
{
    public Jingyuan(Character self): base(self)
    {
    }


    public float dmg, skillAtk, burstAtk, talentAtk;
    public int shenjunAttackTimes = 0;

    public Character character { get { return self; } }
    public override void OnEquipping()
    {
        dmg = (float)(double)self.metaData["atk"]["dmg"]["value"][self.atkLevel];
        skillAtk = (float)(double)self.metaData["skill"]["skillAtk"]["value"][self.skillLevel];
        burstAtk = (float)(double)self.metaData["burst"]["burstAtk"]["value"][self.burstLevel];
        talentAtk = (float)(double)self.metaData["talent"]["talentAtk"]["value"][self.talentLevel];
        shenjunAttackTimes = 3;

        if(self.constellaLevel >= 3)
        {
            self.BurstLevelUp(2);
            self.ATKLevelUp(1);
            if(self.constellaLevel >= 5)
            {
                self.SkillLevelUp(2);
                self.TalentLevelUp(2);
            }
        }
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Enemy enemy = enemies[0];
        Damage d = Damage.NormalDamage(self, enemy, CommonAttribute.ATK, Element.Electro, dmg, DamageType.Attack);
        self.DealDamage(enemy, d);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillEnemyAction(List<Enemy> enemies)
    {
        foreach (Enemy enemy in enemies) { 
            Damage d = Damage.NormalDamage(self, enemy, CommonAttribute.ATK, Element.Electro, skillAtk, DamageType.Attack);
            self.DealDamage(enemy, d);
        }
        shenjunAttackTimes += 2;
        if (self.config.abilityActivated[2])
        {
            self.AddBuff("jingyuanAbility3CrtRate", BuffType.Buff, CommonAttribute.CriticalRate, ValueType.InstantNumber, .1f, 2);
        }
        base.SkillEnemyAction(enemies);
    }

    public override void BurstEnemyAction(List<Enemy> enemies)
    {
        foreach(Enemy enemy in enemies)
        {
            Damage d = Damage.NormalDamage(self, enemy, CommonAttribute.ATK, Element.Electro, burstAtk, DamageType.Attack);
            self.DealDamage(enemy, d);
        }
        shenjunAttackTimes += 3;
        base.BurstEnemyAction(enemies);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        shenjunAttackTimes += 2;
    }

    Summon shenjun;
    public override void OnBattleStart(List<Character> characters)
    {
        if (self.config.abilityActivated[1])
            self.ChangeEnergy(15);
        base.OnBattleStart(characters);
    }

    public override Summon Summon()
    {
        shenjun = new Summon("jingyuanShenjun", self);
        self.onDying.Add(new TriggerEvent<Creature.DyingEvent>("jingyuanDyingRemoveShenjun", () =>
        {
            BattleManager.Instance.RemoveSummon(shenjun);
        }));
        return shenjun;
    }


}
