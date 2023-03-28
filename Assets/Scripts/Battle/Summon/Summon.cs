using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : Creature
{
// 召唤物，目前是为了景元设计了这个类。

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
