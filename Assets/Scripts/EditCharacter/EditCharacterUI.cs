using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EditCharacterUI : MonoBehaviour
{
    Character character;

    public GameObject headButtonGO;
    public GameObject scrollContent;
    public CharacterDetailUI detail;

    public Sprite defaultAvatar;
    List<string> files;

    // Start is called before the first frame update
    void Start()
    {
        detail.SetChangeable(true);
        ScanCharacters();
    }

    public void ScanCharacters()
    {
        files = new List<string>(Directory.GetFiles(GlobalInfoHolder.characterDir));
        files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
        if (files.Count == 0)
            return;
        float w = scrollContent.GetComponent<RectTransform>().rect.height;
        scrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(w * files.Count * 1.05f, 0);
        // 对所有的角色，在上方的滚动条里 add 一个选项，然后找 Resources 里有没有他的照片，有就add，没有就 fall back 到默认图片
        for(int i = 0; i < files.Count; ++i) 
        {
            string s = files[i];
            string dbname = Path.GetFileNameWithoutExtension(s);
            string avatarPath = dbname + "/runway_avatar";
            Sprite avatar = Resources.Load<Sprite>(avatarPath);
            if (avatar == null)
                avatar = defaultAvatar;
            GameObject go = Instantiate(headButtonGO, scrollContent.transform);
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(w * i * 1.05f, 0);
            go.GetComponent<Image>().sprite = avatar;
            go.name = dbname;
            go.GetComponent<Button>().onClick.AddListener(() => OnHeadClick(dbname));
        }
        OnHeadClick(Path.GetFileNameWithoutExtension(files[0]));
     }

    public void OnHeadClick(string name)
    {
        character?.SaveConfig();
        character = new Character(name);
        detail.ShowDetail(character);
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

        // refresh UI
    }

    public void DumpJson()
    {
        BattleTemplate bt = new BattleTemplate();
        FileStream fs;
        fs = File.Open(GlobalInfoHolder.battleDir + "/" + "default_battle.json", FileMode.OpenOrCreate);
        string content = JsonMapper.ToJson(bt);
        fs.Write(Encoding.UTF8.GetBytes(content));
        fs.Close();
        ScanCharacters();
    }

    public void DumpCharacter()
    {
        CharacterConfig c = new CharacterConfig();
        FileStream fs;
        fs = File.Open(GlobalInfoHolder.characterConfigDir + "/" + "babara.json", FileMode.OpenOrCreate);
        string content = JsonMapper.ToJson(c);
        fs.Write(Encoding.UTF8.GetBytes(content));
        fs.Close();
    }
}
