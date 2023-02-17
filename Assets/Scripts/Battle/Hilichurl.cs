using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hilichurl : AEnemyTalents
{
    public Hilichurl(Enemy _self): base(_self)
    {

    }

    public override void MyTurn()
    {
        List<Character> characters = BattleManager.Instance.characters;
        int tauntWeight = 0;
        foreach(Character c in characters)
        {
            tauntWeight += c.tauntWeight;
        }
        int rand = Random.Range(0, tauntWeight);
        int i = 0;
        for(; i < characters.Count; ++i)
        {
            if (rand < characters[i].tauntWeight)
                break;
            rand -= characters[i].tauntWeight;
        }
        if(i >= characters.Count)
        {
            Debug.LogError("Wrong character index selected.");
            return;
        }
        float dmg = DamageCal.ATKDamageEnemy(self, characters[i], Element.Physical, 150);
        BattleManager.Instance.DealDamage(self, characters[i], Element.Physical, DamageType.Attack, dmg);
        self.PlayAudio(AudioType.Attack);
    }
}
