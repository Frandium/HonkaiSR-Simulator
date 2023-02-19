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
        self.onTurnEnd.Add("talent_inner", () =>
        {
            if (talent_activated)
            {
                self.ChangePercentageLocation(15.0f);
                talent_activated = false;
            }
        });
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        float dmg = DamageCal.DamageCharacter(self, enemies[0], CommonAttribute.ATK, Element.Anemo, 50);
        self.DealDamage(enemies[0], Element.Anemo, DamageType.Attack, dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillCharacterAction(List<Character> characters)
    {
        characters[0].ChangePercentageLocation(100);
        characters[0].buffs.Add("bronyaSkill", Utils.valueBuffPool.GetOne().Set(BuffType.Buff, CommonAttribute.GeneralBonus, 1,
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
        characters[0].buffs.Remove(toRemove);
        base.SkillCharacterAction(characters);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        foreach(Character c in characters)
        {
            c.buffs.Add("bronyaBurstATK", Utils.valueBuffPool.GetOne().Set(BuffType.Buff, CommonAttribute.ATK, 2, (s, t) =>
            {
                return s.GetBaseAttr(CommonAttribute.ATK) * 0.36f;
            }));
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
            c.buffs.Add("bronyaMystery", Utils.valueBuffPool.GetOne().Set(BuffType.Buff, CommonAttribute.ATK, 2, (s, t) =>
            {
                return s.GetBaseAttr(CommonAttribute.ATK) * 0.15f;
            }));
        }
    }

}
