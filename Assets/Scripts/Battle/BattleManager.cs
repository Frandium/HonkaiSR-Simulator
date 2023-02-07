using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using LitJson;

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

    // battle turn state
    public TurnStage curStage { get; protected set; } = TurnStage.Instruction;
    bool isAttackOrSkill = false;
    float animTime = .2f;
    int enmWave = -1;
    int unitCount = 0;
    private bool isBurst = false;
    public List<Character> characters;
    public List<Enemy> enemies { get; private set; } = new List<Enemy>();
    public GameObject enemyPrefab;

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
    public Text gameEndText;
    public Image attackImage;
    public Image skillImage;
    public Image splash;
    public Image gameEndImage;
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

    // Start is called before the first frame update
    void Start()
    {
        valueBuffPool = new ObjectPool<ValueBuff>(30);
        triggerBuffPool = new ObjectPool<TriggerBuff>(30);
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
                        if (isAttackOrSkill)
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

        // ����ʱ�̣��������
        for(int i = 0; i < 4; ++i)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                TestAndInsertBurst(i);
            } 
        }
    }

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
            characters[i].Initialize(chaNames[i], unitCount++);
        }
    }

    public void NextTurn()
    {
        // �����ж��Ƿ���Ϸ����
        if (enemies.Count == 0) // ���е��˶�����
        {   
            if (++enmWave >= enmNames.Count) // û����һ���ˣ�����
            {
                StartCoroutine(ShowBanner("�� ս �� ��", new Color(1, .5f, 0, .875f), 1));
                bgm.Stop();
                return;
            }
            else// ������һ��
            {
                StartCoroutine(ShowBanner("�� " + (enmWave + 1).ToString() + " / " + enmNames.Count.ToString() + " �� �� ��", new Color(1, .5f, 0, .875f), 1));
                //gameEndImage.gameObject.SetActive(true);
                //gameEndImage.color = new Color(1, .5f, 0, .875f);
                //gameEndText.text = "�� ս �� ��";
                for (int i = 0; i < enmNames[enmWave].Count; ++i)
                {
                    Enemy e = Instantiate(enemyPrefab, enmOriginal + i * enmInternal, Quaternion.Euler(0, -30, 0)).GetComponent<Enemy>();
                    e.Initialize(enmNames[enmWave][i], unitCount++);
                    enemies.Add(e);
                }
            }
        }
        if (characters.Count == 0)
        {
            gameEndImage.gameObject.SetActive(true);
            gameEndImage.color = new Color(0, .5f, .75f, .875f);
            gameEndText.text = "�� ս ʧ ��";
            bgm.Stop();
            return;
        }

        // ����һ�غϽ�����β��= null ʱ�ǵ�һ�غϣ���������׶�
        if (curCharacter != null)
        {
            // �ս����Ļغ���Ԫ�ر����غ�
            if (isBurst && curCreature is Character)
            {
                (curCreature as Character).BurstEnd();
            }
            else
            {
                // ���Ƿ�Ԫ�ر����غϣ��ͽ��ж����� 0�������غϽ��� hook
                curCreature.location = 0;
                curCreature.EndMyTurn();
            }
        }

        // �����»غ�
        // �� runway ѯ�ʱ��غ��Ƿ���Ԫ�ر����غϡ�Ԫ�ر���������غϣ��������غϿ�ʼ�Ľ����� hook
        bool isBurstPre = isBurst;
        curCreature = runway.UpdateRunway(out isBurst);


        if (curCreature is Character) // ��ҵĻغ�
        {
            curStage = TurnStage.Instruction;
            curCharacter = curCreature as Character;
            // �Ƚ�������ָ��׶�
            if (isBurst)
            { // Ԫ�ر����غϲ����� turn start hook
                BurstTurn(curCharacter);
            }
            else
            {
                if(!isBurstPre) // ��һ�غϲ��� burst���Ŵ����غϿ�ʼ hook����Ȼ���δ����غϿ�ʼ��
                    curCreature.StartMyTurn();
                curCharacter.PlayAudio(AudioType.Change);
                attackImage.sprite = curCharacter.attackIcon;
                skillImage.sprite = curCharacter.skillIcon;
                selection.StartNewTurn(curCharacter);
                SelectTarget();
            }
        }
        else if (curCreature is Enemy)
        {
            // ���˵Ļغ�ֱ�ӽ�����㶯���׶Σ������ burst Ҫ�ȵ����ж���
            curCreature.StartMyTurn();
            curStage = TurnStage.Animation;
            Enemy e = curCreature as Enemy;
            e.enemyAction.MyTurn();
        }
    }

    protected void BurstTurn(Character c)
    {
        curCharacter = c;
        curCreature = c;
        BurstSplash(curCharacter);
        curCharacter.PlayAudio(AudioType.BurstPrepare);
        if (curCharacter.isBurstTargetEnemy)
            selection.StartEnemySelection(curCharacter.burstSelectionType, curCharacter.attackTalents.BurstEnemyAction);
        else
            selection.StartCharacterSelection(curCharacter.burstSelectionType, curCharacter.attackTalents.BurstCharacterAction);
    }

    protected void SelectTarget()
    {
        skillPoint.StartGainPointAnim(curCharacter.attackGainPointCount);
        StartCoroutine(ChangeLocalScale(attackRecttrans, Vector3.one * 1.2f, animTime));
        StartCoroutine(ChangeLocalScale(skillRecttrans, Vector3.one, animTime));
        isAttackOrSkill = true;
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
        if (c.isBurstActivated || !c.isAlive || !c.isFullyCharged)
            return;

        runway.InsertBurst(c, isBurst);
        c.ActivateBurst();
        audioSource.clip = burstInsert;
        audioSource.Play();
        // ֻ����Ŀǰ���� burst �غϣ����Դ��� instruction �׶�ʱ�����ܴ�ϱ��˵��ж�
        if (!isBurst && curStage == TurnStage.Instruction)
        {
            isBurst = true;
            BurstTurn(c);
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

    // Animation Settings
    float burstSplashSpeed = .01f;
    const float BURST_INIT_SPEED = 3f;
    const float BURST_FINAL_SPEED = .1f;
    const float BURST_IMAGE_HALF_DISTANCE = .48f; // �ٶȱ仯�ĳ���
    const float BURST_IMAGE_WIDTH = 1.5f;  // ͼƬ���
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

    IEnumerator CloseVideoAfterPlay(double seconds)
    {
        yield return new WaitForSeconds((float)seconds);
        videoPlayer.enabled = false;
        selection.ApplyAction();
        curStage = TurnStage.Animation;
    }

    IEnumerator ShowBanner(string info, Color c, float seconds)
    {
        gameEndImage.gameObject.SetActive(true);
        gameEndImage.color = c; //new Color(1, .5f, 0, .875f);
        gameEndText.text = info; //  "�� ս �� ��";
        gameEndImage.rectTransform.localScale = new Vector3(1, .01f, 1);
        yield return ChangeLocalScale(gameEndImage.rectTransform, Vector3.one, animTime);
        yield return new WaitForSeconds(seconds);
        yield return ChangeLocalScale(gameEndImage.rectTransform, new Vector3(1, .01f, 1), animTime);
        gameEndImage.gameObject.SetActive(false);
    }
}
