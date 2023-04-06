using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class MyMenuCommand
{
    [MenuItem("Android/Generate StreamingAssets List")]
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

    static string DeepFirstFileList(DirectoryInfo dir, string prefix)
    {
        StringBuilder builder = new StringBuilder();
        string thisDir = prefix + "/" + dir.Name;
        builder.Append(thisDir);
        foreach (FileInfo file in dir.GetFiles())
        {
            if (file.Name.EndsWith(".json"))
            {
                builder.Append("\n");
                builder.Append(thisDir);
                builder.Append("/");
                builder.Append(file.Name);
            }
        }
        foreach (DirectoryInfo d in dir.GetDirectories())
        {
            builder.Append("\n");
            builder.Append(DeepFirstFileList(d, thisDir));
        }

        return builder.ToString();
    }

    [MenuItem("Android/Clear PersistentDataPath")]
    public static void ClearPersistentData()
    {
        foreach (DirectoryInfo d in new DirectoryInfo(Application.persistentDataPath).GetDirectories())
        {
            Directory.Delete(d.FullName, true);
        }
    }

    [MenuItem("Android/Clear and Copy Data", priority = 0)]
    public static void LSD()
    {
        ClearPersistentData();
        ListStreamingAssets();
        string afPath = Application.streamingAssetsPath + "/allfiles.txt";
        string result = File.ReadAllText(afPath);

        string[] filePaths = result.Split("\n");
        foreach (string filePath in filePaths)
        {
            string targetPath = Application.persistentDataPath + filePath;
            if (filePath.EndsWith(".json"))
            {
                string safilePath = Application.streamingAssetsPath + filePath;
                string content = File.ReadAllText(safilePath);
                FileStream fs;
                fs = File.Open(targetPath, FileMode.Create);
                fs.Write(Encoding.UTF8.GetBytes(content));
                fs.Close();
            }
            else if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
        }
    }
}