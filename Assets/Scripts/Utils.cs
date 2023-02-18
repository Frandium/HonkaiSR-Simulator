using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static ObjectPool<ValueBuff> valueBuffPool = new ObjectPool<ValueBuff>(30);
    public static ObjectPool<TriggerBuff> triggerBuffPool = new ObjectPool<TriggerBuff>(10);
    public static double Lerp(double min, double max, float scale, float value){
        return min + (max - min) * scale / value;
    }

    public static double Lerp(double min, double max, int scale, int value) 
    {
        return min + (max - min) * (float)scale / (float)value;
    }
    public static float Lerp(float min, float max, float scale, float value)
    {
        return min + (max - min) * scale / value;
    }
    public static float Lerp(float min, float max, int scale, int value)
    {
        return min + (max - min) * (float)scale / (float)value;
    }

}
