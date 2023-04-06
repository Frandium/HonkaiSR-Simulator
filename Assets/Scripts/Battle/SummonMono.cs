using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonMono : CreatureMono
{
    public override void StartMyTurn()
    {

    }

    public override void EndMyTurn()
    {

    }

    public override void Initialize(Creature c)
    {
        runwayAvatar = Resources.Load<Sprite>(c.dbname + "/runway_avatar");
    }


}
