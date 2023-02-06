using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class CreatureTemplate: MonoBehaviour
{
    public string dbname = "test";
    public string disname = "²âÊÔ½ÇÉ«";
    public int ID = 1;
    public float speed = 50;

    public float maxHp  = 100;
    public float atk  = 10;
    public float def  = 5;

    public float[] elementalBonus = { 0, 0, 0, 0, 0, 0, 0, 0 };

    public float[] elementalResist  = { .1f, .1f, .1f, .1f, .1f, .1f, .1f, .1f };

    public bool isAttackTargetEnemy  = true;
    public SelectionType attackSelectionType  = SelectionType.One;
    public bool isSkillTargetEnemy  = true;
    public SelectionType skillSelectionType  = SelectionType.One;
    public bool isBurstTargetEnemy  = true;
    public SelectionType burstSelectionType  = SelectionType.All;
    public int attackGainPointCount  = 1;
    public int skillConsumePointCount  = 1;

    public void DumpJson()
    {
        string file_path = Application.streamingAssetsPath + "/" + dbname + ".json";
    }

    public void LoadJson(string file_path)
    {

        string s =JsonUtility.ToJson(this);
        Debug.Log(s);
        
    }

    public static List<string> LoadJsonList()
    {
        return null;
    }

}
