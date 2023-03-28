using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public class Summon : Creature
{
// 召唤物，目前是为了景元设计了这个类。
    
    public Creature summoner { get; protected set; }
    public Summon(string _dbname, Creature summoner) {
        dbname = _dbname;
        this.summoner = summoner;
        Initialize();
    }

    public new ASummonTalents talents { get; set; }

    public override void Initialize()
    {
        base.Initialize();
        // Load 角色基本属性
        string jsonString = File.ReadAllText(GlobalInfoHolder.summonDir + "/" + dbname + ".json");
        JsonData metadata = JsonMapper.ToObject(jsonString);

        disname = (string)metadata["disname"];
        for (int i = 0; i < (int)CommonAttribute.Count; ++i)
        {
            attrs[i] = (float)(double)metadata["attrs"][i];
        }
        talents = dbname switch
        {
            "jingyuanShenjun" => new JingyuanShenjun(this, summoner),
            _ => new DefaultSummon(this, summoner)
        };
        talents.OnEquipping();
    }

    public new SummonMono mono;
    public void SetMono(SummonMono mono)
    {
        this.mono = mono;
        base.mono = mono;
        mono.Initialize(this);
    }

}
