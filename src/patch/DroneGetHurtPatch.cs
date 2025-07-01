
using System;
using HarmonyLib;
using UnityEngine;

using ULTRAKILL.Cheats;

namespace StyleGoRound;


[HarmonyPatch(typeof(Drone), nameof(Drone.GetHurt))]
public static class DroneGetHurtPatch
{
    public static bool Prefix(ref Vector3 force,
                              ref float multiplier,
                              ref GameObject sourceWeapon,
                              ref bool fromExplosion,
                              Drone __instance)
    {
        try
        {
            // See EnemyIdentifier.Instakill()
            if (force == Vector3.zero && multiplier >= 999f && sourceWeapon == null && !fromExplosion)
            {
                return true;
            }

            if (RankChecker.IsRanked())
            {
                return true;
            }

            AdjustedMethod(ref force, ref multiplier, ref sourceWeapon, ref fromExplosion, __instance);

            return false;
            
        }
        catch (Exception e)
        {
            Plugin.Log.LogError("Error in DroneGetHurtPatch: " + e.Message);
            return true; // Allow original method to run if an error occurs
        }

    }

    public static void AdjustedMethod(ref Vector3 force,
                                      ref float multiplier,
                                      ref GameObject sourceWeapon,
                                      ref bool fromExplosion,
                                      Drone __instance)
    {
        // Access private *variables*
        var eidField = AccessTools.Field(typeof(Drone), "eid");
        var eid = eidField.GetValue(__instance) as EnemyIdentifier;

        var parryableField = AccessTools.Field(typeof(Drone), "parryable");
        var parryable = parryableField.GetValue(__instance) as bool? ?? false;

        var parryFramesLeftField = AccessTools.Field(typeof(Drone), "parryFramesLeft");
        var parryFramesLeft = parryFramesLeftField.GetValue(__instance) as int? ?? 0;

        var homeRunnableField = AccessTools.Field(typeof(Drone), "homeRunnable");
        var homeRunnable = homeRunnableField.GetValue(__instance) as bool? ?? false;

        var scalcField = AccessTools.Field(typeof(Drone), "scalc");
        var scalc = scalcField.GetValue(__instance) as StyleCalculator;

        var targetProperty = AccessTools.Property(typeof(Drone), "target");
        var target = targetProperty.GetValue(__instance) as EnemyTarget;

        var crashTargetField = AccessTools.Field(typeof(Drone), "crashTarget");
        var crashTarget = crashTargetField.GetValue(__instance) as Vector3? ?? Vector3.zero;

        var canHurtOtherDronesField = AccessTools.Field(typeof(Drone), "canHurtOtherDrones");
        var canHurtOtherDrones = canHurtOtherDronesField.GetValue(__instance) as bool? ?? false;

        var rbField = AccessTools.Field(typeof(Drone), "rb");
        var rb = rbField.GetValue(__instance) as Rigidbody;

        var audField = AccessTools.Field(typeof(Drone), "aud");
        var aud = audField.GetValue(__instance) as AudioSource;

        var typeField = AccessTools.Field(typeof(Drone), "type");
        var type = typeField.GetValue(__instance) as EnemyType? ?? EnemyType.Drone;

        var bsmField = AccessTools.Field(typeof(Drone), "bsm");
        var bsm = bsmField.GetValue(__instance) as BloodsplatterManager;

        var gzField = AccessTools.Field(typeof(Drone), "gz");
        var gz = gzField.GetValue(__instance) as GoreZone;

        var parriedField = AccessTools.Field(typeof(Drone), "parried");
        var parried = parriedField.GetValue(__instance) as bool? ?? false;

        var canInterruptCrashField = AccessTools.Field(typeof(Drone), "canInterruptCrash");
        var canInterruptCrash = canInterruptCrashField.GetValue(__instance) as bool? ?? false;


        // Method start
        bool flag = false;
        if (!__instance.crashing)
        {
            if ((eid.hitter == "shotgunzone" || eid.hitter == "hammerzone") && !parryable && __instance.health - multiplier > 0f)
            {
                return;
            }
            if (((eid.hitter == "shotgunzone" || eid.hitter == "hammerzone") && parryable) || eid.hitter == "punch")
            {
                if (parryable)
                {
                    if (!InvincibleEnemies.Enabled && !eid.blessed)
                    {
                        multiplier = (float)((parryFramesLeft > 0) ? 3 : 4);
                    }
                    MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, eid, "");
                    parryableField.SetValue(__instance, false);
                    //this.parryable = false;
                }
                else
                {
                    parryFramesLeftField.SetValue(__instance, MonoSingleton<FistControl>.Instance.currentPunch.activeFrames);
                }
            }
            if (!eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= 1f * multiplier;
            }
            __instance.health = (float)Math.Round((double)__instance.health, 4);
            if ((double)__instance.health <= 0.001)
            {
                __instance.health = 0f;
            }
            if (eid == null)
            {
                eid = __instance.GetComponent<EnemyIdentifier>();
            }
            if (__instance.health <= 0f)
            {
                flag = true;
            }
            if (homeRunnable && !__instance.fleshDrone && !eid.puppet && flag && (eid.hitter == "punch" || eid.hitter == "heavypunch" || eid.hitter == "hammer"))
            {
                MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.homerun", sourceWeapon, eid, -1, "", "");
                MonoSingleton<StyleCalculator>.Instance.AddToMultiKill(null);
            }
            else if (eid.hitter != "enemy" && !eid.puppet && multiplier != 0f)
            {
                if (scalc == null)
                {
                    scalc = MonoSingleton<StyleCalculator>.Instance;
                }
                if (scalc)
                {
                    scalc.HitCalculator(eid.hitter, "drone", "", flag, eid, sourceWeapon);
                }
            }
            if (__instance.health <= 0f && !__instance.crashing)
            {
                parryableField.SetValue(__instance, false);
                //this.parryable = false;
                AccessTools.Method(typeof(Drone), "Death").Invoke(__instance, new object[] { fromExplosion });
                //this.Death(fromExplosion);
                if (eid.hitter != "punch" && eid.hitter != "heavypunch" && eid.hitter != "hammer")
                {
                    if (target != null)
                    {
                        crashTargetField.SetValue(__instance, target.position);
                        //this.crashTarget = this.target.position;
                    }
                }
                else
                {
                    canHurtOtherDronesField.SetValue(__instance, true);
                    __instance.transform.position += force.normalized;
                    crashTargetField.SetValue(__instance, __instance.transform.position + force);
                    if (!rb.isKinematic)
                    {
                        rb.velocity = force.normalized * 40f;
                    }
                }
                __instance.transform.LookAt(crashTarget);
                if (aud == null)
                {
                    aud = __instance.GetComponent<AudioSource>();
                }
                if (type == EnemyType.Drone)
                {
                    aud.clip = __instance.deathSound;
                    aud.volume = 0.75f;
                    aud.pitch = UnityEngine.Random.Range(0.85f, 1.35f);
                    aud.priority = 11;
                    aud.Play();
                }
                else
                {
                    __instance.PlaySound(__instance.deathSound);
                }
                __instance.Invoke("CanInterruptCrash", 0.5f);
                __instance.Invoke("Explode", 5f);
                return;
            }
            if (!(eid.hitter != "fire"))
            {
                __instance.PlaySound(__instance.hurtSound);
                return;
            }
            GameObject gameObject = null;
            Bloodsplatter bloodsplatter = null;
            if (multiplier != 0f)
            {
                if (!eid.blessed)
                {
                    __instance.PlaySound(__instance.hurtSound);
                }
                gameObject = bsm.GetGore(GoreType.Body, eid, fromExplosion);
                gameObject.transform.position = __instance.transform.position;
                gameObject.SetActive(true);
                gameObject.transform.SetParent(gz.goreZone, true);
                if (eid.hitter == "drill")
                {
                    gameObject.transform.localScale *= 2f;
                }
                bloodsplatter = gameObject.GetComponent<Bloodsplatter>();
            }
            if (__instance.health > 0f)
            {
                if (eid.hitter == "nail")
                {
                    bloodsplatter.hpAmount = (type == EnemyType.Virtue) ? 3 : 1;
                    bloodsplatter.GetComponent<AudioSource>().volume *= 0.8f;
                }
                if (bloodsplatter)
                {
                    bloodsplatter.GetReady();
                }
                if (!eid.blessed && !rb.isKinematic)
                {
                    rb.velocity = rb.velocity / 10f;
                    rb.AddForce(force.normalized * (force.magnitude / 100f), ForceMode.Impulse);
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    if (rb.velocity.magnitude > 50f)
                    {
                        rb.velocity = Vector3.ClampMagnitude(rb.velocity, 50f);
                    }
                }
            }
            if (multiplier >= 1f)
            {
                if (bloodsplatter)
                {
                    bloodsplatter.hpAmount = 30;
                }
                if (__instance.gib != null)
                {
                    int num = 0;
                    while ((float)num <= multiplier)
                    {
                        UnityEngine.Object.Instantiate<GameObject>(__instance.gib.ToAsset(), __instance.transform.position, UnityEngine.Random.rotation).transform.SetParent(gz.gibZone, true);
                        num++;
                    }
                }
            }
            ParticleSystem particleSystem;
            if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && gameObject && gameObject.TryGetComponent<ParticleSystem>(out particleSystem))
            {
                particleSystem.Play();
                return;
            }
        }
        else if ((eid.hitter == "punch" || eid.hitter == "hammer") && !parried)
        {
            parriedField.SetValue(__instance, true);
            //this.parried = true;
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
            }
            __instance.transform.rotation = MonoSingleton<CameraController>.Instance.transform.rotation;
            Punch currentPunch = MonoSingleton<FistControl>.Instance.currentPunch;
            if (eid.hitter == "punch")
            {
                currentPunch.GetComponent<Animator>().Play("Hook", -1, 0.065f);
                currentPunch.Parry(false, eid, "");
            }
            Collider collider;
            if (type == EnemyType.Virtue && __instance.TryGetComponent<Collider>(out collider))
            {
                collider.isTrigger = true;
                return;
            }
        }
        else if (multiplier >= 1f || canInterruptCrash)
        {
            __instance.Explode();
        }

        return;                 
    }
}