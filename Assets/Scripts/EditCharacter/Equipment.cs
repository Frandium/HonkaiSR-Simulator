using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public virtual void OnBattleStart(Character c)
    {
        // ��һЩ buff����װ���߼ӡ�
        // ������ҷ�ȫ��ӣ����� BattleManager
    }

    public virtual float CalBuffValue(CreatureBase source, CreatureBase target,  CommonAttribute a)
    {
        float res = 0;
        foreach(ValueBuff b in buffs)
        {
            res += b.CalBuffValue(source, target, a);
        }
        return res;
    }


    // �������ṩ�� buff��
    protected List<ValueBuff> buffs = new List<ValueBuff>();
}
