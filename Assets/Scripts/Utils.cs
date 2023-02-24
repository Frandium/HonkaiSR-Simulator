using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static string[] attributeNames = new string[(int)CommonAttribute.Count]
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
        "�����˺�����"
    };

    public static string[] ElementName = new string[] {
    "����",
    "��",
    "��",
    "��",
    "��",
    "����",
    "����",
    "������"
    };

    public static string[] CareerName = new string[]
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

    public static bool TwoRandom(float rate)
    {
        int p = (int)(rate * 10000);
        int r = Random.Range(0, 10000);
        return r < p;
    }
}
