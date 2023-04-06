using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using UnityEngine.UI;
using System.Text;

public class Database: MonoBehaviour
{
    static Dictionary<string, JsonData> characters;
    public static List<JsonData> GetAllCharacters()
    {
        if(characters == null)
        {
            characters = new Dictionary<string, JsonData>();
            List<string> files = new List<string>(Directory.GetFiles(GlobalInfoHolder.characterDir));
            files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
            foreach(string s in files)
            {
                string dbname = Path.GetFileNameWithoutExtension(s);
                string jsonString = File.ReadAllText(GlobalInfoHolder.characterDir + "/" + dbname + ".json");
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


    public Text log;
    public Image progress;
    static bool sourceCompleteChecked = false;
    private void Start()
    {
        if (sourceCompleteChecked)
            gameObject.SetActive(false);
        else
        {
            StartCoroutine(CopyStreamingAssets());
        }
    }


    IEnumerator CopyStreamingAssets()
    {
#if !UNITY_EDITOR
        bool copied = PlayerPrefs.GetInt(Application.productName + Application.version + "DataCopy", 0) > 0;
        string afPath = Application.streamingAssetsPath + "/allfiles.txt";
        string result;
        if (afPath.Contains("://") || afPath.Contains(":///"))
        {
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(afPath);
            yield return www.SendWebRequest();
            result = www.downloadHandler.text;
        }
        else
            result = File.ReadAllText(afPath);

        string[] filePaths = result.Split("\n");
        int i = 0;
        foreach(string filePath in filePaths)
        {
            string targetPath = Application.persistentDataPath + filePath;
            log.text = "checking: " + targetPath;
            if (filePath.EndsWith(".json"))
            {
                // 如果是 user，不覆盖；
                // 非 user 的目录，或者我要求更新其他覆盖。
                if (filePath.StartsWith("/user") && copied) // 用户 data 不覆盖
                {
                    // 但是现在是 0.5 版本，人物的 data 结构有变化，所以请覆盖用户所有的旧 data。
                    // 不复制
                }
                else if(!copied || !File.Exists(targetPath))
                {
                    string safilePath = Application.streamingAssetsPath + filePath;
                    UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(safilePath);
                    yield return www.SendWebRequest();
                    string content = www.downloadHandler.text;
                    log.text = "copy file: " + targetPath;
                    FileStream fs;
                    fs = File.Open(targetPath, FileMode.Create);
                    fs.Write(Encoding.UTF8.GetBytes(content));
                    fs.Close();
                }
            }
            else
            {
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                    log.text = "create directory: " + targetPath;
                }
            }
            progress.fillAmount = ++i / (float)filePaths.Length;
            }
#else
        yield return null;
#endif
        sourceCompleteChecked = true;
        gameObject.SetActive(false);
        PlayerPrefs.SetInt(Application.productName + Application.version + "DataCopy", 1);
    }

}
