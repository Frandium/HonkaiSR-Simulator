using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Bailu : ACharacterTalents
{
    public Bailu(Character _self): base(_self)
    {
    }

    float atkdmg, skillMaxHp, skillHpOffset, burstMaxHp, burstHpOffset, talentMaxHp1, talentMaxHp2, talentHpOffset1, talentHpOffset2;
    public override void OnEquipping()
    {
        if (self.constellaLevel >= 5)
        {
            self.SkillLevelUp(2);
            self.ATKLevelUp(1);
        }
        if (self.constellaLevel >= 3)
        {
            self.BurstLevelUp(2);
            self.TalentLevelUp(2);
        }
        atkdmg = (float)(double)self.metaData["atk"]["dmg"]["value"][self.atkLevel];
        skillMaxHp = (float)(double)self.metaData["skill"]["skillMaxHp"]["value"][self.skillLevel];
        skillHpOffset = (float)(double)self.metaData["skill"]["skillHpOffset"]["value"][self.skillLevel];
        burstMaxHp = (float)(double)self.metaData["burst"]["burstMaxHp"]["value"][self.burstLevel];
        burstHpOffset = (float)(double)self.metaData["burst"]["burstHpOffset"]["value"][self.burstLevel];
        talentMaxHp1 = (float)(double)self.metaData["talent"]["talentMaxHp1"]["value"][self.talentLevel];
        talentMaxHp2 = (float)(double)self.metaData["talent"]["talentMaxHp2"]["value"][self.talentLevel];
        talentHpOffset1 = (float)(double)self.metaData["talent"]["talentHpOffset1"]["value"][self.talentLevel];
        talentHpOffset2 = (float)(double)self.metaData["talent"]["talentHpOffset2"]["value"][self.talentLevel];
    }

    public override void AttackEnemyAction(List<Enemy> enemies)
    {
        Damage dmg = Damage.NormalDamage(self, enemies[0], CommonAttribute.ATK, atkdmg, new DamageConfig(DamageType.Attack, Element.Electro));
        self.DealDamage(enemies[0], dmg);
        base.AttackEnemyAction(enemies);
    }

    public override void SkillCharacterAction(List<Character> characters)
    {
        Character mainTarget = characters[0];
        Heal h = Heal.NormalHeal(self, mainTarget, CommonAttribute.MaxHP, skillMaxHp, skillHpOffset);
        self.DealHeal(mainTarget, h);
        List<Character> others = new (BattleManager.Instance.characters);
        others.Remove(mainTarget);
        if (others.Count > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                int j = Random.Range(0, others.Count);
                self.DealHeal(others[j], new Heal(h.fullValue * Mathf.Pow(.85f, i + 1)));
                if (self.constellaLevel >= 4)
                {
                    others[j].AddBuff("bailuConstellation3DmgUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, .1f, 2, maxStack: 3);
                }
            }
        }
        base.SkillCharacterAction(characters);
    }

    public override void BurstCharacterAction(List<Character> characters)
    {
        foreach(Character character in characters)
        {
            Heal h = Heal.NormalHeal(self, character, CommonAttribute.MaxHP, burstMaxHp, burstHpOffset);
            self.DealHeal(character, h);
            var shengxi = character.buffs.Find(b => b.tag == "bailuShengxi");
            if (shengxi != null)
            {
                shengxi.ChangeTurnTime(1);
            }
            else
            {
                GetShengxi(character, 2);
            }
        }
        base.BurstCharacterAction(characters);
    }


    static int shengxiCure = 0;
    public override void OnBattleStart(List<Character> characters)
    {
        foreach(Character character in characters)
        {
            Character thisone = character;
            thisone.afterTakingDamage.Add(new TriggerEvent<Creature.DamageEvent>("bailuShengxiHeal", (s, d) =>
            {
                if (thisone.buffs.Find(b => b.tag == "bailuShengxi") != null)
                {
                    Heal h2 = Heal.NormalHeal(self, thisone, CommonAttribute.MaxHP, talentMaxHp1, talentHpOffset1);
                    self.DealHeal(thisone, h2);
                    if (self.config.abilityActivated[1])
                    {
                        h2 = Heal.NormalHeal(self, thisone, CommonAttribute.MaxHP, talentMaxHp1, talentHpOffset1);
                        self.DealHeal(thisone, h2);
                    }
                }
                return d;
            }));
            thisone.beforeDying.Add(new TriggerEvent<Creature.DamageEvent>("bailuReborn", (s, d) => {
                if (thisone.hp <= 0 && shengxiCure <= (self.constellaLevel >= 6 ? 2 : 1))
                {
                    Heal h = Heal.NormalHeal(self, thisone, CommonAttribute.MaxHP, talentMaxHp2, talentHpOffset2);
                    self.DealHeal(thisone, h);
                    thisone.mono?.ShowMessage("奔走悬壶济世长", Color.blue);
                    shengxiCure++;
                }
                return d;
            }));
            if (self.config.abilityActivated[2])
            {
                thisone.AddBuff("bailuShengxiDmgDown", BuffType.Permanent, CommonAttribute.DmgDown, ValueType.InstantNumber, .1f, (s, t, d) => {
                    return thisone.buffs.Find(t => t.tag == "bailuShengxi") != null;
                });
            }
        }
        self.afterDealingHeal.Add(new TriggerEvent<Creature.HealEvent>("bailuAbility1", (t, h) =>
        {
            if(h.realValue < h.fullValue)
            {
                t.AddBuff("bailuAbility1MaxHpUp", BuffType.Buff, CommonAttribute.MaxHP, ValueType.Percentage, .1f, 2);
            }
            return h;
        }));

        if (self.constellaLevel >= 2)
        {
            self.afterBurst.Add(new TriggerEvent<Character.TalentUponTarget>("bailuConstellation2", (targets) =>
            {
                self.AddBuff("bailuConstellation2", BuffType.Buff, CommonAttribute.HealBonus, ValueType.InstantNumber, .15f, 2);
            }));
        }
    }

    public override void Mystery(List<Character> characters, List<Enemy> enemies)
    {
        foreach(Character character in characters)
        {
            GetShengxi(character, 1);
        }
    }

    void GetShengxi(Character c, int turn)
    {
        c.AddBuff("bailuShengxi", BuffType.Buff, CommonAttribute.Count, (_, _, _) => { return 0; }, null, turn, onremove:
        (c) =>
        {
            Character cha = c as Character;
            if (cha != null && self.constellaLevel >= 1)
            {
                cha.ChangeEnergy(8);
            }
        });
    }
}
