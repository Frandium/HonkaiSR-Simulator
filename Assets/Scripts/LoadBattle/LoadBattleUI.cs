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
    public Dropdown mysteryList;
    public Text battleDetail;

    List<string> files;
    List<string> chaNames;

    // Start is called before the first frame update
    void Start()
    {
        ScanBattles();
        battleList.onValueChanged.AddListener(onBattleSelected);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScanBattles()
    {
        battleList.ClearOptions();
        files = new List<string>(Directory.GetFiles(GlobalInfoHolder.Instance.battleDir));
        files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
        if (files.Count == 0)
            return;
        // 对所有的角色，在上方的滚动条里 add 一个选项，然后找 Resources 里有没有他的照片，有就add，没有就 fall back 到默认图片
        List<string> options = new List<string>();
        for (int i = 0; i < files.Count; ++i)
        {
            string s = files[i];
            string dbname = Path.GetFileNameWithoutExtension(s);
            options.Add(dbname);
        }
        battleList.AddOptions(options);
        onBattleSelected(0);
    }

    public void onBattleSelected(int o)
    {
        string file = files[o];
        GlobalInfoHolder.Instance.battleFilePath = file;
        string jsonString = File.ReadAllText(file);
        JsonData data = JsonMapper.ToObject(jsonString);

        // load json
        string s = "敌人列表："; 
        for (int i = 0; i < data["enemies"].Count; ++i)
        {
            s += "\n第" + (i + 1) + "波： ";
            for (int j = 0; j < data["enemies"][i].Count; ++j)
            {
                JsonData d = JsonMapper.ToObject(File.ReadAllText(GlobalInfoHolder.Instance.enemyDir + "/" + (string)data["enemies"][i][j] + ".json"));
                string disname = (string)d["disname"];
                s += disname + "， ";
            }
        }
        s += "\n角色列表： ";
        chaNames = new List<string>();
        List<string> disNames = new List<string>();
        for (int i = 0; i < data["characters"].Count; ++i)
        {
            string name = (string)data["characters"][i];
            chaNames.Add(name);

            JsonData d = JsonMapper.ToObject(File.ReadAllText(GlobalInfoHolder.Instance.characterDir + "/" + name + ".json"));
            string disname = (string)d["disname"];
            s += disname + "；";
            disNames.Add("(" + disname + ")" + (string)d["mystery"]["name"] + "：" + (string)d["mystery"]["description"]);
        }
        battleDetail.text = s;
        mysteryList.ClearOptions();
        mysteryList.AddOptions(disNames);
        OnMysterySelected(0);
        // enemies are instantiated in NextTurn
    }

    public void OnMysterySelected(int o)
    {
        GlobalInfoHolder.Instance.mystery = chaNames[o];
    }

    public void LoadBattleScene()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync("Battle");
        StartCoroutine(LoadSceneAnim(ao));
    }

    public GameObject cover;
    public Image loading;

    IEnumerator LoadSceneAnim(AsyncOperation ao)
    {
        cover.SetActive(true);
        loading.fillAmount = 0;
        while (!ao.isDone)
        {
            loading.fillAmount = ao.progress;
            yield return new WaitForEndOfFrame();
        }
    }
}
