using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalInfoHolder
{


    public static string characterDir { get; protected set; } = Application.persistentDataPath + "/characters";
    public static string weaponDir { get; protected set; } = Application.persistentDataPath + "/weapons";
    public static string artifactsDir { get; protected set; } = Application.persistentDataPath + "/artifacts";
    public static string enemyDir { get; protected set; } = Application.persistentDataPath + "/enemies";
    public static string summonDir { get; protected set; } = Application.persistentDataPath + "/summons";
    public static string battleDir { get; protected set; } = Application.persistentDataPath + "/battles";
    public static string characterConfigDir { get; protected set; } = Application.persistentDataPath + "/user";

    public static string battleFilePath = Application.persistentDataPath + "/battles/boss4+3.json";

    public static string mystery = "tingyun";

    public static string[] teamMembers = new string[] { "himeko", "clara", "jingyuan", "bronya" };

}
