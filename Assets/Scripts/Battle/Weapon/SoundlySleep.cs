using LitJson;

public class SoundlySleep : AWeaponTalent
{
    public SoundlySleep(JsonData data, int r): base(data, r)
    {

    }

    float crtdmg, crtrate;
    int timer = 0;
    public override void OnEquiping(Character character)
    {
        crtdmg = (float)(double)config["effect"]["crtdmg"]["value"][refine];
        crtrate = (float)(double)config["effect"]["crtdmg"]["value"][refine];
        // write code to add crtdmg buff to character
        character.AddBuff("soundlySleep", BuffType.Permanent, CommonAttribute.CriticalDamage, ValueType.InstantNumber, crtdmg);
        character.afterDealingDamage.Add(new TriggerEvent<Creature.DamageEvent>("soundlySleepCrtrate", (t, d) =>
        {
            if (timer<=0 && (d.type == DamageType.Attack || d.type == DamageType.Skill) && !d.isCritical)
            {
                character.AddBuff("soundlySleepCrtrate", BuffType.Buff, CommonAttribute.CriticalRate, ValueType.InstantNumber, crtrate, 1);
                timer = 3;
            }
            return d;
        }));
        character.onTurnEnd.Add(new TriggerEvent<Creature.TurnEndEvent>("soundlySleepTimerTrigger", () =>
        {
            timer -= 1;
        }));
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("soundlySleep");
    }
}
