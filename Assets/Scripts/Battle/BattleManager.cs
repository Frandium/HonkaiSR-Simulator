using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;


public enum SelectionType
{
    Self,
    One,
    OneExceptSelf,
    All,
    AllExceptSelf,
    Count
}

public enum TurnStage
{
    Instruction,
    Animation,
    Count
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance = null;
    
    public BattleManager()
    {
        if(Instance != null)
        {
            return;
        }
        Instance = this;
    }

    public ObjectPool<ValueBuff> valueBuffPool;
    public ObjectPool<TriggerBuff> triggerBuffPool;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log((int)CharacterAttribute.AnemoBonus);

        valueBuffPool = new ObjectPool<ValueBuff>(30);
        triggerBuffPool = new ObjectPool<TriggerBuff>(30);
        Application.targetFrameRate = 60;
        
        for(int i = 0; i< characters.Count; ++i)
        {
            // 这里后面会改成 Load Battle。
            characters[i].Initialize(GlobalInfoHolder.Instance.teamMembers[i], i);
        }
        for(int i = 0; i < enemies.Count; ++i)
        {
            enemies[i].Initialize(GlobalInfoHolder.Instance.enemyMembers[i], characters.Count + i);
        }
        //characters[0].Initialize("枫原万叶", "kazuha");
        //characters[1].Initialize("甘雨", "ganyu");
        //characters[2].Initialize("申鹤", "shenhe");
        //characters[3].initialize("珊瑚宫心海", "kokomi");
        //enemies[0].Initialize("丘丘人", "hilichurl");
        //enemies[1].Initialize("丘丘人", "hilichurl");
        //enemies[2].Initialize("丘丘人", "hilichurl");
        runway.Initialize();
        skillPoint.GainPoint(2);
        NextTurn();
    }

    public TurnStage curStage = TurnStage.Instruction;
    bool isAttackSelected = false;
    float animTime = .2f;

    // Object Bounding
    public RectTransform attackRecttrans;
    public RectTransform skillRecttrans;

    public List<Character> characters;
    public List<Enemy> enemies;

    public Creature curCreature { get; private set; }
    private Character curCharacter;

    public Selection selection;
    public Runway runway;
    public SkillPoint skillPoint;

    public Image attackImage;
    public Image skillImage;

    public VideoPlayer videoPlayer;

    public AudioClip EAudio;
    public AudioClip QAudio;
    public AudioClip error;
    public AudioClip burstInsert;
    public AudioSource audioSource;
    public AudioSource bgm;

    public Sprite[] elementSymbols;
    public Sprite buffSprite;
    public Sprite debuffSprite;
    public Sprite nullBuffSprite;

    public Image GameEndImage;
    public Text GameEndText;

    private bool isBurst = false;

    
    // 一个我方回合包含指令输入阶段、结算动画阶段
    // 一个敌方回合包含结算动画阶段
    // 当场上所有单位的结算动画阶段结束时，开启下一回合
    void Update()
    {
        // 回合内，EQ切换时
        switch (curStage)
        {
            case TurnStage.Instruction:
                if (Input.GetKeyDown(KeyCode.E) && isAttackSelected)
                {
                    if (skillPoint.IsPointEnough(curCharacter.skillConsumePointCount))
                    {
                        audioSource.clip = EAudio;
                        audioSource.Play();
                        skillPoint.StartConsumePointAnim(curCharacter.skillConsumePointCount);
                        StartCoroutine(ChangeLocalScale(attackRecttrans, Vector3.one, animTime));
                        StartCoroutine(ChangeLocalScale(skillRecttrans, Vector3.one * 1.2f, animTime));

                        isAttackSelected = false;
                        if (curCharacter.isSkillTargetEnemy)
                            selection.StartEnemySelection(curCharacter.skillSelectionType, curCharacter.attackTalents.SkillEnemyAction);
                        else
                            selection.StartCharacterSelection(curCharacter.skillSelectionType, curCharacter.attackTalents.SkillCharacterAction);
                    }
                    else
                    {
                        audioSource.clip = error;
                        audioSource.Play();
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Q) && !isAttackSelected)
                {
                    SelectAttack();
                    audioSource.clip = QAudio;
                    audioSource.Play();
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (isBurst)
                    {
                        // 元素爆发回合，播放完 video 后才进 animation stage
                        curCharacter.PlayAudio(AudioType.Burst);
                        videoPlayer.enabled = true;
                        videoPlayer.clip = curCharacter.burstVideo;
                        videoPlayer.Play();
                        StartCoroutine(CloseVideoAfterPlay(curCharacter.burstVideo.length));
                    }
                    else
                    {
                        curStage = TurnStage.Animation;
                        selection.ApplyAction();
                        if (isAttackSelected)
                            curCharacter.PlayAudio(AudioType.Attack);
                        else
                            curCharacter.PlayAudio(AudioType.Skill);
                    }
                }
                break;
            case TurnStage.Animation:
                if (characters.TrueForAll(c => c.IsPerformanceFinished) && enemies.TrueForAll(e => e.IsPerformanceFinished))
                {
                    NextTurn();
                }
                break;
            default:
                Debug.LogError("Unknown turn stage: " + curStage);
                break;
        }

        // 任意时刻，插入大招
        for(int i = 0; i < 4; ++i)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                TestAndInsertBurst(i);
            } 
        }
    }

    IEnumerator ChangeLocalScale(RectTransform tran, Vector3 target, float time)
    {
        Vector3 diff = target - tran.localScale;
        Vector3 start = tran.localScale;
        float timer = 0;
        while(timer < time)
        {
            timer += Time.deltaTime;
            float r = timer / time;
            tran.localScale = start + diff * r * (2 - r);  // 匀变速直线运动
            yield return new WaitForEndOfFrame();
        }
    }


    public void NextTurn()
    {
        // 首先判断是否游戏结束
        if (enemies.Count == 0)
        {
            GameEndImage.gameObject.SetActive(true);
            GameEndImage.color = new Color(1, .5f, 0, .875f);
            GameEndText.text = "挑 战 成 功";
            bgm.Stop();
            return;
        }
        if (characters.Count == 0)
        {
            GameEndImage.gameObject.SetActive(true);
            GameEndImage.color = new Color(0, .5f, .75f, .875f);
            GameEndText.text = "挑 战 失 败";
            bgm.Stop();
            return;
        }

        // 对上一回合进行收尾，= null 时是第一回合，跳过这个阶段
        if (curCharacter != null)
        {
            // 刚结束的回合是元素爆发回合
            if (isBurst && curCreature is Character)
            {
                (curCreature as Character).BurstEnd();
            }
            else
            {
                // 若是非元素爆发回合，就将行动条置 0
                curCreature.location = 0;
            }
            // 回合结束的 hook
            curCreature.EndMyTurn();
        }
        // 开启新回合
        // 向 runway 询问本回合是否是元素爆发回合
        isBurst = false;
        curCreature = runway.UpdateRunway(out isBurst);

        // 回合开始 hook
        curCreature.StartMyTurn();
        if (curCreature is Character)
        {
            // 先进入输入指令阶段
            curStage = TurnStage.Instruction;
            curCharacter = curCreature as Character;
            if (isBurst)
            {
                BurstSplash(curCharacter);
                curCharacter.PlayAudio(AudioType.BurstPrepare);
                if (curCharacter.isBurstTargetEnemy)
                {
                    selection.StartEnemySelection(curCharacter.burstSelectionType, curCharacter.attackTalents.BurstEnemyAction);
                }
                else
                {
                    selection.StartCharacterSelection(curCharacter.burstSelectionType, curCharacter.attackTalents.BurstCharacterAction);
                }
            }
            else
            {
                curCharacter.PlayAudio(AudioType.Change);
                attackImage.sprite = curCharacter.attackIcon;
                skillImage.sprite = curCharacter.skillIcon;
                selection.StartNewTurn(curCharacter);
                SelectAttack();
            }
        }
        else if (curCreature is Enemy)
        {
            curStage = TurnStage.Animation;
            // 直接进入结算动画阶段
            Enemy e = curCreature as Enemy;
            e.enemyAction.MyTurn();
        }
    }


    protected void SelectAttack()
    {
        skillPoint.StartGainPointAnim(curCharacter.attackGainPointCount);
        StartCoroutine(ChangeLocalScale(attackRecttrans, Vector3.one * 1.2f, animTime));
        StartCoroutine(ChangeLocalScale(skillRecttrans, Vector3.one, animTime));
        isAttackSelected = true;
        if (curCharacter.isAttackTargetEnemy)
        {
            selection.StartEnemySelection(curCharacter.attackSelectionType, curCharacter.attackTalents.AttackEnemyAction);
        }
        else
            selection.StartCharacterSelection(curCharacter.attackSelectionType, curCharacter.attackTalents.AttackCharacterAction);
    }

    private void TestAndInsertBurst(int i)
    {
        Character c = characters[i];
        if(!c.isBurstActivated && c.isAlive && c.isFullyCharged)
        {
            c.ActivateBurst();
            runway.InsertBurst(c);
            audioSource.clip = burstInsert;
            audioSource.Play();
        }
    }


    public Image splash;

    float burstSplashSpeed = .01f;
    const float BURST_INIT_SPEED = 3f;
    const float BURST_FINAL_SPEED = .1f;
    const float BURST_IMAGE_HALF_DISTANCE = .48f; // 速度变化的长度
    const float BURST_IMAGE_WIDTH = 1.5f;  // 图片宽度
    const float ACCELERATION = (BURST_INIT_SPEED * BURST_INIT_SPEED - BURST_FINAL_SPEED * BURST_FINAL_SPEED) / (BURST_IMAGE_HALF_DISTANCE * (1 + BURST_IMAGE_WIDTH)) / 2; // 加速度


    public void BurstSplash(Character c)
    {
        splash.sprite = c.burstSplash;
        StopCoroutine(BurstSplashAnim());
        StartCoroutine(BurstSplashAnim());
    }

    IEnumerator BurstSplashAnim()
    {
        splash.rectTransform.anchorMin = new Vector2(1f, -.5f);
        splash.rectTransform.anchorMax = new Vector2(1 + BURST_IMAGE_WIDTH, 1.05f);
        burstSplashSpeed = BURST_INIT_SPEED;
        float slow_len = (1 + BURST_IMAGE_WIDTH) * (1 - 2 * BURST_IMAGE_HALF_DISTANCE);
        while (splash.rectTransform.anchorMin.x > .5f - BURST_IMAGE_WIDTH / 2 + slow_len / 2)
        {
            yield return new WaitForEndOfFrame();
            splash.rectTransform.anchorMax += Vector2.left * burstSplashSpeed * Time.deltaTime;
            splash.rectTransform.anchorMin += Vector2.left * burstSplashSpeed * Time.deltaTime;
            burstSplashSpeed -= Time.deltaTime * ACCELERATION;
        }
        while (splash.rectTransform.anchorMin.x > .5f - BURST_IMAGE_WIDTH / 2 - slow_len / 2)
        {
            yield return new WaitForEndOfFrame();
            splash.rectTransform.anchorMax += Vector2.left * burstSplashSpeed * Time.deltaTime;
            splash.rectTransform.anchorMin += Vector2.left * burstSplashSpeed * Time.deltaTime;
        }
        while (splash.rectTransform.anchorMin.x > -BURST_IMAGE_WIDTH)
        {
            yield return new WaitForEndOfFrame();
            splash.rectTransform.anchorMax += Vector2.left * burstSplashSpeed * Time.deltaTime;
            splash.rectTransform.anchorMin += Vector2.left * burstSplashSpeed * Time.deltaTime;
            burstSplashSpeed += Time.deltaTime * ACCELERATION;
        }
    }

    public void RemoveEnemy(Enemy e)
    {
        enemies.Remove(e);
    }

    public void RemoveCharacter(Character c)
    {
        characters.Remove(c);
    }


    IEnumerator CloseVideoAfterPlay(double seconds)
    {
        yield return new WaitForSeconds((float)seconds);
        videoPlayer.enabled = false;
        selection.ApplyAction();
        curStage = TurnStage.Animation;
    }

}
