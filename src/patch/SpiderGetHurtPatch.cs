using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

using ULTRAKILL.Cheats;

namespace StyleGoRound;

//部分 private数据要访问并修改
[HarmonyPatch(typeof(SpiderBody), nameof(SpiderBody.GetHurt))]
public static class SpiderGetHurtPatch
{
    public static bool Prefix(ref GameObject target,
                              ref Vector3 force,
                              ref Vector3 hitPoint,
                              ref float multiplier,
                              ref GameObject sourceWeapon,
                              SpiderBody __instance
    )
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

            AdjustedMethod(ref target, ref force, ref hitPoint, ref multiplier, ref sourceWeapon, __instance);

            return false;
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Error in SpiderGetHurtPatch: {e.Message}\n{e.StackTrace}");
            return true; // Allow the original method to run if an error occurs
        }


    }
    
    public static void AdjustedMethod(ref GameObject target,
                                      ref Vector3 force,
                                      ref Vector3 hitPoint,
                                      ref float multiplier,
                                      ref GameObject sourceWeapon,
                                      SpiderBody __instance)
    {
        //method start

        bool dead = false;
        float num = __instance.health;
        if (hitPoint == Vector3.zero)
        {
            hitPoint = target.transform.position;
        }
        bool goreOn = MonoSingleton<BloodsplatterManager>.Instance.goreOn;
        if (__instance.eid == null)
        {
            __instance.eid = __instance.GetComponent<EnemyIdentifier>();
        }
        if (__instance.eid.hitter != "fire")
        {
            if (!__instance.eid.sandified && !__instance.eid.blessed)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, __instance.eid, false), hitPoint, Quaternion.identity);
                if (gameObject)
                {
                    Bloodsplatter component = gameObject.GetComponent<Bloodsplatter>();
                    gameObject.transform.SetParent(__instance.gz.goreZone, true);
                    if (__instance.eid.hitter == "drill")
                    {
                        gameObject.transform.localScale *= 2f;
                    }
                    if (__instance.health > 0f)
                    {
                        component.GetReady();
                    }
                    if (__instance.eid.hitter == "nail")
                    {
                        component.hpAmount = 3;
                        component.GetComponent<AudioSource>().volume *= 0.8f;
                    }
                    else if (multiplier >= 1f)
                    {
                        component.hpAmount = 30;
                    }
                    if (goreOn)
                    {
                        gameObject.GetComponent<ParticleSystem>().Play();
                    }
                }
                if (__instance.eid.hitter != "shotgun" && __instance.eid.hitter != "drill" && __instance.gameObject.activeInHierarchy)
                {
                    if (__instance.dripBlood != null)
                    {
                        __instance.currentDrip = UnityEngine.Object.Instantiate<GameObject>(__instance.dripBlood, hitPoint, Quaternion.identity);
                    }
                    if (__instance.currentDrip)
                    {
                        __instance.currentDrip.transform.parent = __instance.transform;
                        __instance.currentDrip.transform.LookAt(__instance.transform);
                        __instance.currentDrip.transform.Rotate(180f, 180f, 180f);
                        if (goreOn)
                        {
                            __instance.currentDrip.GetComponent<ParticleSystem>().Play();
                        }
                    }
                }
            }
            else
            {
                UnityEngine.Object.Instantiate<GameObject>(MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, __instance.eid, false), hitPoint, Quaternion.identity);
            }
        }
        if (!__instance.eid.dead)
        {
            if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= 1f * multiplier;
            }
            if (__instance.scalc == null)
            {
                // If it follow the original code it will initialize once again(
                __instance.scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            if (__instance.health <= 0f)
            {
                dead = true;
            }
            if (((__instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "hammerzone") && __instance.parryable) || __instance.eid.hitter == "punch")
            {
                if (__instance.parryable)
                {
                    __instance.parryable = false;
                    MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, __instance.eid, "");
                    __instance.currentExplosion = UnityEngine.Object.Instantiate<GameObject>(__instance.beamExplosion.ToAsset(), __instance.transform.position, Quaternion.identity);
                    if (!InvincibleEnemies.Enabled && !__instance.eid.blessed)
                    {
                        //this.health -= (float)((this.parryFramesLeft > 0) ? 4 : 5) / this.eid.totalHealthModifier;
                    }
                    foreach (Explosion explosion in __instance.currentExplosion.GetComponentsInChildren<Explosion>())
                    {
                        explosion.speed *= __instance.eid.totalDamageModifier;
                        explosion.maxSize *= 1.75f * __instance.eid.totalDamageModifier;
                        explosion.damage = Mathf.RoundToInt(50f * __instance.eid.totalDamageModifier);
                        explosion.canHit = AffectedSubjects.EnemiesOnly;
                        explosion.friendlyFire = true;
                    }
                    if (__instance.currentEnrageEffect == null)
                    {
                        __instance.CancelInvoke("BeamFire");
                        __instance.Invoke("StopWaiting", 1f);
                        UnityEngine.Object.Destroy(__instance.currentCE);
                    }
                    __instance.parryFramesLeft = 0;
                }
                else
                {
                    __instance.parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
                }
            }
            if (multiplier != 0f)
            {
                __instance.scalc.HitCalculator(__instance.eid.hitter, "spider", "", dead, __instance.eid, sourceWeapon);
            }
            if (num >= __instance.maxHealth / 2f && __instance.health < __instance.maxHealth / 2f)
            {
                if (__instance.ensims == null || __instance.ensims.Length == 0)
                {
                    __instance.ensims = __instance.GetComponentsInChildren<EnemySimplifier>();
                }
                UnityEngine.Object.Instantiate<GameObject>(__instance.woundedParticle, __instance.transform.position, Quaternion.identity);
                if (!__instance.eid.puppet)
                {
                    foreach (EnemySimplifier enemySimplifier in __instance.ensims)
                    {
                        if (!enemySimplifier.ignoreCustomColor)
                        {
                            enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, __instance.woundedMaterial);
                            enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, __instance.woundedEnrageMaterial);
                        }
                    }
                }
            }
            if (__instance.hurtSound && num > 0f)
            {
                __instance.hurtSound.PlayClipAtPoint(MonoSingleton<AudioMixerController>.Instance.goreGroup, __instance.transform.position, 12, 1f, 0.75f, UnityEngine.Random.Range(0.85f, 1.35f), AudioRolloffMode.Linear, 1f, 100f);
            }
            if (__instance.health <= 0f && !__instance.eid.dead)
            {
                __instance.Die();
                return;
            }
        }
        else if (__instance.eid.hitter == "ground slam")
        {
            __instance.BreakCorpse();
        }

        return;
    }
}