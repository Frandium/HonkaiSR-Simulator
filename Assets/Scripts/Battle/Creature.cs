using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.UI;
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

    public delegate bool TurnStartEvent();
    public delegate void TurnEndEvent();
    public List<TriggerEvent<TurnStartEvent>> onTurnStart { get; protected set; } = new List<TriggerEvent<TurnStartEvent>>();
    public List<TriggerEvent<TurnEndEvent>> onTurnEnd { get; protected set; } = new List<TriggerEvent<TurnEndEvent>>();

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
        return GetFinalAttr(this, this, attr, new DamageConfig(), forView);
    }

    public virtual void SetMono(CreatureMono m)
    {
        mono = m;
    }

    public virtual float GetFinalAttr(Creature c1, Creature c2, CommonAttribute attr, DamageConfig damageAttr, bool forView = false)
    {
        // 为了满足【攻击 XX 类敌人时，攻击力提升 XXX】这种功能
        float res = attrs[(int)attr];
        for(int i = buffs.Count - 1; i>=0; --i)
        {
            float b = buffs[i].CalBuffValue(c1, c2, attr, damageAttr);
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
        mono?.ShowMessage("护盾+" + Mathf.RoundToInt(shield.maxHp), Color.black);
        shields.Add(shield);
    }

    public void RemoveShield()
    {

    }


    public void DealDamage(Creature target, Damage damage)
    {
        // 附加伤害视为同一次伤害，不会触发额外的 damage 事件了
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
        // value 是正数
        float remain = damage.fullValue;
        foreach(var p in shields)
        {
            float r = - p.TakeDamage(damage);
            remain = Mathf.Min(r, remain);
        }
        shields.RemoveAll(s => s.CountDown(CountDownType.Trigger) || s.hp <= 0);
        remain = Mathf.Max(0, remain);
        damage.realValue = remain;

        if (damage.realValue <= 0)
        {
            mono?.TakeDamage(damage);
            return;
        }

        if (damage.type != DamageType.CoAttack)
            foreach (var p in beforeTakingDamage)
            {
                p.trigger(source, damage);
            }

        hp -= damage.realValue;
        mono?.TakeDamage(damage);

        if(damage.type != DamageType.CoAttack)
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
        bool canMoveThisTurn = IsUnderControlledDebuff();
        mono?.StartMyTurn();
        // 必须先调用 mono 再调用 trigger，因为 trigger 的表现需要 mono
        foreach (var p in onTurnStart)
        {
            canMoveThisTurn = p.trigger() && canMoveThisTurn;
        }

        return canMoveThisTurn;
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
        for(int i = states.Count - 1; i >= 0; --i) {
            if (states[i].CountDown(CountDownType.Turn))
            {
                states[i].onremove();
                states.RemoveAt(i);
            }
        }
        
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
            // 非固有的生命上限变化会触发生命上限变化逻辑
            float oldMaxHP = GetFinalAttr(CommonAttribute.MaxHP);
            float newMaxHP = oldMaxHP + buff.CalBuffValue(this, this, CommonAttribute.MaxHP, DamageConfig.defaultDC);
            float oldPercentage = hp / oldMaxHP;
            float newPercentage = hp / newMaxHP;
            if(oldPercentage > newPercentage)
            {
                // 生命上限降低时，保持生命值不变
                hp = Mathf.Min(hp, newMaxHP);
            }
            else
            {
                // 生命上限提升时，保持生命值百分比不变
                hp = oldPercentage * newMaxHP;
            }
            mono?.UpdateHpLine();
        }
        buffs.Add(buff);
        
    }

    public virtual void AddBuff(string tag, BuffType buffType, CommonAttribute attr, ValueType valueType, float value, int turntime = int.MaxValue,
        DamageType damageType = DamageType.All, CountDownType cdtype = CountDownType.Turn, int triggertime = int.MaxValue, int maxStack = 1)
    {
        DamageType dt = damageType;
        AddBuff(tag, buffType, attr, valueType, value, (s, t, d) => { return dt == DamageType.All || d.type == dt; },
            turntime, cdtype, triggertime, maxStack);
    }

    public virtual void AddBuff(string tag, BuffType buffType, CommonAttribute attr, ValueType valueType, float value, Buff.BuffFilter f,
        int turntime = int.MaxValue, CountDownType cdtype = CountDownType.Turn, int triggertime = int.MaxValue,  int maxStack = 1)
    {
        ValueType vt = valueType;
        float v = value;
        CommonAttribute a = attr;
        AddBuff(tag, buffType, attr, (s, t, d) =>
        {
            if (vt == ValueType.InstantNumber)
                return v;
            return s.GetBaseAttr(a) * v;
        }, f, turntime, cdtype, triggertime, maxStack);
    }

    public virtual void AddBuff(string tag, BuffType buffType, CommonAttribute attr, Buff.BuffContent c, Buff.BuffFilter f, int turntime = int.MaxValue,
        CountDownType cdtype = CountDownType.Turn, int triggertime = int.MaxValue, int maxStack = 1, Buff.OnBuffRemove onremove = null)
    {
        CommonAttribute a = attr;
        Buff b = Utils.valueBuffPool.GetOne().Set(tag, buffType, attr, c, f, turntime, cdtype, triggertime, onremove);
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
        float resist = 1 - 1 / (1 + GetFinalAttr(source, this, CommonAttribute.EffectResist, DamageConfig.defaultDC));
        if(!Utils.TwoRandom(resist))
        {
            states.Add(state);
            mono?.UpdateState();
            switch (state.state)
            {
                case StateType.Frozen:
                    mono?.ShowMessage("冻结", CreatureMono.CryoColor);
                    break;
                case StateType.Restricted:
                    mono?.ShowMessage("束缚", CreatureMono.ImaginaryColor);
                    break;
                case StateType.Burning:
                    mono?.ShowMessage("灼烧", CreatureMono.PyroColor); 
                    break;
                default:
                    break;
            }
        }
        else
        {
            mono?.ShowMessage("抵抗", Color.black);
        }
    }

    public virtual void RemoveState(StateType t)
    {
        for (int i = states.Count - 1; i >= 0; --i)
        {
            if (states[i].state == t)
            {
                states[i].onremove();
                states.RemoveAt(i);
            }
        }
        mono?.UpdateState();
    }

    public virtual bool IsUnderState(StateType t)
    {
        return states.Find(s => s.state == t) != null;
    }

    public virtual bool IsUnderControlledDebuff()
    {
        return states.Find(s => 
            s.state == StateType.Frozen || 
            s.state == StateType.Restricted
            ) != null;
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
