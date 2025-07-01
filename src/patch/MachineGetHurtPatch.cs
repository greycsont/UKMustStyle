using UnityEngine;
using System.Reflection;
using HarmonyLib;
using ULTRAKILL.Cheats;
using System;
using System.Diagnostics;


namespace StyleGoRound;


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
        try
        {
            // See EnemyIdentifier.Explode()
            if (multiplier >= 999f)
            {
                return true;
            }

            if (RankChecker.IsRanked())
            {
                return true;
            }

            AdjustedMethod(ref target, ref force, ref multiplier, ref critMultiplier, ref sourceWeapon, ref fromExplosion, __instance);

            return false;
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Error in MachineGetHurtPatch: {e.Message}\n{e.StackTrace}");
            return true;
        }
    }

    public static void AdjustedMethod(ref GameObject target,
                                      ref Vector3 force,
                                      ref float multiplier,
                                      ref float critMultiplier,
                                      ref GameObject sourceWeapon,
                                      ref bool fromExplosion,
                                      Machine __instance)
    {
        // Method start point
        string hitLimb = "";
        bool dead = false;
        bool flag = false;
        float num = multiplier;
        GameObject gameObject = null;
        if (__instance.eid == null)
        {
            __instance.eid = __instance.GetComponent<EnemyIdentifier>();
        }
        if (force != Vector3.zero && !__instance.limp && __instance.sm == null && (__instance.v2 == null || !__instance.v2.inIntro) && (__instance.tur == null || !__instance.tur.lodged || __instance.eid.hitter == "heavypunch" || __instance.eid.hitter == "railcannon" || __instance.eid.hitter == "cannonball" || __instance.eid.hitter == "hammer"))
        {
            if (__instance.tur && __instance.tur.lodged)
            {
                __instance.tur.CancelAim(true);
                __instance.tur.Unlodge();
            }
            __instance.KnockBack(force / 100f);
            if (__instance.eid.hitter == "heavypunch" || (__instance.gc && !__instance.gc.onGround && __instance.eid.hitter == "cannonball"))
            {
                __instance.eid.useBrakes = false;
            }
            else
            {
                __instance.eid.useBrakes = true;
            }
        }
        if (__instance.v2 != null && __instance.v2.secondEncounter && __instance.eid.hitter == "heavypunch")
        {
            __instance.v2.InstaEnrage();
        }
        if (__instance.sc != null && target.gameObject == __instance.sc.canister && !__instance.sc.canisterHit && __instance.eid.hitter == "revolver")
        {
            if (!InvincibleEnemies.Enabled && !__instance.eid.blessed)
            {
                __instance.sc.canisterHit = true;
            }
            if (!__instance.eid.dead && !InvincibleEnemies.Enabled && !__instance.eid.blessed)
            {
                MonoSingleton<StyleHUD>.Instance.AddPoints(200, "ultrakill.instakill", sourceWeapon, __instance.eid, -1, "", "");
            }
            MonoSingleton<TimeController>.Instance.ParryFlash();

            // I don't think this can be seen as a damage event;
            __instance.Invoke("CanisterExplosion", 0.1f);
            return;
        }
        if (__instance.tur != null && __instance.tur.aiming && (__instance.eid.hitter == "revolver" || __instance.eid.hitter == "coin") && __instance.tur.interruptables.Contains(target.transform))
        {
            __instance.tur.Interrupt();
        }
        if (__instance.gm)
        {
            if (__instance.gm.hasShield && !__instance.eid.dead && (__instance.eid.hitter == "heavypunch" || __instance.eid.hitter == "hammer"))
            {
                __instance.gm.ShieldBreak(true, true);
            }
            if (__instance.gm.hasShield)
            {
                multiplier /= 1.5f;
            }
            if (__instance.gm.fallen && !__instance.gm.exploded && __instance.eid.hitter == "ground slam")
            {
                __instance.gm.Explode();
                MonoSingleton<NewMovement>.Instance.Launch(Vector3.up * 750f, 8f, false);
            }
        }
        if (__instance.mf && __instance.mf.dying && (__instance.eid.hitter == "heavypunch" || __instance.eid.hitter == "hammer"))
        {
            __instance.mf.DeadLaunch(force);
        }
        if (__instance.eid.hitter == "punch")
        {
            bool flag2 = __instance.parryables != null && __instance.parryables.Count > 0 && __instance.parryables.Contains(target.transform);
            if (__instance.parryable || (__instance.partiallyParryable && (flag2 || (__instance.parryFramesLeft > 0 && __instance.parryFramesOnPartial))))
            {
                __instance.parryable = false;
                __instance.partiallyParryable = false;
                __instance.parryables.Clear();
                if (!InvincibleEnemies.Enabled && !__instance.eid.blessed)
                {
                    //this.health -= (float)((this.parryFramesLeft > 0) ? 4 : 5);
                }
                MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, __instance.eid, "");
                if (__instance.sm != null && __instance.health > 0f)
                {
                    if (!__instance.sm.enraged)
                    {
                        __instance.sm.Knockdown(true, fromExplosion);
                    }
                    else
                    {
                        __instance.sm.Enrage();
                    }
                }
                else
                {
                    __instance.SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                __instance.parryFramesOnPartial = flag2;
                __instance.parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
            }
        }
        else if (__instance.min && __instance.min.ramTimer > 0f && __instance.eid.hitter == "ground slam")
        {
            __instance.min.GotSlammed();
        }
        if (__instance.sisy && num > 0f)
        {
            if (__instance.eid.burners.Count > 0)
            {
                if (__instance.eid.hitter != "fire")
                {
                    if (num <= 0.5f)
                    {
                        gameObject = __instance.bsm.GetGore(GoreType.Limb, __instance.eid, fromExplosion);
                        __instance.sisy.PlayHurtSound(1);
                    }
                    else
                    {
                        gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                        __instance.sisy.PlayHurtSound(2);
                    }
                }
                else
                {
                    __instance.sisy.PlayHurtSound(0);
                }
            }
            else if (__instance.eid.hitter != "fire")
            {
                gameObject = __instance.bsm.GetGore(GoreType.Smallest, __instance.eid, fromExplosion);
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
        if (num2 == 0f && (__instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "hammerzone"))
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
                MonoSingleton<NewMovement>.Instance.Parry(__instance.eid, "");
                if (__instance.sm != null && __instance.health - num > 0f)
                {
                    if (!__instance.sm.enraged)
                    {
                        __instance.sm.Knockdown(true, fromExplosion);
                    }
                    else
                    {
                        __instance.sm.Enrage();
                    }
                }
                else
                {
                    __instance.SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        if (__instance.sisy && !__instance.limp && __instance.eid.hitter == "fire" && __instance.health > 0f && __instance.health - num < 0.01f && !__instance.eid.isGasolined)
        {
            num = __instance.health - 0.01f;
        }
        if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
        {
            //this.health -= num;
        }
        if (!gameObject && __instance.eid.hitter != "fire" && num > 0f)
        {
            if ((num2 == 1f && (num >= 1f || __instance.health <= 0f)) || __instance.eid.hitter == "hammer")
            {
                gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
            }
            else if (((num >= 1f || __instance.health <= 0f) && __instance.eid.hitter != "explosion") || (__instance.eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
            {
                if (target.gameObject.CompareTag("Body"))
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Body, __instance.eid, fromExplosion);
                }
                else
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Limb, __instance.eid, fromExplosion);
                }
            }
            else if (__instance.eid.hitter != "explosion")
            {
                gameObject = __instance.bsm.GetGore(GoreType.Small, __instance.eid, fromExplosion);
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
            if (__instance.symbiotic)
            {
                if (__instance.sm != null && !__instance.sm.downed && __instance.symbiote.health > 0f)
                {
                    __instance.sm.downed = true;
                    __instance.sm.Down(fromExplosion);
                    __instance.Invoke("StartHealing", 3f);
                }
                else if (__instance.sisy != null && !__instance.sisy.downed && __instance.symbiote.health > 0f)
                {
                    __instance.sisy.downed = true;
                    __instance.sisy.Knockdown(__instance.transform.position + __instance.transform.forward);
                    __instance.Invoke("StartHealing", 3f);
                }
                else if (__instance.symbiote.health <= 0f)
                {
                    __instance.symbiotic = false;
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
                    if (__instance.eid.hitter == "shotgun" || __instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "explosion")
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
                                GameObject gib = __instance.bsm.GetGib(BSType.gib);
                                if (gib && __instance.gz && __instance.gz.gibZone)
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
                                        UnityEngine.Object.Destroy(characterJoint);
                                    }
                                }
                                CharacterJoint component = target.GetComponent<CharacterJoint>();
                                if (component != null)
                                {
                                    component.connectedBody = null;
                                    UnityEngine.Object.Destroy(component);
                                }
                                target.transform.position = child.position;
                                target.transform.SetParent(child);
                                child.SetParent(__instance.gz.gibZone);
                                UnityEngine.Object.Destroy(target.GetComponent<Rigidbody>());
                            }
                        }
                    }
                    else
                    {
                        int num5 = 0;
                        while ((float)num5 < 6f * num3)
                        {
                            GameObject gib = __instance.bsm.GetGib(BSType.skullChunk);
                            if (gib && __instance.gz && __instance.gz.gibZone)
                            {
                                __instance.ReadyGib(gib, target);
                            }
                            num5++;
                        }
                        int num6 = 0;
                        while ((float)num6 < 4f * num3)
                        {
                            GameObject gib = __instance.bsm.GetGib(BSType.brainChunk);
                            if (gib && __instance.gz && __instance.gz.gibZone)
                            {
                                __instance.ReadyGib(gib, target);
                            }
                            num6++;
                        }
                        int num7 = 0;
                        while ((float)num7 < 2f * num3)
                        {
                            GameObject gib = __instance.bsm.GetGib(BSType.eyeball);
                            if (gib && __instance.gz && __instance.gz.gibZone)
                            {
                                __instance.ReadyGib(gib, target);
                            }
                            gib = __instance.bsm.GetGib(BSType.jawChunk);
                            if (gib && __instance.gz && __instance.gz.gibZone)
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
                            UnityEngine.Object.Destroy(obj);
                        }
                        target.transform.localScale = Vector3.zero;
                    }
                    else if (target.gameObject == __instance.chest && __instance.v2 == null && __instance.sc == null)
                    {
                        __instance.chestHP -= num;
                        if (__instance.chestHP <= 0f || __instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "hammerzone")
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
                                        UnityEngine.Object.Destroy(characterJoint2);
                                        characterJoint2.transform.parent = null;
                                    }
                                }
                            }
                            if (MonoSingleton<BloodsplatterManager>.Instance.goreOn)
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    GameObject gib2 = __instance.bsm.GetGib(BSType.gib);
                                    if (gib2 && __instance.gz && __instance.gz.gibZone)
                                    {
                                        __instance.ReadyGib(gib2, target);
                                    }
                                }
                            }
                            GameObject gore = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                            gore.transform.position = target.transform.position;
                            gore.transform.SetParent(__instance.gz.goreZone, true);
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
            if (!__instance.gz)
            {
                __instance.gz = GoreZone.ResolveGoreZone(__instance.transform);
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
            if (__instance.eid.hitter == "drill")
            {
                gameObject.transform.localScale *= 2f;
            }
            if (__instance.gz != null && __instance.gz.goreZone != null)
            {
                gameObject.transform.SetParent(__instance.gz.goreZone, true);
            }
            Bloodsplatter component2 = gameObject.GetComponent<Bloodsplatter>();
            if (component2)
            {
                ParticleSystem.CollisionModule collision = component2.GetComponent<ParticleSystem>().collision;
                if (__instance.eid.hitter == "shotgun" || __instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "explosion")
                {
                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                    {
                        collision.enabled = false;
                    }
                    component2.hpAmount = 3;
                }
                else if (__instance.eid.hitter == "nail")
                {
                    component2.hpAmount = 1;
                    component2.GetComponent<AudioSource>().volume *= 0.8f;
                }
                if (!__instance.noheal)
                {
                    component2.GetReady();
                }
            }
        }


        // May have bug if it create 2 audioSource 
        if ((__instance.health > 0f || __instance.symbiotic) && __instance.hurtSounds.Length != 0 && !__instance.eid.blessed)
        {
            if (__instance.aud == null)
            {
                __instance.aud = __instance.GetComponent<AudioSource>();
            }
            __instance.aud.clip = __instance.hurtSounds[UnityEngine.Random.Range(0, __instance.hurtSounds.Length)];
            if (__instance.tur)
            {
                __instance.aud.volume = 0.85f;
            }
            else if (__instance.min)
            {
                __instance.aud.volume = 1f;
            }
            else
            {
                __instance.aud.volume = 0.5f;
            }
            if (__instance.sm != null)
            {
                __instance.aud.pitch = UnityEngine.Random.Range(0.85f, 1.35f);
            }
            else
            {
                __instance.aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            }
            __instance.aud.priority = 12;
            __instance.aud.Play();
        }
        if (num == 0f || __instance.eid.puppet)
        {
            flag = false;
        }
        if (flag && __instance.eid.hitter != "enemy")
        {
            if (__instance.scalc == null)
            {
                __instance.scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            if (__instance.health <= 0f && !__instance.symbiotic && (__instance.v2 == null || !__instance.v2.dontDie) && (!__instance.eid.flying || __instance.mf))
            {
                dead = true;
                if (__instance.gc && !__instance.gc.onGround)
                {
                    if (__instance.eid.hitter == "explosion" || __instance.eid.hitter == "ffexplosion" || __instance.eid.hitter == "railcannon")
                    {
                        __instance.scalc.shud.AddPoints(120, "ultrakill.fireworks", sourceWeapon, __instance.eid, -1, "", "");
                    }
                    else if (__instance.eid.hitter == "ground slam")
                    {
                        __instance.scalc.shud.AddPoints(160, "ultrakill.airslam", sourceWeapon, __instance.eid, -1, "", "");
                    }
                    else if (__instance.eid.hitter != "deathzone")
                    {
                        __instance.scalc.shud.AddPoints(50, "ultrakill.airshot", sourceWeapon, __instance.eid, -1, "", "");
                    }
                }
            }
            else if (__instance.health > 0f && __instance.gc && !__instance.gc.onGround && (__instance.eid.hitter == "explosion" || __instance.eid.hitter == "ffexplosion" || __instance.eid.hitter == "railcannon"))
            {
                __instance.scalc.shud.AddPoints(20, "ultrakill.fireworksweak", sourceWeapon, __instance.eid, -1, "", "");
            }
            if (__instance.eid.hitter != "secret")
            {
                if (__instance.bigKill)
                {
                    __instance.scalc.HitCalculator(__instance.eid.hitter, "spider", hitLimb, dead, __instance.eid, sourceWeapon);
                    return;
                }
                __instance.scalc.HitCalculator(__instance.eid.hitter, "machine", hitLimb, dead, __instance.eid, sourceWeapon);
            }
        }

        return;
    }
}