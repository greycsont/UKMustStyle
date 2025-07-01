using HarmonyLib;
using UnityEngine;

using ULTRAKILL.Cheats;
using System;

namespace StyleGoRound;


[HarmonyPatch(typeof(Statue), nameof(Statue.GetHurt))]
public static class StatusGetHurtPatch
{
    public static bool Prefix(ref GameObject target,
                              ref Vector3 force,
                              ref float multiplier,
                              ref float critMultiplier,
                              ref Vector3 hurtPos,
                              ref GameObject sourceWeapon,
                              ref bool fromExplosion,
                              Statue __instance)
    {
        try
        {
            if (multiplier >= 999f)
            {
                return true;
            }
            
            if (RankChecker.IsRanked())
            {
                return true;
            }

            AdjustedMethod(ref target, ref force, ref multiplier, ref critMultiplier, ref hurtPos, ref sourceWeapon, ref fromExplosion, __instance);

            return false;
        }
        catch (Exception e)
        {
            Plugin.Log.LogError("Error checking rank: " + e.Message);
            return true;
        }

    }

    public static void AdjustedMethod(ref GameObject target,
                                      ref Vector3 force,
                                      ref float multiplier,
                                      ref float critMultiplier,
                                      ref Vector3 hurtPos,
                                      ref GameObject sourceWeapon,
                                      ref bool fromExplosion,
                                      Statue __instance)
    {
        // Method Start
        string hitLimb = "";
        bool dead = false;
        bool flag = false;
        bool flag2 = false;
        GameObject gameObject = null;
        float num = __instance.health;
        if (__instance.massDying)
        {
            return;
        }
        if (__instance.eid == null)
        {
            return;
        }
        float num2;
        if (target.gameObject.CompareTag("Head"))
        {
            num2 = 1f * multiplier + multiplier * critMultiplier;
            if (__instance.extraDamageZones.Count > 0 && __instance.extraDamageZones.Contains(target))
            {
                num2 *= __instance.extraDamageMultiplier;
                flag2 = true;
            }
            if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num2;
            }
            if (__instance.eid.hitter != "fire" && num2 > 0f)
            {
                if (num2 >= 1f || __instance.health <= 0f)
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                }
                else
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Small, __instance.eid, fromExplosion);
                }
            }
            if (!__instance.limp)
            {
                flag = true;
                hitLimb = "head";
            }
            if (__instance.health <= 0f && !__instance.limp)
            {
                __instance.GoLimp();
            }
        }
        else if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
        {
            num2 = 1f * multiplier + 0.5f * multiplier * critMultiplier;
            if (__instance.extraDamageZones.Count > 0 && __instance.extraDamageZones.Contains(target))
            {
                num2 *= __instance.extraDamageMultiplier;
                flag2 = true;
            }
            if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num2;
            }
            if (__instance.eid.hitter != "fire" && num2 > 0f)
            {
                if (__instance.eid.hitter == "hammer")
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                }
                else if ((num2 >= 1f && __instance.health > 0f) || (__instance.health <= 0f && __instance.eid.hitter != "explosion") || (__instance.eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Limb, __instance.eid, fromExplosion);
                }
                else if (__instance.eid.hitter != "explosion")
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Small, __instance.eid, fromExplosion);
                }
            }
            if (!__instance.limp)
            {
                flag = true;
                hitLimb = "limb";
            }
            if (__instance.health <= 0f && !__instance.limp)
            {
                __instance.GoLimp();
            }
        }
        else
        {
            num2 = 1f * multiplier;
            if (__instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "hammerzone")
            {
                if (!__instance.parryable && (!__instance.partiallyParryable || __instance.parryables == null || !__instance.parryables.Contains(target.transform)) && (target.gameObject != __instance.chest || __instance.health - num2 > 0f))
                {
                    num2 = 0f;
                }
                else if ((__instance.parryable && (target.gameObject == __instance.chest || MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(false).magnitude > 18f)) || (__instance.partiallyParryable && __instance.parryables != null && __instance.parryables.Contains(target.transform)))
                {
                    num2 *= 1.5f;
                    __instance.parryable = false;
                    __instance.partiallyParryable = false;
                    __instance.parryables.Clear();
                    MonoSingleton<NewMovement>.Instance.Parry(__instance.eid, "");
                    __instance.SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
                }
            }
            if (__instance.extraDamageZones.Count > 0 && __instance.extraDamageZones.Contains(target))
            {
                num2 *= __instance.extraDamageMultiplier;
                flag2 = true;
            }
            if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num2;
            }
            if (__instance.eid.hitter != "fire" && num2 > 0f)
            {
                if (__instance.eid.hitter == "hammer")
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                }
                else if ((num2 >= 1f && __instance.health > 0f) || (__instance.health <= 0f && __instance.eid.hitter != "explosion") || (__instance.eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Body, __instance.eid, fromExplosion);
                }
                else if (__instance.eid.hitter != "explosion")
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Small, __instance.eid, fromExplosion);
                }
            }
            if (!__instance.limp)
            {
                flag = true;
                hitLimb = "body";
            }
            if (__instance.health <= 0f)
            {
                if (!__instance.limp)
                {
                    __instance.GoLimp();
                }
                if (target && target.GetComponentInParent<Rigidbody>() != null)
                {
                    target.GetComponentInParent<Rigidbody>().AddForce(force);
                }
            }
        }
        if (__instance.mass != null)
        {
            if (__instance.mass.spearShot && __instance.mass.tempSpear && __instance.mass.tailHitboxes.Contains(target))
            {
                MassSpear component = __instance.mass.tempSpear.GetComponent<MassSpear>();
                if (component != null && component.hitPlayer)
                {
                    if (num2 >= 1f || component.spearHealth - num2 <= 0f)
                    {
                        GameObject gore = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                        AccessTools.Method(typeof(Statue), "ReadyGib").Invoke(__instance, new object[] { gore, __instance.mass.tailEnd.GetChild(0).gameObject });
                    }
                    // FK : U
                    component.spearHealth -= num2;
                }
            }
            else if (__instance.mass.spearShot && !__instance.mass.tempSpear)
            {
                __instance.mass.spearShot = false;
            }
        }
        if (gameObject != null)
        {
            if (__instance.gz == null)
            {
                __instance.gz = GoreZone.ResolveGoreZone(__instance.transform);
            }
            if (hurtPos != Vector3.zero)
            {
                gameObject.transform.position = hurtPos;
            }
            else
            {
                gameObject.transform.position = target.transform.position;
            }
            if (__instance.eid.hitter == "drill")
            {
                gameObject.transform.localScale *= 2f;
            }
            if (__instance.bigBlood)
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
        if (__instance.eid && __instance.eid.hitter == "punch")
        {
            bool flag3 = __instance.parryables != null && __instance.parryables.Count > 0 && __instance.parryables.Contains(target.transform);
            if (__instance.parryable || (__instance.partiallyParryable && (flag3 || (__instance.parryFramesLeft > 0 && __instance.parryFramesOnPartial))))
            {
                __instance.parryable = false;
                __instance.partiallyParryable = false;
                __instance.parryables.Clear();
                if (!InvincibleEnemies.Enabled && !__instance.eid.blessed)
                {
                    num2 = 5f;
                }
                if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
                {
                    //this.health -= num2;
                }
                MonoSingleton<FistControl>.Instance.currentPunch.Parry(true, __instance.eid, "");
                __instance.SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                __instance.parryFramesOnPartial = flag3;
                __instance.parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
            }
        }
        if (flag2 && (num2 >= 1f || (__instance.eid.hitter == "shotgun" && UnityEngine.Random.Range(0f, 1f) > 0.5f) || (__instance.eid.hitter == "nail" && UnityEngine.Random.Range(0f, 1f) > 0.85f)))
        {
            if (__instance.extraDamageMultiplier >= 2f)
            {
                gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
            }
            else
            {
                gameObject = __instance.bsm.GetGore(GoreType.Limb, __instance.eid, fromExplosion);
            }
            if (gameObject)
            {
                gameObject.transform.position = target.transform.position;
                if (__instance.gz != null && __instance.gz.goreZone != null)
                {
                    gameObject.transform.SetParent(__instance.gz.goreZone, true);
                }
                Bloodsplatter component3 = gameObject.GetComponent<Bloodsplatter>();
                if (component3)
                {
                    ParticleSystem.CollisionModule collision2 = component3.GetComponent<ParticleSystem>().collision;
                    if (__instance.eid.hitter == "shotgun" || __instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "explosion")
                    {
                        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                        {
                            collision2.enabled = false;
                        }
                        component3.hpAmount = 3;
                    }
                    else if (__instance.eid.hitter == "nail")
                    {
                        component3.hpAmount = 1;
                        component3.GetComponent<AudioSource>().volume *= 0.8f;
                    }
                    if (!__instance.noheal)
                    {
                        component3.GetReady();
                    }
                }
            }
        }
        if (__instance.health > 0f && __instance.hurtSounds.Length != 0 && !__instance.eid.blessed)
        {
            if (__instance.aud == null)
            {
                __instance.aud = __instance.GetComponent<AudioSource>();
            }
            __instance.aud.clip = __instance.hurtSounds[UnityEngine.Random.Range(0, __instance.hurtSounds.Length)];
            __instance.aud.volume = 0.75f;
            __instance.aud.pitch = UnityEngine.Random.Range(0.85f, 1.35f);
            __instance.aud.priority = 12;
            __instance.aud.Play();
        }
        if (multiplier == 0f || __instance.eid.puppet)
        {
            flag = false;
        }
        if (flag && __instance.eid.hitter != "enemy")
        {
            if (__instance.scalc == null)
            {
                __instance.scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            MinosArm component4 = __instance.GetComponent<MinosArm>();
            if (__instance.health <= 0f && !component4)
            {
                dead = true;
                if (__instance.gc && !__instance.gc.onGround && !__instance.eid.flying)
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
            if (__instance.eid.hitter != "secret" && __instance.scalc)
            {
                __instance.scalc.HitCalculator(__instance.eid.hitter, "spider", hitLimb, dead, __instance.eid, sourceWeapon);
            }
        }
        if ((__instance.woundedMaterial || __instance.woundedModel) && num >= __instance.originalHealth / 2f && __instance.health < __instance.originalHealth / 2f)
        {
            if (__instance.woundedParticle)
            {
                UnityEngine.Object.Instantiate<GameObject>(__instance.woundedParticle, __instance.chest.transform.position, Quaternion.identity);
            }
            if (!__instance.eid.puppet)
            {
                if (__instance.woundedModel)
                {
                    __instance.woundedModel.SetActive(true);
                    __instance.smr.gameObject.SetActive(false);
                    return;
                }
                __instance.smr.material = __instance.woundedMaterial;
                EnemySimplifier enemySimplifier;
                if (__instance.smr.TryGetComponent<EnemySimplifier>(out enemySimplifier))
                {
                    enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, __instance.woundedMaterial);
                    enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, __instance.woundedEnrageMaterial);
                }
            }
        }
        return;
    }
}
