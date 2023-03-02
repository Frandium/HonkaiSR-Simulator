using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public class ArtifactDescription
{
    public string dbname { get; protected set; }
    public string disname { get; protected set; }
    public string two { get; protected set; }
    public string four { get; protected set; }

    public ArtifactDescription(string _dbname)
    {
        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/artifacts/" + _dbname + ".json");
        JsonData data = JsonMapper.ToObject(jsonString);

        dbname = _dbname;
        disname = (string)data["disname"];
        two = (string)data["two"];
        four = (string)data["four"];
    }

    public static Dictionary<string, ArtifactDescription> artifacts;

    public static Dictionary<string, ArtifactDescription> GetAllArtifactSuits()
    {
        if (artifacts != null)
            return artifacts;

        artifacts = new Dictionary<string, ArtifactDescription>();
        List<string> files = new List<string>(Directory.GetFiles(GlobalInfoHolder.Instance.artifactsDir));
        files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
        foreach (string s in files)
        {
            string dbname = Path.GetFileNameWithoutExtension(s);
            ArtifactDescription ad = new ArtifactDescription(dbname);
            artifacts.Add(dbname, ad);
        }
        return artifacts;
    }
}
