using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureMono : MonoBehaviour
{
    public readonly static Color[] ElementColors = { 
        new Color(.875f, .875f, .875f, 1), // 物理
        new Color(1, .25f, 0, 1),     // 火
        new Color(0, .75f, 1, 1),     // 冰
        new Color(.375f, 0, 1, 1),    // 雷
        new Color(0, .625f, .25f, 1), // 风
        new Color(0, .125f, .75f, 1), // 量子
        new Color(1, .75f, .25f, 1),  // 虚数
        new Color(0, 0, 0, 1) };      // 黑色，缺省



    public int uniqueID { get; protected set; } = -1;
    protected Creature self;

    // UI Binding
    public Sprite runwayAvatar;
    public Image hpLine;
    public GameObject dmgGO;
    public SpriteRenderer cardSR;
    public Image buffImage;
    public Transform canvas;
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

    public delegate void Then();

    // Battle Attributes
    public float hpPercentage
    {
        get
        {
            return self.hp / self.GetFinalAttr(CommonAttribute.MaxHP);
        }
    }

    //Battle functions
    public virtual void TakeDamage(Damage d)
    {
        hpLine.fillAmount = hpPercentage;
        int dmg = -Mathf.RoundToInt(d.value);
        string content = dmg > 0 ? "+" + dmg.ToString() : dmg.ToString();
        ShowMessage(content, ElementColors[(int)d.element], d.isCritical?2:1, () => { if (self.hp <= 0) OnDying(); });
    }

    public virtual void TakeHeal(float value)
    {
        hpLine.fillAmount = hpPercentage;
        int dmg = Mathf.RoundToInt(value);
        string content = dmg > 0 ? "+" + dmg.ToString() : dmg.ToString();
        ShowMessage(content, Color.white);
    }


    public virtual void OnDying()
    {

    }

    Queue<string> messages = new Queue<string>();
    Queue<Color> colors = new Queue<Color>();
    Queue<Then> thens = new Queue<Then>();
    Queue<int> fontSize = new Queue<int>();
    bool isMessageShowing = false; 
    public virtual void ShowMessage(string content, Color c, int fontsize = 1, Then t = null)
    {
        messages.Enqueue(content);
        colors.Enqueue(c);
        fontSize.Enqueue(fontsize);
        thens.Enqueue(t);
        if (!isMessageShowing)
        {
            isMessageShowing = true;
            StartCoroutine(ConsumeMessage());
        }
    }

    public virtual IEnumerator ConsumeMessage()
    {
        while (messages.Count > 0) { 
            StartCoroutine(TakeDamangeAnim(messages.Dequeue(), colors.Dequeue(), fontSize.Dequeue(), thens.Dequeue()));
            yield return new WaitForSeconds(.3f);
        }
        isMessageShowing = false;
    }

    protected virtual IEnumerator TakeDamangeAnim(string content, Color c, int fontsize, Then then = null)
    {
        isAnimFinished = false;
        GameObject go = Instantiate(dmgGO);
        go.SetActive(true);
        go.transform.SetParent(canvas, false);
        Text t = go.GetComponentInChildren<Text>();
        t.text = content;
        RectTransform rect = go.GetComponent<RectTransform>();
        Image dmgBgImg = go.GetComponent<Image>();
        dmgBgImg.color = new Color(1, 1, 1, dmgBgBaseAlpha);
        rect.localPosition = new Vector3(0, 0, 0);
        t.color = c;
        t.fontSize = fontsize;
        float dmgAlpha = 1;
        float alphaFadeSpeed = 1 / dmgAnimTime;
        float dmgBgSpeed = (6 - 2.5f) / dmgAnimTime;
        while (rect.localPosition.y < 6)
        {
            rect.localPosition += Vector3.up * dmgBgSpeed * Time.deltaTime;
            dmgBgImg.color = new Color(1, 1, 1, dmgBgBaseAlpha * dmgAlpha);
            c.a = dmgAlpha;
            t.color = c;
            dmgAlpha -= alphaFadeSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(go);
        isAnimFinished = true;
        then?.Invoke();
    }

    public virtual void StartMyTurn()
    {
        isMyTurn = true;
        cardSR.material.SetColor("_lineColor", Color.blue);
        alpha = 1;
    }

    public virtual void EndMyTurn()
    {
        isMyTurn = false;
        alpha = 0;
        cardSR.material.SetFloat("_alpha", 0);
        UpdateState();
    }

    public virtual void Initialize(Creature c)
    {
        self = c;
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
    public virtual void SetUnselected()
    {
        isSelected = false;
        alpha = 0;
        cardSR.material.SetFloat("_alpha", 0);
    }

    public virtual void SetSelected()
    {
        alpha = 1;
        isSelected = true;
        cardSR.material.SetColor("_lineColor", Color.red);
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

    public virtual void UpdateState()
    {
        if(self.IsUnderState(StateType.Frozen))
        {
            cardSR.material.SetColor("_bodyColor", Color.blue);
        }
        else
        {
            cardSR.material.SetColor("_bodyColor", Color.white);
        }
    }
}
