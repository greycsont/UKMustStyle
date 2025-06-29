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
        // Access private variables
        var eidField = AccessTools.Field(typeof(Statue), "eid");
        var eid = eidField.GetValue(__instance) as EnemyIdentifier;

        var massDyingField = AccessTools.Field(typeof(Statue), "massDying");
        var massDying = massDyingField.GetValue(__instance) as bool? ?? false;

        var bsmField = AccessTools.Field(typeof(Statue), "bsm");
        var bsm = bsmField.GetValue(__instance) as BloodsplatterManager;

        var massField = AccessTools.Field(typeof(Statue), "mass");
        var mass = massField.GetValue(__instance) as Mass;

        var gzField = AccessTools.Field(typeof(Statue), "gz");
        var gz = gzField.GetValue(__instance) as GoreZone;

        var gcField = AccessTools.Field(typeof(Statue), "gc");
        var gc = gcField.GetValue(__instance) as GroundCheckEnemy;

        var nohealField = AccessTools.Field(typeof(Statue), "noheal");
        var noheal = nohealField.GetValue(__instance) as bool? ?? false;

        var parryFramesLeftField = AccessTools.Field(typeof(Statue), "parryFramesLeft");
        var parryFramesLeft = parryFramesLeftField.GetValue(__instance) as int? ?? 0;

        var parryFramesOnPartialField = AccessTools.Field(typeof(Statue), "parryFramesOnPartial");
        var parryFramesOnPartial = parryFramesOnPartialField.GetValue(__instance) as bool? ?? false;


        var audField = AccessTools.Field(typeof(Statue), "aud");
        var aud = audField.GetValue(__instance) as AudioSource;

        var scalcField = AccessTools.Field(typeof(Statue), "scalc");
        var scalc = scalcField.GetValue(__instance) as StyleCalculator;

        // Method Start
        string hitLimb = "";
        bool dead = false;
        bool flag = false;
        bool flag2 = false;
        GameObject gameObject = null;
        float num = __instance.health;
        if (massDying)
        {
            return;
        }
        if (eid == null)
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
            if (!eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num2;
            }
            if (eid.hitter != "fire" && num2 > 0f)
            {
                if (num2 >= 1f || __instance.health <= 0f)
                {
                    gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                }
                else
                {
                    gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
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
            if (!eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num2;
            }
            if (eid.hitter != "fire" && num2 > 0f)
            {
                if (eid.hitter == "hammer")
                {
                    gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                }
                else if ((num2 >= 1f && __instance.health > 0f) || (__instance.health <= 0f && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
                {
                    gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
                }
                else if (eid.hitter != "explosion")
                {
                    gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
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
            if (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone")
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
                    MonoSingleton<NewMovement>.Instance.Parry(eid, "");
                    __instance.SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
                }
            }
            if (__instance.extraDamageZones.Count > 0 && __instance.extraDamageZones.Contains(target))
            {
                num2 *= __instance.extraDamageMultiplier;
                flag2 = true;
            }
            if (!eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num2;
            }
            if (eid.hitter != "fire" && num2 > 0f)
            {
                if (eid.hitter == "hammer")
                {
                    gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                }
                else if ((num2 >= 1f && __instance.health > 0f) || (__instance.health <= 0f && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
                {
                    gameObject = bsm.GetGore(GoreType.Body, eid, fromExplosion);
                }
                else if (eid.hitter != "explosion")
                {
                    gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
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
        if (mass != null)
        {
            if (mass.spearShot && mass.tempSpear && mass.tailHitboxes.Contains(target))
            {
                MassSpear component = mass.tempSpear.GetComponent<MassSpear>();
                if (component != null && component.hitPlayer)
                {
                    if (num2 >= 1f || component.spearHealth - num2 <= 0f)
                    {
                        GameObject gore = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                        AccessTools.Method(typeof(Statue), "ReadyGib").Invoke(__instance, new object[] { gore, mass.tailEnd.GetChild(0).gameObject });
                    }
                    // FK : U
                    component.spearHealth -= num2;
                }
            }
            else if (mass.spearShot && !mass.tempSpear)
            {
                mass.spearShot = false;
            }
        }
        if (gameObject != null)
        {
            if (gz == null)
            {
                gz = GoreZone.ResolveGoreZone(__instance.transform);
            }
            if (hurtPos != Vector3.zero)
            {
                gameObject.transform.position = hurtPos;
            }
            else
            {
                gameObject.transform.position = target.transform.position;
            }
            if (eid.hitter == "drill")
            {
                gameObject.transform.localScale *= 2f;
            }
            if (__instance.bigBlood)
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
                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
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
        if (eid && eid.hitter == "punch")
        {
            bool flag3 = __instance.parryables != null && __instance.parryables.Count > 0 && __instance.parryables.Contains(target.transform);
            if (__instance.parryable || (__instance.partiallyParryable && (flag3 || (parryFramesLeft > 0 && parryFramesOnPartial))))
            {
                __instance.parryable = false;
                __instance.partiallyParryable = false;
                __instance.parryables.Clear();
                if (!InvincibleEnemies.Enabled && !eid.blessed)
                {
                    num2 = 5f;
                }
                if (!eid.blessed && !InvincibleEnemies.Enabled)
                {
                    //this.health -= num2;
                }
                MonoSingleton<FistControl>.Instance.currentPunch.Parry(true, eid, "");
                __instance.SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                parryFramesOnPartialField.SetValue(__instance, flag3);
                parryFramesLeftField.SetValue(__instance, MonoSingleton<FistControl>.Instance.currentPunch.activeFrames);
            }
        }
        if (flag2 && (num2 >= 1f || (eid.hitter == "shotgun" && UnityEngine.Random.Range(0f, 1f) > 0.5f) || (eid.hitter == "nail" && UnityEngine.Random.Range(0f, 1f) > 0.85f)))
        {
            if (__instance.extraDamageMultiplier >= 2f)
            {
                gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
            }
            else
            {
                gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
            }
            if (gameObject)
            {
                gameObject.transform.position = target.transform.position;
                if (gz != null && gz.goreZone != null)
                {
                    gameObject.transform.SetParent(gz.goreZone, true);
                }
                Bloodsplatter component3 = gameObject.GetComponent<Bloodsplatter>();
                if (component3)
                {
                    ParticleSystem.CollisionModule collision2 = component3.GetComponent<ParticleSystem>().collision;
                    if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
                    {
                        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                        {
                            collision2.enabled = false;
                        }
                        component3.hpAmount = 3;
                    }
                    else if (eid.hitter == "nail")
                    {
                        component3.hpAmount = 1;
                        component3.GetComponent<AudioSource>().volume *= 0.8f;
                    }
                    if (!noheal)
                    {
                        component3.GetReady();
                    }
                }
            }
        }
        if (__instance.health > 0f && __instance.hurtSounds.Length != 0 && !eid.blessed)
        {
            if (aud == null)
            {
                aud = __instance.GetComponent<AudioSource>();
            }
            aud.clip = __instance.hurtSounds[UnityEngine.Random.Range(0, __instance.hurtSounds.Length)];
            aud.volume = 0.75f;
            aud.pitch = UnityEngine.Random.Range(0.85f, 1.35f);
            aud.priority = 12;
            aud.Play();
        }
        if (multiplier == 0f || eid.puppet)
        {
            flag = false;
        }
        if (flag && eid.hitter != "enemy")
        {
            if (scalc == null)
            {
                scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            MinosArm component4 = __instance.GetComponent<MinosArm>();
            if (__instance.health <= 0f && !component4)
            {
                dead = true;
                if (gc && !gc.onGround && !eid.flying)
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
            if (eid.hitter != "secret" && scalc)
            {
                scalc.HitCalculator(eid.hitter, "spider", hitLimb, dead, eid, sourceWeapon);
            }
        }
        if ((__instance.woundedMaterial || __instance.woundedModel) && num >= __instance.originalHealth / 2f && __instance.health < __instance.originalHealth / 2f)
        {
            if (__instance.woundedParticle)
            {
                UnityEngine.Object.Instantiate<GameObject>(__instance.woundedParticle, __instance.chest.transform.position, Quaternion.identity);
            }
            if (!eid.puppet)
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
