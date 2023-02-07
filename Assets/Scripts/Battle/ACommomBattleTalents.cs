using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ACommomBattleTalents
{
    public virtual void OnTakingDamage(Creature source, float value, Element element, DamageType type)
    {

    }

    public virtual void OnDealingDamage(Creature target, float value, Element element, DamageType type)
    {

    }

    public virtual void OnDying()
    {

    }
}
