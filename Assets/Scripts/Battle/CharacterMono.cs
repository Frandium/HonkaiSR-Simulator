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

    bool isInterrupted = false;

    public Sprite attackIcon;
    public Sprite skillIcon;
    public Sprite burstIcon;
    public Sprite selectedCard;
    public Sprite burstSplash;

    public Image burstImage;
    public Image burstFillingImage;

    public AudioClip[] skillAudios;
    public AudioClip[] burstAudios;
    public AudioClip[] changeAudios;
    public AudioClip[] burstPrepareAudios;

    public VideoClip burstVideo;

    public List<Dictionary<string, float>> attackActionSeries = new List<Dictionary<string, float>>();
    public List<Dictionary<string, float>> skillActionSeries = new List<Dictionary<string, float>>();
    public List<Dictionary<string, float>> BurstActionSeries = new List<Dictionary<string, float>>();

    public new CharacterBase self;

    public CharacterMono()
    {

    }

    public void SetCharacter(CharacterBase c)
    {
        self = c;
        base.self = c;
    }


    // Update is called once per frame
    void Update()
    {
        if (isMyTurn || isSelected)
        {
            alpha += Time.deltaTime * alphaSpeed * alphaDirection;
            if (alpha > 1 || alpha < 0) alphaDirection *= -1;
            if (isSelected) selectedSR.color = new Color(0, 1, 0, alpha);
            else if (isMyTurn) selectedSR.color = new Color(0, 0, 1, alpha);
        }
    }

    public override void SetSelected()
    {
        alpha = 1;
        isSelected = true;
        selectedSR.color = Color.green;
    }

    public override void Initialize(string dbN, int id)
    {
        base.Initialize(dbN, id);
        selectedSR.sprite = selectedCard;
        burstImage.sprite = burstIcon;
        UpdateEnergyIcon();
    }

    public void ChargeEnergy(float e)
    {
        UpdateEnergyIcon();
    }

    public override void TakeDamage(CreatureBase source, float value, Element element, DamageType type)
    {
        PlayAudio(AudioType.TakeDamage);
        base.TakeDamage(source, value, element, type);
    }

    private void UpdateEnergyIcon()
    {
        burstFillingImage.fillAmount = self.energy / self.maxEnergy;
        Color elementColor = ElementColors[(int)self.element];
        if (self.energy < self.maxEnergy)
            elementColor.a = .75f;
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

    public override bool StartMyTurn()
    {
        isMyTurn = true;
        alpha = 1;
        if (isInterrupted)
            isInterrupted = false;
        return false;
    }

    public void StartBurstTurn()
    {
        isMyTurn = true;
        alpha = 1;
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
        selectedSR.color = new Color(0, 0, 0, 0);
    }

    public void InterruptedByBurst()
    {
        isMyTurn = false;
        isInterrupted = true;
        alpha = 0;
        selectedSR.color = new Color(0, 0, 0, 0);
    }

    protected override void OnDying()
    {
        cardSR.color = new Color(.25f, .25f, .25f, .25f);
        base.OnDying();
    }


    public override void PlayAudio(AudioType audioType)
    {
        AudioClip[] audios = attackAudios;
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
        if (audios.Length <= 0)
            return;
        AudioClip clip = audios[Random.Range(0, audios.Length)];
        audioSource.clip = clip;
        audioSource.Play();
        StartCoroutine(SetAudioFinish(clip.length));
    }

}
