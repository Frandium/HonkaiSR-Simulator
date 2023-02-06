using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Element
{
    Anemo, // 风 = 0
    Geo, // 岩 = 1
    Hydro, // 水 = 2
    Pyro, //火 = 3
    Cryo, // 冰 = 4
    Electro, //雷 = 6
    Dendro, // 草 = 6
    Physical, // 物理 = 7
    Count
}

public class Creature : MonoBehaviour
{
    public readonly static Color[] ElementColors = { new Color(.25f, .875f, .625f, 1), new Color(.875f, .75f, 0, 1), new Color(0, .25f, 1, 1), new Color(1, 0, 0, 1), new Color(0, .875f, .875f, 1), new Color(.5f, 0, 1, 1), new Color(0, .625f, .625f, 0), new Color(.375f, .375f, .375f, 1) };

    public int uniqueID { get; protected set; } = -1;
    public float speed { get; protected set; } = 100;

    public float location { get; set; } = 0;

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

    protected bool isSelected = false;

    protected bool isMyTurn = false;
    protected float alpha = 0;
    protected float alphaSpeed = 1;
    protected float alphaDirection = 1;

    public Element elementState { get; protected set; } = Element.Count;

    public string databaseName { get; protected set; } = "default";
    public string displayName { get; protected set; } = "默认名字";
 

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

    public float hp { get; protected set; } = 100;
    public float maxHp { get; protected set; } = 100;
    public float atk { get; protected set; } = 10;
    public float atkBuff { get; protected set; } = 0;
    public float def { get; protected set; }  = 5;
    public float defBuff { get; protected set; }  = 0;
    // 造成的伤害加成
    public float generalBonus { get; protected set; }  = 0;
    public float generalBonusBuff { get; protected set; }  = 0;
    // 物理/元素伤害加成，
    public float[] elementalBonus { get; protected set; }  = { 0, 0, 0, 0, 0, 0, 0, 0 };
    public float[] elementalBonusBuff { get; protected set; } = { 0, 0, 0, 0, 0, 0, 0, 0 };
    // 物理/元素伤害抗性
    public float[] elementalResist { get; protected set; }  = { .1f, .1f, .1f, .1f, .1f, .1f, .1f, .1f };
    public float[] elementalResistBuff { get; protected set; }  = { 0, 0, 0, 0, 0, 0 ,0, 0 };
    // 受到的伤害抗性
    public float generalResist { get; protected set; } = 0;
    public float generalResistBuff { get; protected set; }  = 0;

    public delegate void Then();

    public bool IsPerformanceFinished { 
        get { return isAnimFinished && isAudioFinished; }
        protected set { isAudioFinished = value; isAnimFinished = value; } 
    }
    protected bool isAnimFinished = true;
    protected bool isAudioFinished = true;

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
        StartCoroutine(TakeDamangeAnim(Mathf.RoundToInt(value), then));
    }

    public virtual void TakeElementOnly(Creature source, Element element)
    {
        ElementReaction(element);
    }

    protected float dmgAnimTime = 2f;
    protected float dmgBgBaseAlpha = .5f;

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
    }

    public virtual void EndMyTurn()
    {
        isMyTurn = false;
        alpha = 0;
        selected.color = new Color(0, 0, 0, 0);
        for(int i = valueBuffs.Count - 1; i >= 0; --i)
        {
            ValueBuff b = valueBuffs[i];
            if (b.Progress())
            {
                RemoveBuffEffect(b);
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

    void RemoveBuffEffect(ValueBuff b)
    {
        switch (b.attributeType)
        {
            case AttributeType.CryoBonus:
                elementalBonusBuff[(int)Element.Cryo] -= b.value;
                break;
            case AttributeType.CryoResist:
                elementalResistBuff[(int)Element.Cryo] -= b.value;
                break;
            default:
                break;
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
            case AttributeType.CryoBonus:
                elementalBonusBuff[(int)Element.Cryo] += buff.value;
                break;
            case AttributeType.CryoResist:
                elementalResistBuff[(int)Element.Cryo] += buff.value;
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
        foreach (TriggerBuff tb in triggerBuffs)
        {
            if (tb.triggerMoment == moment)
                tb.Trigger(this);
        }
        // triggerBuffs.RemoveAll(b => b.triggerMoment == moment);
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
