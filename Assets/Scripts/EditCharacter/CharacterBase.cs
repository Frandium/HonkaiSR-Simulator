using System.IO;
using UnityEngine;
using LitJson;
using System.Collections.Generic;

public class CharacterBase: CreatureBase
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

    protected int breakLevel = 4;
    public Weapon weapon;
    public Artifact[] artifacts = new Artifact[(int)ArtifactPosition.Count];

    public DisplayArtifact[] disArtifacts;

    public bool isAttackTargetEnemy  = true;
    public SelectionType attackSelectionType  = SelectionType.One;
    public bool isSkillTargetEnemy  = true;
    public SelectionType skillSelectionType  = SelectionType.One;
    public bool isBurstTargetEnemy  = true;
    public SelectionType burstSelectionType  = SelectionType.All;
    public int attackGainPointCount  = 1;
    public int skillConsumePointCount  = 1;

    public CharacterBase() { }
    public CharacterBase(string _dbname)
    {
        LoadJson(_dbname);
    }

    public void LoadJson(string name)
    {
        // Load ��ɫ��������
        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/characters/" + name + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        dbname = name;
        disname = (string)data["disname"];
        level = (int)data["level"];
        breakLevel = (int)data["breakLevel"];
        for(int i = 0; i < (int)CommonAttribute.Speed; ++i)
        {
            // ��Щ����ÿ���� 14 �����ֱ���1����10��δͻ��/ͻ�ơ���70��δͻ��/ͻ�ƣ�80�� ������
            if(level < 20)
            {
                attrs[i] = Utils.Lerp((float)(double)data["attrs"][i][0], (float)(double)data["attrs"][i][0], 19, level - 1);
                continue;
            }
            int levelRate = level % 10;
            if (levelRate == 0)
            {
                // ͻ�Ƶȼ����Ϊ0�� ���Ϊ 6
                if(breakLevel == level / 10 - 1)
                    attrs[i] = (float)(double)data["attrs"][i][2 * i];
                else
                    attrs[i] = (float)(double)data["attrs"][i][2 * i - 1];
            }
            else
            {
                attrs[i] = Utils.Lerp((float)(double)data["attrs"][i][2 * breakLevel - 2], (float)(double)data["attrs"][i][2 * breakLevel - 1], levelRate, 10);
            }
        }
        for(int i = (int)CommonAttribute.Speed; i < (int)CommonAttribute.Count; ++i)
        {
            attrs[i] = (float)(double)data["attrs"][i];
        }
        atkName = (string)data["atk"]["name"];
        atkDescription = (string)data["atk"]["description"];
        skillName = (string)data["skill"]["name"];
        skillDescription = (string)data["skill"]["description"];
        burstName = (string)data["burst"]["name"];
        burstDescription = (string)data["burst"]["description"];
        talentName = (string)data["talent"]["name"];
        talentDescription = (string)data["talent"]["description"];
        mysteryName = (string)data["mystery"]["name"];
        mysteryDescription = (string)data["mystery"]["description"];

        isAttackTargetEnemy = (bool)data["isAttackTargetEnemy"];
        attackSelectionType = (SelectionType)(int)data["attackSelectionType"];
        isSkillTargetEnemy = (bool)data["isSkillTargetEnemy"];
        skillSelectionType = (SelectionType)(int)data["skillSelectionType"];
        isBurstTargetEnemy = (bool)data["isBurstTargetEnemy"];
        burstSelectionType = (SelectionType)(int)data["burstSelectionType"];
        attackGainPointCount = (int)data["attackGainPointCount"];
        skillConsumePointCount = (int)data["skillConsumePointCount"];

        // Load ��ɫ��׶
        JsonData weaponData = data["weapon"];
        string weaponName = (string)weaponData["name"];
        int wLevel = (int)weaponData["level"];
        int rLevle = (int)weaponData["refine"];
        weapon = new Weapon(weaponName, wLevel, rLevle);
        // �ӹ�׶�ļ����� load �����׶�����֣�����-�ȼ��ɳ���Ϣ��������Ч���ݣ���ʾ����

        // Load ��ɫʥ����
        JsonData artsJson = data["artifacts"];
        foreach(JsonData artJson in artsJson)
        {
            // ��װ��
            string suitName = (string)artJson["suitName"];
            // λ��
            ArtifactPosition pos = (ArtifactPosition)(int)artJson["position"];
            // ������
            SimpleValueBuff b = new SimpleValueBuff((CommonAttribute)(int)artJson["main"]["attr"], (float)(double)artJson["main"]["value"], (ValueType)(int)artJson["main"]["type"]);

            // ������
            artifacts[(int)pos] = new Artifact(b, new List<SimpleValueBuff>());
        }

        // Load ��ɫ�����ȼ�
    }

    public override float GetFinalAttr(CommonAttribute attr)
    {
        float res = attrs[(int)attr];
        res += weapon.CalBuffValue(null, null, attr);
        foreach(Artifact art in artifacts)
        {
            res += art.CalBuffValue(this, this, attr);
        }
        return res;
    }
}
