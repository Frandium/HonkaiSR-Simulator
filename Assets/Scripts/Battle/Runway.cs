using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Runway : MonoBehaviour
{

    public static readonly float Length = 10000;

    private Queue<Creature> burstWaitingQueue;
    private List<RunwayAvatar> burstAvatars;
    private List<RunwayAvatar> runwayAvatars;

    Dictionary<Creature, RunwayAvatar> creature2RunwayAvatar;
    private List<Creature> creatures = new List<Creature>();
    
    private Creature addtionalWaiting;
    private bool firstTime = true;
    private Creature interruptedByBurst = null;
    private Creature curCreature = null;
    private bool isAdditionalTurn = false;


    public GameObject[] burstAvatarGO;
    public RectTransform[] burstAvatarRect;
    public Image[] burstAvartarImage;

    public GameObject avatarPrefab;
    public Transform runwayTransform;


    private readonly Vector3 firstRunwayAvatarPos = new Vector3(10, -25, 0);
    private readonly Vector3 runwayAvatarInternal = new Vector3(0, -55, 0);

    void Start()
    {
        burstWaitingQueue = new Queue<Creature>();
        burstAvatars = new List<RunwayAvatar>();
        runwayAvatars = new List<RunwayAvatar>();
        creature2RunwayAvatar = new Dictionary<Creature, RunwayAvatar>();
    }


    public void AddCreature(Creature c)
    {
        creatures.Add(c);
        c.ChangePercentageLocation(-100);
        RunwayAvatar newOne = Instantiate(avatarPrefab, runwayTransform).GetComponent<RunwayAvatar>();
        newOne.SetCreature(c, false);
        newOne.gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(10, -500, 0);
        runwayAvatars.Add(newOne);
        creature2RunwayAvatar[c] = newOne;
    }

    public Creature UpdateRunway(out bool isBurst, out bool isAdditional)
    {
        isBurst = burstWaitingQueue.Count > 0;
        isAdditional = false;
        if (!firstTime)
        {
            // 移除第一个 avatar，如果被移除的不是 burst，在队尾创建一个新的
            RunwayAvatar firstAvatar = runwayAvatars[0];
            firstAvatar.MoveTowards(new Vector3(10, 25, 0), () => { Destroy(firstAvatar.gameObject); });
            runwayAvatars.RemoveAt(0);
            if (!firstAvatar.IsBurst && !isAdditionalTurn)
            {
                RunwayAvatar newOne = Instantiate(avatarPrefab, runwayTransform).GetComponent<RunwayAvatar>();
                newOne.SetCreature(firstAvatar.creature, false);
                newOne.gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(10, -500, 0);
                runwayAvatars.Add(newOne);
                creature2RunwayAvatar[firstAvatar.creature] = newOne;
            }
        }
        firstTime = false;

        if (addtionalWaiting != null)
        {
            Creature c = addtionalWaiting;
            // 播放插入回合的动画
            GameObject go = Instantiate(avatarPrefab, runwayTransform);
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, -800);
            go.GetComponent<Image>().sprite = c.mono.runwayAvatar;
            RunwayAvatar ra = go.GetComponent<RunwayAvatar>();
            ra.SetCreature(c, true);
            ra.MoveTowards(firstRunwayAvatarPos);
            runwayAvatars.Insert(0, ra);
            addtionalWaiting = null;
            isAdditional = true;
            isAdditionalTurn = true;
            curCreature = c;
            return c;
        }

        isAdditionalTurn = false;
        // 有插入的大招时，先放大招
        if (isBurst)
        {
            for(int i  =0; i < burstAvatars.Count; ++i)
            {
                burstAvatars[i].MoveTowards(firstBurstEndPos + i * burstAvatarInternal);// burstEndPos[i]);
            }
            runwayAvatars.Insert(0, burstAvatars[0]);
            burstAvatars.RemoveAt(0);
            return burstWaitingQueue.Dequeue();
        }

        if(interruptedByBurst != null)
        {
            Creature c = interruptedByBurst;
            curCreature = c;
            interruptedByBurst = null;
            RearrangeRunwayUI();
            return c;
        }

        // 非大招回合，update 所有人的进度，更新UI
        float fastest_time = Length;
        for(int i = 0; i < creatures.Count; ++i)
        {
            fastest_time = Mathf.Min((Length - Mathf.Min(creatures[i].location, Length)) / creatures[i].GetFinalAttr(CommonAttribute.Speed), fastest_time);
        }
        foreach(Creature c in creatures)
        {
            c.ChangeAbsoluteLocation(fastest_time * c.GetFinalAttr(CommonAttribute.Speed));
        }
        creatures.Sort((c1, c2) =>
        {
            if (c1.location == c2.location) 
                return 0;
            return c1.location < c2.location ? 1 : -1;
        });
        RearrangeRunwayUI();
# if UNITY_EDITOR
        string pos = "";
        foreach(Creature c in creatures)
        {
            pos += c.dbname + ":" + c.location + "|";
        }
        Debug.Log(pos);
#endif
        curCreature = creatures[0];
        return creatures[0];
    }

    private void RearrangeRunwayUI()
    {
        for (int i = 0; i < creatures.Count; ++i)
        {
            runwayAvatars[runwayAvatars.Count - 1 - i] = creature2RunwayAvatar[creatures[creatures.Count - 1 - i]];
        }
        for (int i = 0; i < runwayAvatars.Count; ++i)
        {
            runwayAvatars[i].MoveTowards(firstRunwayAvatarPos + i * runwayAvatarInternal);
        }
    }

    public void RemoveCreature(Creature creature)
    {
        creatures.Remove(creature);
        runwayAvatars.Remove(creature2RunwayAvatar[creature]);
        Destroy(creature2RunwayAvatar[creature].gameObject);
        creature2RunwayAvatar.Remove(creature);
    }

    public void InsertBurst(Creature c, bool immediately = false)
    {
        // 首先创建一个新的 avatar，设置它的 creature
        GameObject go = Instantiate(avatarPrefab, runwayTransform);
        go.GetComponent<RectTransform>().anchoredPosition = firstBurstStartPos + burstWaitingQueue.Count * burstAvatarInternal;
        go.GetComponent<Image>().sprite = c.mono.runwayAvatar;
        RunwayAvatar ra = go.GetComponent<RunwayAvatar>();
        ra.SetCreature(c, true);

        if (!immediately)
        { // 如果现在是 burst / 敌人的回合，那么当前插入的回合去等待队列
            burstWaitingQueue.Enqueue(c);
            burstAvatars.Add(ra);
            ra.MoveTowards(firstBurstEndPos + burstWaitingQueue.Count * burstAvatarInternal);
        }
        else
        { // 如果现在不是 burst，那么当前插入的回合到 runway 头，其他人后退
            for (int i = 0; i < runwayAvatars.Count; ++i)
            {
                runwayAvatars[i].MoveTowards(firstRunwayAvatarPos + (i + 1) * runwayAvatarInternal);
            }
            runwayAvatars.Insert(0, ra);
            ra.MoveTowards(firstRunwayAvatarPos);
            interruptedByBurst = curCreature;
        }
    }

    public void InsertAdditionalTurn(Creature c)
    {
        addtionalWaiting = c;
    }

    private readonly Vector3 firstBurstStartPos = new Vector3(200, -25, 0);
    private readonly Vector3 firstBurstEndPos = new Vector3(10, -25, -0);
    private readonly Vector3 burstAvatarInternal = new Vector3(70, 0, 0);
}
