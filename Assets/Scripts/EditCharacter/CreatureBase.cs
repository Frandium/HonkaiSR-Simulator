using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBase
{
    // �����ڲ���ս���У����ﶼҪ�м����������Ե�����
    // ��Ҫ�� Addbuff ����������Ϊ talents �����Ǽ򵥴ֱ������������Ե�
    // ��ɫ�͵��˶�Ҫ�� LoadTalents ����������Ϊ�п��� Talents
    // ����Ҫս����ص� hook����Ϊչʾ����û�������ս�������Ҳ��� Mono
    // Mono �ṩ��������ʵ���Ǹ�������Ѫ��������ͼ�ꡢrunway ͼ�꣬�����������ڽ�ɫ�� Enemy ����һ�µġ�
    // ��ɫ��� Enemy ��ͬ���Ƕ�������� atk/skill/burst ͼ��ȣ����� burst splash��
    public string dbname { get; protected set; } = "test";
    public string disname { get; protected set; } = "���Խ�ɫ";

    protected float[] attrs = new float[(int)CommonAttribute.Count];
    public int level { get; protected set; } = 60;
    public float location { get; protected set; } = 0;
    public float hp { get; protected set; } = 100;
    public CreatureMono mono { get; protected set; }
    public Dictionary<string, Buff> buffs { get; protected set; } = new Dictionary<string, Buff>();

    public delegate float DamageEvent(CreatureBase sourceOrTarget, float value, Element element, DamageType type);
    public Dictionary<string, DamageEvent> onTakingDamage { get; protected set; }
    public Dictionary<string, DamageEvent> onDealingDamage { get; protected set; }

    public delegate float HealEvent(CreatureBase sourceOrTarget, float value);

    public Dictionary<string, HealEvent> onTakingHeal { get; protected set; }
    public Dictionary<string, HealEvent> onDealingHeal { get; protected set; }

    public delegate void TurnStartEndEvent();
    public Dictionary<string, TurnStartEndEvent> onTurnStart { get; protected set; }
    public Dictionary<string, TurnStartEndEvent> onTurnEnd { get; protected set; }
    // ����˺�ʱ���ܵ��˺�ʱ��������ƣ��ܵ����ƣ��غϿ�ʼʱ���غϽ���ʱ����ͨ����ʱ���ͷ�ս�� / ����ʱ��
    public CreatureBase() { }

    public virtual float GetBaseAttr(CommonAttribute attr)
    {
        return attrs[(int)attr];
    }

    public virtual float GetFinalAttr(CommonAttribute attr)
    {
        return GetFinalAttr(this, this, attr);
    }

    public virtual void SetMono(CreatureMono m)
    {
        mono = m;
    }

    public virtual float GetFinalAttr(CreatureBase c1, CreatureBase c2, CommonAttribute attr)
    {
        // Ϊ�����㡾���� XX �����ʱ������������ XXX�����ֹ���
        float res = attrs[(int)attr];
        foreach (Buff buff in buffs.Values)
        {
            res += buff.CalBuffValue(c1, c2, attr);
        }
        return res;
    }

    public void ChangeLocation(float offset)
    {
        location += offset;
    }

    public void DealDamage(CreatureBase target, Element element, DamageType type, float value)
    {
        foreach (DamageEvent e in onDealingDamage.Values)
        {
            e(target, value, element, type);
        }
        target.TakeDamage(this, value, element, type);
    }

    public virtual void TakeDamage(CreatureBase source, float value, Element element, DamageType type)
    {
        foreach (DamageEvent e in onTakingDamage.Values)
        {
            e(source, value, element, type);
        }
        //value = DamageCal.ResistDamage(value, element, this);
        hp -= value;
    }

    public virtual void TakeHeal(CreatureBase source, float value)
    {
        if (hp + value > GetFinalAttr(CommonAttribute.MaxHP))
        {
            value = GetFinalAttr(CommonAttribute.MaxHP) - hp;
            hp = GetFinalAttr(CommonAttribute.MaxHP);
        }
        else
        {
            hp += value;
        }
        foreach(HealEvent e in onTakingHeal.Values)
        {
            e(source, value);
        }
    }

    public virtual void DealHeal(CreatureBase target, float value)
    {
        foreach(HealEvent e in onDealingHeal.Values)
        {
            e(target, value);
        }
    }

    public virtual void StartNormalTurn()
    {
        foreach(TurnStartEndEvent e in onTurnStart.Values)
        {
            e();
        }
    }

    public virtual void EndNormalTurn()
    {
        foreach(TurnStartEndEvent e in onTurnEnd.Values)
        {
            e();
        }
        foreach(Buff b in buffs.Values)
        {
            b.CountDown();
        }
    }

}
