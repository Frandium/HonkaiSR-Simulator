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
    public List<TriggerEvent<DamageEvent>> beforeDealingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();
    public List<TriggerEvent<DamageEvent>> beforeTakingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();
    public List<TriggerEvent<DamageEvent>> beforeDying { get; protected set; } = new List<TriggerEvent<DamageEvent>>();
    public List<TriggerEvent<DamageEvent>> afterTakingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();
    public List<TriggerEvent<DamageEvent>> afterDealingDamage { get; protected set; } = new List<TriggerEvent<DamageEvent>>();

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

        // 如果一个附加伤害在造成伤害前已经死了，还要触发它的击杀 trigger 吗？
        // 或者说，算在谁头上呢？附加伤害的伤害来源算谁的啊……
        if (hp <= 0)
        {
            // 触发急救事件，这里要修改白露和杰帕德的逻辑
            for(int i = 0; i < beforeDying.Count; i++) {
                beforeDying[i].trigger(source, damage);
                if (hp > 0)
                {
                    if (beforeDying[i].CountDown(CountDownType.Trigger))
                    {
                        beforeDying.RemoveAt(i);
                    }
                    break;
                }
            }
        }
        // 没救活
        if(hp <= 0)
        {
            hp = 0;
            foreach (var p in onDying)
            {
                p.trigger();
            }
        }else if (damage.type != DamageType.CoAttack)
            foreach (var p in afterTakingDamage)
            {
                p.trigger(source, damage);
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
        bool canMoveThisTurn = IsUnderControlledState();
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
                states[i].onremove?.Invoke();
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

    public virtual void AddAnemoPhyQuant(Creature source, StateType s, int turn, float instantRate, 
        float continualRate, CommonAttribute dmgAttr = CommonAttribute.ATK)
    {
        // 风 物理 量子
        Damage d = Damage.NormalDamage(source, this, dmgAttr, instantRate, new DamageConfig(DamageType.Continue, (Element)s, s));
        source.DealDamage(this, d);
        AddPyroElecCryo(source, s, turn, continualRate, dmgAttr);
    }

    public virtual void AddPyroElecCryo(Creature source, StateType s, int turn, float rate, CommonAttribute dmgAttr = CommonAttribute.ATK)
    {
        // 风 物理 量子 fall in this
        // 火 雷 冰 start with this
        onTurnStart.Add(new TriggerEvent<TurnStartEvent>(source.dbname, () =>
        {
            if (IsUnderState(s))
            {
                Damage d = Damage.NormalDamage(source, this, dmgAttr, rate, new DamageConfig(DamageType.Continue, (Element)s, s));
                source.DealDamage(this, d);
            }
            return true;
        }));
        AddState(source, new State(s, turn, () => {
            onTurnStart.RemoveAll(t => t.tag == source.dbname);
        }));
    }

    public virtual void AddRestricted(Creature source, int turn, float back, float speedRate)
    {
        ChangePercentageLocation(back);
        AddBuff(source.dbname + "Restricted", BuffType.Permanent, CommonAttribute.Speed, ValueType.Percentage, speedRate, (_, _, _) =>
        {
            return IsUnderState(StateType.Restricted);
        });
        AddState(source, new State(StateType.Restricted, turn));
    }

    public virtual void AddState(Creature source, State state)
    {
        State cur = states.Find(s => s.state == state.state);
        if(cur == null)
        {
            states.Add(state);
        }
        else
        {
            cur.ChangeTurnTime(Mathf.Max(0, state._turnTimes - cur._turnTimes));
            cur.onremove += state.onremove;
        }
        mono?.UpdateState();
        mono?.ShowMessage(state.state switch
        {
            StateType.Split => "裂伤",
            StateType.Weathered => "风化",
            StateType.Burning => "灼烧",
            StateType.Electric => "触电",
            StateType.Frozen => "冻结",
            StateType.Restricted => "禁锢",
            StateType.Entangle => "纠缠",
            _ => "未知状态"
        }, CreatureMono.ElementColors[(int)state.state]);
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

    public virtual bool IsUnderControlledState()
    {
        return states.Find(s => 
            s.state == StateType.Frozen || 
            s.state == StateType.Restricted ||
            s.state == StateType.Entangle
            ) != null;
    }

    public virtual bool IsUnderNegativeState()
    {
        return states.Find(s =>
            s.state == StateType.Frozen ||
            s.state == StateType.Restricted ||
            s.state == StateType.Burning
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

    public delegate void EffectAction();
    public void TestAndAddEffect(float baseProb, Creature target, EffectAction action,  DamageConfig dc = null)
    {
        if (dc == null)
            dc = DamageConfig.defaultDC;
        if( Utils.TwoRandom(baseProb * (1 + GetFinalAttr(this, target, CommonAttribute.EffectHit, dc))) && 
            ! Utils.TwoRandom(1 - 1 / (1 + target.GetFinalAttr(this, target, CommonAttribute.EffectResist, dc))))
        {
            action?.Invoke();
        }
        else
        {
            target.mono?.ShowMessage("抵抗", Color.black);
        }
    }
}
