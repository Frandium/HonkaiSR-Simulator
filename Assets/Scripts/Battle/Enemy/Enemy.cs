using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class Enemy : Creature
{
    public new AEnemyTalents talents { get; protected set; }
    public new EnemyMono mono { get; protected set; }
    public List<Element> weakPoint { get; protected set; } = new List<Element>();
    public float weakMaxHp { get; protected set; } = 100;
    public float weakHp { get; protected set; } = 100;

    public delegate void OnBreak(Creature source, Damage dmg);
    public List<TriggerEvent<OnBreak>> onBreak { get; protected set; } = new List<TriggerEvent<OnBreak>>();

    public void SetMono(EnemyMono m)
    {
        mono = m;
        base.mono = m;
        m.Initialize(this);
    }

    public Enemy(EnemyConfig ec)
    {
        dbname = ec.dbname;
        LoadJson(ec.dbname, ec.level);
    }

    public void LoadJson(string name, int level)
    {
        dbname = name;
        // Load ½ÇÉ«»ù±¾ÊôÐÔ
        string jsonString = File.ReadAllText(GlobalInfoHolder.enemyDir + "/" + dbname + ".json");
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

        talents = dbname switch
        {
            "hilichurl" => new Hilichurl(this),
            "boss" => new Hilichurl(this),
            _ => new Hilichurl(this),
        };
        base.talents = talents;

        hp = GetFinalAttr(CommonAttribute.MaxHP);
    }

    public override void TakeDamage(Creature source, Damage damage)
    {
        if (weakHp > 0 && weakPoint.Contains(damage.element))
        {
            weakHp -= damage.type switch
            {
                DamageType.Attack => 25.0f,
                DamageType.Skill => 35.0f,
                DamageType.Burst => 60.0f,
                DamageType.Additional => 20.0f,
                DamageType.CoAttack => 5.0f,
                _ => 0.0f
            };
            if (weakHp <= 0)
            {
                weakHp = 0;
                ChangePercentageLocation(-.25f);
                mono?.ShowMessage("Èõµã»÷ÆÆ", Color.green);
                mono?.ShowMessage("¼õ·À30%", Color.green);
                AddBuff("breakDefDown", BuffType.Permanent, CommonAttribute.DEF, ValueType.Percentage, -.3f);
                switch (damage.element)
                {
                    case Element.Physical:
                        AddAnemoPhyQuant(source, StateType.Split, 2, 1, .5f);
                        break;
                    case Element.Pyro:
                        AddPyroElecCryo(source, StateType.Burning, 2, .5f);
                        break;
                    case Element.Electro:
                        AddPyroElecCryo(source, StateType.Electric, 2, .5f);
                        break;
                    case Element.Anemo:
                        AddAnemoPhyQuant(source, StateType.Weathered, 2, 1, .5f);
                        break;
                    case Element.Quantus:
                        AddAnemoPhyQuant(source, StateType.Entangle, 1, 1, .5f);
                        break;
                    case Element.Imaginary:
                        AddRestricted(source, 1, .25f, .1f);
                        break;
                    case Element.Cryo:
                        AddPyroElecCryo(source, StateType.Frozen, 1, .5f);
                        break;
                        default: break;
                }
                foreach(var t in onBreak)
                {
                    t.trigger(source, damage);
                }
            }
        }
        base.TakeDamage(source, damage);
    }

    public override bool StartNormalTurn()
    {
        if(weakHp <= 0)
        {
            weakHp = weakMaxHp;
            RemoveBuff("breakDefDown");
            mono?.ShowMessage("ÈÍÐÔ»Ö¸´", Color.black);
            mono?.UpdateHpLine();
        }
        return base.StartNormalTurn();
    }

    public override void EndNormalTurn()
    {
        onBreak.RemoveAll(t => t.CountDown(CountDownType.Turn));
        base.EndNormalTurn();
    }
}
