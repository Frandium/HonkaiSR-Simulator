using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JingyuanShenjun : ASummonTalents
{
    Jingyuan jingyuan;
    public JingyuanShenjun(Summon self, Creature summoner) : base(self, summoner)
    {
        jingyuan = summoner.talents as Jingyuan;
#if UNITY_EDITOR
        if(jingyuan == null)
        {
            Debug.LogError("错误的生物召唤了景元神君：" + summoner.dbname);
        }
#endif
    }

    public override void MyTurn(List<Character> characters, List<Enemy> enemies)
    {
        if (jingyuan.character.config.abilityActivated[0])
        {
            jingyuan.character.AddBuff("jingyuanShenjunCrtDmg", BuffType.Permanent, CommonAttribute.CriticalDamage, ValueType.InstantNumber, .2f, (s, t, dt) => { return jingyuan.shenjunAttackTimes >= 6; });
        }
        for (int i = 0; i < jingyuan.shenjunAttackTimes;  i++) {
            int j = Random.Range(0, enemies.Count);
            Damage d = Damage.NormalDamage(jingyuan.character, enemies[j], CommonAttribute.ATK,jingyuan.talentAtk, new DamageConfig(DamageType.Additional, Element.Electro));
            jingyuan.character.DealDamage(enemies[j], d); 
            if (jingyuan.character.constellaLevel >= 6)
            {
                enemies[j].AddBuff("jingyuanC6Hurt", BuffType.Buff, CommonAttribute.DmgDown, ValueType.InstantNumber, .12f, 1, maxStack: 3);
            }
            if (j - 1 >= 0)
            {
                Damage d1 = new Damage(d.fullValue * neighbourRate, d.element, d.type, d.isCritical);
                jingyuan.character.DealDamage(enemies[j - 1], d1);
            }
            if(j + 1 <enemies.Count) {
                Damage d2 = new Damage(d.fullValue * neighbourRate, d.element, d.type, d.isCritical);
                jingyuan.character.DealDamage(enemies[j + 1], d2);
            }
            if (jingyuan.character.constellaLevel >= 4)
            {
                jingyuan.character.ChangeEnergy(2);
                
            }
        }
        if (jingyuan.character.config.abilityActivated[0])
        {
            jingyuan.character.RemoveBuff("jingyuanShenjunCrtDmg");
        }
        jingyuan.shenjunAttackTimes = 3;
        if(jingyuan.character.constellaLevel >= 2)
        {
            jingyuan.character.AddBuff("jingyuanC2DmgUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .2f, 
                (s,t,dt)=> { return dt.type == DamageType.Attack || dt.type == DamageType.Skill || dt.type == DamageType.Burst;  }, 2);
        }
    }

    public override void BattleStart(List<Character> characters)
    {
        base.BattleStart(characters);
    }

    float neighbourRate = .25f;
    public override void OnEquipping()
    {
        jingyuan.shenjunAttackTimes = 3;

        self.AddBuff("jingyuanShenjunSpeed", BuffType.Permanent, CommonAttribute.Speed,
            (s, t, dt) => {
                return jingyuan.shenjunAttackTimes * 10;
            }, null
        );
        if(jingyuan.character.constellaLevel >= 1)
        {
            neighbourRate += .25f;
        }
        self.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEvent>("jingyuanShenjunCanMove", () =>
        {
            return jingyuan.character.IsUnderControlledState();
        }));
    }
}
