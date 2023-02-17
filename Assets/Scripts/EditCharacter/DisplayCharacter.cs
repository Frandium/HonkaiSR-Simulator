using System.IO;
using UnityEngine;
using LitJson;

public class DisplayCharacter
{
    public string dbname = "test";
    public string disname = "测试角色";

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
        // Load 角色基本属性
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

        // Load 角色光锥
        JsonData weaponData = data["weapon"];
        string weaponName = (string)weaponData["name"];
        int wLevel = (int)weaponData["level"];
        // 从光锥文件夹里 load 这个光锥的名字，属性-等级成长信息，技能特效内容，显示出来

        // Load 角色圣遗物
        JsonData artsJson = data["artifacts"];
        foreach(JsonData artJson in artsJson)
        {
            // 套装名
            string suitName = (string)artJson["suitName"];
            // 位置
            ArtifactPosition pos = (ArtifactPosition)(int)artJson["pos"];
            // 主属性
            SimpleValueBuff b = new SimpleValueBuff((int)artJson["main"]["attr"], (float)(double)artJson["main"]["value"], (ValueType)(int)artJson["main"]["type"]);
            // 副属性

        }

        // Load 角色命座等级
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
