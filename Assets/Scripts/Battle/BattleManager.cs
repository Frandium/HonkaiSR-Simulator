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
    Self, // ���� = 0
    One,  // ���� = 1
    OneExceptSelf, // ���������һ�� = 2
    All, // ȫ�� = 3
    AllExceptSelf, //��������ȫ�� = 4
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
    float minTurnTime = 3; // ÿ���غ����� 3 �룬��ֹ��������̫��� bug
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
    Vector3 enmOriginal = new Vector3(147.2f, 8.1f, 99); // �� 0 �����˵�λ��
    Vector3 enmInternal = new Vector3(12.8f, 0, 7.4f);  // �����Ų����
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
        // һ���ҷ��غϰ���ָ������׶Ρ����㶯���׶�
        // һ���з��غϰ������㶯���׶�
        // ���������е�λ�Ľ��㶯���׶ν���ʱ��������һ�غ�
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

        // ����ʱ�̣��������
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
        mystery = (string)data["mystery"];
    }

    public void NextTurn()
    {
        turnTime = 0;
        // �����ж��Ƿ���Ϸ����
        if (enemies.Count == 0) // ���е��˶�����
        {   
            if (++enmWave >= enmNames.Count) // û����һ���ˣ�����
            {
                StartCoroutine(ShowBanner("�� ս �� ��", new Color(1, .5f, 0, .875f), 1, true));
                bgm.Stop();
                curStage = TurnStage.GameEnd;
                return;
            }
            else// ������һ��
            {
                StartCoroutine(ShowBanner("�� " + (enmWave + 1).ToString() + " / " + enmNames.Count.ToString() + " �� �� ��", new Color(1, .5f, 0, .875f), 1));
                for (int i = 0; i < enmNames[enmWave].Count; ++i)
                {
                    EnemyMono em = Instantiate(enemyPrefab, enmOriginal + i * enmInternal, Quaternion.Euler(0, -30, 0)).GetComponent<EnemyMono>();
                    Enemy e = new Enemy(enmNames[enmWave][i]);
                    enemies.Add(e);
                    e.SetMono(em);
                    runway.AddCreature(e);
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

        // ����һ�غϽ�����β��= null ʱ�ǵ�һ�غϣ���������׶�
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
                curCreature.ChangePercentageLocation(-100);
                curCreature.EndNormalTurn();
            }
        }
        else
        {
            // ��һ�غϷ����ؼ�
            Character c = characters.Find(x => x.dbname == mystery);
            c.talents.Mystery(characters, enemies);
        }

        // �����»غ�
        // �� runway ѯ�ʱ��غ��Ƿ���Ԫ�ر����غϡ�Ԫ�ر���������غϣ��������غϿ�ʼ�Ľ����� hook
        curCreature = runway.UpdateRunway(out isBurst);
        if (curCreature is Character) // ��ҵĻغ�
        {
            curStage = TurnStage.Instruction;
            curCharacter = curCreature as Character;
            // �Ƚ�������ָ��׶�
            if (isBurst)
            { // Ԫ�ر����غϴ��� burst start hook
                BurstTurn();
            }
            else
            {
                // ���������غ��Ǹղű�Ԫ�ر�����ϵĻغϣ������� start��
                if (interrupted)
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
            // ���˵Ļغ�ֱ�ӽ�����㶯���׶Σ������ burst Ҫ�ȵ����ж���
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

    private void ShowCharacterDetail(int x)
    {
        if (chaDetailActivated)
            return;
        Character c = characters[x];
        chaDetailActivated = true;
        characterDetail.SetActive(true);
        Text t = characterDetail.GetComponentInChildren<Text>();
        string show = "��ɫ����" + c.disname + "\n����ֵ��" + c.hp + "\nλ�ã�" + (c.location / Runway.Length * 100) + "%";

        for(int i = 0; i < (int)CommonAttribute.Count; ++i)
        {
            float b = c.GetBaseAttr((CommonAttribute)i);
            float f = c.GetFinalAttr((CommonAttribute)i);
            show += "\n" + Utils.attributeNames[i] + "��" +
                 b + " + <color=green>" + (f-b) + "</color> = " + f;
        }
        t.text = show;
    }

    public void RemoveEnemy(Enemy e)
    {
        enemies.Remove(e);
        runway.RemoveCreature(e);
    }

    public void RemoveCharacter(Character c)
    {
        runway.RemoveCreature(c);
        characters.Remove(c);
        deadCharacters.Add(c);
    }


    // Animation Settings
    float burstSplashSpeed = .01f;
    const float BURST_INIT_SPEED = 3f;
    const float BURST_FINAL_SPEED = .1f;
    const float BURST_IMAGE_HALF_DISTANCE = .48f; // �ٶȱ仯�ĳ���
    const float BURST_IMAGE_WIDTH = 1f;  // ͼƬ���
    const float ACCELERATION = (BURST_INIT_SPEED * BURST_INIT_SPEED - BURST_FINAL_SPEED * BURST_FINAL_SPEED) / (BURST_IMAGE_HALF_DISTANCE * (1 + BURST_IMAGE_WIDTH)) / 2; // ���ٶ�

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
        screenCanvas.SetActive(false);
        yield return new WaitForSeconds((float)seconds);
        videoPlayer.enabled = false;
        selection.ApplyAction(curCharacter.onBurst);
        curStage = TurnStage.Animation;
        screenCanvas.SetActive(true);
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
}
