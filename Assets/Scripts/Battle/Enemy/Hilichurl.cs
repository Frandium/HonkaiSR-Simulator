using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hilichurl : AEnemyTalents
{
    public Hilichurl(Enemy _self): base(_self)
    {

    }

    public override void OnEquipping()
    {

    }

    public override void MyTurn(List<Character> characters, List<Enemy> enemies)
    {
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
        Damage dmg = Damage.NormalDamage(self, characters[i], CommonAttribute.ATK, 1.5f, new DamageConfig(DamageType.Attack, Element.Physical));
        self.DealDamage(characters[i], dmg);
        self.mono.PlayAudio(AudioType.Attack);
    }
}
