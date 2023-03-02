using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class Utils
{
    public static ObjectPool<Buff> valueBuffPool = new ObjectPool<Buff>(100);

    public static double Lerp(double min, double max, float scale, float length){
        return min + (max - min) * scale / length;
    }

    public static double Lerp(double min, double max, int scale, int length) 
    {
        return min + (max - min) * (float)scale / (float)length;
    }
    public static float Lerp(float min, float max, float scale, float length)
    {
        return min + (max - min) * scale / length;
    }
    public static float Lerp(float min, float max, int scale, int length)
    {
        return min + (max - min) * (float)scale / (float)length;
    }

    public static string[] attributeNames = new string[(int)CommonAttribute.Count + 1]
    {
        "������",
        "������",
        "��������",
        "�ٶ�",
        "������",
        "�����˺�",
        "���Ƽӳ�",
        "�����Ƽӳ�",
        "����Ч��",
        "����Ȩ��",
        "Ч������",
        "Ч���ֿ�",
        "ȫ�����˺��ӳ�",
        "�����˺��ӳ�",
        "�������˺��ӳ�",
        "�������˺��ӳ�",
        "�������˺��ӳ�",
        "�������˺��ӳ�",
        "�����˺��ӳ�",
        "�����˺��ӳ�",
        "ȫ���Կ���",
        "�����˺�����",
        "�������˺�����",
        "�������˺�����",
        "�������˺�����",
        "�������˺�����",
        "�����˺�����",
        "�����˺�����",
        "����͸",
        "�����Դ�͸",
        "�����Դ�͸",
        "�����Դ�͸",
        "�����Դ�͸",
        "�������Դ�͸",
        "�������Դ�͸",
        "ռλ��"
    };

    public static string[] ElementName = new string[(int)Element.Count + 1] {
    "����",
    "��",
    "��",
    "��",
    "��",
    "����",
    "����",
    "������"
    };

    public static string[] CareerName = new string[(int)Element.Count + 1]
    {
    "����",
    "Ѳ��",
    "��ʶ",
    "ͬг",
    "����",
    "�滤",
    "����",
    "������"
    };

    public static string[] valueTypeName = new string[(int)ValueType.Count + 1]
    {
        "�ٷֱ�",
        "ֱ����",
        "ռλ"
    };

    public static bool TwoRandom(float rate)
    {
        int p = (int)(rate * 10000);
        int r = Random.Range(0, 10000);
        return r < p;
    }

    public static string FormatDescription(string format, JsonData d, int l)
    {
        string[] strs = format.Split("#"); // ��1�� 3�� 5 ���� ��Ҫ�滻���ַ���
        string res = "";
        for (int i = 0; i < strs.Length; i++)
        {
            if (i % 2 == 0)
                res += strs[i];
            else
            {
                if((string)d[strs[i]]["type"] == "instant")
                {
                    if (d[strs[i]]["value"][0].IsDouble)
                        res += "<color=#f08>" + ((float)(double)d[strs[i]]["value"][l] * 100).ToString() + "</color>";
                    else if (d[strs[i]]["value"][0].IsInt)
                        res += "<color=#f08>" + ((int)d[strs[i]]["value"][l]).ToString() + "</color>";
                }
                else if ((string)d[strs[i]]["type"] == "percentage")
                    if (d[strs[i]]["value"][0].IsDouble)
                        res += "<color=#f08>" + ((float)(double)d[strs[i]]["value"][l] * 100).ToString() + "%</color>";
            }
        }
        return res;
    }
}
