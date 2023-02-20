using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class Enemy : Creature
{
    public AEnemyTalents talents;
    public new EnemyMono mono { get; protected set; }
    public List<Element> weakPoint { get; protected set; } = new List<Element>();
    public float weakMaxHp { get; protected set; } = 100;
    public float weakHp { get; protected set; } = 100;

    public void SetMono(EnemyMono m)
    {
        mono = m;
        base.mono = m;
        m.Initialize(this);
    }

    public Enemy(string _dbname)
    {
        dbname = _dbname;
        LoadJson(_dbname);
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

        foreach (JsonData d in data["weakPoint"])
        {
            weakPoint.Add((Element)(int)d);
        }

        weakMaxHp = (float)(double)data["weakMaxHp"];
        weakHp = weakMaxHp;

        switch (dbname)
        {
            case "hilichurl":
                talents = new Hilichurl(this);
                break;
            default:
                break;
        }

        hp = GetFinalAttr(CommonAttribute.MaxHP);
    }

    public override void TakeDamage(Creature source, float value, Element element, DamageType type)
    {
        if (weakHp > 0 && weakPoint.Contains(element))
        {
            weakHp -= 50.0f;
            if (weakHp <= 0)
            {
                weakHp = 0;
                ChangePercentageLocation(-.25f);
                // 破防事件
            }
        }
        base.TakeDamage(source, value, element, type);
    }
}
