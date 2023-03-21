using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using LitJson;
using UnityEngine.SceneManagement;

public class LoadBattleUI : MonoBehaviour
{
    public Dropdown battleList;
    public Dropdown[] teamCharacter;
    public Dropdown mysteryList;
    public Text battleDetail;
    public Text warn;
    public Button loadBattle;

    List<string> files;
    Dictionary<string, string> chaDbname2Disname;
    List<string> chaDisname;
    List<string> chaDbname;

    // Start is called before the first frame update
    void Awake()
    {
        battleList.onValueChanged.AddListener(OnBattleSelected);
        mysteryList.onValueChanged.AddListener(OnMysterySelected);
        chaDbname2Disname = new Dictionary<string, string>();
        chaDisname = new List<string>();
        chaDbname = new List<string>();
        foreach (JsonData d in Database.GetAllCharacters())
        {
            chaDisname.Add((string)d["disname"]);
            chaDbname.Add((string)d["dbname"]);
            chaDbname2Disname.Add((string)d["disname"], (string)d["dbname"]);
        }
        chaDbname2Disname.Add("none", "（空）");
        chaDisname.Add("（空）");
        chaDbname.Add("none");
        for(int i = 0; i < 4; ++i)
        {
            int idx = i;
            teamCharacter[i].ClearOptions();
            teamCharacter[i].AddOptions(chaDisname);
            teamCharacter[i].SetValueWithoutNotify(chaDisname.Count - 1);
            GlobalInfoHolder.teamMembers[i] = "none";
            teamCharacter[i].onValueChanged.AddListener(ci =>
            {
                string newName = chaDbname[ci];
                GlobalInfoHolder.teamMembers[idx] = newName;
                List<string> mysteryOptions = new();
                foreach(string name in GlobalInfoHolder.teamMembers)
                {
                    if (name == "none")
                        continue;
                    JsonData d = Database.GetCharacterByDbname(name);
                    mysteryOptions.Add("(" + d["disname"] + ")" + (string)d["mystery"]["name"] + "：" + (string)d["mystery"]["description"]);
                }
                mysteryOptions.Add("空");
                mysteryList.ClearOptions();
                mysteryList.AddOptions(mysteryOptions);
                OnMysterySelected(0);

                List<string> chas = new List<string>(GlobalInfoHolder.teamMembers);
                chas.RemoveAll(s => s == "none");
                if(chas.Count == 0)
                {
                    warn.gameObject.SetActive(true);
                    warn.text = "*队伍中至少要有一名角色";
                    loadBattle.interactable = false;
                }
                else
                {
                    bool dup = false;
                    for (int j = 0; j < chas.Count - 1; ++j)
                    {
                        for (int k = j + 1; k < chas.Count; ++k)
                        {
                            dup = dup || chas[j] == chas[k];
                        }
                    }
                    warn.gameObject.SetActive(dup);
                    warn.text = "*队伍中不能有重复的角色";
                    loadBattle.interactable = !dup;
                }
            });
        }
        warn.gameObject.SetActive(true);
        warn.text = "*队伍中至少要有一名角色";
        loadBattle.interactable = false;
        mysteryList.ClearOptions();
    }

    private void Start()
    {
        ScanBattles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScanBattles()
    {
        battleList.ClearOptions();
        files = new List<string>(Directory.GetFiles(GlobalInfoHolder.battleDir));
        files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
        if (files.Count == 0)
            return;

        List<string> options = new List<string>();
        for (int i = 0; i < files.Count; ++i)
        {
            string s = files[i];
            string dbname = Path.GetFileNameWithoutExtension(s);
            options.Add(dbname);
        }
        battleList.AddOptions(options);
        OnBattleSelected(0);
    }

    public void OnBattleSelected(int o)
    {
        string file = files[o];
        GlobalInfoHolder.battleFilePath = file;
        string jsonString = File.ReadAllText(file);
        JsonData data = JsonMapper.ToObject(jsonString);

        // load json
        string s = "敌人列表："; 
        for (int i = 0; i < data["enemies"].Count; ++i)
        {
            s += "\n第" + (i + 1) + "波： ";
            for (int j = 0; j < data["enemies"][i].Count; ++j)
            {
                JsonData d = JsonMapper.ToObject(File.ReadAllText(GlobalInfoHolder.enemyDir + "/" + (string)data["enemies"][i][j] + ".json"));
                string disname = (string)d["disname"];
                s += disname + "， ";
            }
        }
        battleDetail.text = s;
    }

    public void OnMysterySelected(int o)
    {
        if (o >= GlobalInfoHolder.teamMembers.Length)
            GlobalInfoHolder.mystery = "none";
        else
            GlobalInfoHolder.mystery = GlobalInfoHolder.teamMembers[o];
    }

    public GameObject cover;
    public Image loading;
}
