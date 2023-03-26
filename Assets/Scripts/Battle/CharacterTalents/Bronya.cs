using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bronya : ACharacterTalents
{
    public Bronya(Character _self): base(_self)
    {

    }
    bool talent_activated = false;
    float atkdmg;
    float skilldmgUp;
    float burstAtkUp, burstCrtDmgPct, burstCrtDmgIns;
    float locationUp;
    bool isC1CD = false;
    int c1CD = 0;
    public override void OnEquipping()
    {
        if (self.constellaLevel >= 3)
        {
            self.BurstLevelUp(2);
            self.ATKLevelUp(1);
        }
        if (self.constellaLevel >= 5)
        {
            self.SkillLevelUp(2);
            self.TalentLevelUp(2);
        }
        atkdmg = (float)(double)self.metaData["atk"]["dmg"]["value"][self.atkLevel];
        skilldmgUp = (float)(double)self.metaData["skill"]["dmgUp"]["value"][self.skillLevel];
        burstAtkUp = (float)(double)self.metaData["burst"]["atkUp"]["value"][self.skillLevel];
        burstCrtDmgIns = (float)(double)self.metaData["burst"]["crtDmgIns"]["value"][self.burstLevel];
        burstCrtDmgPct = (float)(double)self.metaData["burst"]["crtDmgPct"]["value"][self.burstLevel];
        locationUp = (float)(double)self.metaData["talent"]["location"]["value"][self.talentLevel];
        self.afterNormalAttack.Add(new TriggerEvent<Character.TalentUponTarget>("talent", e =>
        {
            talent_activated = true;
            self.mono?.ShowMessage("行动提前", Color.green);
        }));
        self.onTurnEnd.Add(new TriggerEvent<Creature.TurnStartEndEvent>("talent_inner", () =>
        {
            if (talent_activated)
            {
                self.ChangePercentageLocation(locationUp);
                talent_activated = false;
            }
            if (isC1CD)
            {
                c1CD++;
                if (c1CD >= 2)
                    isC1CD = false;
            }
        }));
        if (self.constellaLevel >= 1)
        {
            self.afterSkill.Add(new TriggerEvent<Character.TalentUponTarget>("bronyaConstellation1", cs =>
            {
                Character c = cs[0] as Character;
                if (!isC1CD && Utils.TwoRandom(.5f))
                {
                    BattleManager.Instance.skillPoint.GainPoint(1);
                    isC1CD = true;
                }
                if (self.constellaLevel >= 2)
                {
                    c.onTurnEnd.Add(new TriggerEvent<Creature.TurnStartEndEvent>("bronyaConstallation2Trigger", () =>
                    {
                        c.AddBuff("bronyaConstallation2Speed", BuffType.Buff, CommonAttribute.Speed, ValueType.Percentage, .3f, 2);
                    }, 1));
                }
            }));
        }
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        if (self.config.abilityActivated[0])
        {
            self.AddBuff("tempCritDmg", BuffType.Buff, CommonAttribute.CriticalRate, ValueType.InstantNumber, 10, 1);
        }
        Damage dmg = Damage.NormalDamage(self, enemies[0], CommonAttribute.ATK, Element.Anemo, atkdmg, DamageType.Attack);
        self.RemoveBuff("tempCritDmg");
        self.DealDamage(enemies[0], dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillCharacterAction(List<Character> characters)
    {
        Character c = characters[0];
        c.ChangePercentageLocation(1);
        c.AddBuff("bronyaSkill", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, skilldmgUp, self.constellaLevel >= 6 ? 2 : 1);
        c.mono?.ShowMessage("行动提前", Color.black);
        Buff toRemove = c.buffs.Find(b => b.buffType == BuffType.Debuff);
        c.buffs.Remove(toRemove);
        base.SkillCharacterAction(characters);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        foreach(Character c in characters)
        {
            c.AddBuff("bronyaBurstATK", BuffType.Buff, CommonAttribute.ATK, ValueType.Percentage, burstAtkUp, 2);
            c.AddBuff(Utils.valueBuffPool.GetOne().Set("bronyaBurstCrtDmg", BuffType.Buff, CommonAttribute.CriticalDamage, (s, t, _) =>
            {
                return burstCrtDmgIns + self.GetBaseAttr(CommonAttribute.CriticalDamage) * burstCrtDmgPct;
            }, 2));
            c.mono?.ShowMessage("攻击提升", Color.green);
            c.mono?.ShowMessage("暴击伤害提升", Color.green);
        }
        base.BurstCharacterAction(characters);
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        foreach(Character c in characters)
        {
            c.AddBuff("bronyaMystery", BuffType.Buff, CommonAttribute.ATK, ValueType.Percentage, .15f, 2);
        }
    }

    public override void OnBattleStart(List<Character> characters)
    {
        foreach(Character c in characters)
        {
            if(self.config.abilityActivated[1])
                c.AddBuff("bronyaAbility2Def", BuffType.Buff, CommonAttribute.DEF, ValueType.Percentage, .2f, 2);
            if(self.config.abilityActivated[2])
            {
                c.AddBuff("bronyaAbility3Dmgup", BuffType.Permanent, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .1f);
            }
            if(self.constellaLevel >= 4 && c != self)
            {
                c.afterNormalAttack.Add(new TriggerEvent<Character.TalentUponTarget>("bronyaConstalltion4", t =>
                {
                    Enemy e = t[0] as Enemy;
                    if (e != null && e.weakPoint.Contains(Element.Anemo))
                    {
                        Damage dmg = Damage.NormalDamage(self, e, CommonAttribute.ATK, Element.Anemo, atkdmg, DamageType.Attack);
                        dmg.fullValue *= .8f;
                        dmg.type = DamageType.CoAttack;
                        e.TakeDamage(self, dmg);
                    }
                }));
            }
        }
        if (self.config.abilityActivated[2])
            self.onDying.Add(new TriggerEvent<Creature.DyingEvent>("bronyaAbility3", () =>
            {
                foreach (Character c in characters)
                {
                    c.RemoveBuff("bronyaAbility3Dmgup");
                }
            }));
    }


}
