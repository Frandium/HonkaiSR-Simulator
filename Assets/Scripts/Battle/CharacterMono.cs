using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;
using LitJson;

public enum AudioType
{
    Attack,
    Skill,
    Burst,
    Change,
    TakeDamage,
    BurstPrepare,
    Count
}

public class CharacterMono : CreatureMono
{
    public Sprite attackIcon { get; protected set; }
    public Sprite skillIcon { get; protected set; }
    public Sprite burstIcon { get; protected set; }
    public Sprite selectedCard { get; protected set; }
    public Sprite burstSplash { get; protected set; }

    public Image face;

    public Image burstImage;
    public Image burstFillingImage;
    public GameObject avatar;

    List<AudioClip> skillAudios = new List<AudioClip>();
    List<AudioClip> burstAudios = new List<AudioClip>();
    List<AudioClip> changeAudios = new List<AudioClip>();
    List<AudioClip> burstPrepareAudios = new List<AudioClip>();

    public VideoClip burstVideo { get; protected set; }

    public List<Dictionary<string, float>> attackActionSeries = new List<Dictionary<string, float>>();
    public List<Dictionary<string, float>> skillActionSeries = new List<Dictionary<string, float>>();
    public List<Dictionary<string, float>> BurstActionSeries = new List<Dictionary<string, float>>();

    public new Character self;

    public CharacterMono()
    {

    }


    // Update is called once per frame
    void Update()
    {
        if (isMyTurn || isSelected)
        {
            alpha += Time.deltaTime * alphaSpeed * alphaDirection;
            if (alpha > 1 || alpha < 0) alphaDirection *= -1;
            cardSR.material.SetFloat("_alpha", alpha);
        }
    }

    public override void SetSelected(bool isMainTarget = true)
    {
        alpha = 1;
        isSelected = true;
        cardSR.material.SetColor("_lineColor", Color.green);
    }

    public override void SetUnselected()
    {
        isSelected = false;
        if (isMyTurn)
            cardSR.material.SetColor("_lineColor", Color.blue);
        else
            cardSR.material.SetFloat("_alpha", 0);
    }

    public void Initialize(Character c)
    {
        base.Initialize(c);
        self = c;
        attackIcon = Resources.Load<Sprite>(c.dbname + "/attack");
        skillIcon = Resources.Load<Sprite>(c.dbname + "/skill");
        burstIcon = Resources.Load<Sprite>(c.dbname + "/burst");
        burstImage.sprite = burstIcon;
        burstSplash = Resources.Load<Sprite>(c.dbname + "/splash");
        face.sprite = Resources.Load<Sprite>(c.dbname + "/face");
        burstVideo = Resources.Load<VideoClip>(c.dbname + "/burst_video");
        int i = 1;
        AudioClip a = Resources.Load<AudioClip>(c.dbname + "/attack" + i);
        while (a != null)
        {
            attackAudios.Add(a);
            i++;
            a = Resources.Load<AudioClip>(c.dbname + "/attack" + i);
        }
        i = 1;
        a = Resources.Load<AudioClip>(c.dbname + "/skill" + i);
        while (a != null)
        {
            skillAudios.Add(a);
            i++;
            a = Resources.Load<AudioClip>(c.dbname + "/skill" + i);
        }
        i = 1;
        a = Resources.Load<AudioClip>(c.dbname + "/burst" + i);
        while (a != null)
        {
            burstAudios.Add(a);
            i++;
            a = Resources.Load<AudioClip>(c.dbname + "/burst" + i);
        }
        i = 1;
        a = Resources.Load<AudioClip>(c.dbname + "/change" + i);
        while (a != null)
        {
            changeAudios.Add(a);
            i++;
            a = Resources.Load<AudioClip>(c.dbname + "/change" + i);
        }
        i = 1;
        a = Resources.Load<AudioClip>(c.dbname + "/burst_prepare" + i);
        while (a != null)
        {
            burstPrepareAudios.Add(a);
            i++;
            a = Resources.Load<AudioClip>(c.dbname + "/burst_prepare" + i);
        }

        UpdateEnergyIcon();
        base.Initialize(c);
    }

    public void UpdateEnergyIcon()
    {
        burstFillingImage.fillAmount = self.energy / self.maxEnergy;
        Color elementColor = ElementColors[(int)self.element];
        if (self.energy < self.maxEnergy)
            elementColor.a = .5f;
        burstFillingImage.color = elementColor;
    }


    public void ActivateBurst()
    {
        isBurstActivated = true;
        StartCoroutine(BurstActivateAnim());
    }

    private const float burstAlphaFadeSpeed = 1;
    public bool isBurstActivated { get; private set; } = false;

    private IEnumerator BurstActivateAnim()
    {
        float alpha = 1;
        float alphaDir = -1;
        while (isBurstActivated)
        {
            if (alpha >= 1) alphaDir = -1;
            else if (alpha <= 0) alphaDir = 1;
            alpha += alphaDir * Time.deltaTime * burstAlphaFadeSpeed;
            Color c = ElementColors[(int)self.element];
            c.a = alpha;
            burstFillingImage.color = c;
            yield return new WaitForEndOfFrame();
        }
    }


    public void EndBurstTurn()
    {
        isMyTurn = false;
        isBurstActivated = false;
        StopCoroutine(BurstActivateAnim());
        Color c = ElementColors[(int)self.element];
        c.a = 1;
        burstFillingImage.color = c;
        alpha = 0;
        cardSR.material.SetFloat("Alpha", 0);
    }

    public void InterruptedByBurst()
    {
        isMyTurn = false;
        alpha = 0;
        cardSR.material.SetFloat("Alpha", 0);
    }

    public override void OnDying()
    {
        cardSR.color = new Color(.25f, .25f, .25f, .25f);
        BattleManager.Instance.RemoveCharacter(self);
        avatar.SetActive(false);
        gameObject.SetActive(false);
    }


    public override void PlayAudio(AudioType audioType)
    {
        List<AudioClip> audios = attackAudios;
        switch (audioType)
        {
            case AudioType.Attack:
                audios = attackAudios;
                break;
            case AudioType.Skill:
                audios = skillAudios;
                break;
            case AudioType.Burst:
                audios = burstAudios;
                break;
            case AudioType.Change:
                audios = changeAudios;
                break;
            case AudioType.BurstPrepare:
                audios = burstPrepareAudios;
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

}
