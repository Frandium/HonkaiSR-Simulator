using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class EnemyBase : CreatureBase
{
    public AEnemyTalents talents;
    public new EnemyMono mono { get; protected set; }

    public void SetMono(EnemyMono m)
    {
        mono = m;
        base.mono = m;
    }

    public void LoadJson(string name)
    {
        dbname = name;
        // Load 角色基本属性
        string jsonString = File.ReadAllText(GlobalInfoHolder.Instance.enemyDir + "/" + dbname + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        dbname = (string)data["dbname"];
        disname = (string)data["disname"];
        attrs[(int)CommonAttribute.ATK] = (float)(double)data["atk"];
        attrs[(int)CommonAttribute.DEF] = (float)(double)data["def"];
        attrs[(int)CommonAttribute.Speed] = (float)(double)data["speed"];
        attrs[(int)CommonAttribute.MaxHP] = (float)(double)data["maxHp"];

        for (int i = 0; i < (int)Element.Count; ++i)
        {
            attrs[(int)CommonAttribute.PhysicalResist + i] = (float)(double)data["elementalResist"][i];
        }

        switch (dbname)
        {
            case "hilichurl":
                talents = new Hilichurl(this);
                break;
            default:
                break;
        }
    }
}
