using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ASummonTalents : ACommomBattleTalents
{
    public Creature summoner;
    public Summon self;
    public ASummonTalents(Summon self, Creature summoner)
    {
        this.summoner = summoner;
        this.self = self;
    }

    public virtual void MyTurn(List<Character> characters, List<Enemy> enemies)
    {

    }

    public virtual void BattleStart(List<Character> characters)
    {

    }
}
