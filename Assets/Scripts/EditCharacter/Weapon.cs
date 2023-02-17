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
    public float[] atk;
    public float[] def;
    public float[] maxHP;

    int level;
    string effectName;
    string effectDescription;

    public override float CalBuffValue(Creature source, Creature target, CharacterAttribute a)
    {
        float res = 0;
        return res + base.CalBuffValue(source, target, a);
    }

    public override float CalBuffValue(Creature source, Creature target, CommonAttribute a)
    {
        float res = 0;
        if (a == CommonAttribute.ATK)
            res += atk[level];
        else if (a == CommonAttribute.DEF)
            res += def[level];
        else if (a == CommonAttribute.MaxHP)
            res += maxHP[level];
        return res + base.CalBuffValue(source, target, a);
    }

    public void LoadJson(string _dbname)
    {
        dbName = _dbname;

        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/weapons/" + dbName + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        disName = (string)data["disname"];
        int maxLevel = data["atk"].Count;
        atk = new float[maxLevel];
        def = new float[maxLevel];
        maxHP = new float[maxLevel];
        for(int i = 0; i< maxLevel; ++i)
        {
            atk[i] = (float)(double)data["atk"][i];
            def[i] = (float)(double)data["def"][i];
            maxHP[i] = (float)(double)data["maxHp"][i];
        }

        effectName = (string)data["effectName"];
        effectDescription = (string)data["effectDescription"];
    }
}
