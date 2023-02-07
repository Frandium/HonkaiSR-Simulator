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

public class Character : Creature
{
    public bool isAttackTargetEnemy { get; protected set; } = true;
    public SelectionType attackSelectionType { get; protected set; } = SelectionType.One;
    public bool isSkillTargetEnemy { get; protected set; } = true;
    public SelectionType skillSelectionType { get; protected set; } = SelectionType.One;
    public bool isBurstTargetEnemy { get; protected set; } = true;
    public SelectionType burstSelectionType { get; protected set; } = SelectionType.All;
    public int attackGainPointCount { get; protected set; } = 1;
    public int skillConsumePointCount { get; protected set; } = 1;

    public int tauntWeight { get; protected set; } = 100;

    float energy = 0;
    public float maxEnergy { get; protected set; } = 60;

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


    public Element element { get; private set; } = Element.Physical;

    public IBattleTalents attackTalents { get; private set; }

    public Character()
    {
        attributes = new float[(int)CharacterAttribute.Count];
    }
 

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

    public override void Initialize(string dbN, int id)
    {
        string jsonString = File.ReadAllText(GlobalInfoHolder.Instance.characterDir + "/" + dbN + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        databaseName = (string)data["dbname"];
        displayName = (string)data["disname"];
        atk = (float)(double)data["atk"];
        def = (float)(double)data["def"];
        speed = (float)(double)data["speed"];
        maxHp = (float)(double)data["maxHp"];
        maxEnergy = (float)(double)data["maxEnergy"];
        element = (Element)(int)data["element"];

        for (int i = 0; i < (int)Element.Count; ++i)
        {
            attributes[(int)CharacterAttribute.AnemoBonus + i] = (float)(double)data["elementalBonus"][i];
        }

        for (int i = 0; i < (int)Element.Count; ++i)
        {
            attributes[(int)CommonAttribute.AnemoResist + i] = (float)(double)data["elementalResist"][i];
        }

        isAttackTargetEnemy = (bool)data["isAttackTargetEnemy"];
        attackSelectionType = (SelectionType)(int)data["attackSelectionType"];
        isSkillTargetEnemy = (bool)data["isSkillTargetEnemy"];
        skillSelectionType = (SelectionType)(int)data["skillSelectionType"];
        isBurstTargetEnemy = (bool)data["isBurstTargetEnemy"];
        burstSelectionType = (SelectionType)(int)data["burstSelectionType"];
        attackGainPointCount = (int)data["attackGainPointCount"];
        skillConsumePointCount = (int)data["skillConsumePointCount"];


        // 也许之后人物的技能要改成 Lua 脚本，就不用 switch 了。
        energy = maxEnergy;
        switch (dbN)
        {
            case "kazuha":
                attackTalents = new Kazuha(this);
                break;
            case "ganyu":
                attackTalents = new Ganyu(this);
                energy = maxEnergy;
                break;
            case "shenhe":
                attackTalents = new Shenhe(this);
                break;
            case "kokomi":
                attackTalents = new Kokomi(this);
                break;
        }
        base.Initialize(dbN, id);
        selected.sprite = selectedCard;
        burstImage.sprite = burstIcon;
        UpdateEnergyIcon();
    }

    protected void LoadFilesAndData()
    {
        // 之后资源要从 Resources 或者 AssetBundle 里 Load
        // runwayAvatar = Resources.Load(databaseName + "/runway_avatar") as Sprite;
    }

    public void ChargeEnergy(float e)
    {
        energy += e;
        if (energy > maxEnergy) energy = maxEnergy;
        if (energy < 0) energy = 0;
        UpdateEnergyIcon();
    }

    public override void TakeDamage(float value, Creature source, Element element, Then then = null)
    {
        PlayAudio(AudioType.TakeDamage);
        attackTalents.OnTakingDamage(source, value, element);
        base.TakeDamage(value, source, element, then);
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
        BattleManager.Instance.RemoveCharacter(this);
        base.OnDying();
    }

    public override void AddBuff(ValueBuff buff, Then then = null)
    {
        switch (buff.attributeType)
        {
            case (int)CharacterAttribute.Taunt:
                tauntWeight -= (int)buff.value;
                break;
        }
        base.AddBuff(buff, then);
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
