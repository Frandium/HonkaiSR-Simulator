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
    public int curTurnNumber { get; protected set; } = 0;
    bool isAttackOrSkill = false;
    float animTime = .2f;
    int enmWave = -1;
    float turnTime = 0;
    float minTurnTime = 3; // 每个回合最少 3 秒，防止操作输入太快出 bug
    private bool isBurst = false;
    public List<Character> characters { get; protected set; } = new List<Character>();
    public List<Character> deadCharacters { get; protected set; } = new List<Character>();
    public List<Character> allCharacters { get; protected set; } = new List<Character>();
    public List<Summon> summons { get; protected set; } = new List<Summon>();

    public List<CharacterMono> cMonos = new List<CharacterMono>();
    public List<Enemy> enemies { get; protected set; } = new List<Enemy>();

    public Camera characterCamera;
    public Camera enemyCamera;
    public GameObject enemyPrefab;
    public GameObject summonPrefab;
    public GameObject screenCanvas;
    public GameObject characterDetail;

    // battle configuration
    List<string> chaNames;
    List<List<string>> enmNames;

    // characters & enemies

    // Other Monos
    public Creature curCreature { get; private set; }
    public Character curCharacter { get; private set; }
    public Selection selection;
    public Runway runway;
    public SkillPoint skillPoint;

    // UI Bounding
    public Text bannerText;
    public Image attackImage;
    public Image skillImage;
    public Image splash;
    public Image bannerImgae;
    public Button QButton;
    public Button EButton;
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
    Vector3 enmInternal = - new Vector3(4f, 0, 0f);  // 敌人排布间距
    Vector3 enmOriginal = new Vector3(157.61f, 6.4f, 76.55f);
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
        bgm.volume = GlobalSettings.volume;
        LoadBattle();
        skillPoint.GainPoint(2);
        NextTurn();
        characterDetail.GetComponent<CharacterDetailUI>().SetChangeable(false);
    }

    void Update()
    {
        // 一个我方回合包含指令输入阶段、结算动画阶段
        // 一个敌方回合包含结算动画阶段
        // 当场上所有单位的结算动画阶段结束时，开启下一回合
        switch (curStage)
        {
            case TurnStage.Instruction:
                if(Input.GetKeyDown(KeyCode.E))
                    RespondtoKeycodeE();
                else if (Input.GetKeyDown(KeyCode.Q))
                    RespondtoKeycodeQ();
                else if(Input.GetKeyDown(KeyCode.Space))
                    RespondtoKeycodeSpace();
                break;
            case TurnStage.Animation:
                // 问题出在，敌人死了，被从列表里移除，但这个时候它的动画可能还没播完，不能进下一回合。
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
                if (characterDetail.activeSelf)
                {
                    if (curShowingDetail != p.Value)
                        ShowCharacterDetail(p.Value);
                    else
                    {
                        characterDetail.SetActive(false);
                        curShowingDetail = -1;
                    }
                }
                else
                {
                    ShowCharacterDetail(p.Value);
                }
            }
        }
    }

    // Game process
    public void LoadBattle()
    {
        string jsonString = File.ReadAllText(GlobalInfoHolder.battleFilePath);
        JsonData data = JsonMapper.ToObject(jsonString);

        // load json
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
        for (int i = 0; i < GlobalInfoHolder.teamMembers.Length; ++i)
        {
            string chaname = GlobalInfoHolder.teamMembers[i];
            if (chaname == "none")
            {
                cMonos[i].gameObject.SetActive(false);
                cMonos[i].avatar.SetActive(false);
            }
            else
            {
                Character c = new Character(chaname);
                allCharacters.Add(c);
                characters.Add(c);
                c.SetMono(cMonos[i]);
                runway.AddCreature(c);
            }
        }
        mystery = GlobalInfoHolder.mystery;
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
                    EnemyMono em = Instantiate(enemyPrefab, enmOriginal + i * enmInternal, Quaternion.identity).GetComponent<EnemyMono>();
                    em.SetOrigPosition(enmOriginal + i * enmInternal, Quaternion.identity);
                    Enemy e = new Enemy(enmNames[enmWave][i]);
                    enemies.Add(e);
                    e.SetMono(em);
                    runway.AddCreature(e);
                }
                foreach (Character c in characters)
                {
                    c.talents.OnEnemyRefresh(enemies);
                    foreach (AArtifactTalent at in c.artifactsSuit)
                    {
                        at.OnEnemyRefresh(c, enemies);
                    }
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

        // 对上一回合进行收尾，= null 时是第一回合
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
                if (curCreature.hp > 0)
                {
                    curCreature.ChangePercentageLocation(-1);
                    curCreature.EndNormalTurn();
                } 
            }
        }
        else
        {
            // 第一回合流程
            // 1. 生成所有角色召唤物
            foreach(Character c in characters)
            {
                Summon s = c.talents.Summon();
                if(s != null)
                {
                    summons.Add(s);
                    SummonMono sm = Instantiate(summonPrefab).GetComponent<SummonMono>();
                    s.SetMono(sm);
                    runway.AddCreature(s);
                }
            }
            // 2. 触发角色 on battle start
            // 3. 触发角色武器 on battle start
            // 4. 触发角色遗器套装 on battle start
            // 5. 触发角色秘技
            foreach (Character c in characters)
            {
                c.talents.OnBattleStart(characters);
                c.weapon.talents.OnBattleStart(c, characters);
                foreach (AArtifactTalent at in c.artifactsSuit)
                {
                    at.OnBattleStart(c, characters);
                }
                if (c.dbname == mystery)
                    c.talents.Mystery(characters, enemies);
            }
        }

        // 开启新回合
        // 向 runway 询问本回合是否是元素爆发回合。元素爆发是特殊回合，不触发回合开始的结束的 hook
        curCreature = runway.UpdateRunway(out isBurst, out bool isAdditional);
        foreach(Character c in characters)
        {
            c.mono.MoveBack();
            c.mono.cardSR.enabled = true;
        }
        foreach(Enemy e in enemies)
        {
            e.mono.gameObject.SetActive(true);
            e.mono.MoveBack();
        }
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
                characterCamera.transform.position = chaCamOrigPos;
                characterCamera.transform.rotation = chacamorigRot;
                enemyCamera.transform.position = enmCamOrigPos;
                enemyCamera.transform.rotation = enmCamOrigRot;
                ++curTurnNumber;
                // 如果我这个回合是刚才被元素爆发打断的回合，不触发 start。
                if (interrupted && !isAdditional)
                {
                    curCharacter.mono.StartMyTurn();
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
            ++curTurnNumber;
            // 敌人的回合直接进入结算动画阶段，插入的 burst 要等敌人行动完
            Enemy e = curCreature as Enemy;
            e.mono.MoveToSpot();
            characterCamera.enabled = true;
            enemyCamera.enabled = false;
            characterCamera.transform.position = chaCamEnemyActionPos;
            characterCamera.transform.rotation = chaCamEnemyActionRot;
            
            bool skip = e.StartNormalTurn();
            curStage = TurnStage.Animation;
            if(!skip && e.hp > 0)
                e.talents.MyTurn(characters, enemies);
        }else if (curCreature is Summon)
        {
            ++curTurnNumber;
            Summon s = curCreature as Summon;
            foreach(Character c in characters)
            {
                c.mono.cardSR.enabled = false;
            }
            enemyCamera.enabled = true;
            characterCamera.enabled = false;
            bool skip = s.StartNormalTurn();
            curStage = TurnStage.Animation;
            if (!skip)
                s.talents.MyTurn(characters, enemies);
        }
    }

    protected void BurstTurn()
    {
        curCharacter.StartBurstTurn();
        QButton.gameObject.SetActive(false);
        EButton.gameObject.SetActive(false);
        curCharacter.mono.PlayAudio(AudioType.BurstPrepare);
        if (curCharacter.isBurstTargetEnemy)
            selection.StartEnemySelection(curCharacter.burstSelectionType, curCharacter.talents.BurstEnemyAction, true);
        else
            selection.StartCharacterSelection(curCharacter.burstSelectionType, curCharacter.talents.BurstCharacterAction, true);
        BurstSplash(curCharacter.mono);
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

    public void RespondtoKeycodeE()
    {
        if (curStage != TurnStage.Instruction || !isAttackOrSkill)
            return;
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

    public void RespondtoKeycodeQ()
    {
        if (curStage != TurnStage.Instruction || isAttackOrSkill)
            return;

        SelectTarget();
        audioSource.clip = QAudio;
        audioSource.Play();
    }

    public void RespondtoKeycodeSpace()
    {
        if (curStage != TurnStage.Instruction)
            return;
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
                selection.ApplyAction(curCharacter.beforeNormalAttack, curCharacter.afterNormalAttack);
            }
            else
            {
                curCharacter.mono.PlayAudio(AudioType.Skill);
                selection.ApplyAction(curCharacter.beforeSkill, curCharacter.afterSkill);
            }
        }
    }

    public void TestAndInsertBurst(int i)
    {
        if (i >= characters.Count)
            return;
        Character c = allCharacters[i];
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

    public void ShowCharacterDetail(int x)
    {
        if (x >= allCharacters.Count)
            return;
        characterDetail.SetActive(true);
        curShowingDetail = x;
        characterDetail.GetComponent<CharacterDetailUI>().ShowDetail(allCharacters[x]);
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

    public void RemoveSummon(Summon s)
    {
        if (!summons.Contains(s))
            return;
        summons.Remove(s);
        runway.RemoveCreature(s);
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
    readonly Vector3 chaCamOrigPos = new Vector3(190, 5, 95);
    readonly Quaternion chacamorigRot = Quaternion.Euler(new Vector3(0, 180, 0));
    readonly Vector3 chaCamStartPos = new Vector3(195, 15, 90);
    readonly Vector3 chaCamEnemyActionPos = new Vector3(182, 1.44f, 98.48f);
    readonly Quaternion chaCamEnemyActionRot = Quaternion.Euler(new Vector3(-4.5f, 168, 0));


    readonly Vector3 enmCamOrigPos = new Vector3(153.2f, 6, 63.3f);
    readonly Quaternion enmCamOrigRot = Quaternion.Euler(new Vector3(0, -18, 0));
    readonly Vector3 enmCamStartPos = new Vector3(140, 10.22f, 70.6f);
    readonly Vector3 enmCamEnemyActionPos = new Vector3(182.5f, 1.73f, 96.7f);
    readonly Vector3 enmCamEnemyActionRot = new Vector3(-4.5f, 171, 0);
    IEnumerator BurstSplashAnim()
    {
        float burstSplashSpeed = .5f;
        splash.transform.parent.gameObject.SetActive(true);
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
        splash.transform.parent.gameObject.SetActive(false);
        characterCamera.enabled = !curCharacter.isBurstTargetEnemy;
        enemyCamera.enabled = curCharacter.isBurstTargetEnemy;
        if (curCharacter.isBurstTargetEnemy)
        {
            videoPlayer.targetCamera = enemyCamera;
            enemyCamera.transform.position = enmCamStartPos;
            enemyCamera.transform.rotation = enmCamOrigRot;
            Vector3 dir = enmCamOrigPos - enmCamStartPos;
            while (dir.magnitude > .01f)
            {
                yield return new WaitForEndOfFrame();
                enemyCamera.transform.Translate(dir * Time.deltaTime * 5, Space.World);
                dir = enmCamOrigPos - enemyCamera.transform.position;
            }
            enemyCamera.transform.position = enmCamOrigPos;
        }
        else
        {
            foreach(Character c in characters)
            {
                c.mono.MoveBack();
            }
            // 镜头运动：从 （195，15，100）运动到（190，5，95）            
            videoPlayer.targetCamera = characterCamera;
            characterCamera.transform.position = chaCamStartPos;
            characterCamera.transform.rotation = chacamorigRot;
            Vector3 dir = chaCamOrigPos - chaCamStartPos;
            while (dir.magnitude > .01f)
            {
                yield return new WaitForEndOfFrame();
                characterCamera.transform.Translate(dir * Time.deltaTime * 5, Space.World);
                dir = chaCamOrigPos - characterCamera.transform.position;
            }
            characterCamera.transform.position = chaCamOrigPos;
        }
    }

    IEnumerator CloseVideoAfterPlay(double seconds)
    {
        bgm.Pause();
        screenCanvas.SetActive(false);
        foreach(Character c in characters)
        {
            c.mono.cardSR.enabled = false;
        }
        foreach(Enemy e in enemies)
        {
            e.mono.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds((float)seconds);
        foreach (Character c in characters)
        {
            c.mono.cardSR.enabled = !curCharacter.isBurstTargetEnemy;
        }
        foreach (Enemy e in enemies)
        {
            e.mono.gameObject.SetActive(curCharacter.isBurstTargetEnemy);
        }

        videoPlayer.enabled = false;
        selection.ApplyAction(curCharacter.beforeBurst, curCharacter.afterBurst);
        curStage = TurnStage.Animation;
        screenCanvas.SetActive(true);
        bgm.Play();
        QButton.gameObject.SetActive(true);
        EButton.gameObject.SetActive(true);
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
