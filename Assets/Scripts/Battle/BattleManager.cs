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
    float minTurnTime = 3; // ÿ���غ����� 3 �룬��ֹ��������̫��� bug
    private bool isBurst = false;
    public List<Character> characters { get; protected set; } = new List<Character>();
    public List<Character> deadCharacters { get; protected set; } = new List<Character>();
    public List<Character> allCharacters { get; protected set; } = new List<Character>();
    public List<Summon> summons { get; protected set; } = new List<Summon>();

    public List<CharacterMono> cMonos = new List<CharacterMono>();
    public List<Enemy> enemies { get; protected set; } = new List<Enemy>();

    public Camera mainCamera;
    public GameObject enemyPrefab;
    public GameObject summonPrefab;
    public GameObject screenCanvas;
    public GameObject characterDetail;

    // battle configuration
    List<List<EnemyConfig>> enemyConfigs;

    // characters & enemies

    // Other Monos
    public Creature curCreature { get; private set; }
    public Character curCharacter { get; private set; }
    public Selection selection;
    public Runway runway;
    public SkillPoint skillPoint;

    // UI Bounding
    public Text bannerText;
    public Text[] dealDamageText;
    public Text[] takeDamageText;
    public Image attackImage;
    public Image skillImage;
    public Image splash;
    public RectTransform[] dealDamageBar;
    public RectTransform[] takeDamageBar;
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
    Vector3 enmInternal = - new Vector3(4f, 0, 0f);  // �����Ų����
    Vector3 enmOriginal = new Vector3(157.61f, 6.4f, 76.55f);
    Vector3 chaOriginal = new Vector3(196.5f, 5, 84.41f);
    Vector3 chaInternal = - new Vector3(4.5f, 0, 0);
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
        // һ���ҷ��غϰ���ָ������׶Ρ����㶯���׶�
        // һ���з��غϰ������㶯���׶�
        // ���������е�λ�Ľ��㶯���׶ν���ʱ��������һ�غ�
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
                // ������ڣ��������ˣ������б����Ƴ��������ʱ�����Ķ������ܻ�û���꣬���ܽ���һ�غϡ�
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

        if (!isBurstVideoShowing)
        {
            // ����ʱ�̣��������
            for (int i = 0; i < 4; ++i)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    TestAndInsertBurst(i);
                }
            }


            foreach (KeyValuePair<KeyCode, int> p in detailKey2Character)
            {
                if (Input.GetKeyDown(p.Key))
                {
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
    }

    // Game process
    public void LoadBattle()
    {
        // load json
        enemyConfigs = GlobalInfoHolder.battle.enemies;

        // enemies are instantiated in NextTurn
        
        GlobalInfoHolder.teamMembers.RemoveAll(s => s == "none");
        int i;
        for (i = 0; i < GlobalInfoHolder.teamMembers.Count; ++i)
        {
            string chaname = GlobalInfoHolder.teamMembers[i];
            if (chaname == "none")
            {
                cMonos[i].gameObject.SetActive(false);
                cMonos[i].avatar.SetActive(false);
                dealDamageBar[i].anchorMin = new Vector2(1, 0);
                dealDamageBar[i].anchorMax = new Vector2(1, 1);
                takeDamageBar[i].anchorMin = new Vector2(1, 0);
                takeDamageBar[i].anchorMax = new Vector2(1, 1);
            }
            else
            {
                Character c = new Character(chaname);
                allCharacters.Add(c);
                characters.Add(c);
                c.SetMono(cMonos[i]);
                runway.AddCreature(c);
                characterDealDamage[c] = 0;
                characterTakeDamage[c] = 0;
                cMonos[i].SetOrigPosition(chaOriginal + i * chaInternal, Quaternion.Euler(new Vector3(0, 180, 0)));
            }
        }
        for (; i < 4; ++i)
        {
            cMonos[i].gameObject.SetActive(false);
            cMonos[i].avatar.SetActive(false);
            dealDamageBar[i].anchorMin = new Vector2(1, 0);
            dealDamageBar[i].anchorMax = new Vector2(1, 1);
            takeDamageBar[i].anchorMin = new Vector2(1, 0);
            takeDamageBar[i].anchorMax = new Vector2(1, 1);
        }
        mystery = GlobalInfoHolder.mystery;
    }

    public void NextTurn()
    {
        turnTime = 0;
        // �����ж��Ƿ���Ϸ����
        if (enemies.Count == 0) // ���е��˶�����
        {   
            if (++enmWave >= enemyConfigs.Count) // û����һ���ˣ�����
            {
                StartCoroutine(ShowBanner("�� ս �� ��", new Color(1, .5f, 0, .875f), 1, true));
                bgm.Stop();
                curStage = TurnStage.GameEnd;
                return;
            }
            else// ������һ��
            {
                StartCoroutine(ShowBanner("�� " + (enmWave + 1).ToString() + " / " + enemyConfigs.Count.ToString() + " �� �� ��", new Color(1, .5f, 0, .875f), 1));
                for (int i = 0; i < enemyConfigs[enmWave].Count; ++i)
                {
                    EnemyMono em = Instantiate(enemyPrefab, enmOriginal + i * enmInternal, Quaternion.identity).GetComponent<EnemyMono>();
                    em.SetOrigPosition(enmOriginal + i * enmInternal, Quaternion.identity);
                    Enemy e = new Enemy(enemyConfigs[enmWave][i]);
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
            StartCoroutine(ShowBanner("�� ս ʧ ��", new Color(1, .5f, 0, .875f), 1, true));
            bgm.Stop();
            curStage = TurnStage.GameEnd;
            return;
        }

        // ����һ�غϽ�����β��= null ʱ�ǵ�һ�غ�
        if (curCreature != null)
        {
            // �ս����Ļغ���Ԫ�ر����غ�
            if (isBurst && curCreature is Character)
            {
                (curCreature as Character).EndBurstTurn();
            }
            else
            {
                // ���Ƿ�Ԫ�ر����غϣ��ͽ��ж����� 0�������غϽ��� hook
                if (curCreature.hp > 0)
                {
                    curCreature.ChangePercentageLocation(-1);
                    curCreature.EndNormalTurn();
                } 
            }
        }
        else
        {
            // ��һ�غ�����
            // 1. �������н�ɫ�ٻ���
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
            // 2. ������ɫ on battle start
            // 3. ������ɫ���� on battle start
            // 4. ������ɫ������װ on battle start
            // 5. ������ɫ�ؼ�
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

        // �����»غ�
        // �� runway ѯ�ʱ��غ��Ƿ���Ԫ�ر����غϡ�Ԫ�ر���������غϣ��������غϿ�ʼ�Ľ����� hook
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
        if (curCreature is Character) // ��ҵĻغ�
        {
            curStage = TurnStage.Instruction;
            curCharacter = curCreature as Character;
            // �Ƚ�������ָ��׶�
            if (isBurst)
            {   
                // Ԫ�ر����غϴ��� burst start hook
                // Ԫ�ر����غϲ�ռ�ûغ���
                BurstTurn();
            }
            else
            {
                ++curTurnNumber;
                // ���������غ��Ǹղű�Ԫ�ر�����ϵĻغϣ������� start��
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
            // ���˵Ļغ�ֱ�ӽ�����㶯���׶Σ������ burst Ҫ�ȵ����ж���
            Enemy e = curCreature as Enemy;
            e.mono.MoveToSpot();
            mainCamera.transform.position = chaCamEnemyActionPos;
            mainCamera.transform.rotation = chaCamEnemyActionRot;
            
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
            mainCamera.transform.position = enmCamOrigPos;
            mainCamera.transform.rotation = enmCamOrigRot;

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
            mainCamera.transform.SetPositionAndRotation(enmCamOrigPos, enmCamOrigRot);
            selection.StartEnemySelection(curCharacter.attackSelectionType, curCharacter.talents.AttackEnemyAction);
        }
        else
        {
            mainCamera.transform.SetPositionAndRotation(chaCamOrigPos, chacamorigRot);
            selection.StartCharacterSelection(curCharacter.attackSelectionType, curCharacter.talents.AttackCharacterAction);
        }
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
            {
                mainCamera.transform.SetPositionAndRotation(enmCamOrigPos, enmCamOrigRot);
                selection.StartEnemySelection(curCharacter.skillSelectionType, curCharacter.talents.SkillEnemyAction);
            }
            else
            {
                mainCamera.transform.SetPositionAndRotation(chaCamOrigPos, chacamorigRot);
                selection.StartCharacterSelection(curCharacter.skillSelectionType, curCharacter.talents.SkillCharacterAction);
            }
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
            // Ԫ�ر����غϣ������� video ��Ž� animation stage
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
        // ֻ����Ŀǰ���� burst �غϣ����Դ��� instruction �׶�ʱ�����ܴ�ϱ��˵��ж�
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

    IEnumerator ChangeLocalScale(RectTransform tran, Vector3 target, float time)
    {
        Vector3 diff = target - tran.localScale;
        Vector3 start = tran.localScale;
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            float r = timer / time;
            tran.localScale = start + diff * r * (2 - r);  // �ȱ���ֱ���˶�
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

    const float ACCELERATION = .5f; // ���ٶ�
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
        if (curCharacter.isBurstTargetEnemy)
        {
            mainCamera.transform.position = enmCamStartPos;
            mainCamera.transform.rotation = enmCamOrigRot;
            Vector3 dir = enmCamOrigPos - enmCamStartPos;
            while (dir.magnitude > .01f)
            {
                yield return new WaitForEndOfFrame();
                mainCamera.transform.Translate(5 * Time.deltaTime * dir, Space.World);
                dir = enmCamOrigPos - mainCamera.transform.position;
            }
            mainCamera.transform.position = enmCamOrigPos;
        }
        else
        {
            foreach(Character c in characters)
            {
                c.mono.MoveBack();
            }
            // ��ͷ�˶����� ��195��15��100���˶�����190��5��95��
            mainCamera.transform.position = chaCamStartPos;
            mainCamera.transform.rotation = chacamorigRot;
            Vector3 dir = chaCamOrigPos - chaCamStartPos;
            while (dir.magnitude > .01f)
            {
                yield return new WaitForEndOfFrame();
                mainCamera.transform.Translate(5 * Time.deltaTime * dir, Space.World);
                dir = chaCamOrigPos - mainCamera.transform.position;
            }
            mainCamera.transform.position = chaCamOrigPos;
        }
    }

    bool isBurstVideoShowing = false;
    IEnumerator CloseVideoAfterPlay(double seconds)
    {
        isBurstVideoShowing = true;
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
        isBurstVideoShowing = false;
    }

    IEnumerator ShowBanner(string info, Color c, float seconds, bool returnToIndex = false)
    {
        bannerImgae.gameObject.SetActive(true);
        bannerImgae.color = c; //new Color(1, .5f, 0, .875f);
        bannerText.text = info; //  "�� ս �� ��";
        bannerImgae.rectTransform.localScale = new Vector3(1, .01f, 1);
        yield return ChangeLocalScale(bannerImgae.rectTransform, Vector3.one, animTime);
        yield return new WaitForSeconds(seconds);
        yield return ChangeLocalScale(bannerImgae.rectTransform, new Vector3(1, .01f, 1), animTime);
        bannerImgae.gameObject.SetActive(false);
        if(returnToIndex)
            SceneManager.LoadScene("Index");
    }

    float totalDealDamage = 0;
    Dictionary<Character, float> characterDealDamage = new Dictionary<Character, float>();
    public void UpdateDealDamage(Creature source, Damage d)
    {
        Character character = source as Character;
        if(character == null)
            return;
        
        characterDealDamage[character] += d.fullValue;
        totalDealDamage += d.fullValue;
        if(totalDealDamage == 0)
        {

        }
        else
        {
            float prevPos = 0;
            for(int i = 0; i < allCharacters.Count; ++i)
            {
                Character c = allCharacters[i];
                float pct = characterDealDamage[c] / totalDealDamage;
                dealDamageText[i].text = c.disname + ":" + characterDealDamage[c].ToString("F2") + "(" + (pct * 100).ToString("F2") + "%)";
                dealDamageBar[i].anchorMin = new Vector2(prevPos, 0);
                prevPos = i == allCharacters.Count - 1 ? 1 : prevPos + pct;
                dealDamageBar[i].anchorMax = new Vector2(prevPos, 1);
            }
        }
    }

    float totalTakeDamage = 0;
    Dictionary<Character, float> characterTakeDamage = new Dictionary<Character, float>();
    public void UpdateTakeDamage(Creature target, Damage d)
    {
        Character character = target as Character;
        if (character == null)
            return;

        characterTakeDamage[character] += d.fullValue;
        totalTakeDamage += d.fullValue;
        if (totalTakeDamage == 0)
        {

        }
        else
        {
            float prevPos = 0;
            for (int i = 0; i < allCharacters.Count; ++i)
            {
                Character c = allCharacters[i];
                float pct = characterTakeDamage[c] / totalTakeDamage;
                takeDamageText[i].text = c.disname + ":" + characterTakeDamage[c].ToString("F2") + "(" + (pct * 100).ToString("F2") + "%)";
                takeDamageBar[i].anchorMin = new Vector2(prevPos, 0);
                prevPos = i == allCharacters.Count - 1 ? 1 : prevPos + pct;
                takeDamageBar[i].anchorMax = new Vector2(prevPos, 1);
            }
        }
    }
}
