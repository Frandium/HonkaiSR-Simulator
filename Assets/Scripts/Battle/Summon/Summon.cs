using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public class Summon : Creature
{
// �ٻ��Ŀǰ��Ϊ�˾�Ԫ���������ࡣ
    
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
        // Load ��ɫ��������
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
