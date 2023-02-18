using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using LitJson;
using UnityEngine.SceneManagement;

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
    GameEnd,
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


    // battle turn state
    public TurnStage curStage { get; protected set; } = TurnStage.Instruction;
    bool isAttackOrSkill = false;
    float animTime = .2f;
    int enmWave = -1;
    float turnTime = 0;
    float minTurnTime = 3; // 每个回合最少 3 秒，防止操作输入太快出 bug
    private bool isBurst = false;
    public List<CharacterBase> characters { get; protected set; } = new List<CharacterBase>();
    public List<CharacterMono> cMonos { get; protected set; } = new List<CharacterMono>();
    public List<EnemyBase> enemies { get; protected set; } = new List<EnemyBase>();
    public List<EnemyMono> eMonos { get; private set; } = new List<EnemyMono>();

    public GameObject enemyPrefab;

    // battle configuration
    List<string> chaNames;
    List<List<string>> enmNames;

    // characters & enemies

    // Other Monos
    public CreatureBase curCreature { get; private set; }
    private CharacterBase curCharacter;
    private CharacterMono curCMono;
    public Selection selection;
    public Runway runway;
    public SkillPoint skillPoint;

    // UI Bounding
    public Text bannerText;
    public Image attackImage;
    public Image skillImage;
    public Image splash;
    public Image bannerImgae;
    public AudioClip EAudio;
    public AudioClip QAudio;
    public AudioClip error;
    public AudioClip burstInsert;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public AudioSource bgm;
    public RectTransform attackRecttrans;
    public RectTransform skillRecttrans;

    // UI Resources
    public Sprite[] elementSymbols;
    public Sprite buffSprite;
    public Sprite debuffSprite;
    public Sprite nullBuffSprite;
    Vector3 enmOriginal = new Vector3(147.2f, 8.1f, 99); // 第 0 个敌人的位置
    Vector3 enmInternal = new Vector3(12.8f, 0, 7.4f);  // 敌人排布间距

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        LoadBattle();
        skillPoint.GainPoint(2);
        NextTurn();
    }

    void Update()
    {
        // 一个我方回合包含指令输入阶段、结算动画阶段
        // 一个敌方回合包含结算动画阶段
        // 当场上所有单位的结算动画阶段结束时，开启下一回合
        switch (curStage)
        {
            case TurnStage.Instruction:
                if (Input.GetKeyDown(KeyCode.E) && isAttackOrSkill)
                {
                    if (skillPoint.IsPointEnough(curCharacter.skillConsumePointCount))
                    {
                        audioSource.clip = EAudio;
                        audioSource.Play();
                        skillPoint.StartConsumePointAnim(curCharacter.skillConsumePointCount);
                        StartCoroutine(ChangeLocalScale(attackRecttrans, Vector3.one, animTime));
                        StartCoroutine(ChangeLocalScale(skillRecttrans, Vector3.one * 1.2f, animTime));

                        isAttackOrSkill = false;
                        if (curCharacter.isSkillTargetEnemy)
                            selection.StartEnemySelection(curCharacter.skillSelectionType, curCharacter.talents.SkillEnemyAction);
                        else
                            selection.StartCharacterSelection(curCharacter.skillSelectionType, curCharacter.talents.SkillCharacterAction);
                    }
                    else
                    {
                        audioSource.clip = error;
                        audioSource.Play();
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Q) && !isAttackOrSkill)
                {
                    SelectTarget();
                    audioSource.clip = QAudio;
                    audioSource.Play();
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (isBurst)
                    {
                        // 元素爆发回合，播放完 video 后才进 animation stage
                        curCMono.PlayAudio(AudioType.Burst);
                        videoPlayer.enabled = true;
                        videoPlayer.clip = curCMono.burstVideo;
                        videoPlayer.Play();
                        StartCoroutine(CloseVideoAfterPlay(curCMono.burstVideo.length));
                    }
                    else
                    {
                        curStage = TurnStage.Animation;
                        selection.ApplyAction();
                        if (isAttackOrSkill)
                            curCMono.PlayAudio(AudioType.Attack);
                        else
                            curCMono.PlayAudio(AudioType.Skill);
                    }
                }
                break;
            case TurnStage.Animation:
                if (turnTime > minTurnTime && cMonos.TrueForAll(c => c.IsPerformanceFinished) && enemies.TrueForAll(e => e.mono.IsPerformanceFinished))
                {
                    NextTurn();
                }
                turnTime += Time.deltaTime;
                break;
            case TurnStage.GameEnd:
                break;
            default:
                Debug.LogError("Unhandled turn stage: " + curStage);
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

    // Game process
    public void LoadBattle()
    {
        string jsonString = File.ReadAllText(GlobalInfoHolder.Instance.battleFilePath);
        JsonData data = JsonMapper.ToObject(jsonString);

        // load json
       chaNames = new List<string>();
        for(int i = 0; i < data["characters"].Count; ++i)
        {
            chaNames.Add((string)data["characters"][i]);
        }
        enmNames = new List<List<string>>();
        for(int i = 0; i < data["enemies"].Count; ++i)
        {
            List<string> enm = new List<string>();
            for(int j = 0; j < data["enemies"][i].Count; ++j)
            {
                enm.Add((string)data["enemies"][i][j]);
            }
            enmNames.Add(enm);
        }
        // enemies are instantiated in NextTurn
        for (int i = 0; i < characters.Count; ++i)
        {
            characters[i].LoadJson(chaNames[i]);
        }
    }

    public void NextTurn()
    {
        turnTime = 0;
        // 首先判断是否游戏结束
        if (enemies.Count == 0) // 所有敌人都死了
        {   
            if (++enmWave >= enmNames.Count) // 没有下一波了，结束
            {
                StartCoroutine(ShowBanner("挑 战 成 功", new Color(1, .5f, 0, .875f), 1, true));
                bgm.Stop();
                curStage = TurnStage.GameEnd;
                return;
            }
            else// 还有下一波
            {
                StartCoroutine(ShowBanner("第 " + (enmWave + 1).ToString() + " / " + enmNames.Count.ToString() + " 波 敌 人", new Color(1, .5f, 0, .875f), 1));
                for (int i = 0; i < enmNames[enmWave].Count; ++i)
                {
                    EnemyBase e = new EnemyBase();
                    EnemyMono em = Instantiate(enemyPrefab, enmOriginal + i * enmInternal, Quaternion.Euler(0, -30, 0)).GetComponent<EnemyMono>();
                    e.LoadJson(enmNames[enmWave][i]);
                    e.SetMono(em);
                    enemies.Add(e);
                    eMonos.Add(em);
                }
            }
        }
        if (characters.Count == 0)
        {
            StartCoroutine(ShowBanner("挑 战 失败", new Color(1, .5f, 0, .875f), 1, true));
            bgm.Stop();
            curStage = TurnStage.GameEnd;
            return;
        }

        // 对上一回合进行收尾，= null 时是第一回合，跳过这个阶段
        if (curCharacter != null)
        {
            // 刚结束的回合是元素爆发回合
            if (isBurst && curCreature is CharacterBase)
            {
                (curCreature as CharacterBase).EndNormalTurn();
            }
            else
            {
                // 若是非元素爆发回合，就将行动条置 0，触发回合结束 hook
                curCreature.ChangeLocation(-100);
                curCreature.EndNormalTurn();
            }
        }

        // 开启新回合
        // 向 runway 询问本回合是否是元素爆发回合。元素爆发是特殊回合，不触发回合开始的结束的 hook
        curCreature = runway.UpdateRunway(out isBurst);

        if (curCreature is CharacterBase) // 玩家的回合
        {
            curStage = TurnStage.Instruction;
            curCharacter = curCreature as CharacterBase;
            // 先进入输入指令阶段
            if (isBurst)
            { // 元素爆发回合不触发 turn start hook
                BurstTurn();
            }
            else
            {
                bool skip = false;// curCreature.StartNormalTurn();
                curCMono.PlayAudio(AudioType.Change);
                attackImage.sprite = curCharacter.mono.attackIcon;
                skillImage.sprite = curCharacter.mono.skillIcon;
                if (skip)
                {
                    curStage = TurnStage.Animation;
                }
                else
                {
                    selection.StartNewTurn(curCMono);
                    SelectTarget();
                }
            }
        }
        else if (curCreature is EnemyBase)
        {
            // 敌人的回合直接进入结算动画阶段，插入的 burst 要等敌人行动完

            bool skip = false; // curCreature.StartNormalTurn();
            curStage = TurnStage.Animation;
            EnemyBase e = curCreature as EnemyBase;
            if(!skip)
                e.talents.MyTurn();
        }
    }

    protected void BurstTurn()
    {
//        curCharacter.StartBurstTurn();
        BurstSplash(curCMono);
        curCMono.PlayAudio(AudioType.BurstPrepare);
        if (curCharacter.isBurstTargetEnemy)
            selection.StartEnemySelection(curCharacter.burstSelectionType, curCharacter.talents.BurstEnemyAction);
        else
            selection.StartCharacterSelection(curCharacter.burstSelectionType, curCharacter.talents.BurstCharacterAction);
    }

    protected void SelectTarget()
    {
        BattleManager.Instance.skillPoint.StartGainPointAnim(curCharacter.attackGainPointCount);
        StartCoroutine(ChangeLocalScale(attackRecttrans, Vector3.one * 1.2f, animTime));
        StartCoroutine(ChangeLocalScale(skillRecttrans, Vector3.one, animTime));
        isAttackOrSkill = true;
        if (curCharacter.isAttackTargetEnemy)
        {
            selection.StartEnemySelection(curCharacter.attackSelectionType, curCharacter.talents.AttackEnemyAction);
        }
        else
            selection.StartCharacterSelection(curCharacter.attackSelectionType, curCharacter.talents.AttackCharacterAction);
    }

    private void TestAndInsertBurst(int i)
    {
        CharacterBase c = characters[i];
        if (c.mono.isBurstActivated || !(c.hp > 0) || !(c.energy < c.maxEnergy))
            return;

        c.mono.ActivateBurst();
        audioSource.clip = burstInsert;
        audioSource.Play();
        // 只有在目前不是 burst 回合，且仍处于 instruction 阶段时，才能打断别人的行动
        if (!isBurst && curStage == TurnStage.Instruction)
        {
            runway.InsertBurst(c, true);
            isBurst = true;
            curCharacter.mono.InterruptedByBurst();
            curCharacter = c;
            curCreature = c;
            BurstTurn();
        }
        else
        {
            runway.InsertBurst(c, false);
        }
    }

    public void RemoveEnemy(EnemyBase e)
    {
        enemies.Remove(e);
        eMonos.Remove(e.mono);
    }

    public void RemoveCharacter(CharacterBase c)
    {
        characters.Remove(c);
        cMonos.Remove(c.mono);
    }


    // Animation Settings
    float burstSplashSpeed = .01f;
    const float BURST_INIT_SPEED = 3f;
    const float BURST_FINAL_SPEED = .1f;
    const float BURST_IMAGE_HALF_DISTANCE = .48f; // 速度变化的长度
    const float BURST_IMAGE_WIDTH = 1.5f;  // 图片宽度
    const float ACCELERATION = (BURST_INIT_SPEED * BURST_INIT_SPEED - BURST_FINAL_SPEED * BURST_FINAL_SPEED) / (BURST_IMAGE_HALF_DISTANCE * (1 + BURST_IMAGE_WIDTH)) / 2; // 加速度

    IEnumerator ChangeLocalScale(RectTransform tran, Vector3 target, float time)
    {
        Vector3 diff = target - tran.localScale;
        Vector3 start = tran.localScale;
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            float r = timer / time;
            tran.localScale = start + diff * r * (2 - r);  // 匀变速直线运动
            yield return new WaitForEndOfFrame();
        }
        tran.localScale = target;
    }

    public void BurstSplash(CharacterMono c)
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

    IEnumerator CloseVideoAfterPlay(double seconds)
    {
        yield return new WaitForSeconds((float)seconds);
        videoPlayer.enabled = false;
        selection.ApplyAction();
        curStage = TurnStage.Animation;
    }

    IEnumerator ShowBanner(string info, Color c, float seconds, bool returnToIndex = false)
    {
        bannerImgae.gameObject.SetActive(true);
        bannerImgae.color = c; //new Color(1, .5f, 0, .875f);
        bannerText.text = info; //  "挑 战 成 功";
        bannerImgae.rectTransform.localScale = new Vector3(1, .01f, 1);
        yield return ChangeLocalScale(bannerImgae.rectTransform, Vector3.one, animTime);
        yield return new WaitForSeconds(seconds);
        yield return ChangeLocalScale(bannerImgae.rectTransform, new Vector3(1, .01f, 1), animTime);
        bannerImgae.gameObject.SetActive(false);
        if(returnToIndex)
            SceneManager.LoadScene("Index");
    }
}
