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

    CharacterBase character = new CharacterBase();

    public GameObject headButtonGO;
    public GameObject scrollContent;

    public Sprite defaultAvatar;

    public Text chaName;
    public Text hpText;
    public Text atkText;
    public Text DefText;
    public Text crtRateText;
    public Text crtDmgText;

    List<string> files;

    // Start is called before the first frame update
    void Start()
    {
       // temp = new CharacterTemplate();
        ScanCharacters();
//        chaList.onValueChanged.AddListener(OptionChange);
    }

    public void ScanCharacters()
    {
        files = new List<string>(Directory.GetFiles(GlobalInfoHolder.Instance.characterDir));
        files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
        if (files.Count == 0)
            return;
        scrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(70 * files.Count, 0);
        // �����еĽ�ɫ�����Ϸ��Ĺ������� add һ��ѡ�Ȼ���� Resources ����û��������Ƭ���о�add��û�о� fall back ��Ĭ��ͼƬ
        for(int i = 0; i < files.Count; ++i)
        {
            string s = files[i];
            string dbname = Path.GetFileNameWithoutExtension(s);
            string avatarPath = dbname + "/runway_avatar";
            Sprite avatar = Resources.Load<Sprite>(avatarPath);
            if (avatar == null)
                avatar = defaultAvatar;
            GameObject go = Instantiate(headButtonGO, scrollContent.transform);
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(35 + 70 * i, 0);
            go.GetComponent<Image>().sprite = avatar;
            go.name = dbname;
            go.GetComponent<Button>().onClick.AddListener(() => OnHeadClick(dbname));
        }
        OnHeadClick(Path.GetFileNameWithoutExtension(files[0]));
     }

    public void OnHeadClick(string dbname)
    {
        // Load json ��ý�ɫ��������
        // Load json ��ý�ɫ��װ��������׶�������ȼ����츳�ȼ���ʥ����
        // Load json ��ù�׶���ԡ�ʥ�������ԡ��츳����
        // ˢ�µ�ǰҳ��
        character.LoadJson(dbname);
        chaName.text = character.disname;
        hpText.text = character.GetFinalAttr(CommonAttribute.MaxHP).ToString();
        atkText.text = character.GetFinalAttr(CommonAttribute.ATK).ToString();
        DefText.text = character.GetFinalAttr(CommonAttribute.DEF).ToString();
        crtRateText.text = character.GetFinalAttr(CommonAttribute.CriticalRate).ToString();
        crtDmgText.text = character.GetFinalAttr(CommonAttribute.CriticalDamage).ToString();


        Debug.Log(dbname);
    }

    public void FastTest()
    {
        Debug.Log("123");
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
        fs = File.Open(GlobalInfoHolder.Instance.battleDir + "/" + "default_battle.json", FileMode.OpenOrCreate);
        string content = JsonMapper.ToJson(bt);
        fs.Write(Encoding.UTF8.GetBytes(content));
        fs.Close();
        ScanCharacters();
    }
}
