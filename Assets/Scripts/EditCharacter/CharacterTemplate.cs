using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class CreatureTemplate
{
    public string dbname = "test";
    public string disname = "²âÊÔ½ÇÉ«";

    public double speed = 50;

    public double maxHp  = 100;
    public double atk  = 10;
    public double def  = 5;
    public double maxEnergy = 60;
    public Element element = Element.Anemo;

    public double[] elementalBonus = { 0, 0, 0, 0, 0, 0, 0, 0 };

    public double[] elementalResist  = { .1f, .1f, .1f, .1f, .1f, .1f, .1f, .1f };

    public bool isAttackTargetEnemy  = true;
    public SelectionType attackSelectionType  = SelectionType.One;
    public bool isSkillTargetEnemy  = true;
    public SelectionType skillSelectionType  = SelectionType.One;
    public bool isBurstTargetEnemy  = true;
    public SelectionType burstSelectionType  = SelectionType.All;
    public int attackGainPointCount  = 1;
    public int skillConsumePointCount  = 1;


}
