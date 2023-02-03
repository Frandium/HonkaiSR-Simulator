using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hilichurl : IEnemyAction
{
    Enemy self;
    BattleManager bm;

    public Hilichurl(Enemy _self)
    {
        self = _self;
        bm = GameObject.Find("battleManager").GetComponent<BattleManager>();
    }

    public void MyTurn()
    {
        List<Character> characters = bm.characters;
        int i = Random.Range(0, characters.Count);
        float dmg = DamageCal.ATKDamage(self, Element.Physical, 150);
        characters[i].TakeDamage(dmg, self, Element.Physical);
        self.PlayAudio(AudioType.Attack);
    }

}
