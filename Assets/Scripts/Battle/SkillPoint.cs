using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPoint : MonoBehaviour
{
    int maxPoint = 5;
    int pointCount = 0;

    public Image[] pointImage;
    public Text pointCountText;

    public void GainPoint(int c)
    {
        StopAllCoroutines();
        pointCount = Mathf.Min(maxPoint, pointCount + c);
        pointCountText.text = pointCount.ToString();
        RefreshPointColor();
    }

    public void ConsumePoint(int c)
    {
        StopAllCoroutines();
        if (!IsPointEnough(c))
        {
            Debug.LogError("Point Not Enough in Consume Point!");
            return;
        }
        pointCount = Mathf.Max(0, pointCount - c);
        pointCountText.text = pointCount.ToString();
        RefreshPointColor();
    }

    public bool IsPointEnough(int i)
    {
        return pointCount >= i;
    }

    public void StartGainPointAnim(int c)
    {
        StopAllCoroutines();
        RefreshPointColor();
        if (pointCount >= maxPoint) return;
        for (int i = pointCount; i < Mathf.Min(pointCount + c, maxPoint); ++i)
        {
            StartCoroutine(PointAnim(pointImage[i], Color.green));
        }
    }

    public void StartConsumePointAnim(int c)
    {
        StopAllCoroutines();
        RefreshPointColor();
        if (!IsPointEnough(c))
        {
            Debug.LogError("Point Not Enough in Consume Point!");
            return;
        }
        for (int i = pointCount - 1; i >= Mathf.Max(0, pointCount - c); --i)
        {
            StartCoroutine(PointAnim(pointImage[i], Color.red));
        }
    }

    public void RefreshPointColor()
    {
        for(int i = 0; i < pointCount; ++i)
        {
            pointImage[i].color = Color.white;
        }
        for(int i = pointCount; i < maxPoint; ++i)
        {
            pointImage[i].color = new Color(.75f, .75f, .75f, .75f);
        }
    }

    public IEnumerator PointAnim(Image img, Color targetColor)
    {
        float alpha = 1;
        float dir = -1;
        while (true)
        {
            alpha += Time.deltaTime * dir;
            if (alpha <= .25f) dir = 1;
            else if (alpha >= 1) dir = -1;
            targetColor.a = alpha;
            img.color = targetColor;
            yield return new WaitForEndOfFrame();
        }
    }
}
