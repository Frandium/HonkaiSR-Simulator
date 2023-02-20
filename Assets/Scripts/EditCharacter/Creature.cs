using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature
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

    public delegate float DamageEvent(Creature sourceOrTarget, float value, Element element, DamageType type);
    public Dictionary<string, DamageEvent> onTakingDamage { get; protected set; } = new Dictionary<string, DamageEvent>();
    public Dictionary<string, DamageEvent> onDealingDamage { get; protected set; } = new Dictionary<string, DamageEvent>();

    public delegate float HealEvent(Creature sourceOrTarget, float value);

    public Dictionary<string, HealEvent> onTakingHeal { get; protected set; } = new Dictionary<string, HealEvent>();
    public Dictionary<string, HealEvent> onDealingHeal { get; protected set; } = new Dictionary<string, HealEvent>();

    public delegate void TurnStartEndEvent();
    public Dictionary<string, TurnStartEndEvent> onTurnStart { get; protected set; } = new Dictionary<string, TurnStartEndEvent>();
    public Dictionary<string, TurnStartEndEvent> onTurnEnd { get; protected set; } = new Dictionary<string, TurnStartEndEvent>();
    // ����˺�ʱ���ܵ��˺�ʱ��������ƣ��ܵ����ƣ��غϿ�ʼʱ���غϽ���ʱ����ͨ����ʱ���ͷ�ս�� / ����ʱ��
    public Creature() { }

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

    public virtual float GetFinalAttr(Creature c1, Creature c2, CommonAttribute attr)
    {
        // Ϊ�����㡾���� XX �����ʱ������������ XXX�����ֹ���
        float res = attrs[(int)attr];
        foreach (Buff buff in buffs.Values)
        {
            res += buff.CalBuffValue(c1, c2, attr);
        }
        return res;
    }

    public void ChangePercentageLocation(float offset)
    {
        location += offset * Runway.Length / 100.0f;
        if (location < 0)
            location = 0;
        if (location > Runway.Length)
            location = Runway.Length;
    }

    public void ChangeAbsoluteLocation(float offset)
    {
        location += offset;
    }

    public void DealDamage(Creature target, Element element, DamageType type, float value)
    {
        foreach (DamageEvent e in onDealingDamage.Values)
        {
            e(target, value, element, type);
        }
        target.TakeDamage(this, value, element, type);
    }

    public virtual void TakeDamage(Creature source, float value, Element element, DamageType type)
    {
        foreach (DamageEvent e in onTakingDamage.Values)
        {
            e(source, value, element, type);
        }
        hp -= value;
        mono?.TakeDamage(value, element);
    }

    public virtual void TakeHeal(Creature source, float value)
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
        mono?.TakeHeal(value);
    }

    public virtual void DealHeal(Creature target, float value)
    {
        foreach(HealEvent e in onDealingHeal.Values)
        {
            e(target, value);
        }
        target.TakeHeal(this, value);
    }

    public virtual bool StartNormalTurn()
    {
        foreach(TurnStartEndEvent e in onTurnStart.Values)
        {
            e();
        }
        mono?.StartMyTurn();
        return false;
    }

    public virtual void EndNormalTurn()
    {
        foreach(TurnStartEndEvent e in onTurnEnd.Values)
        {
            e();
        }
        List<string> toremove = new List<string>();
        foreach(KeyValuePair<string ,Buff> p in buffs)
        {
            if (p.Value.CountDown())
            {
                toremove.Add(p.Key);
            }
        }
        foreach(string tag in toremove)
        {
            buffs.Remove(tag);
        }
        mono.EndMyTurn();
    }


}
