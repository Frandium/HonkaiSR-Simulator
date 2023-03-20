using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class TestData : MonoBehaviour
{
    public UnityEngine.UI.Text text;

    public void ListPersistentData()
    {
        StartCoroutine(DeepFirstFileList());
    }

    IEnumerator DeepFirstFileList()
    {
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
        result = "";
        foreach (string filePath in filePaths)
        {
            result += Application.streamingAssetsPath + filePath + "\t";
            string targetPath = Application.persistentDataPath + filePath;
            result += targetPath + "\t";
        }
        text.text = result;
    }
}
