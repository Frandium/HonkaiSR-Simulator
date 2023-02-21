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
    public float hp = 100;
    public CreatureMono mono { get; protected set; }
    public Dictionary<string, Buff> buffs { get; protected set; } = new Dictionary<string, Buff>();

    public delegate float DamageEvent(Creature sourceOrTarget, float value, Element element, DamageType type);
    public Dictionary<string, TriggerEvent<DamageEvent>> onTakingDamage { get; protected set; } =  new Dictionary<string, TriggerEvent<DamageEvent>>();
    public Dictionary<string, TriggerEvent<DamageEvent>> onDealingDamage { get; protected set; } = new Dictionary<string, TriggerEvent<DamageEvent>>();

    public delegate float HealEvent(Creature sourceOrTarget, float value);

    public Dictionary<string, TriggerEvent<HealEvent>> onTakingHeal { get; protected set; } = new Dictionary<string, TriggerEvent<HealEvent>>();
    public Dictionary<string, TriggerEvent<HealEvent>> onDealingHeal { get; protected set; } = new Dictionary<string, TriggerEvent<HealEvent>>();

    public delegate void TurnStartEndEvent();
    public Dictionary<string, TriggerEvent<TurnStartEndEvent>> onTurnStart { get; protected set; } = new Dictionary<string, TriggerEvent<TurnStartEndEvent>>();
    public Dictionary<string, TriggerEvent<TurnStartEndEvent>> onTurnEnd { get; protected set; } = new Dictionary<string, TriggerEvent<TurnStartEndEvent>>();

    public Dictionary<string, Shield> shields = new Dictionary<string, Shield>();
    
    public Creature() { }

    public virtual float GetBaseAttr(CommonAttribute attr)
    {
        return attrs[(int)attr];
    }

    public virtual float GetFinalAttr(CommonAttribute attr)
    {
        return GetFinalAttr(this, this, attr, DamageType.Count);
    }

    public virtual void SetMono(CreatureMono m)
    {
        mono = m;
    }

    public virtual float GetFinalAttr(Creature c1, Creature c2, CommonAttribute attr, DamageType damageType)
    {
        // Ϊ�����㡾���� XX �����ʱ������������ XXX�����ֹ���
        float res = attrs[(int)attr];
        foreach (Buff buff in buffs.Values)
        {
            res += buff.CalBuffValue(c1, c2, attr, damageType);
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

    public void GetShield(string tag, Shield shield, bool removeIfExists = true)
    {
        if (shields.ContainsKey(tag))
        {
            if (removeIfExists)
                shields.Remove(tag);
            else
                return;
        }
        shields.Add(tag, shield);
    }

    public void RemoveShield()
    {

    }


    public void DealDamage(Creature target, Element element, DamageType type, float value)
    {
        // �� take damage���ٴ����¼�����Ȼ damage ���ܾʹ���
        target.TakeDamage(this, value, element, type);
        List<string> toremove = new List<string>();
        foreach (var p in onDealingDamage)
        {
            p.Value.trigger(target, value, element, type);
            if (p.Value.CountDown())
                toremove.Add(p.Key);
        }
        foreach (string s in toremove)
        {
            onDealingDamage.Remove(s);
        }
    }

    public virtual void TakeDamage(Creature source, float value, Element element, DamageType type)
    {
        // value ������
        List<string> toremove = new List<string>();
        foreach (var p in onTakingDamage)
        {
            p.Value.trigger(source, value, element, type);
            if (p.Value.CountDown())
                toremove.Add(p.Key);
        }
        foreach (string s in toremove)
        {
            onTakingDamage.Remove(s);
        }
        toremove.Clear();
        float remain = value;
        Debug.Log(value);
        foreach(var p in shields)
        {
            float r = - p.Value.TakeDamage(value);
            remain = Mathf.Min(r, remain);
            if(r <= 0)
                toremove.Add(p.Key);
        }
        foreach(string s in toremove)
        {
            shields.Remove(s);
        }
        remain = Mathf.Max(0, remain);
        hp -= remain;
        mono?.TakeDamage(remain, element);
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
        List<string> toremove = new List<string>();
        foreach (var p in onTakingHeal)
        {
            p.Value.trigger(source, value);
            if (p.Value.CountDown())
                toremove.Add(p.Key);
        }
        foreach (string s in toremove)
        {
            onTakingHeal.Remove(s);
        }
        mono?.TakeHeal(value);
    }

    public virtual void DealHeal(Creature target, float value)
    {
        List<string> toremove = new List<string>();
        foreach (var p in onDealingHeal)
        {
            p.Value.trigger(target, value);
            if (p.Value.CountDown())
                toremove.Add(p.Key);
        }
        foreach (string s in toremove)
        {
            onDealingHeal.Remove(s);
        }
        target.TakeHeal(this, value);
    }

    public virtual bool StartNormalTurn()
    {
        List<string> toremove = new List<string>();
        foreach (var p in onTurnStart)
        {
            p.Value.trigger();
            if (p.Value.CountDown())
                toremove.Add(p.Key);
        }
        foreach (string s in toremove)
        {
            onTurnStart.Remove(s);
        }
        mono?.StartMyTurn();
        return false;
    }

    public virtual void EndNormalTurn()
    {
        List<string> toremove = new List<string>();
        foreach (var p in onTurnEnd)
        {
            p.Value.trigger();
            if (p.Value.CountDown())
                toremove.Add(p.Key);
        }
        foreach (string s in toremove)
        {
            onTurnEnd.Remove(s);
        }
        toremove.Clear();
        foreach(KeyValuePair<string ,Buff> p in buffs)
        {
            if (p.Value.CountDown())
            {
                toremove.Add(p.Key);
            }
        }
        foreach(string tag in toremove)
        {
            RemoveBuff(tag);
        }
        mono?.EndMyTurn();
    }

    public virtual void AddBuff(string tag, Buff buff, bool removeIfExist = true)
    {
        if (buffs.ContainsKey(tag))
        {
            if (removeIfExist)
            {
                buffs.Remove(tag);
            }
            else
            {
                return;
            }
        }
        buffs.Add(tag, buff);
    }

    public virtual void AddBuff(string tag, BuffType buffType, CommonAttribute attr, int duration, ValueType valueType, float value, DamageType damageType = DamageType.All)
    {
        Buff b = Utils.valueBuffPool.GetOne().Set(buffType, attr, duration, (s, t, d) =>
        {
            if(damageType != DamageType.All && damageType != d)
                return 0;
            if(valueType == ValueType.InstantNumber)
                return value;
            return s.GetBaseAttr(attr) * value;
        });
        AddBuff(tag, b);
    }

    public virtual void RemoveBuff(string tag)
    {
        Buff b = buffs[tag];
        buffs.Remove(tag);
        Utils.valueBuffPool.ReturnOne(b);
    }

}
