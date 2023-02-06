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

    // ֮���ĳ� battle.json ��������Щ
    public string[] teamMembers = { "kazuha", "ganyu", "shenhe", "kokomi" };
    public string[] enemyMembers = { "hilichurl", "hilichurl", "hilichurl" };
}
