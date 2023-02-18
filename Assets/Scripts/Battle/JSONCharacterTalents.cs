using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class JSONCharacterTalents : ACharacterTalents
{
    public JSONCharacterTalents(CharacterBase _self): base(_self)
    {

    }

    public void ResolveJSON(List<Dictionary<string, float>> actions, DamageType dt, List<CreatureBase> creatures)
    {
        //foreach (Dictionary<string, float> action in actions)
        //{
        //    int actionType = (int)action["actionType"];
        //    switch ((ActionType)actionType)
        //    {
        //        case ActionType.DealDamage:
        //            int attr = (int)action["valueBase"];
        //            float rate = action["rate"];
        //            float offset = action["offset"];
        //            Element e = (Element)(int)action["element"];
        //            //float dmg = DamageCal.DamageCharacter(self, (CommonAttribute)attr, e, rate);
        //            foreach (CreatureBase c in creatures)
        //            {
        //               // self.DealDamage(c, e, dt, dmg);
        //            }
        //            break;
        //        case ActionType.DealHeal:
        //            attr = (int)action["valueBase"];
        //            rate = action["rate"];
        //            offset = action["offset"];
        //            //dmg = DamageCal.Heal(self, (CommonAttribute)attr, rate, offset);
        //            foreach (CreatureBase c in creatures)
        //            {
        //                //c.TakeHeal(dmg, self);
        //            }
        //            break;
        //        case ActionType.DealElement:
        //            e = (Element)(int)action["element"];
        //            foreach (CreatureBase c in creatures)
        //            {
        //                c.TakeElementOnly(self, e);
        //            }
        //            break;
        //        case ActionType.AddBuff:
        //            BuffType b = (BuffType)(int)action["buffType"];
        //            attr = (int)action["attribute"];
        //            ValueType vt = (ValueType)(int)action["valueType"];
        //            float value = (float)action["value"];
        //            int duration = (int)action["duration"];
        //            foreach (CreatureBase c in creatures)
        //            {
        //                //c.AddBuff(valueBuffPool.GetOne().Set(b, vt, attr, value, duration));
        //            }
        //            break;
        //    }
        //}
    }

    public override void AttackCharacterAction(List<CharacterBase> characters)
    {
        //List<CreatureBase> creatures = new List<CreatureBase>(characters);
        //ResolveJSON(self.attackActionSeries, DamageType.Attack, creatures);
        //base.AttackCharacterAction(characters);
    }

    public override void AttackEnemyAction(List<EnemyBase> enemies)
    {
        //List<CreatureBase> creatures = new List<CreatureBase>(enemies);
        //ResolveJSON(self.attackActionSeries, DamageType.Attack, creatures);
        //base.AttackEnemyAction(enemies);
    }

    public override void SkillCharacterAction(List<CharacterBase> characters)
    {
        //List<CreatureBase> creatures = new List<CreatureBase>(characters);
        //ResolveJSON(self.skillActionSeries, DamageType.Skill, creatures);
        //base.SkillCharacterAction(characters);
    }

    public override void SkillEnemyAction(List<EnemyBase> enemies)
    {
        //List<CreatureBase> creatures = new List<CreatureBase>(enemies);
        //ResolveJSON(self.skillActionSeries, DamageType.Skill, creatures);
        //base.SkillEnemyAction(enemies);
    }

    public override void BurstCharacterAction(List<CharacterBase> characters)
    {
        //List<CreatureBase> creatures = new List<CreatureBase>(characters);
        //ResolveJSON(self.BurstActionSeries, DamageType.Burst, creatures);
        //base.BurstCharacterAction(characters);
    }

    public override void BurstEnemyAction(List<EnemyBase> enemies)
    {
        //List<CreatureBase> creatures = new List<CreatureBase>(enemies);
        //ResolveJSON(self.BurstActionSeries, DamageType.Burst, creatures);
        //base.BurstEnemyAction(enemies);
    }
}
