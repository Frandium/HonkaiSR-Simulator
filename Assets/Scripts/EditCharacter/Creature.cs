using System.Collections.Generic;
using UnityEditor.UI;
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
    public List<Buff> buffs { get; protected set; } = new List<Buff>();
    public List<State> states { get; protected set; } = new List<State>();
    public ACommomBattleTalents talents { get; protected set; }

    public delegate Damage DamageEvent(Creature sourceOrTarget, Damage damage);
    public DamageEvent d;
    public List<TriggerEvent<DamageEvent>> beforeTakingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();
    public List<TriggerEvent<DamageEvent>> beforeDealingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();
    public List<TriggerEvent<DamageEvent>> afterDealingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();
    public List<TriggerEvent<DamageEvent>> afterTakingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();

    public delegate Heal HealEvent(Creature sourceOrTarget, Heal heal);

    public List<TriggerEvent<HealEvent>> beforeTakingHeal { get; protected set; } = new List<TriggerEvent<HealEvent>>();
    public List<TriggerEvent<HealEvent>> afterTakingHeal { get; protected set; } = new List<TriggerEvent<HealEvent>>();
    public List<TriggerEvent<HealEvent>> beforeDealingHeal { get; protected set; } = new List<TriggerEvent<HealEvent>>();
    public List<TriggerEvent<HealEvent>> afterDealingHeal { get; protected set; } = new List<TriggerEvent<HealEvent>>();

    public delegate void TurnStartEndEvent();
    public List<TriggerEvent<TurnStartEndEvent>> onTurnStart { get; protected set; } = new List<TriggerEvent<TurnStartEndEvent>>();
    public List<TriggerEvent<TurnStartEndEvent>> onTurnEnd { get; protected set; } = new List<TriggerEvent<TurnStartEndEvent>>();

    public delegate void DyingEvent();
    public List<TriggerEvent<DyingEvent>> onDying { get; protected set; } = new List<TriggerEvent<DyingEvent>>();

    public List<Shield> shields = new List<Shield>();
    
    public Creature() { }

    public virtual float GetBaseAttr(CommonAttribute attr)
    {
        return attrs[(int)attr];
    }

    public virtual float GetFinalAttr(CommonAttribute attr, bool forView = false)
    {
        return GetFinalAttr(this, this, attr, DamageType.All, forView);
    }

    public virtual void SetMono(CreatureMono m)
    {
        mono = m;
    }

    public virtual float GetFinalAttr(Creature c1, Creature c2, CommonAttribute attr, DamageType damageType, bool forView = false)
    {
        // Ϊ�����㡾���� XX �����ʱ������������ XXX�����ֹ���
        float res = attrs[(int)attr];
        for(int i = buffs.Count - 1; i>=0; --i)
        {
            float b = buffs[i].CalBuffValue(c1, c2, attr, damageType);
            res += b;
            if(!forView && b > 0)
            {
                if (buffs[i].CountDown(CountDownType.Trigger))
                    buffs.RemoveAt(i);
            }
        }
        return res;
    }

    public void ChangePercentageLocation(float offset)
    {
        location += offset * Runway.Length;
        if (location < 0)
            location = 0;
        if (location > Runway.Length)
            location = Runway.Length;
    }

    public void ChangeAbsoluteLocation(float offset)
    {
        location += offset;
    }

    public void GetShield(Shield shield, bool removeIfExists = true)
    {
        if (removeIfExists)
            shields.RemoveAll(s => s.tag == shield.tag);
        mono?.ShowMessage("����+" + Mathf.RoundToInt(shield.maxHp), Color.black);
        shields.Add(shield);
    }

    public void RemoveShield()
    {

    }


    public void DealDamage(Creature target, Damage damage)
    {
        // �����˺���Ϊͬһ���˺������ᴥ������� damage �¼���
        if (damage.type != DamageType.CoAttack)
            foreach(var p in beforeDealingDamage)
            {
                p.trigger(target, damage);
            }
        target.TakeDamage(this, damage);
        if(damage.type != DamageType.CoAttack)
            foreach (var p in afterDealingDamage)
            {
                p.trigger(target, damage);
            }
    }

    public virtual void TakeDamage(Creature source, Damage damage)
    {
        // value ������
        float remain = damage.fullValue;
        foreach(var p in shields)
        {
            float r = - p.TakeDamage(damage);
            remain = Mathf.Min(r, remain);
        }
        shields.RemoveAll(s => s.CountDown(CountDownType.Trigger) || s.hp <= 0);
        remain = Mathf.Max(0, remain);
        damage.realValue = remain;

        foreach (var p in beforeTakingDamage)
        {
            p.trigger(source, damage);
        }

        hp -= damage.realValue;
        foreach (var p in afterTakingDamage)
        {
            p.trigger(source, damage);
        }
        if (hp < 0)
        {
            hp = 0;
            foreach(var p in onDying)
            {
                p.trigger();
            }
        }
        mono?.TakeDamage(damage);
    }

    public virtual void TakeHeal(Creature source, Heal heal)
    {
        foreach(var p in beforeTakingHeal) { p.trigger(source, heal); }
        heal.fullValue *= 1 + GetFinalAttr(CommonAttribute.HealedBonus);
        if (hp + heal.fullValue > GetFinalAttr(CommonAttribute.MaxHP))
        {
            heal.realValue = GetFinalAttr(CommonAttribute.MaxHP) - hp;
            hp = GetFinalAttr(CommonAttribute.MaxHP);
        }
        else
        {
            hp += heal.fullValue;
            heal.realValue = heal.fullValue;
        }
        foreach (var p in afterTakingHeal) { p.trigger(source, heal); }
        mono?.TakeHeal(heal.fullValue);
    }

    public virtual void DealHeal(Creature target, Heal heal)
    {
        foreach (var p in beforeDealingHeal) { p.trigger(target, heal); }
        target.TakeHeal(this, heal);
        foreach(var p in afterDealingHeal) { p.trigger(target, heal); }
    }

    public virtual bool StartNormalTurn()
    {
        foreach (var p in onTurnStart)
        {
            p.trigger();
        }
        mono?.StartMyTurn();

        if (states.Find(s => s.state == StateType.Frozen || s.state == StateType.Restricted) != null)
            return true;
        return false;
    }

    public virtual void EndNormalTurn()
    {
        // Remove event
        for(int i = onTurnEnd.Count - 1; i >= 0; --i)
        {
            onTurnEnd[i].trigger();
            if (onTurnEnd[i].CountDown(CountDownType.Trigger))
            {
                onTurnEnd.RemoveAt(i);
            }
        }
        onTurnEnd.RemoveAll(p => p.CountDown(CountDownType.Turn));
        onTurnStart.RemoveAll(p => p.CountDown(CountDownType.Turn));
        beforeDealingDamage.RemoveAll(p => p.CountDown(CountDownType.Turn));
        beforeTakingDamage.RemoveAll(p => p.CountDown(CountDownType.Turn));
        afterTakingDamage.RemoveAll(p => p.CountDown(CountDownType.Turn));
        afterDealingDamage.RemoveAll(p => p.CountDown(CountDownType.Turn));
        beforeDealingHeal.RemoveAll(p => p.CountDown(CountDownType.Turn));
        beforeTakingHeal.RemoveAll(p => p.CountDown(CountDownType.Turn));
        afterTakingHeal.RemoveAll(p => p.CountDown(CountDownType.Turn));
        afterDealingHeal.RemoveAll(p => p.CountDown(CountDownType.Turn));
        
        // Remove Buff
        for(int i = buffs.Count - 1; i >=0; --i)
        {
            if (buffs[i].CountDown(CountDownType.Turn))
            {
                buffs[i].onRemove?.Invoke(this);
                Utils.valueBuffPool.ReturnOne(buffs[i]);
                buffs.RemoveAt(i);
            }
        }

        // Remove shields
        shields.RemoveAll(s => s.CountDown(CountDownType.Turn));

        // Remove states
        states.RemoveAll(s => s.CountDown(CountDownType.Turn));
        
        mono?.EndMyTurn();
    }

    public virtual void AddBuff(Buff buff, bool removeIfExist = true)
    {
        if (removeIfExist)
        {
            buffs.RemoveAll(b => b.tag == buff.tag);
        }
        if (buff.buffType != BuffType.Permanent && buff.targetAttribute == CommonAttribute.MaxHP)
        {
            // �ǹ��е��������ޱ仯�ᴥ���������ޱ仯�߼�
            float oldMaxHP = GetFinalAttr(CommonAttribute.MaxHP);
            float newMaxHP = oldMaxHP + buff.CalBuffValue(this, this, CommonAttribute.MaxHP, DamageType.All);
            float oldPercentage = hp / oldMaxHP;
            float newPercentage = hp / newMaxHP;
            if(oldPercentage > newPercentage)
            {
                // �������޽���ʱ����������ֵ����
                hp = Mathf.Min(hp, newMaxHP);
            }
            else
            {
                // ������������ʱ����������ֵ�ٷֱȲ���
                hp = oldPercentage * newMaxHP;
            }
            mono?.UpdateHpLine();
        }
        buffs.Add(buff);
        
    }

    public virtual void AddBuff(string tag, BuffType buffType, CommonAttribute attr, ValueType valueType, float value, int turntime = int.MaxValue,
        DamageType damageType = DamageType.All, CountDownType cdtype = CountDownType.Turn, int triggertime = int.MaxValue,  int maxStack = 1)
    {
        DamageType dt = damageType;
        ValueType vt = valueType;
        float v = value;
        CommonAttribute a = attr;
        Buff b = Utils.valueBuffPool.GetOne().Set(tag, buffType, attr, (s, t, d) =>
        {
            if (dt != DamageType.All && d != dt)
                return 0;
            if (vt == ValueType.InstantNumber)
                return v;
            return s.GetBaseAttr(a) * v;
        },  turntime, cdtype, triggertime);
        if (maxStack > 1)
        {
            List<Buff> stacks = buffs.FindAll(t => t.tag == tag);
            if (stacks.Count >= maxStack)
            {
                int minDuration = int.MaxValue;
                foreach (Buff buff in stacks)
                {
                    minDuration = Mathf.Min(buff._turnTimes, minDuration);
                }
                Buff toremove = stacks.Find(b => b._turnTimes == minDuration);
                RemoveBuff(toremove);
            }
        }
        AddBuff(b, maxStack <= 1);
    }

    public virtual void RemoveBuff(string tag, bool removeAll = false)
    {
        if (removeAll)
        {
            List<Buff> toremoves = buffs.FindAll(b => b.tag == tag);
            RemoveBuff(toremoves);
        }
        else
        {
            Buff b = buffs.Find(p => p.tag == tag);
            RemoveBuff(b);
        }
    }
    public void RemoveBuff(Buff b)
    {
        if (b != null)
        {
            b.onRemove?.Invoke(this);
            buffs.Remove(b);
            Utils.valueBuffPool.ReturnOne(b);
        }       
    }

    public void RemoveBuff(ICollection<Buff> toremoves)
    {
        foreach(Buff b in toremoves)
        {
            b.onRemove?.Invoke(this);
            RemoveBuff(b);
        }
    }

    public virtual void AddState(Creature source, State state)
    {
        states.Add(state);
        mono?.UpdateState();
    }

    public virtual bool IsUnderState(StateType t)
    {
        return states.Find(s => s.state == t) != null;
    }


    public virtual void Initialize()
    {
        beforeTakingDamage.Clear();
        afterDealingDamage.Clear();
        afterTakingHeal.Clear();
        beforeDealingHeal.Clear();
        buffs.Clear();
        states.Clear();
        onTurnEnd.Clear();
        onTurnStart.Clear();
        onDying.Clear();
    }
}
