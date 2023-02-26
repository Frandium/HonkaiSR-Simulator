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
    public float maxHp;

    public int level { get; protected set; }
    public int breakLevel { get; protected set; }
    public string effectName { get; protected set; }
    public string effectDescription { get; protected set; }
    int refine = 1;
    public Career career { get; protected set; } = Career.Count;


    AEquipmentTalents talents;

    public Weapon(string _dbname, int level, int breakLevel, int refineLevel)
    {
        LoadJson(_dbname, level, breakLevel, refineLevel);
    }

    public override float CalBuffValue(Creature source, Creature target, CommonAttribute a, DamageType damageType)
    {
        float res = 0;
        if (a == CommonAttribute.ATK)
            res += atk;
        else if (a == CommonAttribute.DEF)
            res += def;
        else if (a == CommonAttribute.MaxHP)
            res += maxHp;
        return res + base.CalBuffValue(source, target, a, damageType);
    }

    public void LoadJson(string _dbname, int _level, int blevel, int refineLevel)
    {
        dbName = _dbname;
        level = _level;
        refine = refineLevel;
        breakLevel = blevel;

        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/weapons/" + dbName + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        disName = (string)data["disname"];
        int maxLevel = data["atk"].Count;
        if (level < 20)
        {
            atk = (float)Utils.Lerp((double)data["atk"][0], (double)data["atk"][1], 19, level % 20);
            def = (float)Utils.Lerp((double)data["def"][0], (double)data["def"][1], 19, level % 20);
            maxHp = (float)Utils.Lerp((double)data["maxHp"][0], (double)data["maxHp"][1], 19, level % 20);
        }
        else if (level >= 20)
        {
            int l = 2 * (level / 10 - 2);
            if (level % 10 == 0 && blevel == (level / 10 - 1))
            {
                l++;
            }

            atk = (float)Utils.Lerp((double)data["atk"][l], (double)data["atk"][l + 1], level - (level / 10 - 1) * 10, 10);
            def = (float)Utils.Lerp((double)data["def"][l], (double)data["def"][l + 1], level - (level / 10 - 1) * 10, 10);
            maxHp = (float)Utils.Lerp((double)data["maxHp"][l], (double)data["maxHp"][l + 1], level - (level / 10 - 1) * 10, 10);
        }

        effectName = (string)data["effect"]["name"];
        effectDescription = Utils.FormatDescription((string)data["effect"]["description"], data["effect"], refine);
        career = (Career)(int)data["career"];

        switch (_dbname)
        {
            case "battleNotEnd":
                talents = new BattleNotEnd(data, refine);
                break;
            case "inTheNight":
                talents = new InTheNight(data, refine);
                break;
            case "victoryMoment":
                talents = new VictoryMoment(data, refine);
                break;
            default:
                break;
        }
    }

    public void OnEquipping(Character character)
    {
        talents.OnEquiping(character);
    }

    public void OnTakingOff(Character character)
    {
        talents.OnTakingOff(character);
    }
}
