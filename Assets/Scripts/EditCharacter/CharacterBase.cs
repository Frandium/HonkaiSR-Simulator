using System.IO;
using UnityEngine;
using LitJson;
using System.Collections.Generic;

public class CharacterBase: CreatureBase
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
    public Element element { get; protected set; } = Element.Count;
    public float energy { get; protected set; } = 0;
    public float maxEnergy { get; protected set; } = 60;
    public ACharacterTalents talents { get; protected set; }
    public new CharacterMono mono { get; protected set; }
    public Weapon weapon;
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

    public delegate void TalentUponTarget(CreatureBase target);
    public Dictionary<string, TalentUponTarget> onNormalAttack { get; protected set; } = new Dictionary<string, TalentUponTarget>();
    public Dictionary<string, TalentUponTarget> onSkill { get; protected set; } = new Dictionary<string, TalentUponTarget>();
    public Dictionary<string, TalentUponTarget> onBurst { get; protected set; } = new Dictionary<string, TalentUponTarget>();
    public CharacterBase() { }

    public CharacterBase(string _dbname)
    {
        LoadJson(_dbname);
    }

    public void LoadJson(string name)
    {
        // Load 角色基本属性
        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/characters/" + name + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        dbname = name;
        disname = (string)data["disname"];
        level = (int)data["level"];
        breakLevel = (int)data["breakLevel"];
        for(int i = 0; i < (int)CommonAttribute.Speed; ++i)
        {
            // 这些数据每个有 14 条，分别是1级，10级未突破/突破……70级未突破/突破，80级 的数据
            if(level < 20)
            {
                attrs[i] = Utils.Lerp((float)(double)data["attrs"][i][0], (float)(double)data["attrs"][i][0], 19, level - 1);
                continue;
            }
            int levelRate = level % 10;
            if (levelRate == 0)
            {
                // 突破等级最低为0， 最高为 6
                if(breakLevel == level / 10 - 1)
                    attrs[i] = (float)(double)data["attrs"][i][2 * breakLevel];
                else
                    attrs[i] = (float)(double)data["attrs"][i][2 * breakLevel - 1];
            }
            else
            {
                attrs[i] = Utils.Lerp((float)(double)data["attrs"][i][2 * breakLevel - 2], (float)(double)data["attrs"][i][2 * breakLevel - 1], levelRate, 10);
            }
        }
        for(int i = (int)CommonAttribute.Speed; i < (int)CommonAttribute.Count; ++i)
        {
            attrs[i] = (float)(double)data["attrs"][i];
        }
        atkName = (string)data["atk"]["name"];
        atkDescription = (string)data["atk"]["description"];
        skillName = (string)data["skill"]["name"];
        skillDescription = (string)data["skill"]["description"];
        burstName = (string)data["burst"]["name"];
        burstDescription = (string)data["burst"]["description"];
        talentName = (string)data["talent"]["name"];
        talentDescription = (string)data["talent"]["description"];
        mysteryName = (string)data["mystery"]["name"];
        mysteryDescription = (string)data["mystery"]["description"];

        isAttackTargetEnemy = (bool)data["isAttackTargetEnemy"];
        attackSelectionType = (SelectionType)(int)data["attackSelectionType"];
        isSkillTargetEnemy = (bool)data["isSkillTargetEnemy"];
        skillSelectionType = (SelectionType)(int)data["skillSelectionType"];
        isBurstTargetEnemy = (bool)data["isBurstTargetEnemy"];
        burstSelectionType = (SelectionType)(int)data["burstSelectionType"];
        attackGainPointCount = (int)data["attackGainPointCount"];
        skillConsumePointCount = (int)data["skillConsumePointCount"];
        attackGainEnergy = (float)(double)data["attackGainEnergy"];
        skillGainEnergy = (float)(double)data["skillGainEnergy"];
        takeDmgGainEnergy = (float)(double)data["takeDmgGainEnergy"];

        // Load 角色光锥
        JsonData weaponData = data["weapon"];
        string weaponName = (string)weaponData["name"];
        int wLevel = (int)weaponData["level"];
        int rLevle = (int)weaponData["refine"];
        weapon = new Weapon(weaponName, wLevel, rLevle);
        weapon.OnEquipping(this);

        // Load 角色圣遗物
        JsonData artsJson = data["artifacts"];
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

        switch (dbname)
        {
            case "bronya":
                talents = new Bronya(this);
                break;
            default:
                break;
        }
    }

    public override float GetFinalAttr(CreatureBase c1, CreatureBase c2, CommonAttribute attr)
    {
        float res = base.GetFinalAttr(c1, c2, attr);
        res += weapon.CalBuffValue(null, null, attr);
        foreach(Artifact art in artifacts)
        {
            res += art.CalBuffValue(this, this, attr);
        }
        return res;
    }
    
    public void ChangeEnergy(float offset)
    {
        energy += offset;
        if (energy > maxEnergy) energy = maxEnergy;
        if (energy < 0) energy = 0;
    }

    public void SetMono(CharacterMono m)
    {
        mono = m;
        base.mono = m;
    }
}
