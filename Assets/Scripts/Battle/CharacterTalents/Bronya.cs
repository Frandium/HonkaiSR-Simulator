using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bronya : ACharacterTalents
{
    public Bronya(Character _self): base(_self)
    {

    }
    bool talent_activated = false;
    public override void OnEquipping()
    {
        self.onNormalAttack.Add("talent", e =>
        {
            talent_activated = true;
        });
        self.onTurnEnd.Add("talent_inner", new TriggerEvent<Creature.TurnStartEndEvent>(() =>
        {
            if (talent_activated)
            {
                self.ChangePercentageLocation(15.0f);
                talent_activated = false;
            }
        }));
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.DamageCharacter(self, enemies[0], CommonAttribute.ATK, Element.Pyro, 50);
        self.DealDamage(enemies[0], Element.Pyro, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillCharacterAction(List<Character> characters)
    {
        Character c = characters[0];
        if (c.buffs.ContainsKey("bronyaBurstATK"))
        {
            Utils.valueBuffPool.ReturnOne(c.buffs["bronyaBurstATK"]);
            c.buffs.Remove("bronyaBurstATK");
        }
        c.ChangePercentageLocation(100);
        c.buffs.Add("bronyaSkill", Utils.valueBuffPool.GetOne().Set(BuffType.Buff, CommonAttribute.GeneralBonus, 1,
            (s, t) =>
            {
                return 0.36f;
            })
        );
        string toRemove = "";
        foreach(KeyValuePair<string, Buff> kv in characters[0].buffs)
        {
            if(kv.Value.buffType == BuffType.Debuff)
            {
                toRemove = kv.Key;
                break;
            }
        }
        c.buffs.Remove(toRemove);
        base.SkillCharacterAction(characters);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        foreach(Character c in characters)
        {
            if (c.buffs.ContainsKey("bronyaBurstATK"))
            {
                Utils.valueBuffPool.ReturnOne(c.buffs["bronyaBurstATK"]);
                c.buffs.Remove("bronyaBurstATK");
            }
            c.buffs.Add("bronyaBurstATK", Utils.valueBuffPool.GetOne().Set(BuffType.Buff, CommonAttribute.ATK, 2, (s, t) =>
            {
                return s.GetBaseAttr(CommonAttribute.ATK) * 0.36f;
            }));
            if (c.buffs.ContainsKey("bronyaBurstCrtDmg"))
            {
                Utils.valueBuffPool.ReturnOne(c.buffs["bronyaBurstCrtDmg"]);
                c.buffs.Remove("bronyaBurstCrtDmg");
            }
            c.buffs.Add("bronyaBurstCrtDmg", Utils.valueBuffPool.GetOne().Set(BuffType.Buff, CommonAttribute.CriticalDamage, 2, (s, t) =>
            {
                return 0.12f + self.GetBaseAttr(CommonAttribute.CriticalDamage) * 0.12f;
            }));
        }
        base.BurstCharacterAction(characters);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        foreach(Character c in characters)
        {
            c.buffs.Add("bronyaMystery", Utils.valueBuffPool.GetOne().Set(BuffType.Permanent, CommonAttribute.ATK, 2, (s, t) =>
            {
                return s.GetBaseAttr(CommonAttribute.ATK) * 0.15f;
            }));
        }
    }

}
