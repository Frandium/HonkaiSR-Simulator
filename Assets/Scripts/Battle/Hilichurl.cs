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
        float tauntWeight = 0;
        foreach(Character c in characters)
        {
            tauntWeight += c.GetFinalAttr(CommonAttribute.Taunt);
        }
        float rand = Random.Range(0, tauntWeight);
        int i = 0;
        for(; i < characters.Count; ++i)
        {
            if (rand < characters[i].GetFinalAttr(CommonAttribute.Taunt))
                break;
            rand -= characters[i].GetFinalAttr(CommonAttribute.Taunt);
        }
        if(i >= characters.Count)
        {
            Debug.LogError("Wrong character index selected.");
            return;
        }
        float dmg = DamageCal.ATKDamageEnemy(self, characters[i], Element.Physical, 150);
        self.DealDamage(characters[i], Element.Physical, DamageType.Attack, dmg);
        self.DealDamage(characters[i], Element.Physical, DamageType.Attack, dmg);
        self.mono.PlayAudio(AudioType.Attack);
    }
}
