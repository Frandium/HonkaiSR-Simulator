using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalInfoHolder
{
    public static GlobalInfoHolder Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GlobalInfoHolder();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    static GlobalInfoHolder _instance;

    public string characterDir { get; protected set; } = Application.streamingAssetsPath + "/characters";
    public string weaponDir { get; protected set; } = Application.streamingAssetsPath + "/weapons";
    public string artifactsDir { get; protected set; } = Application.streamingAssetsPath + "/artifacts";
    public string enemyDir { get; protected set; } = Application.streamingAssetsPath + "/enemies";
    public string battleDir { get; protected set; } = Application.streamingAssetsPath + "/battles";
    public string characterConfigDir { get; protected set; } = Application.streamingAssetsPath + "/user";

    public string battleFilePath = Application.streamingAssetsPath + "/battles/boss.json";

    public string mystery = "japard";

    public string[] teamMembers = new string[] { "bronya", "japard", "seele", "tingyun" };

}
