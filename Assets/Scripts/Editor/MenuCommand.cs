using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using System.Text;

public class MenuCommand
{
    [MenuItem("Assets/StreamingAssests/Generate Directory List")]
    public static void ListStreamingAssets()
    {
        DirectoryInfo sa = new DirectoryInfo(Application.streamingAssetsPath);
        StringBuilder builder = new StringBuilder();
        foreach (DirectoryInfo d in sa.GetDirectories())
        {
            builder.Append("\n");
            builder.Append(DeepFirstFileList(d, ""));
        }

        FileStream fs;
        fs = File.Open(Application.streamingAssetsPath + "/allfiles.txt", FileMode.Create);
        fs.Write(Encoding.UTF8.GetBytes(builder.ToString()));
        fs.Close();
    }

    static string DeepFirstFileList(DirectoryInfo dir,string prefix)
    {
        StringBuilder builder = new StringBuilder();
        string thisDir = prefix + "/" + dir.Name;
        builder.Append(thisDir);
        foreach(FileInfo file in dir.GetFiles())
        {
            if (file.Name.EndsWith(".json"))
            {
                builder.Append("\n");
                builder.Append(thisDir);
                builder.Append("/");
                builder.Append(file.Name);
            }
        }
        foreach(DirectoryInfo d in dir.GetDirectories())
        {
            builder.Append("\n");
            builder.Append(DeepFirstFileList(d, thisDir));
        }

        return builder.ToString();
    }

    [MenuItem("File/LogPersistentDataPath")]
    public static void LPD()
    {
        Debug.Log(Application.persistentDataPath);
    }
}
