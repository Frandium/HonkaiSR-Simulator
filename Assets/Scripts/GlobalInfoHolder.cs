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

    public string characterDir = Application.streamingAssetsPath + "/characters";
    public string enemyDir = Application.streamingAssetsPath + "/enemies";
    public string battleDir = Application.streamingAssetsPath + "/battles";

    // 之后会改成 battle.json 里配置这些
    public string[] teamMembers = { "kazuha", "ganyu", "shenhe", "kokomi" };
    public string[] enemyMembers = { "hilichurl", "hilichurl", "hilichurl" };

    public string battleFilePath = Application.streamingAssetsPath + "/battles/default_battle.json";
}
