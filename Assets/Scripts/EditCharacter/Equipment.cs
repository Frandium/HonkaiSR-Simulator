using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public virtual void OnBattleStart(CharacterMono c)
    {
        // ��һЩ buff����װ���߼ӡ�
        // ������ҷ�ȫ��ӣ����� BattleManager
    }

    public virtual float CalBuffValue(Creature source, Creature target,  CommonAttribute a, DamageType damageType)
    {
        float res = 0;
        foreach(Buff b in buffs)
        {
            res += b.CalBuffValue(source, target, a, damageType);
        }
        return res;
    }


    // �������ṩ�� buff��
    protected List<Buff> buffs = new List<Buff>();
}
