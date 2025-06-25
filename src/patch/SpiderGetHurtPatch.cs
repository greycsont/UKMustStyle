using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

using ULTRAKILL.Cheats;

namespace Only;

//部分 private数据要访问并修改
[HarmonyPatch(typeof(SpiderBody), "GetHurt")]
public static class SpiderGetHurtPatch
{
    public static bool Prefix(ref GameObject target,
                              ref Vector3 force,
                              ref Vector3 hitPoint,
                              ref float multiplier,
                              GameObject sourceWeapon,
                              SpiderBody __instance
    )
    {
        // Access private variable
        var eidField = AccessTools.Field(typeof(SpiderBody), "eid");
        var eid = eidField.GetValue(__instance) as EnemyIdentifier;

        var gzField = AccessTools.Field(typeof(SpiderBody), "gz");
        var gz = gzField.GetValue(__instance) as GoreZone;

        var currentDripField = AccessTools.Field(typeof(SpiderBody), "currentDrip");
        var currentDrip = currentDripField.GetValue(__instance) as GameObject;

        var scalcField = AccessTools.Field(typeof(SpiderBody), "scalc");
        var scalc = scalcField.GetValue(__instance) as StyleCalculator;

        var parryableField = AccessTools.Field(typeof(SpiderBody), "parryable");
        var parryable = parryableField.GetValue(__instance) as bool? ?? false;

        var currentExplosionField = AccessTools.Field(typeof(SpiderBody), "currentExplosion");
        var currentExplosion = currentExplosionField.GetValue(__instance) as GameObject;

        var beamExplosionField = AccessTools.Field(typeof(SpiderBody), "beamExplosion");
        var beamExplosion = beamExplosionField.GetValue(__instance) as AssetReference;

        var parryFramesLeftField = AccessTools.Field(typeof(SpiderBody), "parryFramesLeft");
        var parryFramesLeft = parryFramesLeftField.GetValue(__instance) as int?;

        var maxHealthField = AccessTools.Field(typeof(Zombie), "maxHealth");
        var maxHealth = maxHealthField.GetValue(__instance) as float?;

        var ensimsField = AccessTools.Field(typeof(SpiderBody), "ensims");
        var ensims = ensimsField.GetValue(__instance) as EnemySimplifier[];

        //method start

        bool dead = false;
        float num = __instance.health;
        if (hitPoint == Vector3.zero)
        {
            hitPoint = target.transform.position;
        }
        bool goreOn = MonoSingleton<BloodsplatterManager>.Instance.goreOn;
        if (eid == null)
        {
            eid = __instance.GetComponent<EnemyIdentifier>();
        }
        if (eid.hitter != "fire")
        {
            if (!eid.sandified && !eid.blessed)
            {
                GameObject gameObject = Object.Instantiate<GameObject>(MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, eid, false), hitPoint, Quaternion.identity);
                if (gameObject)
                {
                    Bloodsplatter component = gameObject.GetComponent<Bloodsplatter>();
                    gameObject.transform.SetParent(gz.goreZone, true);
                    if (eid.hitter == "drill")
                    {
                        gameObject.transform.localScale *= 2f;
                    }
                    if (__instance.health > 0f)
                    {
                        component.GetReady();
                    }
                    if (eid.hitter == "nail")
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
                if (eid.hitter != "shotgun" && eid.hitter != "drill" && __instance.gameObject.activeInHierarchy)
                {
                    if (__instance.dripBlood != null)
                    {
                        currentDrip = Object.Instantiate<GameObject>(__instance.dripBlood, hitPoint, Quaternion.identity);
                    }
                    if (currentDrip)
                    {
                        currentDrip.transform.parent = __instance.transform;
                        currentDrip.transform.LookAt(__instance.transform);
                        currentDrip.transform.Rotate(180f, 180f, 180f);
                        if (goreOn)
                        {
                            currentDrip.GetComponent<ParticleSystem>().Play();
                        }
                    }
                }
            }
            else
            {
                Object.Instantiate<GameObject>(MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, eid, false), hitPoint, Quaternion.identity);
            }
        }
        if (!eid.dead)
        {
            if (!eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= 1f * multiplier;
            }
            if (scalc == null)
            {
                scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            if (__instance.health <= 0f)
            {
                dead = true;
            }
            if (((eid.hitter == "shotgunzone" || eid.hitter == "hammerzone") && parryable) || eid.hitter == "punch")
            {
                if (parryable)
                {
                    parryable = false;
                    MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, eid, "");
                    currentExplosion = Object.Instantiate<GameObject>(beamExplosion.ToAsset(), __instance.transform.position, Quaternion.identity);
                    if (!InvincibleEnemies.Enabled && !eid.blessed)
                    {
                        //this.health -= (float)((this.parryFramesLeft > 0) ? 4 : 5) / this.eid.totalHealthModifier;
                    }
                    foreach (Explosion explosion in currentExplosion.GetComponentsInChildren<Explosion>())
                    {
                        explosion.speed *= eid.totalDamageModifier;
                        explosion.maxSize *= 1.75f * eid.totalDamageModifier;
                        explosion.damage = Mathf.RoundToInt(50f * eid.totalDamageModifier);
                        explosion.canHit = AffectedSubjects.EnemiesOnly;
                        explosion.friendlyFire = true;
                    }
                    if (__instance.currentEnrageEffect == null)
                    {
                        __instance.CancelInvoke("BeamFire");
                        __instance.Invoke("StopWaiting", 1f);
                        Object.Destroy(__instance.currentCE);
                    }
                    parryFramesLeft = 0;
                }
                else
                {
                    parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
                }
            }
            if (multiplier != 0f)
            {
                scalc.HitCalculator(eid.hitter, "spider", "", dead, eid, sourceWeapon);
            }
            if (num >= maxHealth / 2f && __instance.health < maxHealth / 2f)
            {
                if (ensims == null || ensims.Length == 0)
                {
                    ensims = __instance.GetComponentsInChildren<EnemySimplifier>();
                }
                Object.Instantiate<GameObject>(__instance.woundedParticle, __instance.transform.position, Quaternion.identity);
                if (!eid.puppet)
                {
                    foreach (EnemySimplifier enemySimplifier in ensims)
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
                __instance.hurtSound.PlayClipAtPoint(MonoSingleton<AudioMixerController>.Instance.goreGroup, __instance.transform.position, 12, 1f, 0.75f, Random.Range(0.85f, 1.35f), AudioRolloffMode.Linear, 1f, 100f);
            }
            if (__instance.health <= 0f && !eid.dead)
            {
                __instance.Die();
                return false;
            }
        }
        else if (eid.hitter == "ground slam")
        {
            __instance.BreakCorpse();
        }

        return false;
    }
}