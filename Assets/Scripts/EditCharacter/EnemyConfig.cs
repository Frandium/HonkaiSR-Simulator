using LitJson;
using System.Collections.Generic;
using System.IO;

public class EnemyConfig
{
    public string dbname { get; set; }
    public int level;

    public EnemyConfig(string dbname, int level)
    {
        this.dbname = dbname;
        this.level = level;
    }

    public EnemyConfig(JsonData data)
    {
        dbname = (string)data["name"];
        level = (int)data["level"];
    }
}

public class BattleConfig
{
    public string dbname { get; protected set; }
    public string disname { get; protected set; }
    public List<List<EnemyConfig>> enemies { get; protected set; }

    public BattleConfig(string file)
    {
        string jsonString = File.ReadAllText(file);
        JsonData data = JsonMapper.ToObject(jsonString);
        
        dbname = (string)data["dbname"];
        disname = (string)data["disname"];
        enemies = new List<List<EnemyConfig>>();

        for (int i = 0; i < data["enemies"].Count; ++i)
        {
            enemies.Add(new List<EnemyConfig>());
            for (int j = 0; j < data["enemies"][i].Count; ++j)
            {
                enemies[i].Add(new EnemyConfig(data["enemies"][i][j]));
            }
        }
    }
}