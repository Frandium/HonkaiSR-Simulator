using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public class WeaponConfig
{
    public string dbname { get; protected set; } = "default";
    public int level { get; protected set; } = 10;
    public int breakLevel { get; protected set; } = 10;
    public int refine { get; protected set; } = 10;

    public WeaponConfig(JsonData config)
    {
        dbname = (string)config["dbname"];
        level = (int)config["level"];
        refine = (int)config["refine"];
        breakLevel = (int)config["breakLevel"];
    }

    public WeaponConfig()
    {

    }
}

public class ArtifactConfig
{
    public string suitName { get; protected set; } = "default";
    public ArtifactPosition position { get; protected set; }
    public PhraseConfig mainPhrase { get; protected set; }
    public List<PhraseConfig> vicePhrases { get; protected set; }

    public ArtifactConfig(JsonData d)
    {
        suitName = (string)d["suitName"];
        position = (ArtifactPosition)(int)d["position"];
        mainPhrase = new PhraseConfig(d["mainPhrase"]);
        vicePhrases = new List<PhraseConfig>();
        for (int i = 0; i < d["vicePhrases"].Count; ++i)
        {
            vicePhrases.Add(new PhraseConfig(d["vicePhrases"][i]));
        }
    }

    public ArtifactConfig()
    {
        position = ArtifactPosition.Head;
        mainPhrase = new PhraseConfig();
        vicePhrases = new List<PhraseConfig>(4);
        for(int i = 0; i < 4; ++i)
        {
            vicePhrases.Add(new PhraseConfig());
        }
    }
}

public class PhraseConfig
{
    public CommonAttribute attr { get; protected set; }
    public ValueType type { get; protected set; }
    public double value { get; protected set; }

    public PhraseConfig(JsonData d)
    {
        attr = (CommonAttribute)(int)d["attr"];
        type = (ValueType)(int)d["type"];
        value = (double)d["value"];
    }

    public PhraseConfig()
    {
        attr = CommonAttribute.MaxHP;
        type = ValueType.Percentage;
        value = 0;
    }
}

public class CharacterConfig
{
    public string dbname { get; protected set; } = "bronya";
    public int level { get; protected set; } = 10;
    public int breakLevel { get; protected set; } = 10;
    public int constellaLevel { get; protected set; } = 10;
    public int atkLevel { get; protected set; } = 10;
    public int skillLevel { get; protected set; } = 10;
    public int burstLevel { get; protected set; } = 10;
    public int talentLevel { get; protected set; } = 10;
    public WeaponConfig weaponConfig { get; protected set; }
    public List<ArtifactConfig> artifacts { get; protected set; }
    public List<bool> abilityActivated { get; protected set; }

    public CharacterConfig()
    {
        weaponConfig = new WeaponConfig();
        artifacts = new List<ArtifactConfig>((int)ArtifactPosition.Count);
        for (int i = 0; i < (int)ArtifactPosition.Count; ++i)
        {
            artifacts.Add(new ArtifactConfig());
        }
        abilityActivated = new List<bool>(3);
        for(int i = 0; i < 3; ++i)
        {
            abilityActivated.Add(false);
        }
    }


    public CharacterConfig(string name)
    {
        string jsonString = File.ReadAllText(GlobalInfoHolder.Instance.characterConfigDir + "/" + name + ".json");
        JsonData config = JsonMapper.ToObject(jsonString);
        dbname = name;
        level = (int)config["level"];
        breakLevel = (int)config["breakLevel"];
        constellaLevel = (int)config["constellaLevel"];
        atkLevel = (int)config["atkLevel"];
        skillLevel = (int)config["skillLevel"];
        burstLevel = (int)config["burstLevel"];
        talentLevel = (int)config["talentLevel"];

        weaponConfig = new WeaponConfig(config["weaponConfig"]);
        
        artifacts = new List<ArtifactConfig>();
        foreach(JsonData d in config["artifacts"])
        {
            artifacts.Add(new ArtifactConfig(d));
        }

        abilityActivated = new List<bool>();
        foreach(JsonData d in config["abilityActivated"])
        {
            abilityActivated.Add((bool)d);
        }
    }

    public void ATKLevelUp(int offset)
    {
        atkLevel += offset;
        if (atkLevel > 15)
            atkLevel = 15;
    }
    public void SkillLevelUp(int offset)
    {
        skillLevel += offset;
        if (skillLevel > 15)
            skillLevel = 15;
    }
    public void BurstLevelUp(int offset)
    {
        burstLevel += offset;
        if (burstLevel > 15)
            burstLevel = 15;
    }
}
