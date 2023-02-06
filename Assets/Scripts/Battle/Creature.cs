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
    
    public float location { get; set; } = 0;

    // UI Binding
    public Sprite runwayAvatar;// { get; set; }
    public Image hpLine;
    public Text dmgText;
    public GameObject dmgGO;

    public SpriteRenderer selected;

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


    public Element elementState { get; protected set; } = Element.Count;


    public void SetLocation(float new_location)
    {
        location = new_location;
    }

    public void SetUnselected()
    {
        isSelected = false;
        alpha = 0;
        selected.color = new Color(0, 0, 0, 0);
    }

    public virtual void SetSelected()
    {
        alpha = 1;
        isSelected = true;
        selected.color = Color.red;
    }

    // Battle Attributes
    public string databaseName { get; protected set; } = "default";
    public string displayName { get; protected set; } = "默认名字";

    public float hp { 
        get {
            return attributes[(int)CommonAttribute.HP];
        }
        protected set {
            attributes[(int)CommonAttribute.HP] = value;
        }
    }

    public float maxHp
    {
        get
        {
            return attributes[(int)CommonAttribute.MaxHP];
        }
        protected set
        {
            attributes[(int)CommonAttribute.MaxHP] = value;
        }
    }
    public float speed
    {
        get
        {
            return attributes[(int)CommonAttribute.Speed];
        }
        protected set
        {
            attributes[(int)CommonAttribute.Speed] = value;
        }
    }

    public float atk
    {
        get
        {
            return attributes[(int)CommonAttribute.ATK];
        }
        protected set
        {
            attributes[(int)CommonAttribute.ATK] = value;
        }
    }

    public float def
    {
        get
        {
            return attributes[(int)CommonAttribute.DEF];
        }
        protected set
        {
            attributes[(int)CommonAttribute.DEF] = value;
        }
    }

    public float cryoResist
    {
        get
        {
            return attributes[(int)CommonAttribute.MaxHP];
        }
        protected set
        {
            attributes[(int)CommonAttribute.MaxHP] = value;
        }
    }

    protected float[] attributes;

    public float GetBaseAttr(int attr)
    {
        return attributes[attr];
    }

    public float GetFinalAttr(int attr)
    {
        float b = attributes[attr];
        float p = 0, n = 1;
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

    public delegate void Then();

    List<TriggerBuff> triggerBuffs = new List<TriggerBuff>();
    List<ValueBuff> valueBuffs = new List<ValueBuff>();

    public virtual void TakeDamage(float value, Creature source, Element element, Then then = null)
    {
        value = DamageCal.ResistDamage(value, element, this);
        hp -= value;
        hpLine.fillAmount = hp / maxHp;
        ElementReaction(element);
        // 执行受到伤害时的buff
        TriggerBuffsAtMoment(BuffTriggerMoment.OnTakingDamage);
        StartCoroutine(TakeDamangeAnim(Mathf.RoundToInt(-value), () =>
        {
            if (hp < 0)
            {
                OnDying();
            }
        }));
    }

    void ElementReaction(Element e)
    {
        if (e == Element.Physical) return;
        if (elementState == Element.Count)
        {
            if (e == Element.Anemo || e == Element.Geo)
            {

            }
            else
            {
                elementState = e;
            }
        }
        else if (elementState != e)
        {
            elementState = Element.Count;
        }
        eleImage.sprite = BattleManager.Instance.elementSymbols[(int)elementState];
    }

    public virtual void TakeHeal(float  value, Creature source, Then then = null)
    {
        if (hp + value > maxHp)
        {
            value = maxHp - hp;
            hp = maxHp;
        }
        else
        {
            hp += value;
        }
        hpLine.fillAmount = hp / maxHp;
        TriggerBuffsAtMoment(BuffTriggerMoment.OnHealed);
        StartCoroutine(TakeDamangeAnim(Mathf.RoundToInt(value), then));
    }

    public virtual void TakeElementOnly(Creature source, Element element)
    {
        ElementReaction(element);
    }


    protected virtual void OnDying()
    {
        BattleManager.Instance.runway.RemoveFromRunway(this);
        Destroy(this.gameObject);
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

    public virtual void StartMyTurn()
    {
        isMyTurn = true;
        alpha = 1;
        TriggerBuffsAtMoment(BuffTriggerMoment.OnTurnBegin);
    }

    public virtual void EndMyTurn()
    {
        isMyTurn = false;
        alpha = 0;
        selected.color = new Color(0, 0, 0, 0);
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


    public virtual void Initialize(string dbN, int id)
    {
        uniqueID = id;
        databaseName = dbN;
        hp = maxHp;
        hpLine.fillAmount = hp / maxHp;
    }

    public float hpPercentage {
        get
        {
            return hp / maxHp;
        }
    }

    public bool isAlive {
        get {
            return hp > 0;
        } 
    }

    public virtual void AddBuff(ValueBuff buff, Then then = null)
    {
        valueBuffs.Add(buff);
        switch (buff.attributeType)
        {
            case (int)CharacterAttribute.CryoBonus:
                // elementalBonusBuff[(int)Element.Cryo] += buff.value;
                break;
            case (int)CommonAttribute.CryoResist:
                // elementalResistBuff[(int)Element.Cryo] += buff.value;
                break;
            default:
                break;
        }
        if (buff.buffType == BuffType.Debuff)
        {
            buffImage.sprite = BattleManager.Instance.debuffSprite;

        }
        else if (buffImage.sprite = BattleManager.Instance.nullBuffSprite)
        {
            buffImage.sprite = BattleManager.Instance.buffSprite;
        }
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
