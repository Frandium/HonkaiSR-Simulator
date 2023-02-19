using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class Weapon: Equipment
{
    // Weapon 的 Buff 的特点是没办法被解增益效果清除

    public string dbName;
    public string disName;
    public float atk;
    public float def;
    public float maxHP;

    int level;
    string effectName;
    string effectDescription;
    int refine = 1;


    AEquipmentTalents talents;

    public Weapon(string _dbname, int level, int refineLevel)
    {
        LoadJson(_dbname, level, refineLevel);
    }

    public override float CalBuffValue(CreatureBase source, CreatureBase target, CommonAttribute a)
    {
        float res = 0;
        if (a == CommonAttribute.ATK)
            res += atk;
        else if (a == CommonAttribute.DEF)
            res += def;
        else if (a == CommonAttribute.MaxHP)
            res += maxHP;
        return res + base.CalBuffValue(source, target, a);
    }

    public void LoadJson(string _dbname, int _level, int refineLevel)
    {
        dbName = _dbname;
        level = _level;
        refine = refineLevel;

        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/weapons/" + dbName + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        disName = (string)data["disname"];
        int maxLevel = data["atk"].Count;
        atk = (float)((double)data["atk"][0] + ((double)data["atk"][1] - (double)data["atk"][0]) * (level - 1) / 79.0);
        def = (float)((double)data["def"][0] + ((double)data["def"][1] - (double)data["def"][0]) * (level - 1) / 79.0);
        maxHP = (float)((double)data["def"][0] + ((double)data["def"][1] - (double)data["def"][0]) * (level - 1) / 79.0);

        effectName = (string)data["effect"]["name"];
        effectDescription = (string)data["effect"]["description"][refineLevel];

        switch (_dbname)
        {
            case "battleNotEnd":
                talents = new BattleNotEnd(data, refine);
                break;
            default:
                break;
        }
    }

    public void OnEquipping(CharacterBase character)
    {
        talents.OnEquiping(character);
    }

    public void OnTakingOff(CharacterBase character)
    {
        talents.OnTakingOff(character);
    }
}
