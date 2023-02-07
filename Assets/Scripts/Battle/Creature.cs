using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Creature : MonoBehaviour
{
    public readonly static Color[] ElementColors = { new Color(.25f, .875f, .625f, 1), new Color(.875f, .75f, 0, 1), new Color(0, .25f, 1, 1), new Color(1, 0, 0, 1), new Color(0, .875f, .875f, 1), new Color(.5f, 0, 1, 1), new Color(0, .625f, .625f, 0), new Color(.375f, .375f, .375f, 1) };

    public Creature()
    {
        attributes = new float[(int)CommonAttribute.Count];
    }

    public int uniqueID { get; protected set; } = -1;
    public float location { get; protected set; } = 0;
    public ACommomBattleTalents talents { get; protected set; }

    // UI Binding
    public Sprite runwayAvatar;
    public Image hpLine;
    public Text dmgText;
    public GameObject dmgGO;
    public SpriteRenderer cardSR;
    public SpriteRenderer selectedSR;
    public Image eleImage;
    public Image buffImage;
    public AudioClip[] attackAudios;
    public AudioClip[] takeDamageAudios;
    public AudioSource audioSource;

    // Animation
    protected bool isSelected = false;
    protected bool isMyTurn = false;
    protected float alpha = 0;
    protected float alphaSpeed = 1;
    protected float alphaDirection = 1;
    protected float dmgAnimTime = 2f;
    protected float dmgBgBaseAlpha = .5f;
    public bool IsPerformanceFinished
    {
        get { return isAnimFinished && isAudioFinished; }
        protected set { isAudioFinished = value; isAnimFinished = value; }
    }
    protected bool isAnimFinished = true;
    protected bool isAudioFinished = true;

    // Battle Attributes
    public string databaseName { get; protected set; } = "default";
    public string displayName { get; protected set; } = "默认名字";
    public float hp { get; protected set; }
    public float hpPercentage
    {
        get
        {
            return hp / GetFinalAttr(CommonAttribute.MaxHP);
        }
    }
    public bool isAlive
    {
        get
        {
            return hp > 0;
        }
    }
    protected float[] attributes;
    public Element elementState { get; protected set; } = Element.Count;
    public ElementBuff elementBuff { get; protected set; } = ElementBuff.Count;
    List<TriggerBuff> triggerBuffs = new List<TriggerBuff>();
    List<ValueBuff> valueBuffs = new List<ValueBuff>();

    public void SetLocation(float new_location)
    {
        location = new_location;
    }

    public float GetBaseAttr(int attr)
    {
        return attributes[attr];
    }

    public float GetBaseAttr(CommonAttribute attr)
    {
        return GetBaseAttr((int)attr);
    }

    public float GetBaseAttr(CharacterAttribute attr)
    {
        return GetBaseAttr((int)attr);
    }

    public float GetBaseAttr(EnemyAttribute attr)
    {
        return GetBaseAttr((int)attr);
    }

    public float GetFinalAttr(int attr)
    {
        float b = attributes[attr];
        float p = 1, n = 0;
        foreach(ValueBuff buff in valueBuffs)
        {
            if (buff.attributeType != (int)attr)
                continue;
            if (buff.valueType == ValueType.Percentage)
                p += buff.value;
            else
                n += buff.value;
        }
        return b * p + n;
    }

    public float GetFinalAttr(CommonAttribute attr)
    {
        return GetFinalAttr((int)attr);
    }

    public float GetFinalAttr(CharacterAttribute attr)
    {
        return GetFinalAttr((int)attr);
    }

    public float GetFinalAttr(EnemyAttribute attr)
    {
        return GetFinalAttr((int)attr);
    }

    public delegate void Then();


    //Battle functions
    public virtual void TakeDamage(Creature source, float value, Element element, DamageType type, Then then = null)
    {
        talents.OnTakingDamage(source, value, element, type);
        value = DamageCal.ResistDamage(value, element, this);
        hp -= value;
        hpLine.fillAmount = hpPercentage;
        ElementalReaction(element);
        TriggerBuffsAtMoment(BuffTriggerMoment.OnTakingDamage);
        StartCoroutine(TakeDamangeAnim(Mathf.RoundToInt(-value), () =>
        {
            if (hp < 0)
            {
                OnDying();
            }
        }));
    }

    void ElementalReaction(Element e)
    {
        // 物理不反应
        if (e == Element.Physical) return; 
        // 当前无元素，不反应，但是可以挂上风岩之外的元素
        if (elementState == Element.Count && e != Element.Anemo && e != Element.Geo)
        {
            elementState = e;
        }
        else if (elementState == e)
        {
            // 同样的元素，不反应
        }
        else // 发生反应，清空元素附着
        {
            if ((elementState == Element.Hydro && e == Element.Cryo) ||
                 (elementState == Element.Cryo && e == Element.Hydro))
            { // 冻结
                elementBuff = ElementBuff.Frozen;
                cardSR.color = Color.blue;
            }
            else if (e == Element.Anemo)
            {
                // 扩散？
            }
            elementState = Element.Count;
        }
        eleImage.sprite = BattleManager.Instance.elementSymbols[(int)elementState];
    }

    public virtual void TakeHeal(float  value, Creature source, Then then = null)
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
        hpLine.fillAmount = hpPercentage;
        TriggerBuffsAtMoment(BuffTriggerMoment.OnHealed);
        StartCoroutine(TakeDamangeAnim(Mathf.RoundToInt(value), then));
    }

    public virtual void TakeElementOnly(Creature source, Element element)
    {
        ElementalReaction(element);
    }

    protected virtual void OnDying()
    {
        BattleManager.Instance.runway.RemoveCreature(this);
        gameObject.SetActive(false);
    }

    protected virtual IEnumerator TakeDamangeAnim(int dmg, Then then = null)
    {
        isAnimFinished = false;
        if (dmg < 0)
            PlayAudio(AudioType.TakeDamage);
        dmgGO.SetActive(true);
        dmgText.text = dmg>0 ? "+" + dmg.ToString() :  dmg.ToString();
        RectTransform rect = dmgGO.GetComponent<RectTransform>();
        Image dmgBgImg = dmgGO.GetComponent<Image>();
        dmgBgImg.color = new Color(1, 1, 1, dmgBgBaseAlpha);
        rect.localPosition = new Vector3(0, 0, 0);
        dmgText.color = Color.white;
        float dmgAlpha = 1;
        float alphaFadeSpeed = 1 / dmgAnimTime;
        float dmgBgSpeed = (6 - 2.5f) / dmgAnimTime;
        while (rect.localPosition.y < 6)
        {
            rect.localPosition += Vector3.up * dmgBgSpeed * Time.deltaTime;
            dmgBgImg.color = new Color(1, 1, 1, dmgBgBaseAlpha * dmgAlpha);
            dmgText.color = new Color(0, 0, 0, dmgAlpha);
            dmgAlpha -= alphaFadeSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        dmgGO.SetActive(false);
        isAnimFinished = true;
        then?.Invoke();
    }

    public virtual bool StartMyTurn()
    {
        isMyTurn = true;
        alpha = 1;
        TriggerBuffsAtMoment(BuffTriggerMoment.OnTurnBegin);
        if(elementBuff == ElementBuff.Frozen)
        {
            elementBuff = ElementBuff.Count;
            cardSR.color = Color.white;
            return true;
        }
        return false;
    }

    public virtual void EndMyTurn()
    {
        isMyTurn = false;
        alpha = 0;
        selectedSR.color = new Color(0, 0, 0, 0);
        TriggerBuffsAtMoment(BuffTriggerMoment.OnTurnEnd);
        for (int i = valueBuffs.Count - 1; i >= 0; --i)
        {
            ValueBuff b = valueBuffs[i];
            if (b.Progress())
            {
                valueBuffs.Remove(b);
                BattleManager.Instance.valueBuffPool.ReturnOne(b);
            }
        }
        UpdateBuffIcon();
    }

    public virtual void Initialize(string dbN, int id)
    {
        location = 0;
        BattleManager.Instance.runway.AddCreature(this);
        uniqueID = id;
        databaseName = dbN;
        hp = GetFinalAttr(CommonAttribute.MaxHP);
        hpLine.fillAmount = hp / GetFinalAttr(CommonAttribute.MaxHP);
    }

    public virtual void AddBuff(ValueBuff buff, Then then = null)
    {
        valueBuffs.Add(buff);
        if (buff.buffType == BuffType.Debuff) {
            buffImage.sprite = BattleManager.Instance.debuffSprite;
        } 
        else if (buffImage.sprite = BattleManager.Instance.nullBuffSprite) {
            buffImage.sprite = BattleManager.Instance.buffSprite;
        }
        hpLine.fillAmount = hpPercentage;
        StartCoroutine(InvokeNextFrame(then));
    }

    IEnumerator InvokeNextFrame(Then then)
    {
        yield return new WaitForEndOfFrame();
        then?.Invoke();
    }

    public void AddBuff(TriggerBuff buff, Then then = null)
    {
        triggerBuffs.Add(buff);
        then?.Invoke();
    }

    protected void TriggerBuffsAtMoment(BuffTriggerMoment moment)
    {
        for (int i = triggerBuffs.Count - 1; i >= 0; --i)
        {
            TriggerBuff b = triggerBuffs[i];
            if (b.triggerMoment == moment && b.Trigger(this)){
                triggerBuffs.Remove(b);
                BattleManager.Instance.triggerBuffPool.ReturnOne(b);
            }
        }
    }

    // UI functions
    public void SetUnselected()
    {
        isSelected = false;
        alpha = 0;
        selectedSR.color = new Color(0, 0, 0, 0);
    }

    public virtual void SetSelected()
    {
        alpha = 1;
        isSelected = true;
        selectedSR.color = Color.red;
    }

    void UpdateBuffIcon()
    {
        if (valueBuffs.Count == 0)
        {
            buffImage.sprite = BattleManager.Instance.nullBuffSprite;
            return;
        }
        buffImage.sprite = BattleManager.Instance.buffSprite;
        foreach (ValueBuff b in valueBuffs)
        {
            if (b.buffType == BuffType.Debuff)
            {
                buffImage.sprite = BattleManager.Instance.debuffSprite;
                return;
            }
        }
    }

    public virtual void PlayAudio(AudioType audioType)
    {
        AudioClip[] audios = attackAudios;
        switch (audioType)
        {
            case AudioType.Attack:
                audios = attackAudios;
                break;
            case AudioType.TakeDamage:
                audios = takeDamageAudios;
                break;
            default:
                break;
        }
        if (audios.Length <= 0)
            return;
        AudioClip clip = audios[Random.Range(0, audios.Length)];
        audioSource.clip = clip;
        audioSource.Play();
        StartCoroutine(SetAudioFinish(clip.length));
    }

    protected IEnumerator SetAudioFinish(float s)
    {
        isAudioFinished = false;
        yield return new WaitForSeconds(s);
        isAudioFinished = true;
    }
}
