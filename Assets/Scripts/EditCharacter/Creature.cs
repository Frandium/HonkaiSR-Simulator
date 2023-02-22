using System.Collections.Generic;
using UnityEngine;

public class Creature
{
    // 无论在不在战斗中，生物都要有计算自身属性的能力
    // 都要有 Addbuff 的能力，因为 talents 可能是简单粗暴提升自身属性的
    // 角色和敌人都要有 LoadTalents 的能力，因为有可能 Talents
    // 不需要战斗相关的 hook，因为展示界面没有真的在战斗，而且不是 Mono
    // Mono 提供的能力其实就是更新自身血条、能量图标、runway 图标，这三个能力在角色和 Enemy 间是一致的。
    // 角色相比 Enemy 不同的是多了自身的 atk/skill/burst 图标等，还有 burst splash。
    public string dbname { get; protected set; } = "test";
    public string disname { get; protected set; } = "测试角色";

    protected float[] attrs = new float[(int)CommonAttribute.Count];
    public int level { get; protected set; } = 60;
    public float location { get; protected set; } = 0;
    public float hp = 100;
    public CreatureMono mono { get; protected set; }
    public List<Buff> buffs { get; protected set; } = new List<Buff>();

    public List<State> states { get; protected set; } = new List<State>();

    public delegate float DamageEvent(Creature sourceOrTarget, float value, Element element, DamageType type);
    public List<TriggerEvent<DamageEvent>> onTakingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();
    public List<TriggerEvent<DamageEvent>> onDealingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();

    public delegate float HealEvent(Creature sourceOrTarget, float value);

    public List<TriggerEvent<HealEvent>> onTakingHeal { get; protected set; } = new List<TriggerEvent<HealEvent>>();
    public List<TriggerEvent<HealEvent>> onDealingHeal { get; protected set; } = new List<TriggerEvent<HealEvent>>();

    public delegate void TurnStartEndEvent();
    public List<TriggerEvent<TurnStartEndEvent>> onTurnStart { get; protected set; } = new List<TriggerEvent<TurnStartEndEvent>>();
    public List<TriggerEvent<TurnStartEndEvent>> onTurnEnd { get; protected set; } = new List<TriggerEvent<TurnStartEndEvent>>();

    public List<Shield> shields = new List<Shield>();
    
    public Creature() { }

    public virtual float GetBaseAttr(CommonAttribute attr)
    {
        return attrs[(int)attr];
    }

    public virtual float GetFinalAttr(CommonAttribute attr)
    {
        return GetFinalAttr(this, this, attr, DamageType.All);
    }

    public virtual void SetMono(CreatureMono m)
    {
        mono = m;
    }

    public virtual float GetFinalAttr(Creature c1, Creature c2, CommonAttribute attr, DamageType damageType)
    {
        // 为了满足【攻击 XX 类敌人时，攻击力提升 XXX】这种功能
        float res = attrs[(int)attr];
        foreach (Buff buff in buffs)
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

    public void GetShield(Shield shield, bool removeIfExists = true)
    {
        if (removeIfExists)
            shields.RemoveAll(s => s.tag == shield.tag);
        
        shields.Add(shield);
    }

    public void RemoveShield()
    {

    }


    public void DealDamage(Creature target, Element element, DamageType type, float value)
    {
        // 先 take damage，再触发事件，不然 damage 可能就错了
        target.TakeDamage(this, value, element, type);
        List<string> toremove = new List<string>();
        foreach (var p in onDealingDamage)
        {
            p.trigger(target, value, element, type);
        }
        onDealingDamage.RemoveAll(p => p.CountDown());
    }

    public virtual void TakeDamage(Creature source, float value, Element element, DamageType type)
    {
        // value 是正数
        foreach (var p in onTakingDamage)
        {
            p.trigger(source, value, element, type);
        }
        onTakingDamage.RemoveAll(p => p.CountDown());

        float remain = value;
        foreach(var p in shields)
        {
            float r = - p.TakeDamage(value);
            remain = Mathf.Min(r, remain);
        }
        shields.RemoveAll(s => s.CountDown() || s.hp <= 0);
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
        foreach (var p in onTakingHeal)
        {
            p.trigger(source, value);
        }
        onTakingHeal.RemoveAll(p => p.CountDown());
        mono?.TakeHeal(value);
    }

    public virtual void DealHeal(Creature target, float value)
    {
        foreach (var p in onDealingHeal)
        {
            p.trigger(target, value);
        }
        onDealingHeal.RemoveAll(p => p.CountDown());
        target.TakeHeal(this, value);
    }

    public virtual bool StartNormalTurn()
    {
        foreach (var p in onTurnStart)
        {
            p.trigger();
        }
        onTurnStart.RemoveAll(p => p.CountDown());
        mono?.StartMyTurn();

        if (states.Find(s => s.state == StateType.Frozen) != null)
            return true;
        return false;
    }

    public virtual void EndNormalTurn()
    {
        // Remove event
        foreach (var p in onTurnEnd)
        {
            p.trigger();
        }
        onTurnEnd.RemoveAll(p => p.CountDown());
        
        // Remove Buff
        for(int i = buffs.Count - 1; i >=0; --i)
        {
            if (buffs[i].CountDown())
            {
                Utils.valueBuffPool.ReturnOne(buffs[i]);
                buffs.RemoveAt(i);
            }
        }

        // Remove shields
        shields.RemoveAll(s => s.CountDown());

        // Remove states
        states.RemoveAll(s => s.CountDown());
        
        mono?.EndMyTurn();
    }

    public virtual void AddBuff(Buff buff, bool removeIfExist = true)
    {
        if (removeIfExist)
        {
            buffs.RemoveAll(b => b.tag == buff.tag);
        }
        buffs.Add(buff);
    }

    public virtual void AddBuff(string tag, BuffType buffType, CommonAttribute attr, ValueType valueType, float value, int duration = int.MaxValue, DamageType damageType = DamageType.All)
    {
        DamageType dt = damageType;
        ValueType vt = valueType;
        float v = value;
        CommonAttribute a = attr;
        Buff b = Utils.valueBuffPool.GetOne().Set("default", buffType, attr, duration, (s, t, d) =>
        {
            if(dt != DamageType.All && d != dt)
                return 0;
            if(vt == ValueType.InstantNumber)
                return v;
            return s.GetBaseAttr(a) * v;
        });
        AddBuff(b, false);
    }

    public virtual void RemoveBuff(string tag)
    {
        Buff b = buffs.Find(p => p.tag == tag);
        if (b != null)
        {
            buffs.Remove(b);
            Utils.valueBuffPool.ReturnOne(b);
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

}
