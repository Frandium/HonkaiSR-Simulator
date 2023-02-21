using System.IO;
using UnityEngine;
using LitJson;
using System.Collections.Generic;

public class Character: Creature
{
    public string atkName = "普通攻击";
    public string atkDescription = "普通攻击伤害";
    public string skillName = "战技";
    public string skillDescription = "战技伤害";
    public string burstName = "终结技";
    public string burstDescription = "终结技伤害";
    public string talentName = "天赋";
    public string talentDescription = "天赋效果";
    public string mysteryName = "秘技";
    public string mysteryDescription = "秘技效果";

    protected int breakLevel = 4;
    public int constellation { get; protected set; } = 0; // 命之座
    public int atkLevel { get; protected set; } = 1; 
    public int skillLevel { get; protected set; } = 1; 
    public int burstLevel { get; protected set; } = 1; 

    public Element element { get; protected set; } = Element.Count;
    public float energy { get; protected set; } = 0;
    public float maxEnergy { get; protected set; } = 60;
    public ACharacterTalents talents { get; protected set; }
    public JsonData config { get; protected set; }
    public new CharacterMono mono { get; protected set; }
    public Weapon weapon;
    public List<AEquipmentTalents> artifactsSuit = new List<AEquipmentTalents>();
    public Artifact[] artifacts = new Artifact[(int)ArtifactPosition.Count];

    public bool isAttackTargetEnemy { get; protected set; } = true;
    public SelectionType attackSelectionType { get; protected set; } = SelectionType.One;
    public bool isSkillTargetEnemy { get; protected set; } = true;
    public SelectionType skillSelectionType { get; protected set; } = SelectionType.One;
    public bool isBurstTargetEnemy { get; protected set; } = true;
    public SelectionType burstSelectionType { get; protected set; } = SelectionType.All;
    public int attackGainPointCount { get; protected set; } = 1;
    public int skillConsumePointCount { get; protected set; } = 1;
    public float attackGainEnergy { get; protected set; } = 2.5f;
    public float skillGainEnergy { get; protected set; } = 2.5f;
    public float takeDmgGainEnergy { get; protected set; } = 2.5f;

    public delegate void TalentUponTarget(Creature target);
    public Dictionary<string, TalentUponTarget> onNormalAttack { get; protected set; } = new Dictionary<string, TalentUponTarget>();
    public Dictionary<string, TalentUponTarget> onSkill { get; protected set; } = new Dictionary<string, TalentUponTarget>();
    public Dictionary<string, TalentUponTarget> onBurst { get; protected set; } = new Dictionary<string, TalentUponTarget>();
    public Character() { }

    public Character(string _dbname)
    {
        LoadJson(_dbname);
    }

    public void LoadJson(string name)
    {
        // 先把之前的脱下来
        weapon?.OnTakingOff(this);
        foreach(AEquipmentTalents t in artifactsSuit)
        {
            t.OnTakingOff(this);
        }
        artifactsSuit.Clear();

        // Load 角色基本属性
        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/characters/" + name + ".json");
        config = JsonMapper.ToObject(jsonString);

        // set character template
        dbname = name;
        disname = (string)config["disname"];
        level = (int)config["level"];
        element = (Element)(int)config["element"];
        breakLevel = (int)config["breakLevel"];
        maxEnergy = (float)(double)config["maxEnergy"];
        constellation = (int)config["constellation"];
        atkLevel = (int)config["atkLevel"];
        skillLevel = (int)config["skillLevel"];
        burstLevel = (int)config["burstLevel"];
        for(int i = 0; i < (int)CommonAttribute.Speed; ++i)
        {
            // 这些数据每个有 14 条，分别是1级，10级未突破/突破……70级未突破/突破，80级 的数据
            if(level < 20)
            {
                attrs[i] = Utils.Lerp((float)(double)config["attrs"][i][0], (float)(double)config["attrs"][i][0], 19, level - 1);
                continue;
            }
            int levelRate = level % 10;
            if (levelRate == 0)
            {
                // 突破等级最低为0， 最高为 6
                if(breakLevel == level / 10 - 1)
                    attrs[i] = (float)(double)config["attrs"][i][2 * breakLevel];
                else
                    attrs[i] = (float)(double)config["attrs"][i][2 * breakLevel - 1];
            }
            else
            {
                attrs[i] = Utils.Lerp((float)(double)config["attrs"][i][2 * breakLevel - 2], (float)(double)config["attrs"][i][2 * breakLevel - 1], levelRate, 10);
            }
        }
        for(int i = (int)CommonAttribute.Speed; i < (int)CommonAttribute.Count; ++i)
        {
            attrs[i] = (float)(double)config["attrs"][i];
        }
        atkName = (string)config["atk"]["name"];
        atkDescription = (string)config["atk"]["description"];
        skillName = (string)config["skill"]["name"];
        skillDescription = (string)config["skill"]["description"];
        burstName = (string)config["burst"]["name"];
        burstDescription = (string)config["burst"]["description"];
        talentName = (string)config["talent"]["name"];
        talentDescription = (string)config["talent"]["description"];
        mysteryName = (string)config["mystery"]["name"];
        mysteryDescription = (string)config["mystery"]["description"];

        isAttackTargetEnemy = (bool)config["isAttackTargetEnemy"];
        attackSelectionType = (SelectionType)(int)config["attackSelectionType"];
        isSkillTargetEnemy = (bool)config["isSkillTargetEnemy"];
        skillSelectionType = (SelectionType)(int)config["skillSelectionType"];
        isBurstTargetEnemy = (bool)config["isBurstTargetEnemy"];
        burstSelectionType = (SelectionType)(int)config["burstSelectionType"];
        attackGainPointCount = (int)config["attackGainPointCount"];
        skillConsumePointCount = (int)config["skillConsumePointCount"];
        attackGainEnergy = (float)(double)config["attackGainEnergy"];
        skillGainEnergy = (float)(double)config["skillGainEnergy"];
        takeDmgGainEnergy = (float)(double)config["takeDmgGainEnergy"];

        // Load 角色光锥
        JsonData weaponData = config["weapon"];
        string weaponName = (string)weaponData["name"];
        int wLevel = (int)weaponData["level"];
        int rLevle = (int)weaponData["refine"];
        int bLevle = (int)weaponData["breakLevel"];
        weapon = new Weapon(weaponName, wLevel, bLevle, rLevle);
        weapon.OnEquipping(this);

        // Load 角色圣遗物
        JsonData artsJson = config["artifacts"];
        Dictionary<string, int> suitCount = new Dictionary<string, int>();
        foreach(JsonData artJson in artsJson)
        {
            // 套装名
            string suitName = (string)artJson["suitName"];
            if (!suitCount.ContainsKey(suitName))
                suitCount[suitName] = 0;
            suitCount[suitName]++;
            // 位置
            ArtifactPosition pos = (ArtifactPosition)(int)artJson["position"];
            // 主属性
            SimpleValueBuff b = new SimpleValueBuff((CommonAttribute)(int)artJson["main"]["attr"], (float)(double)artJson["main"]["value"], (ValueType)(int)artJson["main"]["type"]);

            // 副属性
            artifacts[(int)pos] = new Artifact(b, new List<SimpleValueBuff>());
        }

        // 之后改成反射 dict?
        switch (dbname)
        {
            case "bronya":
                talents = new Bronya(this);
                break;
            case "seele":
                talents = new Seele(this);
                break;
            default:
                talents = new Bronya(this);
                break;
        }
        talents.OnEquipping();
        hp = GetFinalAttr(CommonAttribute.MaxHP);
    }

    public override float GetBaseAttr(CommonAttribute attr)
    {
        if (attr == CommonAttribute.ATK)
            return attrs[(int)CommonAttribute.ATK] + weapon.atk;
        if (attr == CommonAttribute.MaxHP)
            return attrs[(int)CommonAttribute.MaxHP] + weapon.maxHp;
        if (attr == CommonAttribute.DEF)
            return attrs[(int)CommonAttribute.DEF] + weapon.def;
        return base.GetBaseAttr(attr);
    }

    public override float GetFinalAttr(Creature c1, Creature c2, CommonAttribute attr, DamageType damageType)
    {
        float res = base.GetFinalAttr(c1, c2, attr, damageType);
        res += weapon.CalBuffValue(null, null, attr, damageType);
        foreach(Artifact art in artifacts)
        {
            res += art.CalBuffValue(this, this, attr, damageType);
        }
        return res;
    }
    
    public void ChangeEnergy(float offset)
    {
        energy += offset;
        if (energy > maxEnergy) energy = maxEnergy;
        if (energy < 0) energy = 0;
        mono.UpdateEnergyIcon();
    }

    public void SetMono(CharacterMono m)
    {
        mono = m;
        base.mono = m;
        m.Initialize(this);
    }

    public virtual void StartBurstTurn()
    {
        List<string> toremove = new List<string>();
        foreach (var p in onTurnStart)
        {
            p.Value.trigger();
            if (p.Value.CountDown())
                toremove.Add(p.Key);
        }
        foreach (string s in toremove)
        {
            onTurnStart.Remove(s);
        }
        mono?.StartMyTurn();
    }

    public virtual void EndBurstTurn()
    {
        List<string> toremove = new List<string>();
        foreach (var p in onTurnEnd)
        {
            p.Value.trigger();
            if (p.Value.CountDown())
                toremove.Add(p.Key);
        }
        foreach (string s in toremove)
        {
            onTurnEnd.Remove(s);
        }
        toremove.Clear();
        foreach (KeyValuePair<string, Buff> p in buffs)
        {
            if (p.Value.CountDown())
            {
                toremove.Add(p.Key);
            }
        }
        foreach (string tag in toremove)
        {
            RemoveBuff(tag);
        }
        mono?.EndBurstTurn();
    }

}
