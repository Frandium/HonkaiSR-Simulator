using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class Weapon: Equipment
{
    // Weapon 的 Buff 的特点是没办法被解增益效果清除

    public string dbName { get; protected set; }
    public string disName { get; protected set; }
    public float atk { get; protected set; }
    public float def { get; protected set; }
    public float maxHp { get; protected set; }

    public int level { get { return config.level; } }
    public int breakLevel { get { return config.breakLevel; } }
    public int refine { get { return config.refine; } }
    public string effectName { get; protected set; }
    public string effectDescription { get; protected set; }
    public Career career { get; protected set; } = Career.Count;

    WeaponConfig config;
    AEquipmentTalents talents;

    static Dictionary<string, string> dbname2Disname;
    static Dictionary<string, int> dbname2Career;

    public Weapon(WeaponConfig c)
    {
        config = c;
        LoadJson(c.dbname);
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

    public void LoadJson(string _dbname)
    {
        dbName = _dbname;
        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/weapons/" + dbName + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        disName = (string)data["disname"];
        if (level < 20)
        {
            atk = (float)Utils.Lerp((double)data["atk"][0], (double)data["atk"][1], level - 1, 19);
            def = (float)Utils.Lerp((double)data["def"][0], (double)data["def"][1], level - 1, 19);
            maxHp = (float)Utils.Lerp((double)data["maxHp"][0], (double)data["maxHp"][1], level - 1, 19);
        }
        else if (level >= 20)
        {
            int levelRate = level % 10;
            if (levelRate == 0)
            {
                // 突破等级最低为0， 最高为 6
                if (breakLevel == level / 10 - 1)
                {
                    atk = (float)(double)data["atk"][2 * breakLevel];
                    def = (float)(double)data["def"][2 * breakLevel];
                    maxHp = (float)(double)data["maxHp"][2 * breakLevel];
                }
                else
                { // 应该是 breaklevel == level / 10 - 2，没突破
                    atk = (float)(double)data["atk"][2 * breakLevel + 1];
                    def = (float)(double)data["def"][2 * breakLevel + 1];
                    maxHp = (float)(double)data["maxHp"][2 * breakLevel + 1];
                }
            }
            else
            {
                // breaklevel = level / 10 - 1
                atk = (float)Utils.Lerp((double)data["atk"][2 * breakLevel], (double)data["atk"][2 * breakLevel + 1], level % 10, 10);
                def = (float)Utils.Lerp((double)data["def"][2 * breakLevel], (double)data["def"][2 * breakLevel + 1], level % 10 * 10, 10);
                maxHp = (float)Utils.Lerp((double)data["maxHp"][2 * breakLevel], (double)data["maxHp"][2 * breakLevel + 1], level % 10, 10);
            }

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

    public static Dictionary<string, string> GetAllWeapons()
    {
        if(dbname2Disname == null)
        {
            dbname2Disname = new Dictionary<string, string>();
            dbname2Career = new Dictionary<string, int>();

            List<string> files = new List<string>(Directory.GetFiles(GlobalInfoHolder.Instance.weaponDir));
            files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
            
            foreach (string s in files)
            {
                string dbname = Path.GetFileNameWithoutExtension(s);
                string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/weapons/" + dbname + ".json");
                JsonData data = JsonMapper.ToObject(jsonString);
                dbname2Disname.Add(dbname, (string)data["disname"]);
                dbname2Career.Add(dbname, (int)data["career"]);
            }
        }
        return dbname2Disname;
    }

    public static Dictionary<string, int> GetAllWeaponCareers()
    {
        if(dbname2Career == null)
        {
            dbname2Disname = new Dictionary<string, string>();
            dbname2Career = new Dictionary<string, int>();

            List<string> files = new List<string>(Directory.GetFiles(GlobalInfoHolder.Instance.weaponDir));
            files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));

            foreach (string s in files)
            {
                string dbname = Path.GetFileNameWithoutExtension(s);
                string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/weapons/" + dbname + ".json");
                JsonData data = JsonMapper.ToObject(jsonString);
                dbname2Disname.Add(dbname, (string)data["disname"]);
                dbname2Career.Add(dbname, (int)data["career"]);
            }
        }
        return dbname2Career;
    }
}
