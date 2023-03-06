using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public class Database
{
    static Dictionary<string, JsonData> characters;
    public static List<JsonData> GetAllCharacters()
    {
        if(characters == null)
        {
            characters = new Dictionary<string, JsonData>();
            List<string> files = new List<string>(Directory.GetFiles(GlobalInfoHolder.Instance.characterDir));
            files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
            // �����еĽ�ɫ�����Ϸ��Ĺ������� add һ��ѡ�Ȼ���� Resources ����û��������Ƭ���о�add��û�о� fall back ��Ĭ��ͼƬ
            foreach(string s in files)
            {
                string dbname = Path.GetFileNameWithoutExtension(s);
                string jsonString = File.ReadAllText(GlobalInfoHolder.Instance.characterDir + "/" + dbname + ".json");
                JsonData metaData = JsonMapper.ToObject(jsonString);
                characters.Add(dbname, metaData);
            }
        }
        return new List<JsonData>(characters.Values);
    }

    public static JsonData GetCharacterByDbname(string dbname)
    {
        return characters[dbname];
    }

}
