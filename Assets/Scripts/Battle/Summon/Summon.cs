using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : Creature
{
// �ٻ��Ŀǰ��Ϊ�˾�Ԫ���������ࡣ

    public Summon(string _dbname) {
        dbname = _dbname;
    }

    public new ASummonTalents talents { get; set; }

    public override void Initialize()
    {
        base.Initialize();
    }

    public new SummonMono mono;
    public void SetMono(SummonMono mono)
    {
        this.mono = mono;
        mono.Initialize(this);
    }

}
