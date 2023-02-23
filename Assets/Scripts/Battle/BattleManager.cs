using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using LitJson;
using UnityEngine.SceneManagement;


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
    bool chaDetailActivated = false;
    public List<Character> characters { get; protected set; } = new List<Character>();
    public List<Character> deadCharacters { get; protected set; } = new List<Character>();
    public List<CharacterMono> cMonos = new List<CharacterMono>();
    public List<Enemy> enemies { get; protected set; } = new List<Enemy>();

    public GameObject enemyPrefab;
    public GameObject screenCanvas;
    public GameObject characterDetail;

    // battle configuration
    List<string> chaNames;
    List<List<string>> enmNames;

    // characters & enemies

    // Other Monos
    public Creature curCreature { get; private set; }
    private Character curCharacter;
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
    Vector3 enmInternal = - new Vector3(12.8f, 0, 7.4f);  // 敌人排布间距
    Vector3 enmOriginal = new Vector3(185.6f, 8.1f, 121.2f);
    bool interrupted = false;
    string mystery = "";

    int curShowingDetail = -1; 
    Dictionary<KeyCode, int> detailKey2Character = new Dictionary<KeyCode, int>() {
        {KeyCode.Z, 0 },
        {KeyCode.X, 1 },
        {KeyCode.C, 2 },
        {KeyCode.V, 3 }
    };

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
                        curCharacter.mono.PlayAudio(AudioType.Burst);
                        videoPlayer.enabled = true;
                        videoPlayer.clip = curCharacter.mono.burstVideo;
                        videoPlayer.Play();
                        StartCoroutine(CloseVideoAfterPlay(curCharacter.mono.burstVideo.length));
                    }
                    else
                    {
                        curStage = TurnStage.Animation;
                        if (isAttackOrSkill)
                        {
                            curCharacter.mono.PlayAudio(AudioType.Attack);
                            selection.ApplyAction(curCharacter.onNormalAttack);
                        }
                        else
                        {
                            curCharacter.mono.PlayAudio(AudioType.Skill);
                            selection.ApplyAction(curCharacter.onSkill);
                        }
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


        foreach(KeyValuePair<KeyCode, int> p in detailKey2Character)
        {
            if (Input.GetKeyDown(p.Key)) {
                chaDetailActivated = false;
                ShowCharacterDetail(p.Value);
                curShowingDetail = p.Value;
            }
            if (Input.GetKeyUp(p.Key) && curShowingDetail == p.Value)
            {
                characterDetail.SetActive(false);
                chaDetailActivated = false;
                curShowingDetail = -1;
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
        for (int i = 0; i < chaNames.Count; ++i)
        {
            Character c = new Character(chaNames[i]);
            characters.Add(c);
            c.SetMono(cMonos[i]);
            runway.AddCreature(c);
        }
        mystery = GlobalInfoHolder.Instance.mystery;
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
                    EnemyMono em = Instantiate(enemyPrefab, enmOriginal + i * enmInternal, Quaternion.Euler(0, -65, 0)).GetComponent<EnemyMono>();
                    Enemy e = new Enemy(enmNames[enmWave][i]);
                    enemies.Add(e);
                    e.SetMono(em);
                    runway.AddCreature(e);
                }
            }
        }
        if (characters.Count == 0)
        {
            StartCoroutine(ShowBanner("挑 战 失 败", new Color(1, .5f, 0, .875f), 1, true));
            bgm.Stop();
            curStage = TurnStage.GameEnd;
            return;
        }

        // 对上一回合进行收尾，= null 时是第一回合，跳过这个阶段
        if (curCreature != null)
        {
            // 刚结束的回合是元素爆发回合
            if (isBurst && curCreature is Character)
            {
                (curCreature as Character).EndBurstTurn();
            }
            else
            {
                // 若是非元素爆发回合，就将行动条置 0，触发回合结束 hook
                curCreature.ChangePercentageLocation(-100);
                curCreature.EndNormalTurn();
            }
        }
        else
        {
            // 第一回合发动秘技
            if (mystery != "none")
            {
                Character c = characters.Find(x => x.dbname == mystery);
                c.talents.Mystery(characters, enemies);
            }
        }

        // 开启新回合
        // 向 runway 询问本回合是否是元素爆发回合。元素爆发是特殊回合，不触发回合开始的结束的 hook
        bool isAdditional = false;
        curCreature = runway.UpdateRunway(out isBurst, out isAdditional);
        if (curCreature is Character) // 玩家的回合
        {
            curStage = TurnStage.Instruction;
            curCharacter = curCreature as Character;
            // 先进入输入指令阶段
            if (isBurst)
            { // 元素爆发回合触发 burst start hook
                BurstTurn();
            }
            else
            {
                // 如果我这个回合是刚才被元素爆发打断的回合，不触发 start。
                if (interrupted && !isAdditional)
                {
                    curCharacter.mono.StartMyTurn();
                    selection.StartNewTurn(curCharacter.mono);
                    SelectTarget();
                    interrupted = false;
                }
                else
                {
                    bool skip = curCharacter.StartNormalTurn();
                    if (skip)
                    {
                        curStage = TurnStage.Animation;
                    }
                    else
                    {
                        selection.StartNewTurn(curCharacter.mono);
                        SelectTarget();
                    }
                }
                curCreature.mono.PlayAudio(AudioType.Change);
                attackImage.sprite = curCharacter.mono.attackIcon;
                skillImage.sprite = curCharacter.mono.skillIcon;
            }
        }
        else if (curCreature is Enemy)
        {
            // 敌人的回合直接进入结算动画阶段，插入的 burst 要等敌人行动完
            Enemy e = curCreature as Enemy;
            bool skip = e.StartNormalTurn();
            curStage = TurnStage.Animation;
            if(!skip)
                e.talents.MyTurn();
        }
    }

    protected void BurstTurn()
    {
        curCharacter.StartBurstTurn();
        BurstSplash(curCharacter.mono);
        curCharacter.mono.PlayAudio(AudioType.BurstPrepare);
        if (curCharacter.isBurstTargetEnemy)
            selection.StartEnemySelection(curCharacter.burstSelectionType, curCharacter.talents.BurstEnemyAction);
        else
            selection.StartCharacterSelection(curCharacter.burstSelectionType, curCharacter.talents.BurstCharacterAction);
    }

    protected void SelectTarget()
    {
        skillPoint.StartGainPointAnim(curCharacter.attackGainPointCount);
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
        Character c = characters[i];
        if (c.mono.isBurstActivated || !(c.hp > 0) || c.energy < c.maxEnergy)
            return;

        c.mono.ActivateBurst();
        audioSource.clip = burstInsert;
        audioSource.Play();
        // 只有在目前不是 burst 回合，且仍处于 instruction 阶段时，才能打断别人的行动
        if (!isBurst && curStage == TurnStage.Instruction)
        {
            interrupted = true;
            curCharacter.mono.InterruptedByBurst();
            runway.InsertBurst(c, true);
            isBurst = true;
            curCharacter = c;
            curCreature = c;
            BurstTurn();
        }
        else
        {
            runway.InsertBurst(c, false);
        }
    }

    private void ShowCharacterDetail(int x)
    {
        if (chaDetailActivated)
            return;
        Character c = characters[x];
        chaDetailActivated = true;
        characterDetail.SetActive(true);
        Text t = characterDetail.GetComponentInChildren<Text>();
        string show = "角色名：" + c.disname + "  Lv." + c.level + "  突破" + c.breakLevel + "  " + Utils.ElementName[(int)c.element]
            + "  " + Utils.CareerName[(int)c.career];
        show += "\n" + c.atkName + "：" + c.atkDescription;
        show += "\n" + c.skillName + "：" + c.skillDescription;
        show += "\n" + c.burstName + "：" + c.burstDescription;
        show += "\n" + c.talentName + "：" + c.talentDescription;

        show += "\n光锥：" + c.weapon.disName + "  Lv." + c.weapon.level + "  突破" + c.weapon.breakLevel;
        show += "\n" + c.weapon.effectName + "：" + c.weapon.effectDescription;
        show += "\n生命值：" + c.hp + "  位置：" + (c.location / Runway.Length * 100) + "%";

        for(int i = 0; i < (int)CommonAttribute.Count; ++i)
        {
            float b = c.GetBaseAttr((CommonAttribute)i);
            float f = c.GetFinalAttr((CommonAttribute)i);
            show += "\n" + Utils.attributeNames[i] + "：" +
                 b + " + <color=green>" + (f-b) + "</color> = " + f;
        }
        t.text = show;
    }

    public void RemoveEnemy(Enemy e)
    {
        if (!enemies.Contains(e))
            return;
        enemies.Remove(e);
        runway.RemoveCreature(e);
    }

    public void RemoveCharacter(Character c)
    {
        if (!characters.Contains(c))
            return;
        runway.RemoveCreature(c);
        characters.Remove(c);
        deadCharacters.Add(c);
    }


    // Animation Settings

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

    const float ACCELERATION = .5f; // 加速度

    IEnumerator BurstSplashAnim()
    {
        float burstSplashSpeed = .5f;
        splash.gameObject.SetActive(true);
        float scale = 1.25f;
        splash.rectTransform.localScale = Vector3.one * scale;
        float timer = 0;
        while (timer <= .8f)
        {
            yield return new WaitForEndOfFrame();
            scale -= burstSplashSpeed * Time.deltaTime;
            splash.rectTransform.localScale = Vector3.one * scale;
            burstSplashSpeed -= Time.deltaTime * ACCELERATION;
            if (burstSplashSpeed <= .01f)
                burstSplashSpeed = .01f;
            timer += Time.deltaTime;
        }
        new WaitForSeconds(.2f);
        splash.gameObject.SetActive(false);
    }

    IEnumerator CloseVideoAfterPlay(double seconds)
    {
        bgm.Pause();
        screenCanvas.SetActive(false);
        yield return new WaitForSeconds((float)seconds);
        videoPlayer.enabled = false;
        selection.ApplyAction(curCharacter.onBurst);
        curStage = TurnStage.Animation;
        screenCanvas.SetActive(true);
        bgm.Play();
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
