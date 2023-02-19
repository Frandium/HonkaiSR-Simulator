using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureMono : MonoBehaviour
{
    public readonly static Color[] ElementColors = { new Color(.25f, .875f, .625f, 1), new Color(.875f, .75f, 0, 1), new Color(0, .25f, 1, 1), new Color(1, 0, 0, 1), new Color(0, .875f, .875f, 1), new Color(.5f, 0, 1, 1), new Color(0, .625f, .625f, 0), new Color(.375f, .375f, .375f, 1) };

    public CreatureMono()
    {
        attributes = new float[(int)CommonAttribute.Count];
    }

    public int uniqueID { get; protected set; } = -1;
    protected Creature self;

    // UI Binding
    public Sprite runwayAvatar;
    public Image hpLine;
    public Text dmgText;
    public GameObject dmgGO;
    public SpriteRenderer cardSR;
    public SpriteRenderer selectedSR;
    public Image eleImage;
    public Image buffImage;
    protected List<AudioClip> attackAudios = new List<AudioClip>();
    protected List<AudioClip> takeDamageAudios = new List<AudioClip>();
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
    public float hpPercentage
    {
        get
        {
            return self.hp / self.GetFinalAttr(CommonAttribute.MaxHP);
        }
    }

    protected float[] attributes;
    public Element elementState { get; protected set; } = Element.Count;
    public ElementBuff elementBuff { get; protected set; } = ElementBuff.Count;

    //Battle functions
    public virtual void TakeDamage(float value)
    {
        hpLine.fillAmount = hpPercentage;
        StartCoroutine(TakeDamangeAnim(Mathf.RoundToInt(-value)));
    }

    public virtual void TakeHeal(float value)
    {
        hpLine.fillAmount = hpPercentage;
        StartCoroutine(TakeDamangeAnim(Mathf.RoundToInt(value)));
    }


    public virtual void OnDying()
    {

    }

    protected virtual IEnumerator TakeDamangeAnim(int dmg)
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
        if(self.hp <= 0)
        {
            OnDying();
        }
        isAnimFinished = true;
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
        selectedSR.color = new Color(0, 0, 0, 0);
    }

    public virtual void Initialize(Creature c)
    {
        self = c;
        selectedSR.sprite = Resources.Load<Sprite>(c.dbname + "/card_selected");
        cardSR.sprite = Resources.Load<Sprite>(c.dbname + "/card");
        runwayAvatar = Resources.Load<Sprite>(c.dbname + "/runway_avatar");
        int i = 1;
        AudioClip a = Resources.Load<AudioClip>(c.dbname + "/takedmg" + i);
        while (a != null)
        {
            takeDamageAudios.Add(a);
            i++;
            a = Resources.Load<AudioClip>(c.dbname + "/takedmg" + i);
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

    void UpdateBuffIcon(List<Buff> valueBuffs)
    {
        if (valueBuffs.Count == 0)
        {
            buffImage.sprite = BattleManager.Instance.nullBuffSprite;
            return;
        }
        buffImage.sprite = BattleManager.Instance.buffSprite;
        foreach (Buff b in valueBuffs)
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
        List<AudioClip> audios = attackAudios;
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
        if (audios.Count <= 0)
            return;
        AudioClip clip = audios[Random.Range(0, audios.Count)];
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
