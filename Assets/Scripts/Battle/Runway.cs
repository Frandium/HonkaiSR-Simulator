using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Runway : MonoBehaviour
{

    float len = 100;

    private Queue<Creature> burstInsertQueue;
    private List<RunwayAvatar> burstAvatars;
    private List<RunwayAvatar> runwayAvatars;
    Dictionary<Creature, RunwayAvatar> creature2RunwayAvatar;
    private List<Creature> creatures = new List<Creature>();
    

    public GameObject[] burstAvatarGO;
    public RectTransform[] burstAvatarRect;
    public Image[] burstAvartarImage;

    public GameObject avatarPrefab;
    public Transform runwayTransform;


    private readonly Vector3 firstRunwayAvatarPos = new Vector3(10, -25, 0);

    public void Initialize()
    {
        burstInsertQueue = new Queue<Creature>();
        burstAvatars = new List<RunwayAvatar>();
        runwayAvatars = new List<RunwayAvatar>();
        creature2RunwayAvatar = new Dictionary<Creature, RunwayAvatar>();

        float fastest_time = 100;
        foreach (Creature c in creatures)
        {
            fastest_time = Mathf.Min((len - c.location) / c.speed, fastest_time);
        }
        foreach (Creature c in creatures)
        {
            c.SetLocation(c.location + fastest_time * c.speed);
        }

        creatures.Sort((c1, c2) =>
        {
            if (c1.location == c2.location)
            {
                return c1.uniqueID < c2.uniqueID ? 1: -1;
            }
            return c1.location < c2.location ? 1 : -1;
        });

        for (int i = 0; i < creatures.Count; ++i)
        {
            Creature c = creatures[i];
            GameObject go = Instantiate(avatarPrefab, runwayTransform);
            RunwayAvatar ra = go.GetComponent<RunwayAvatar>();
            runwayAvatars.Add(ra);
            creature2RunwayAvatar[c] = ra;
            ra.SetCreature(c, false);
            go.GetComponent<RectTransform>().anchoredPosition = firstRunwayAvatarPos + new Vector3(0, -55 * i, 0);
        }
    }

    private bool firstTime = true;

    public void AddToRunway(Creature c)
    {
        creatures.Add(c);
        c.SetLocation(0);
    }

    public Creature UpdateRunway(out bool isBurst)
    {
        isBurst = burstInsertQueue.Count > 0;
        if (!firstTime)
        {
            RunwayAvatar firstAvatar = runwayAvatars[0];
            firstAvatar.MoveTowards(new Vector3(10, 25, 0), () => { Destroy(firstAvatar.gameObject); });
            runwayAvatars.RemoveAt(0);
            if (!firstAvatar.IsBurst)
            {
                RunwayAvatar newOne = Instantiate(avatarPrefab, runwayTransform).GetComponent<RunwayAvatar>();
                newOne.SetCreature(firstAvatar.creature, false);
                newOne.gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(10, -500, 0);
                runwayAvatars.Add(newOne);
                creature2RunwayAvatar[firstAvatar.creature] = newOne;
            }
        }
        else
        {
            firstTime = false;
            return creatures[0];
        }

        // 有插入的大招时，先放大招
        if (isBurst)
        {
            for(int i  =0; i < burstAvatars.Count; ++i)
            {
                burstAvatars[i].MoveTowards(burstEndPos[i]);
            }
            runwayAvatars.Insert(0, burstAvatars[0]);
            burstAvatars.RemoveAt(0);
            return burstInsertQueue.Dequeue();
        }

        float fastest_time = 100;
        foreach(Creature c in creatures)
        {
            fastest_time = Mathf.Min((len - c.location) / c.speed, fastest_time);
        }
        foreach(Creature c in creatures)
        {
            c.SetLocation(c.location + fastest_time * c.speed);
        }
        RearrangeRunway();
        return creatures[0];
    }

    private void RearrangeRunway()
    {
        creatures.Sort((c1, c2) =>
        {
            if (c1.location == c2.location) return 0;
            return c1.location < c2.location ? 1 : -1;
        });
        runwayAvatars.Sort((ra1, ra2) =>
       {
           if (ra1.IsBurst) return 1;
           if (ra2.IsBurst) return -1;
           if (ra1.creature.location == ra2.creature.location) return 0;
           return ra1.creature.location < ra2.creature.location ? 1 : -1;
       });
        for(int i = 0;i<runwayAvatars.Count; ++i)
        {
            runwayAvatars[i].MoveTowards(firstRunwayAvatarPos + new Vector3(0, -55 * i, 0));
        }
    }

    public void RemoveFromRunway(Creature creature)
    {
        creatures.Remove(creature);
        RunwayAvatar ra = runwayAvatars.Find(r =>
        {
            return r.creature == creature;
        });
        Destroy(ra.gameObject);
        runwayAvatars.Remove(ra);
    }

    public void InsertBurst(Character c)
    {
        burstInsertQueue.Enqueue(c);
        GameObject go = Instantiate(avatarPrefab, runwayTransform);
        RunwayAvatar ra = go.GetComponent<RunwayAvatar>();
        burstAvatars.Add(ra);
        ra.SetCreature(c, true);
        go.GetComponent<RectTransform>().anchoredPosition = burstStartPos[burstInsertQueue.Count];
        go.GetComponent<Image>().sprite = c.runwayAvatar;
        ra.MoveTowards(burstEndPos[burstInsertQueue.Count]);
    }

    private Vector3[] burstStartPos = { new Vector3(0, 0, 0), new Vector3(270, -25, 0), new Vector3(340, -25, 0), new Vector3(410, -25, 0), new Vector3(480, -25, 0) };

    private Vector3[] burstEndPos = { new Vector3(10, -25, 0), new Vector3(90, -25, 0), new Vector3(160, -25, 0), new Vector3(230, -25, 0), new Vector3(300, -25, 0) };


}
