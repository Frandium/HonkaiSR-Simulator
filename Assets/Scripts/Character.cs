using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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

public class Character : Creature
{
    // -1是全体，0是不选择，1是选择。
    public bool isAttackTargetEnemy { get; protected set; } = true;
    public SelectionType attackSelectionType { get; protected set; } = SelectionType.One;
    public bool isSkillTargetEnemy { get; protected set; } = true;
    public SelectionType skillSelectionType { get; protected set; } = SelectionType.One;
    public bool isBurstTargetEnemy { get; protected set; } = true;
    public SelectionType burstSelectionType { get; protected set; } = SelectionType.All;
    public int attackGainPointCount { get; protected set; } = 1;
    public int skillConsumePointCount { get; protected set; } = 1;

    public Sprite attackIcon;
    public Sprite skillIcon;
    public Sprite burstIcon;
    public Sprite selectedCard;
    public Sprite burstSplash;

    public SpriteRenderer selectedCardSR;
    public SpriteRenderer cardSR;

    public Image burstImage;
    public Image burstFillingImage;

    public AudioClip[] skillAudios;
    public AudioClip[] burstAudios;
    public AudioClip[] changeAudios;
    public AudioClip[] burstPrepareAudios;

    public VideoClip burstVideo;


    public float tuntWeight = 100;

    float energy = 0;
    float maxEnergy = 60;
    public Element element { get; private set; } = Element.Physical;

    public IBattleTalents attackTalents { get; private set; }

    public 

    // Update is called once per frame
    void Update()
    {
        if (isMyTurn || isSelected)
        {
            alpha += Time.deltaTime * alphaSpeed * alphaDirection;
            if (alpha > 1 || alpha < 0) alphaDirection *= -1;
            if (isSelected) selected.color = new Color(0, 1, 0, alpha);
            else if (isMyTurn) selected.color = new Color(0, 0, 1, alpha);
        }
    }

    public override void SetSelected()
    {
        alpha = 1;
        isSelected = true;
        selected.color = Color.green;
    }

    public override void Initialize(string disN, string dbN, BattleManager _bm)
    {
        base.Initialize(disN, dbN, _bm);
        selected.sprite = selectedCard;
        switch (databaseName)
        {
            case "kazuha":
                isAttackTargetEnemy = true;
                attackSelectionType = SelectionType.One;
                isSkillTargetEnemy = true;
                skillSelectionType = SelectionType.One;
                isBurstTargetEnemy = true;
                burstSelectionType = SelectionType.All;
                uniqueID = 1;
                atk = 1313;
                speed = 41;
                maxHp = 20676;
                element = Element.Anemo;
                def = 1131;
                maxEnergy = 60;
                attackTalents = new Kazuha(this);
                break;
            case "ganyu":
                isAttackTargetEnemy = true;
                attackSelectionType = SelectionType.One;
                isSkillTargetEnemy = true;
                skillSelectionType = SelectionType.One;
                isBurstTargetEnemy = true;
                burstSelectionType = SelectionType.All;
                uniqueID = 2;
                atk = 2602;
                speed = 43;
                maxHp = 15085;
                def = 674;
                maxEnergy = 60;
                element = Element.Cryo;
                attackTalents = new Ganyu(this);
                break;
            case "shenhe":
                isAttackTargetEnemy = true;
                attackSelectionType = SelectionType.One;
                isSkillTargetEnemy = false;
                skillSelectionType = SelectionType.One;
                isBurstTargetEnemy = true;
                burstSelectionType = SelectionType.All;
                uniqueID = 3;
                atk = 3159;
                speed = 38;
                maxHp = 18473;
                def = 806;
                maxEnergy = 80;
                element = Element.Cryo;
                attackTalents = new Shenhe(this);
                break;
            case "kokomi":
                isAttackTargetEnemy = true;
                attackSelectionType = SelectionType.One;
                isSkillTargetEnemy = false;
                skillSelectionType = SelectionType.One;
                isBurstTargetEnemy = false;
                burstSelectionType = SelectionType.All;
                uniqueID = 4;
                atk = 1190;
                maxHp = 40277;
                speed = 30;
                def = 911;
                maxEnergy = 70;
                element = Element.Hydro;
                attackTalents = new Kokomi(this);
                break;
        }
        hp = maxHp;
        // 目前 files 在 editor 里手动绑定了，data在上面这个 switch 里 hardcode 了。
        //        LoadFilesAndData();
        hpLine.fillAmount = hp / maxHp;
        burstImage.sprite = burstIcon;
        UpdateEnergyIcon();
    }

    protected void LoadFilesAndData()
    {
        runwayAvatar = Resources.Load(databaseName + "/runway_avatar") as Sprite;
    }

    public void ChargeEnergy(float e)
    {
        energy += e;
        if (energy > maxEnergy) energy = maxEnergy;
        if (energy < 0) energy = 0;
        UpdateEnergyIcon();
    }

    public override void TakeDamage(float value, Creature source, Element damangeType, Then then = null)
    {
        PlayAudio(AudioType.TakeDamage);
        attackTalents.OnTakingDamage(source, value);
        base.TakeDamage(value, source, damangeType, then);
    }

    private void UpdateEnergyIcon()
    {
        burstFillingImage.fillAmount = energy / maxEnergy;
        Color elementColor = ElementColors[(int)element];
        if (energy < maxEnergy)
            elementColor.a = .75f;
        burstFillingImage.color = elementColor;
    }

    public float chargePercentage
    {
        get
        {
            return energy / maxEnergy;
        }
    }

    public bool isFullyCharged
    {
        get
        {
            return energy >= maxEnergy;
        }
    }

    public void ClearEnergy()
    {
        ChargeEnergy(-maxEnergy);
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
            Color c = ElementColors[(int)element];
            c.a = alpha;
            burstFillingImage.color = c;
            yield return new WaitForEndOfFrame();
        }
    }

    public void BurstEnd()
    {
        isBurstActivated = false;
        StopCoroutine(BurstActivateAnim());
        Color c = ElementColors[(int)element];
        c.a = 1;
        burstFillingImage.color = c;
    }

    protected override void OnDying()
    {
        Debug.Log(this);
        cardSR.color = new Color(.25f, .25f, .25f, .25f);
        attackTalents.OnDying();
        bm.RemoveCharacter(this);
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
