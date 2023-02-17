using System.IO;
using UnityEngine;
using LitJson;

public class DisplayCharacter
{
    public string dbname = "test";
    public string disname = "���Խ�ɫ";

    public double[] attrs = new double[(int)CharacterAttribute.Count];
    public Weapon weapon;
    public Artifact[] artifacts;

    public DisplayArtifact[] disArtifacts;

    
    

    public bool isAttackTargetEnemy  = true;
    public SelectionType attackSelectionType  = SelectionType.One;
    public bool isSkillTargetEnemy  = true;
    public SelectionType skillSelectionType  = SelectionType.One;
    public bool isBurstTargetEnemy  = true;
    public SelectionType burstSelectionType  = SelectionType.All;
    public int attackGainPointCount  = 1;
    public int skillConsumePointCount  = 1;

    public DisplayCharacter()
    {
        LoadJson(dbname);
    }

    public void LoadJson(string name)
    {
        // Load ��ɫ��������
        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/characters/" + name + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        dbname = name;
        disname = (string)data["disname"];
        for(int i = 0; i < (int)CharacterAttribute.Count; ++i)
        {
            attrs[i] = (double)data["attrs"][i];
        }
       
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
        // �ӹ�׶�ļ����� load �����׶�����֣�����-�ȼ��ɳ���Ϣ��������Ч���ݣ���ʾ����

        // Load ��ɫʥ����
        JsonData artsJson = data["artifacts"];
        foreach(JsonData artJson in artsJson)
        {
            // ��װ��
            string suitName = (string)artJson["suitName"];
            // λ��
            ArtifactPosition pos = (ArtifactPosition)(int)artJson["pos"];
            // ������
            SimpleValueBuff b = new SimpleValueBuff((int)artJson["main"]["attr"], (float)(double)artJson["main"]["value"], (ValueType)(int)artJson["main"]["type"]);
            // ������

        }

        // Load ��ɫ�����ȼ�
    }

    public double GetBaseAttribute(CharacterAttribute attr)
    {
        return attrs[(int)attr];
    }

    public double GetFinalAttribute(CharacterAttribute attr)
    {
        return 0;
    }
}
