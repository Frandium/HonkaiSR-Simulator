using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;

public class WeaponConfig
{
    public string dbname = "default";
    public int level = 10;
    public int breakLevel = 10;
    public int refine = 10;

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
    public string suitName { get; set; } = "default";
    public ArtifactPosition position { get; set; }
    public PhraseConfig mainPhrase { get; set; }
    public List<PhraseConfig> vicePhrases { get; set; }

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
    public CommonAttribute attr { get; set; }
    public ValueType type { get; set; }
    public double value { get; set; }

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
    public int level  = 10;
    public int breakLevel  = 10;
    public int constellaLevel  = 10;
    public int atkLevel  = 10;
    public int skillLevel  = 10;
    public int burstLevel  = 10;
    public int talentLevel  = 10;
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
        string jsonString = File.ReadAllText(GlobalInfoHolder.characterConfigDir + "/" + name + ".json");
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


    public void Save()
    {
        FileStream fs;
        fs = File.Open(GlobalInfoHolder.characterConfigDir + "/" + dbname + ".json", FileMode.Create);
        string content = JsonMapper.ToJson(this);
        fs.Write(Encoding.UTF8.GetBytes(content));
        fs.Close();
    }
}
