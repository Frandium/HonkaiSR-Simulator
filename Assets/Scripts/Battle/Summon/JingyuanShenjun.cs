using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JingyuanShenjun : ASummonTalents
{
    public new Jingyuan summoner;
    public JingyuanShenjun(Summon self, Jingyuan talents) : base(self, talents)
    {

    }

    public override void MyTurn(List<Character> characters, List<Enemy> enemies)
    {
        if (summoner.character.config.abilityActivated[0])
        {
            summoner.character.AddBuff(Utils.valueBuffPool.GetOne().Set("jingyuanShenjunCrtDmg", BuffType.Permanent, CommonAttribute.CriticalDamage,
                (s, t, dt) => {
                    return summoner.shenjunAttackTimes * 10;
                })
            );
        }
        for (int i = 0; i < summoner.shenjunAttackTimes;  i++) {
            int j = Random.Range(0, enemies.Count);
            Damage d = Damage.NormalDamage(summoner.character, enemies[j], CommonAttribute.ATK, Element.Electro, summoner.talentAtk, DamageType.Additional);
            summoner.character.DealDamage(enemies[j], d); 
            if (summoner.character.constellaLevel >= 6)
            {
                enemies[j].AddBuff("jingyuanC6Hurt", BuffType.Buff, CommonAttribute.DmgDown, ValueType.InstantNumber, .12f, 1, maxStack: 3);
            }
            if (j - 1 >= 0)
            {
                Damage d1 = new Damage(d.fullValue * neighbourRate, d.element, d.type, d.isCritical);
                summoner.character.DealDamage(enemies[j - 1], d1);
            }
            if(j + 1 <enemies.Count) {
                Damage d2 = new Damage(d.fullValue * neighbourRate, d.element, d.type, d.isCritical);
                summoner.character.DealDamage(enemies[j + 1], d2);
            }
            if (summoner.character.constellaLevel >= 4)
            {
                summoner.character.ChangeEnergy(2);
                
            }
        }
        if (summoner.character.config.abilityActivated[0])
        {
            summoner.character.RemoveBuff("jingyuanShenjunCrtDmg");
        }
        summoner.shenjunAttackTimes = 3;
        if(summoner.character.constellaLevel >= 2)
        {
            summoner.character.AddBuff(Utils.valueBuffPool.GetOne().Set("jingyuanC2DmgUp", BuffType.Buff, CommonAttribute.GeneralBonus, (s, t, dt) => {
                if (dt == DamageType.Attack || dt == DamageType.Skill || dt == DamageType.Burst)
                    return .2f;
                return 0;
            }, 2));
        }
    }

    public override void BattleStart(List<Character> characters)
    {
        base.BattleStart(characters);
    }

    float neighbourRate = .25f;
    public override void OnEquipping()
    {
        summoner.shenjunAttackTimes = 3;
        self.AddBuff(Utils.valueBuffPool.GetOne().Set("jingyuanShenjunSpeed", BuffType.Permanent, CommonAttribute.Speed, 
            (s, t, dt) => {
                return summoner.shenjunAttackTimes * 10;
            })
        );
        if(summoner.character.constellaLevel >= 1)
        {
            neighbourRate += .25f;
        }
        self.onTurnStart.Add(new TriggerEvent<Creature.TurnStartEvent>("jingyuanShenjunCanMove", () =>
        {
            return summoner.character.IsUnderControlledDebuff();
        }));
    }
}
