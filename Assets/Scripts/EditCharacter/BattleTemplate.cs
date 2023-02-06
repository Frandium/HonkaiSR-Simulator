using System.Collections.Generic;

public class BattleTemplate
{
    public List<string> characters;
    public List<List<string>> enemies;

    public BattleTemplate()
    {
        characters = new List<string>(new string[]{ "kazuha", "ganyu", "shenhe", "kokomi" });
        enemies = new List<List<string>>();
        enemies.Add(new List<string>(new string[] { "hilichurl", "hilichurl" , "hilichurl" }));
    }
}
