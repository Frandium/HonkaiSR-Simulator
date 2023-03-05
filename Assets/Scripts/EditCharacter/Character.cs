using System.IO;
using UnityEngine;
using LitJson;
using System.Collections.Generic;

public class Character : Creature
{
    public string atkName = "��ͨ����";
    public string atkDescription = "��ͨ�����˺�";
    public string skillName = "ս��";
    public string skillDescription = "ս���˺�";
    public string burstName = "�սἼ";
    public string burstDescription = "�սἼ�˺�";
    public string talentName = "�츳";
    public string talentDescription = "�츳Ч��";
    public string mysteryName = "�ؼ�";
    public string mysteryDescription = "�ؼ�Ч��";

    public new int level { get { return config.level; } }
    public int breakLevel { get { return config.breakLevel; } }
    public int constellaLevel { get { return config.constellaLevel; }} // ��֮��
    public int atkLevel { get { return Mathf.Min(15, config.atkLevel + additionalAtkLevel); } }
    int additionalAtkLevel = 0;
    public int skillLevel { get { return Mathf.Min(15, config.skillLevel + additionalSkillLevel); } }
    int additionalSkillLevel = 0;
    public int burstLevel { get { return Mathf.Min(15, config.burstLevel + additionalBurstLevel); } }
    int additionalBurstLevel = 0;
    public int talentLevel { get { return config.talentLevel; } }
    int additionalTalentLevel = 0;
    public Element element { get; protected set; } = Element.Count;
    public Career career { get; protected set; } = Career.Count;
    public float energy { get; protected set; } = 0;
    public float maxEnergy { get; protected set; } = 60;
    public new ACharacterTalents talents { get; protected set; }
    public JsonData metaData { get; protected set; }
    public new CharacterMono mono { get; protected set; }
    public CharacterConfig config { get; protected set; }
    public Weapon weapon;
    public List<AEquipmentTalents> artifactsSuit = new List<AEquipmentTalents>();
    public List<Artifact> artifacts = new List<Artifact>((int)ArtifactPosition.Count);

    public bool isAttackTargetEnemy { get; protected set; } = true;
    public SelectionType attackSelectionType { get; protected set; } = SelectionType.One;
    public bool isSkillTargetEnemy { get; protected set; } = true;
    public SelectionType skillSelectionType { get; protected set; } = SelectionType.One;
    public bool isBurstTargetEnemy { get; protected set; } = true;
    public SelectionType burstSelectionType { get; protected set; } = SelectionType.All;
    public int attackGainPointCount { get; protected set; } = 1;
    public int skillConsumePointCount { get; protected set; } = 1;
    public float attackGainEnergy { get; protected set; } = 2.5f;
    public float skillGainEnergy { get; protected set; } = 2.5f;
    public float takeDmgGainEnergy { get; protected set; } = 2.5f;

    public delegate void TalentUponTarget(Creature target);
    public List<TriggerEvent<TalentUponTarget>> onNormalAttack { get; protected set; } = new List<TriggerEvent<TalentUponTarget>>();
    public List<TriggerEvent<TalentUponTarget>> onSkill { get; protected set; } = new List<TriggerEvent<TalentUponTarget>>();
    public List<TriggerEvent<TalentUponTarget>> onBurst { get; protected set; } = new List<TriggerEvent<TalentUponTarget>>();
    public Character() { }

    public Character(string _dbname)
    {
        LoadJson(_dbname);
    }

    public void LoadJson(string name)
    {
        // �Ȱ�֮ǰ��������
        weapon?.OnTakingOff(this);
        foreach(AEquipmentTalents t in artifactsSuit)
        {
            t.OnTakingOff(this);
        }
        artifactsSuit.Clear();

        config = new CharacterConfig(name);

        // Load ��ɫ��������
        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/characters/" + name + ".json");
        metaData = JsonMapper.ToObject(jsonString);

        // set character template
        dbname = name;
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
        onBurst.Clear();
        onSkill.Clear();
        onNormalAttack.Clear();
        artifacts.Clear();
        artifactsSuit.Clear();
        additionalAtkLevel = 0;
        additionalBurstLevel = 0;
        additionalSkillLevel = 0;
        additionalTalentLevel = 0;
        
        // ֮��ĳɷ��� dict?
        switch (dbname)
        {
            case "bronya":
                talents = new Bronya(this);
                break;
            case "seele":
                talents = new Seele(this);
                break;
            case "japard":
                talents = new Japard(this);
                break;
            case "tingyun":
                talents = new Tingyun(this);
                break;
            default:
                talents = new Bronya(this);
                break;
        }
        talents.OnEquipping();
        base.talents = talents;

        disname = (string)metaData["disname"];
        element = (Element)(int)metaData["element"];
        career = (Career)(int)metaData["career"];
        maxEnergy = (float)(double)metaData["maxEnergy"];
        for (int i = 0; i < (int)CommonAttribute.Speed; ++i)
        {
            // ��Щ����ÿ���� 14 �����ֱ���1����10��δͻ��/ͻ�ơ���70��δͻ��/ͻ�ƣ�80�� ������
            if (level < 20)
            {
                attrs[i] = Utils.Lerp((float)(double)metaData["attrs"][i][0], (float)(double)metaData["attrs"][i][1], level - 1, 19);
                continue;
            }
            int levelRate = level % 10;
            if (levelRate == 0)
            {
                // ͻ�Ƶȼ����Ϊ0�� ���Ϊ 6
                if (breakLevel == level / 10 - 1) // ͻ����
                    attrs[i] = (float)(double)metaData["attrs"][i][2 * breakLevel];
                else // Ӧ���� breaklevel == level / 10 - 2��ûͻ��
                    attrs[i] = (float)(double)metaData["attrs"][i][2 * breakLevel + 1];
            }
            else
            {
                // breaklevel = level / 10 - 1
                attrs[i] = Utils.Lerp((float)(double)metaData["attrs"][i][2 * breakLevel], (float)(double)metaData["attrs"][i][2 * breakLevel + 1], levelRate, 10);
            }
        }
        for (int i = (int)CommonAttribute.Speed; i < (int)CommonAttribute.Count; ++i)
        {
            attrs[i] = (float)(double)metaData["attrs"][i];
        }
        atkName = (string)metaData["atk"]["name"];
        atkDescription = Utils.FormatDescription((string)metaData["atk"]["description"], metaData["atk"], atkLevel);
        skillName = (string)metaData["skill"]["name"];
        skillDescription = Utils.FormatDescription((string)metaData["skill"]["description"], metaData["skill"], skillLevel);
        burstName = (string)metaData["burst"]["name"];
        burstDescription = Utils.FormatDescription((string)metaData["burst"]["description"], metaData["burst"], burstLevel);
        talentName = (string)metaData["talent"]["name"];
        talentDescription = Utils.FormatDescription((string)metaData["talent"]["description"], metaData["talent"], talentLevel);
        mysteryName = (string)metaData["mystery"]["name"];
        mysteryDescription = (string)metaData["mystery"]["description"];

        isAttackTargetEnemy = (bool)metaData["isAttackTargetEnemy"];
        attackSelectionType = (SelectionType)(int)metaData["attackSelectionType"];
        isSkillTargetEnemy = (bool)metaData["isSkillTargetEnemy"];
        skillSelectionType = (SelectionType)(int)metaData["skillSelectionType"];
        isBurstTargetEnemy = (bool)metaData["isBurstTargetEnemy"];
        burstSelectionType = (SelectionType)(int)metaData["burstSelectionType"];
        attackGainPointCount = (int)metaData["attackGainPointCount"];
        skillConsumePointCount = (int)metaData["skillConsumePointCount"];
        attackGainEnergy = (float)(double)metaData["attackGainEnergy"];
        skillGainEnergy = (float)(double)metaData["skillGainEnergy"];
        takeDmgGainEnergy = (float)(double)metaData["takeDmgGainEnergy"];

        // Load ��ɫ��׶
        weapon = new Weapon(config.weaponConfig);
        if (weapon.career == career)
            weapon.OnEquipping(this);

        // Load ��ɫʥ����
        Dictionary<string, int> suitCount = new Dictionary<string, int>();
        foreach (ArtifactConfig arti in config.artifacts)
        {
            // ��װ��
            string suitName = arti.suitName;
            if (!suitCount.ContainsKey(suitName))
                suitCount[suitName] = 0;
            suitCount[suitName]++;
            // λ�� TODO: ����λ���ظ�
            ArtifactPosition pos = arti.position;
            
            artifacts.Add(new Artifact(arti.mainPhrase, arti.vicePhrases));
        }

        hp = GetFinalAttr(CommonAttribute.MaxHP);
    }

    public override float GetBaseAttr(CommonAttribute attr)
    {
        if (attr == CommonAttribute.ATK)
            return attrs[(int)CommonAttribute.ATK] + weapon.atk;
        if (attr == CommonAttribute.MaxHP)
            return attrs[(int)CommonAttribute.MaxHP] + weapon.maxHp;
        if (attr == CommonAttribute.DEF)
            return attrs[(int)CommonAttribute.DEF] + weapon.def;
        return base.GetBaseAttr(attr);
    }

    public override float GetFinalAttr(Creature c1, Creature c2, CommonAttribute attr, DamageType damageType)
    {
        float res = base.GetFinalAttr(c1, c2, attr, damageType);
        res += weapon.CalBuffValue(null, null, attr, damageType);
        foreach(Artifact art in artifacts)
        {
            res += art.CalBuffValue(this, this, attr, damageType);
        }
        return res;
    }

    public override void TakeDamage(Creature source, Damage damage)
    {
        base.TakeDamage(source, damage);
        ChangeEnergy(takeDmgGainEnergy);
    }

    public void ChangeEnergy(float offset)
    {
        energy += offset;
        if (energy > maxEnergy) energy = maxEnergy;
        if (energy < 0) energy = 0;
        mono.UpdateEnergyIcon();
    }

    public void SetMono(CharacterMono m)
    {
        mono = m;
        base.mono = m;
        m.Initialize(this);
    }

    public override void EndNormalTurn()
    {
        base.EndNormalTurn();
        onNormalAttack.RemoveAll(p => p.CountDown());
        onSkill.RemoveAll(p => p.CountDown());
        onBurst.RemoveAll(p => p.CountDown());        
    }

    public virtual void StartBurstTurn()
    {
        foreach (var p in onTurnStart)
        {
            p.trigger();
        }
        onTurnStart.RemoveAll(p => p.CountDown());
        mono?.StartMyTurn();
    }

    public virtual void EndBurstTurn()
    {
        // Remove event
        foreach (var p in onTurnEnd)
        {
            p.trigger();
        }
        onTurnEnd.RemoveAll(p => p.CountDown());

        // Remove Buff
        for (int i = buffs.Count - 1; i >= 0; --i)
        {
            if (buffs[i].CountDown())
            {
                Utils.valueBuffPool.ReturnOne(buffs[i]);
                buffs.RemoveAt(i);
            }
        }

        // Remove shields
        shields.RemoveAll(s => s.CountDown());

        // Remove states
        states.RemoveAll(s => s.CountDown());

        mono?.EndBurstTurn();
    }

    public void SaveConfig()
    {
        config.Save();
        Initialize();
    }

    public void ATKLevelUp(int offset)
    {
        additionalAtkLevel += offset;
    }
    public void SkillLevelUp(int offset)
    {
        additionalSkillLevel += offset;
    }
    public void BurstLevelUp(int offset)
    {
        additionalBurstLevel += offset;
    }
    public void TalentLevelUp(int offset)
    {
        additionalTalentLevel += offset;
    }
}
