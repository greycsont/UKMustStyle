using UnityEngine;
using System.Reflection;
using HarmonyLib;
using ULTRAKILL.Cheats;


namespace Only;


[HarmonyPatch(typeof(Machine), nameof(Machine.GetHurt))]
public static class MachineGetHurtPatch
{
    public static bool Prefix(ref GameObject target,
                              ref Vector3 force,
                              ref float multiplier,
                              ref float critMultiplier,
                              ref GameObject sourceWeapon,
                              ref bool fromExplosion,
                              Machine __instance)
    {
        if (RankChecker.IsRanked())
        {
            return true;
        }

        // Access private variables
        var eidField = AccessTools.Field(typeof(Machine), "eid");
        var eid = eidField.GetValue(__instance) as EnemyIdentifier;

        var smField = AccessTools.Field(typeof(Machine), "sm");
        var sm = smField.GetValue(__instance) as SwordsMachine;

        var v2Field = AccessTools.Field(typeof(Machine), "v2");
        var v2 = v2Field.GetValue(__instance) as V2;

        var turField = AccessTools.Field(typeof(Machine), "tur");
        var tur = turField.GetValue(__instance) as Turret;

        var scField = AccessTools.Field(typeof(Machine), "sc");
        var sc = scField.GetValue(__instance) as Streetcleaner;

        var gmField = AccessTools.Field(typeof(Machine), "gm");
        var gm = gmField.GetValue(__instance) as Gutterman;

        var mfField = AccessTools.Field(typeof(Machine), "mf");
        var mf = mfField.GetValue(__instance) as Mindflayer;

        var minField = AccessTools.Field(typeof(Machine), "min");
        var min = minField.GetValue(__instance) as Minotaur;

        var sisyField = AccessTools.Field(typeof(Machine), "sisy");
        var sisy = sisyField.GetValue(__instance) as Sisyphus;

        var parryFramesLeftField = AccessTools.Field(typeof(Machine), "parryFramesLeft");
        var parryFramesLeft = parryFramesLeftField.GetValue(__instance) as int? ?? 0;

        var parryFramesOnPartialField = AccessTools.Field(typeof(Machine), "parryFramesOnPartial");
        var parryFramesOnPartial = parryFramesOnPartialField.GetValue(__instance) as bool? ?? false;

        var bsmField = AccessTools.Field(typeof(Machine), "bsm");
        var bsm = bsmField.GetValue(__instance) as BloodsplatterManager;

        var symbioticField = AccessTools.Field(typeof(Machine), "symbiotic");
        var symbiotic = symbioticField.GetValue(__instance) as bool? ?? false;

        var gzField = AccessTools.Field(typeof(Machine), "gz");
        var gz = gzField.GetValue(__instance) as GoreZone;

        var chestHPField = AccessTools.Field(typeof(Machine), "chestHP");
        var chestHP = chestHPField.GetValue(__instance) as float?;

        var nohealField = AccessTools.Field(typeof(Machine), "noheal");
        var noheal = nohealField.GetValue(__instance) as bool? ?? false;

        var audField = AccessTools.Field(typeof(Machine), "aud");
        var aud = audField.GetValue(__instance) as AudioSource;

        var scalcField = AccessTools.Field(typeof(Machine), "scalc");
        var scalc = scalcField.GetValue(__instance) as StyleCalculator;

        // Method start point
        string hitLimb = "";
        bool dead = false;
        bool flag = false;
        float num = multiplier;
        GameObject gameObject = null;
        if (eid == null)
        {
            eid = __instance.GetComponent<EnemyIdentifier>();
        }
        if (force != Vector3.zero && !__instance.limp && sm == null && (v2 == null || !v2.inIntro) && (tur == null || !tur.lodged || eid.hitter == "heavypunch" || eid.hitter == "railcannon" || eid.hitter == "cannonball" || eid.hitter == "hammer"))
        {
            if (tur && tur.lodged)
            {
                tur.CancelAim(true);
                tur.Unlodge();
            }
            __instance.KnockBack(force / 100f);
            if (eid.hitter == "heavypunch" || (__instance.gc && !__instance.gc.onGround && eid.hitter == "cannonball"))
            {
                eid.useBrakes = false;
            }
            else
            {
                eid.useBrakes = true;
            }
        }
        if (v2 != null && v2.secondEncounter && eid.hitter == "heavypunch")
        {
            v2.InstaEnrage();
        }
        if (sc != null && target.gameObject == sc.canister && !sc.canisterHit && eid.hitter == "revolver")
        {
            if (!InvincibleEnemies.Enabled && !eid.blessed)
            {
                sc.canisterHit = true;
            }
            if (!eid.dead && !InvincibleEnemies.Enabled && !eid.blessed)
            {
                MonoSingleton<StyleHUD>.Instance.AddPoints(200, "ultrakill.instakill", sourceWeapon, eid, -1, "", "");
            }
            MonoSingleton<TimeController>.Instance.ParryFlash();

            // I don't think this can be seen as a damage event;
            __instance.Invoke("CanisterExplosion", 0.1f);
            return false;
        }
        if (tur != null && tur.aiming && (eid.hitter == "revolver" || eid.hitter == "coin") && tur.interruptables.Contains(target.transform))
        {
            tur.Interrupt();
        }
        if (gm)
        {
            if (gm.hasShield && !eid.dead && (eid.hitter == "heavypunch" || eid.hitter == "hammer"))
            {
                gm.ShieldBreak(true, true);
            }
            if (gm.hasShield)
            {
                multiplier /= 1.5f;
            }
            if (gm.fallen && !gm.exploded && eid.hitter == "ground slam")
            {
                gm.Explode();
                MonoSingleton<NewMovement>.Instance.Launch(Vector3.up * 750f, 8f, false);
            }
        }
        if (mf && mf.dying && (eid.hitter == "heavypunch" || eid.hitter == "hammer"))
        {
            mf.DeadLaunch(force);
        }
        if (eid.hitter == "punch")
        {
            bool flag2 = __instance.parryables != null && __instance.parryables.Count > 0 && __instance.parryables.Contains(target.transform);
            if (__instance.parryable || (__instance.partiallyParryable && (flag2 || (parryFramesLeft > 0 && parryFramesOnPartial))))
            {
                __instance.parryable = false;
                __instance.partiallyParryable = false;
                __instance.parryables.Clear();
                if (!InvincibleEnemies.Enabled && !eid.blessed)
                {
                    //this.health -= (float)((this.parryFramesLeft > 0) ? 4 : 5);
                }
                MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, eid, "");
                if (sm != null && __instance.health > 0f)
                {
                    if (!sm.enraged)
                    {
                        sm.Knockdown(true, fromExplosion);
                    }
                    else
                    {
                        sm.Enrage();
                    }
                }
                else
                {
                    __instance.SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                parryFramesOnPartialField.SetValue(__instance, flag2);
                parryFramesLeftField.SetValue(__instance, MonoSingleton<FistControl>.Instance.currentPunch.activeFrames);
            }
        }
        else if (min && min.ramTimer > 0f && eid.hitter == "ground slam")
        {
            min.GotSlammed();
        }
        if (sisy && num > 0f)
        {
            if (eid.burners.Count > 0)
            {
                if (eid.hitter != "fire")
                {
                    if (num <= 0.5f)
                    {
                        gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
                        sisy.PlayHurtSound(1);
                    }
                    else
                    {
                        gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                        sisy.PlayHurtSound(2);
                    }
                }
                else
                {
                    sisy.PlayHurtSound(0);
                }
            }
            else if (eid.hitter != "fire")
            {
                gameObject = bsm.GetGore(GoreType.Smallest, eid, fromExplosion);
            }
        }
        float num2 = 0f;
        if (target.gameObject.CompareTag("Head"))
        {
            num2 = 1f;
        }
        else if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
        {
            num2 = 0.5f;
        }
        num = multiplier + num2 * multiplier * critMultiplier;
        if (num2 == 0f && (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone"))
        {
            if (!__instance.parryable && (target.gameObject != __instance.chest || __instance.health - num > 0f))
            {
                num = 0f;
            }
            else if ((__instance.parryable && (target.gameObject == __instance.chest || MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(false).magnitude > 18f)) || (__instance.partiallyParryable && __instance.parryables != null && __instance.parryables.Contains(target.transform)))
            {
                num *= 1.5f;
                __instance.parryable = false;
                __instance.partiallyParryable = false;
                __instance.parryables.Clear();
                MonoSingleton<NewMovement>.Instance.Parry(eid, "");
                if (sm != null && __instance.health - num > 0f)
                {
                    if (!sm.enraged)
                    {
                        sm.Knockdown(true, fromExplosion);
                    }
                    else
                    {
                        sm.Enrage();
                    }
                }
                else
                {
                    __instance.SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        if (sisy && !__instance.limp && eid.hitter == "fire" && __instance.health > 0f && __instance.health - num < 0.01f && !eid.isGasolined)
        {
            num = __instance.health - 0.01f;
        }
        if (!eid.blessed && !InvincibleEnemies.Enabled)
        {
            //this.health -= num;
        }
        if (!gameObject && eid.hitter != "fire" && num > 0f)
        {
            if ((num2 == 1f && (num >= 1f || __instance.health <= 0f)) || eid.hitter == "hammer")
            {
                gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
            }
            else if (((num >= 1f || __instance.health <= 0f) && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
            {
                if (target.gameObject.CompareTag("Body"))
                {
                    gameObject = bsm.GetGore(GoreType.Body, eid, fromExplosion);
                }
                else
                {
                    gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
                }
            }
            else if (eid.hitter != "explosion")
            {
                gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
            }
        }
        if (!__instance.limp)
        {
            flag = true;
            string text = target.gameObject.tag.ToLower();
            if (text == "endlimb")
            {
                text = "limb";
            }
            hitLimb = text;
        }
        if (__instance.health <= 0f)
        {
            if (symbiotic)
            {
                if (sm != null && !sm.downed && __instance.symbiote.health > 0f)
                {
                    sm.downed = true;
                    sm.Down(fromExplosion);
                    __instance.Invoke("StartHealing", 3f);
                }
                else if (sisy != null && !sisy.downed && __instance.symbiote.health > 0f)
                {
                    sisy.downed = true;
                    sisy.Knockdown(__instance.transform.position + __instance.transform.forward);
                    __instance.Invoke("StartHealing", 3f);
                }
                else if (__instance.symbiote.health <= 0f)
                {
                    symbioticField.SetValue(__instance, false);
                    //this.symbiotic = false;
                    if (!__instance.limp)
                    {
                        __instance.GoLimp(fromExplosion);
                    }
                }
            }
            else
            {
                if (!__instance.limp)
                {
                    __instance.GoLimp(fromExplosion);
                }
                if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && !target.gameObject.CompareTag("EndLimb"))
                {
                    float num3 = 1f;
                    if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
                    {
                        num3 = 0.5f;
                    }
                    string tag = target.gameObject.tag;
                    if (!(tag == "Head"))
                    {
                        if (tag == "Limb")
                        {
                            int num4 = 0;
                            while ((float)num4 < 4f * num3)
                            {
                                GameObject gib = bsm.GetGib(BSType.gib);
                                if (gib && gz && gz.gibZone)
                                {
                                    __instance.ReadyGib(gib, target);
                                }
                                num4++;
                            }
                            if (target.transform.childCount > 0 && __instance.dismemberment)
                            {
                                Transform child = target.transform.GetChild(0);
                                CharacterJoint[] componentsInChildren = target.GetComponentsInChildren<CharacterJoint>();
                                if (componentsInChildren.Length != 0)
                                {
                                    foreach (CharacterJoint characterJoint in componentsInChildren)
                                    {
                                        EnemyIdentifierIdentifier enemyIdentifierIdentifier;
                                        if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier))
                                        {
                                            enemyIdentifierIdentifier.SetupForHellBath();
                                        }
                                        Object.Destroy(characterJoint);
                                    }
                                }
                                CharacterJoint component = target.GetComponent<CharacterJoint>();
                                if (component != null)
                                {
                                    component.connectedBody = null;
                                    Object.Destroy(component);
                                }
                                target.transform.position = child.position;
                                target.transform.SetParent(child);
                                child.SetParent(gz.gibZone);
                                Object.Destroy(target.GetComponent<Rigidbody>());
                            }
                        }
                    }
                    else
                    {
                        int num5 = 0;
                        while ((float)num5 < 6f * num3)
                        {
                            GameObject gib = bsm.GetGib(BSType.skullChunk);
                            if (gib && gz && gz.gibZone)
                            {
                                __instance.ReadyGib(gib, target);
                            }
                            num5++;
                        }
                        int num6 = 0;
                        while ((float)num6 < 4f * num3)
                        {
                            GameObject gib = bsm.GetGib(BSType.brainChunk);
                            if (gib && gz && gz.gibZone)
                            {
                                __instance.ReadyGib(gib, target);
                            }
                            num6++;
                        }
                        int num7 = 0;
                        while ((float)num7 < 2f * num3)
                        {
                            GameObject gib = bsm.GetGib(BSType.eyeball);
                            if (gib && gz && gz.gibZone)
                            {
                                __instance.ReadyGib(gib, target);
                            }
                            gib = bsm.GetGib(BSType.jawChunk);
                            if (gib && gz && gz.gibZone)
                            {
                                __instance.ReadyGib(gib, target);
                            }
                            num7++;
                        }
                    }
                }
                if (__instance.dismemberment)
                {
                    if (!target.gameObject.CompareTag("Body"))
                    {
                        Collider obj;
                        if (target.TryGetComponent<Collider>(out obj))
                        {
                            Object.Destroy(obj);
                        }
                        target.transform.localScale = Vector3.zero;
                    }
                    else if (target.gameObject == __instance.chest && v2 == null && sc == null)
                    {
                        chestHP -= num;
                        if (chestHP <= 0f || eid.hitter == "shotgunzone" || eid.hitter == "hammerzone")
                        {
                            CharacterJoint[] componentsInChildren2 = target.GetComponentsInChildren<CharacterJoint>();
                            if (componentsInChildren2.Length != 0)
                            {
                                foreach (CharacterJoint characterJoint2 in componentsInChildren2)
                                {
                                    if (characterJoint2.transform.parent.parent == __instance.chest.transform)
                                    {
                                        EnemyIdentifierIdentifier enemyIdentifierIdentifier2;
                                        if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint2.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier2))
                                        {
                                            enemyIdentifierIdentifier2.SetupForHellBath();
                                        }
                                        Object.Destroy(characterJoint2);
                                        characterJoint2.transform.parent = null;
                                    }
                                }
                            }
                            if (MonoSingleton<BloodsplatterManager>.Instance.goreOn)
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    GameObject gib2 = bsm.GetGib(BSType.gib);
                                    if (gib2 && gz && gz.gibZone)
                                    {
                                        __instance.ReadyGib(gib2, target);
                                    }
                                }
                            }
                            GameObject gore = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                            gore.transform.position = target.transform.position;
                            gore.transform.SetParent(gz.goreZone, true);
                            target.transform.localScale = Vector3.zero;
                        }
                    }
                }
            }
            if (__instance.limp)
            {
                Rigidbody componentInParent = target.GetComponentInParent<Rigidbody>();
                if (componentInParent != null)
                {
                    componentInParent.AddForce(force);
                }
            }
        }
        if (gameObject != null)
        {
            if (!gz)
            {
                gz = GoreZone.ResolveGoreZone(__instance.transform);
            }
            Collider collider;
            if (__instance.thickLimbs && target.TryGetComponent<Collider>(out collider))
            {
                gameObject.transform.position = collider.ClosestPoint(MonoSingleton<NewMovement>.Instance.transform.position);
            }
            else
            {
                gameObject.transform.position = target.transform.position;
            }
            if (eid.hitter == "drill")
            {
                gameObject.transform.localScale *= 2f;
            }
            if (gz != null && gz.goreZone != null)
            {
                gameObject.transform.SetParent(gz.goreZone, true);
            }
            Bloodsplatter component2 = gameObject.GetComponent<Bloodsplatter>();
            if (component2)
            {
                ParticleSystem.CollisionModule collision = component2.GetComponent<ParticleSystem>().collision;
                if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
                {
                    if (Random.Range(0f, 1f) > 0.5f)
                    {
                        collision.enabled = false;
                    }
                    component2.hpAmount = 3;
                }
                else if (eid.hitter == "nail")
                {
                    component2.hpAmount = 1;
                    component2.GetComponent<AudioSource>().volume *= 0.8f;
                }
                if (!noheal)
                {
                    component2.GetReady();
                }
            }
        }


        // May have bug if it create 2 audioSource 
        if ((__instance.health > 0f || symbiotic) && __instance.hurtSounds.Length != 0 && !eid.blessed)
        {
            if (aud == null)
            {
                aud = __instance.GetComponent<AudioSource>();
            }
            aud.clip = __instance.hurtSounds[Random.Range(0, __instance.hurtSounds.Length)];
            if (tur)
            {
                aud.volume = 0.85f;
            }
            else if (min)
            {
                aud.volume = 1f;
            }
            else
            {
                aud.volume = 0.5f;
            }
            if (sm != null)
            {
                aud.pitch = Random.Range(0.85f, 1.35f);
            }
            else
            {
                aud.pitch = Random.Range(0.9f, 1.1f);
            }
            aud.priority = 12;
            aud.Play();
        }
        if (num == 0f || eid.puppet)
        {
            flag = false;
        }
        if (flag && eid.hitter != "enemy")
        {
            if (scalc == null)
            {
                scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            if (__instance.health <= 0f && !symbiotic && (v2 == null || !v2.dontDie) && (!eid.flying || mf))
            {
                dead = true;
                if (__instance.gc && !__instance.gc.onGround)
                {
                    if (eid.hitter == "explosion" || eid.hitter == "ffexplosion" || eid.hitter == "railcannon")
                    {
                        scalc.shud.AddPoints(120, "ultrakill.fireworks", sourceWeapon, eid, -1, "", "");
                    }
                    else if (eid.hitter == "ground slam")
                    {
                        scalc.shud.AddPoints(160, "ultrakill.airslam", sourceWeapon, eid, -1, "", "");
                    }
                    else if (eid.hitter != "deathzone")
                    {
                        scalc.shud.AddPoints(50, "ultrakill.airshot", sourceWeapon, eid, -1, "", "");
                    }
                }
            }
            else if (__instance.health > 0f && __instance.gc && !__instance.gc.onGround && (eid.hitter == "explosion" || eid.hitter == "ffexplosion" || eid.hitter == "railcannon"))
            {
                scalc.shud.AddPoints(20, "ultrakill.fireworksweak", sourceWeapon, eid, -1, "", "");
            }
            if (eid.hitter != "secret")
            {
                if (__instance.bigKill)
                {
                    scalc.HitCalculator(eid.hitter, "spider", hitLimb, dead, eid, sourceWeapon);
                    return false;
                }
                scalc.HitCalculator(eid.hitter, "machine", hitLimb, dead, eid, sourceWeapon);
            }
        }

        return false;
    }
}