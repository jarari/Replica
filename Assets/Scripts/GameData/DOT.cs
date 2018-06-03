using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOT {
    private float timeLeft;
    private float damage;
    private float tickDelay;
    private float nextDamage;
    private Character attacker;
    private Character victim;

    public DOT(float duration, float tickDamage, float tickdelay, Character _attacker, Character _victim) {
        timeLeft = duration;
        damage = tickDamage;
        tickDelay = tickdelay;
        attacker = _attacker;
        victim = _victim;
        nextDamage = Time.time;
    }

    public DOT(float duration, float tickdamage, float tickdelay) {
        timeLeft = duration;
        damage = tickdamage;
        tickDelay = tickdelay;
    }

    public void SetDuration(float d) {
        timeLeft = d;
    }

    public void SetTickDamage(float d) {
        damage = d;
    }

    public void SetTickDelay(float td) {
        tickDelay = td;
    }

    public void DoDamage() {
        timeLeft -= tickDelay;
        nextDamage = Time.time + tickDelay;
        if (victim == null) {
            timeLeft = 0;
            return;
        }
        victim.DoDamage(attacker, damage, 0);
    }

    public bool ShouldDoDamage() {
        return nextDamage <= Time.time && !MenuManager.instance.IsPaused();
    }

    public float GetDuration() {
        return timeLeft;
    }

    public float GetTickDamage() {
        return damage;
    }

    public float GetTickDelay() {
        return tickDelay;
    }
}