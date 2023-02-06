using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class EditCharacterUI : MonoBehaviour
{
    public Dropdown chaList;

    CreatureTemplate temp;

    string jsonPath { get {
            return GlobalInfoHolder.Instance.characterDir + "/" + temp.dbname + ".json";
        } }
    List<string> files;
    // Start is called before the first frame update
    void Start()
    {
        temp = new CreatureTemplate();
        ListAllJson();
        chaList.onValueChanged.AddListener(OptionChange);
    }

    public void ListAllJson()
    {
        files = new List<string>(Directory.GetFiles(GlobalInfoHolder.Instance.characterDir));
        files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
        if (files.Count == 0)
            return;
        chaList.ClearOptions();
        List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
        foreach(string s in files)
        {
            data.Add(new Dropdown.OptionData(Path.GetFileNameWithoutExtension(s)));
        }
        chaList.AddOptions(data);
        chaList.SetValueWithoutNotify(0);
        OptionChange(0);
    }

    public void OptionChange(int v)
    {
        if(v >= files.Count)
        {
            Debug.LogError("file list error");
            return;
        }
        string path = files[v];
        LoadJson(path);
    }

    public void LoadJson(string filePath)
    {
        string jsonString = File.ReadAllText(filePath);
        JsonData data = JsonMapper.ToObject(jsonString);

        // set character template
        temp.dbname = (string)data["dbname"];
        temp.disname = (string)data["disname"];
        temp.atk = (double)data["atk"];
        temp.def = (double)data["def"];
        temp.speed = (double)data["speed"];
        temp.maxHp = (double)data["maxHp"];
        temp.maxEnergy = (double)data["maxEnergy"];
        temp.element = (Element)(int)data["Element"];
        
        for(int i = 0; i < (int)Element.Count; ++i)
        {
            temp.elementalBonus[i] = (double)data["elementalBonus"][i];
        }

        for (int i = 0; i < (int)Element.Count; ++i)
        {
            temp.elementalResist[i] = (double)data["elementalResist"][i];
        }

        temp.isAttackTargetEnemy = (bool)data["isAttackTargetEnemy"];
        temp.attackSelectionType = (SelectionType)(int)data["attackSelectionType"];
        temp.isSkillTargetEnemy = (bool)data["isSkillTargetEnemy"];
        temp.skillSelectionType = (SelectionType)(int)data["skillSelectionType"];
        temp.isBurstTargetEnemy = (bool)data["isBurstTargetEnemy"];
        temp.burstSelectionType = (SelectionType)(int)data["burstSelectionType"];
        temp.attackGainPointCount = (int)data["attackGainPointCount"];
        temp.skillConsumePointCount = (int)data["skillConsumePointCount"];

        // refresh UI
    }

    public void DumpJson()
    {
        string s = JsonMapper.ToJson(temp);
        Debug.Log(s);
        FileStream fs;
        fs = File.Open(jsonPath, FileMode.OpenOrCreate);
        string content = JsonMapper.ToJson(temp);
        fs.Write(Encoding.UTF8.GetBytes(content));
        fs.Close();
        ListAllJson();
    }
}
