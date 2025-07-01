using System;
using HarmonyLib;
using UnityEngine;

using ULTRAKILL.Cheats;


namespace StyleGoRound;


[HarmonyPatch(typeof(Zombie), nameof(Zombie.GetHurt))]
public static class ZombieGetHurtPatch
{
    public static bool Prefix(ref GameObject target,
                              ref Vector3 force,
                              ref float multiplier,
                              ref float critMultiplier,
                              ref GameObject sourceWeapon,
                              ref bool fromExplosion,
                              Zombie __instance
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

            AdjustedMethod(ref target, ref force, ref multiplier, ref critMultiplier, ref sourceWeapon, ref fromExplosion, __instance);

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
                                      ref GameObject sourceWeapon,
                                      ref bool fromExplosion,
                                      Zombie __instance
    )
    {
        // Access private variable
        var eidField = AccessTools.Field(typeof(Zombie), "eid");
        var eid = eidField.GetValue(__instance) as EnemyIdentifier;

        var gcField = AccessTools.Field(typeof(Zombie), "gc");
        var gc = gcField.GetValue(__instance) as GroundCheckEnemy;

        var bsmField = AccessTools.Field(typeof(Zombie), "bsm");
        var bsm = bsmField.GetValue(__instance) as BloodsplatterManager;

        var gzField = AccessTools.Field(typeof(Zombie), "gz");
        var gz = gzField.GetValue(__instance) as GoreZone;

        var zmField = AccessTools.Field(typeof(Zombie), "zm");
        var zm = zmField.GetValue(__instance) as ZombieMelee;

        var parryFramesLeftField = AccessTools.Field(typeof(Zombie), "parryFramesLeft");
        var parryFramesLeft = parryFramesLeftField.GetValue(__instance) as int?;

        var nohealField = AccessTools.Field(typeof(Zombie), "noheal");
        var noheal = nohealField.GetValue(__instance) as bool? ?? false;

        var audField = AccessTools.Field(typeof(Zombie), "aud");
        var aud = audField.GetValue(__instance) as AudioSource;

        var scalcField = AccessTools.Field(typeof(Zombie), "scalc");
        var scalc = scalcField.GetValue(__instance) as StyleCalculator;

        var chestHPField = AccessTools.Field(typeof(Zombie), "chestHP");
        var chestHP = chestHPField.GetValue(__instance) as float?;

        var chestExplodedField = AccessTools.Field(typeof(Zombie), "chestExploded");
        var chestExploded = chestExplodedField.GetValue(__instance) as bool? ?? false;


        // Method
        string hitLimb = "";
        bool flag = false;
        bool flag2 = false;
        if (eid == null)
        {
            eid = __instance.GetComponent<EnemyIdentifier>();
        }
        if (gc && !gc.onGround && eid.hitter != "fire")
        {
            multiplier *= 1.5f;
        }
        if (force != Vector3.zero && !__instance.limp)
        {
            __instance.KnockBack(force / 100f);
            if (eid.hitter == "heavypunch" || (eid.hitter == "cannonball" && gc && !gc.onGround))
            {
                eid.useBrakes = false;
            }
            else
            {
                eid.useBrakes = true;
            }
        }
        if (__instance.chestExploding && __instance.health <= 0f && (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb")) && target.GetComponentInParent<EnemyIdentifier>() != null)
        {
            __instance.ChestExplodeEnd();
        }
        GameObject gameObject = null;
        if (bsm == null)
        {
            bsm = MonoSingleton<BloodsplatterManager>.Instance;
        }
        if (zm && zm.diving)
        {
            zm.CancelAttack();
        }
        if (eid.hitter == "punch")
        {
            if (__instance.attacking)
            {
                if (!InvincibleEnemies.Enabled && !eid.blessed)
                {
                    //__instance.health -= (float)((parryFramesLeft > 0) ? 4 : 5);
                }
                __instance.attacking = false;
                MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, eid, "");
            }
            else
            {
                parryFramesLeftField.SetValue(__instance, MonoSingleton<FistControl>.Instance.currentPunch.activeFrames);
            }
        }
        if (target.gameObject.CompareTag("Head"))
        {
            float num = 1f * multiplier + multiplier * critMultiplier;
            if (!eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num;
            }
            if (eid.hitter != "fire" && num > 0f)
            {
                if (num >= 1f || __instance.health <= 0f)
                {
                    gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                }
                else
                {
                    gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
                }
            }
            Vector3 normalized = (target.transform.position - __instance.transform.position).normalized;
            if (!__instance.limp)
            {
                flag2 = true;
                hitLimb = "head";
            }
            if (__instance.health <= 0f)
            {
                if (!__instance.limp)
                {
                    __instance.GoLimp();
                }
                if (eid.hitter != "fire" && eid.hitter != "sawblade")
                {
                    float num2 = 1f;
                    if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone")
                    {
                        num2 = 0.5f;
                    }
                    else if (eid.hitter == "Explosion")
                    {
                        num2 = 0.25f;
                    }
                    if (target.transform.parent != null && target.transform.parent.GetComponentInParent<Rigidbody>() != null)
                    {
                        target.transform.parent.GetComponentInParent<Rigidbody>().AddForce(force * 10f);
                    }
                    if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && eid.hitter != "harpoon")
                    {
                        AccessTools.Method(typeof(Zombie), "GetGoreZone").Invoke(__instance, null);
                        int num3 = 0;
                        while ((float)num3 < 6f * num2)
                        {
                            GameObject gib = bsm.GetGib(BSType.skullChunk);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib, target });
                            num3++;
                        }
                        int num4 = 0;
                        while ((float)num4 < 4f * num2)
                        {
                            GameObject gib = bsm.GetGib(BSType.brainChunk);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib, target });
                            num4++;
                        }
                        int num5 = 0;
                        while ((float)num5 < 2f * num2)
                        {
                            GameObject gib = bsm.GetGib(BSType.eyeball);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib, target });
                            gib = bsm.GetGib(BSType.jawChunk);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib, target });
                            num5++;
                        }
                    }
                }
            }
        }
        else if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
        {
            if (eid == null)
            {
                eid = __instance.GetComponent<EnemyIdentifier>();
            }
            float num = 1f * multiplier + 0.5f * multiplier * critMultiplier;
            if (!eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num;
            }
            if (eid.hitter != "fire" && num > 0f)
            {
                if (eid.hitter == "hammer")
                {
                    gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                }
                else if (((num >= 1f || __instance.health <= 0f) && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
                {
                    gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
                }
                else if (eid.hitter != "explosion")
                {
                    gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
                }
            }
            Vector3 normalized2 = (target.transform.position - __instance.transform.position).normalized;
            if (!__instance.limp)
            {
                flag2 = true;
                hitLimb = "limb";
            }
            if (__instance.health <= 0f)
            {
                if (!__instance.limp)
                {
                    __instance.GoLimp();
                }
                if (eid.hitter == "sawblade")
                {
                    if (!chestExploded && target.transform.position.y > __instance.chest.transform.position.y - 1f)
                    {
                        __instance.ChestExplosion(true, false);
                    }
                }
                else if (eid.hitter != "fire" && eid.hitter != "harpoon")
                {
                    if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && eid.hitter != "explosion" && target.gameObject.CompareTag("Limb"))
                    {
                        float num6 = 1f;
                        AccessTools.Method(typeof(Zombie), "GetGoreZone").Invoke(__instance, null);
                        if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone")
                        {
                            num6 = 0.5f;
                        }
                        int num7 = 0;
                        while ((float)num7 < 4f * num6)
                        {
                            GameObject gib2 = bsm.GetGib(BSType.gib);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib2, target });
                            num7++;
                        }
                    }
                    else
                    {
                        target.transform.localScale = Vector3.zero;
                        target.SetActive(false);
                    }
                }
            }
        }
        else
        {
            float num = multiplier;
            if (eid == null)
            {
                eid = __instance.GetComponent<EnemyIdentifier>();
            }
            if (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone")
            {
                if (!__instance.attacking && (target.gameObject != __instance.chest || __instance.health - num > 0f))
                {
                    num = 0f;
                }
                else if (__instance.attacking && (target.gameObject == __instance.chest || eid.target.GetVelocity().magnitude > 18f))
                {
                    if (!InvincibleEnemies.Enabled && !eid.blessed)
                    {
                        num *= 2f;
                    }
                    MonoSingleton<NewMovement>.Instance.Parry(eid, "");
                }
            }
            if (!eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num;
            }
            if (eid.hitter != "fire" && num > 0f)
            {
                if (eid.hitter == "hammer")
                {
                    gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
                }
                else if (num >= 1f || __instance.health <= 0f)
                {
                    gameObject = bsm.GetGore(GoreType.Body, eid, fromExplosion);
                }
                else
                {
                    gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
                }
            }
            if (__instance.health <= 0f && target.gameObject == __instance.chest && eid.hitter != "fire")
            {
                if (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone" || eid.hitter == "sawblade")
                {
                    chestHP = 0f;
                }
                else
                {
                    chestHP -= num;
                }
                if (chestHP <= 0f && eid.hitter != "harpoon")
                {
                    __instance.ChestExplosion(eid.hitter == "sawblade", fromExplosion);
                }
            }
            if (!__instance.limp)
            {
                flag2 = true;
                hitLimb = "body";
            }
            if (__instance.health <= 0f)
            {
                if (!__instance.limp)
                {
                    __instance.GoLimp();
                }
                if (eid.hitter != "sawblade" && target.GetComponentInParent<Rigidbody>() != null)
                {
                    target.GetComponentInParent<Rigidbody>().AddForce(force * 10f);
                }
            }
        }
        if (gameObject != null)
        {
            AccessTools.Method(typeof(Zombie), "GetGoreZone").Invoke(__instance, null);
            gameObject.transform.position = target.transform.position;
            if (eid.hitter == "drill")
            {
                gameObject.transform.localScale *= 2f;
            }
            if (gz != null && gz.goreZone != null)
            {
                gameObject.transform.SetParent(gz.goreZone, true);
            }
            Bloodsplatter component = gameObject.GetComponent<Bloodsplatter>();
            if (component)
            {
                ParticleSystem.CollisionModule collision = component.GetComponent<ParticleSystem>().collision;
                if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
                {
                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                    {
                        collision.enabled = false;
                    }
                    component.hpAmount = 3;
                }
                else if (eid.hitter == "nail")
                {
                    component.hpAmount = 1;
                    component.GetComponent<AudioSource>().volume *= 0.8f;
                }
                if (!noheal)
                {
                    component.GetReady();
                }
            }
        }
        if (__instance.health <= 0f)
        {
            if (eid.hitter == "sawblade")
            {
                __instance.Cut(target);
            }
            else if (eid.hitter != "harpoon" && eid.hitter != "fire")
            {
                if (target.gameObject.CompareTag("Limb"))
                {
                    if (target.transform.childCount > 0)
                    {
                        Transform child = target.transform.GetChild(0);
                        CharacterJoint[] componentsInChildren = target.GetComponentsInChildren<CharacterJoint>();
                        AccessTools.Method(typeof(Zombie), "GetGoreZone").Invoke(__instance, null);
                        if (componentsInChildren.Length != 0)
                        {
                            foreach (CharacterJoint characterJoint in componentsInChildren)
                            {
                                EnemyIdentifierIdentifier enemyIdentifierIdentifier;
                                if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier))
                                {
                                    enemyIdentifierIdentifier.SetupForHellBath();
                                }
                                characterJoint.transform.SetParent(gz.transform);
                                UnityEngine.Object.Destroy(characterJoint);
                            }
                        }
                        CharacterJoint component2 = target.GetComponent<CharacterJoint>();
                        if (component2 != null)
                        {
                            component2.connectedBody = null;
                            UnityEngine.Object.Destroy(component2);
                        }
                        target.transform.position = child.position;
                        target.transform.SetParent(child);
                        child.SetParent(gz.transform, true);
                        UnityEngine.Object.Destroy(target.GetComponent<Rigidbody>());
                    }
                    UnityEngine.Object.Destroy(target.GetComponent<Collider>());
                    target.transform.localScale = Vector3.zero;
                    target.gameObject.SetActive(false);
                }
                else if (target.gameObject.CompareTag("EndLimb") || target.gameObject.CompareTag("Head"))
                {
                    target.transform.localScale = Vector3.zero;
                    target.gameObject.SetActive(false);
                }
            }
        }
        if (__instance.health > 0f && !__instance.limp && __instance.hurtSounds.Length != 0 && !eid.blessed && eid.hitter != "blocked")
        {
            aud.clip = __instance.hurtSounds[UnityEngine.Random.Range(0, __instance.hurtSounds.Length)];
            aud.volume = __instance.hurtSoundVol;
            aud.pitch = UnityEngine.Random.Range(0.85f, 1.35f);
            aud.priority = 12;
            aud.Play();
        }
        if (eid == null)
        {
            eid = __instance.GetComponent<EnemyIdentifier>();
        }
        if (multiplier == 0f || eid.puppet)
        {
            flag2 = false;
        }
        if (flag2 && eid.hitter != "enemy")
        {
            if (scalc == null)
            {
                scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            if (__instance.health <= 0f)
            {
                flag = true;
                if (gc && !gc.onGround)
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
                scalc.HitCalculator(eid.hitter, "zombie", hitLimb, flag, eid, sourceWeapon);
            }
            if (flag && eid.hitter != "fire")
            {
                Flammable componentInChildren = __instance.GetComponentInChildren<Flammable>();
                if (componentInChildren && componentInChildren.burning && scalc)
                {
                    scalc.shud.AddPoints(50, "ultrakill.finishedoff", sourceWeapon, eid, -1, "", "");
                }
            }
        }

        return;
    }
}