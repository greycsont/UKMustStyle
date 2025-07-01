
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
        // Method start
        bool flag = false;
        if (!__instance.crashing)
        {
            if ((__instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "hammerzone") && !__instance.parryable && __instance.health - multiplier > 0f)
            {
                return;
            }
            if (((__instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "hammerzone") && __instance.parryable) || __instance.eid.hitter == "punch")
            {
                if (__instance.parryable)
                {
                    if (!InvincibleEnemies.Enabled && !__instance.eid.blessed)
                    {
                        multiplier = (float)((__instance.parryFramesLeft > 0) ? 3 : 4);
                    }
                    MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, __instance.eid, "");
                    __instance.parryable = false;
                    //this.parryable = false;
                }
                else
                {
                    __instance.parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
                }
            }
            if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= 1f * multiplier;
            }
            __instance.health = (float)Math.Round((double)__instance.health, 4);
            if ((double)__instance.health <= 0.001)
            {
                __instance.health = 0f;
            }
            if (__instance.eid == null)
            {
                __instance.eid = __instance.GetComponent<EnemyIdentifier>();
            }
            if (__instance.health <= 0f)
            {
                flag = true;
            }
            if (__instance.homeRunnable && !__instance.fleshDrone && !__instance.eid.puppet && flag && (__instance.eid.hitter == "punch" || __instance.eid.hitter == "heavypunch" || __instance.eid.hitter == "hammer"))
            {
                MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.homerun", sourceWeapon, __instance.eid, -1, "", "");
                MonoSingleton<StyleCalculator>.Instance.AddToMultiKill(null);
            }
            else if (__instance.eid.hitter != "enemy" && !__instance.eid.puppet && multiplier != 0f)
            {
                if (__instance.scalc == null)
                {
                    __instance.scalc = MonoSingleton<StyleCalculator>.Instance;
                }
                if (__instance.scalc)
                {
                    __instance.scalc.HitCalculator(__instance.eid.hitter, "drone", "", flag, __instance.eid, sourceWeapon);
                }
            }
            if (__instance.health <= 0f && !__instance.crashing)
            {
                __instance.parryable = false;
                //this.parryable = false;
                __instance.Death(fromExplosion);
                //this.Death(fromExplosion);
                if (__instance.eid.hitter != "punch" && __instance.eid.hitter != "heavypunch" && __instance.eid.hitter != "hammer")
                {
                    if (__instance.target != null)
                    {
                        __instance.crashTarget = __instance.target.position;
                        //this.crashTarget = this.target.position;
                    }
                }
                else
                {
                    __instance.canHurtOtherDrones = true;
                    __instance.transform.position += force.normalized;
                    __instance.crashTarget = __instance.transform.position + force;
                    if (!__instance.rb.isKinematic)
                    {
                        __instance.rb.velocity = force.normalized * 40f;
                    }
                }
                __instance.transform.LookAt(__instance.crashTarget);
                if (__instance.aud == null)
                {
                    __instance.aud = __instance.GetComponent<AudioSource>();
                }
                if (__instance.type == EnemyType.Drone)
                {
                    __instance.aud.clip = __instance.deathSound;
                    __instance.aud.volume = 0.75f;
                    __instance.aud.pitch = UnityEngine.Random.Range(0.85f, 1.35f);
                    __instance.aud.priority = 11;
                    __instance.aud.Play();
                }
                else
                {
                    __instance.PlaySound(__instance.deathSound);
                }
                __instance.Invoke("CanInterruptCrash", 0.5f);
                __instance.Invoke("Explode", 5f);
                return;
            }
            if (!(__instance.eid.hitter != "fire"))
            {
                __instance.PlaySound(__instance.hurtSound);
                return;
            }
            GameObject gameObject = null;
            Bloodsplatter bloodsplatter = null;
            if (multiplier != 0f)
            {
                if (!__instance.eid.blessed)
                {
                    __instance.PlaySound(__instance.hurtSound);
                }
                gameObject = __instance.bsm.GetGore(GoreType.Body, __instance.eid, fromExplosion);
                gameObject.transform.position = __instance.transform.position;
                gameObject.SetActive(true);
                gameObject.transform.SetParent(__instance.gz.goreZone, true);
                if (__instance.eid.hitter == "drill")
                {
                    gameObject.transform.localScale *= 2f;
                }
                bloodsplatter = gameObject.GetComponent<Bloodsplatter>();
            }
            if (__instance.health > 0f)
            {
                if (__instance.eid.hitter == "nail")
                {
                    bloodsplatter.hpAmount = (__instance.type == EnemyType.Virtue) ? 3 : 1;
                    bloodsplatter.GetComponent<AudioSource>().volume *= 0.8f;
                }
                if (bloodsplatter)
                {
                    bloodsplatter.GetReady();
                }
                if (!__instance.eid.blessed && !__instance.rb.isKinematic)
                {
                    __instance.rb.velocity = __instance.rb.velocity / 10f;
                    __instance.rb.AddForce(force.normalized * (force.magnitude / 100f), ForceMode.Impulse);
                    __instance.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    if (__instance.rb.velocity.magnitude > 50f)
                    {
                        __instance.rb.velocity = Vector3.ClampMagnitude(__instance.rb.velocity, 50f);
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
                        UnityEngine.Object.Instantiate<GameObject>(__instance.gib.ToAsset(), __instance.transform.position, UnityEngine.Random.rotation).transform.SetParent(__instance.gz.gibZone, true);
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
        else if ((__instance.eid.hitter == "punch" || __instance.eid.hitter == "hammer") && !__instance.parried)
        {
            __instance.parried = true;
            //this.parried = true;
            if (!__instance.rb.isKinematic)
            {
                __instance.rb.velocity = Vector3.zero;
            }
            __instance.transform.rotation = MonoSingleton<CameraController>.Instance.transform.rotation;
            Punch currentPunch = MonoSingleton<FistControl>.Instance.currentPunch;
            if (__instance.eid.hitter == "punch")
            {
                currentPunch.GetComponent<Animator>().Play("Hook", -1, 0.065f);
                currentPunch.Parry(false, __instance.eid, "");
            }
            Collider collider;
            if (__instance.type == EnemyType.Virtue && __instance.TryGetComponent<Collider>(out collider))
            {
                collider.isTrigger = true;
                return;
            }
        }
        else if (multiplier >= 1f || __instance.canInterruptCrash)
        {
            __instance.Explode();
        }

        return;                 
    }
}