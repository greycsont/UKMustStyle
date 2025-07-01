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
            // 这里的问题是interruption和deathzone都调用的是enemyIdentifier的Explode方法，如何分辨具体？
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
        // Method
        string hitLimb = "";
        bool flag = false;
        bool flag2 = false;
        if (__instance.eid == null)
        {
            __instance.eid = __instance.GetComponent<EnemyIdentifier>();
        }
        if (__instance.gc && !__instance.gc.onGround && __instance.eid.hitter != "fire")
        {
            multiplier *= 1.5f;
        }
        if (force != Vector3.zero && !__instance.limp)
        {
            __instance.KnockBack(force / 100f);
            if (__instance.eid.hitter == "heavypunch" || (__instance.eid.hitter == "cannonball" && __instance.gc && !__instance.gc.onGround))
            {
                __instance.eid.useBrakes = false;
            }
            else
            {
                __instance.eid.useBrakes = true;
            }
        }
        if (__instance.chestExploding && __instance.health <= 0f && (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb")) && target.GetComponentInParent<EnemyIdentifier>() != null)
        {
            __instance.ChestExplodeEnd();
        }
        GameObject gameObject = null;
        if (__instance.bsm == null)
        {
            __instance.bsm = MonoSingleton<BloodsplatterManager>.Instance;
        }
        if (__instance.zm && __instance.zm.diving)
        {
            __instance.zm.CancelAttack();
        }
        if (__instance.eid.hitter == "punch")
        {
            if (__instance.attacking)
            {
                if (!InvincibleEnemies.Enabled && !__instance.eid.blessed)
                {
                    //__instance.health -= (float)((parryFramesLeft > 0) ? 4 : 5);
                }
                __instance.attacking = false;
                MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, __instance.eid, "");
            }
            else
            {
                __instance.parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
            }
        }
        if (target.gameObject.CompareTag("Head"))
        {
            float num = 1f * multiplier + multiplier * critMultiplier;
            if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num;
            }
            if (__instance.eid.hitter != "fire" && num > 0f)
            {
                if (num >= 1f || __instance.health <= 0f)
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                }
                else
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Small, __instance.eid, fromExplosion);
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
                if (__instance.eid.hitter != "fire" && __instance.eid.hitter != "sawblade")
                {
                    float num2 = 1f;
                    if (__instance.eid.hitter == "shotgun" || __instance.eid.hitter == "shotgunzone")
                    {
                        num2 = 0.5f;
                    }
                    else if (__instance.eid.hitter == "Explosion")
                    {
                        num2 = 0.25f;
                    }
                    if (target.transform.parent != null && target.transform.parent.GetComponentInParent<Rigidbody>() != null)
                    {
                        target.transform.parent.GetComponentInParent<Rigidbody>().AddForce(force * 10f);
                    }
                    if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && __instance.eid.hitter != "harpoon")
                    {
                        AccessTools.Method(typeof(Zombie), "GetGoreZone").Invoke(__instance, null);
                        int num3 = 0;
                        while ((float)num3 < 6f * num2)
                        {
                            GameObject gib = __instance.bsm.GetGib(BSType.skullChunk);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib, target });
                            num3++;
                        }
                        int num4 = 0;
                        while ((float)num4 < 4f * num2)
                        {
                            GameObject gib = __instance.bsm.GetGib(BSType.brainChunk);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib, target });
                            num4++;
                        }
                        int num5 = 0;
                        while ((float)num5 < 2f * num2)
                        {
                            GameObject gib = __instance.bsm.GetGib(BSType.eyeball);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib, target });
                            gib = __instance.bsm.GetGib(BSType.jawChunk);
                            AccessTools.Method(typeof(Zombie), "ReadyGib").Invoke(__instance, new object[] { gib, target });
                            num5++;
                        }
                    }
                }
            }
        }
        else if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
        {
            if (__instance.eid == null)
            {
                __instance.eid = __instance.GetComponent<EnemyIdentifier>();
            }
            float num = 1f * multiplier + 0.5f * multiplier * critMultiplier;
            if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num;
            }
            if (__instance.eid.hitter != "fire" && num > 0f)
            {
                if (__instance.eid.hitter == "hammer")
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                }
                else if (((num >= 1f || __instance.health <= 0f) && __instance.eid.hitter != "explosion") || (__instance.eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Limb, __instance.eid, fromExplosion);
                }
                else if (__instance.eid.hitter != "explosion")
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Small, __instance.eid, fromExplosion);
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
                if (__instance.eid.hitter == "sawblade")
                {
                    if (!__instance.chestExploded && target.transform.position.y > __instance.chest.transform.position.y - 1f)
                    {
                        __instance.ChestExplosion(true, false);
                    }
                }
                else if (__instance.eid.hitter != "fire" && __instance.eid.hitter != "harpoon")
                {
                    if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && __instance.eid.hitter != "explosion" && target.gameObject.CompareTag("Limb"))
                    {
                        float num6 = 1f;
                        AccessTools.Method(typeof(Zombie), "GetGoreZone").Invoke(__instance, null);
                        if (__instance.eid.hitter == "shotgun" || __instance.eid.hitter == "shotgunzone")
                        {
                            num6 = 0.5f;
                        }
                        int num7 = 0;
                        while ((float)num7 < 4f * num6)
                        {
                            GameObject gib2 = __instance.bsm.GetGib(BSType.gib);
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
            if (__instance.eid == null)
            {
                __instance.eid = __instance.GetComponent<EnemyIdentifier>();
            }
            if (__instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "hammerzone")
            {
                if (!__instance.attacking && (target.gameObject != __instance.chest || __instance.health - num > 0f))
                {
                    num = 0f;
                }
                else if (__instance.attacking && (target.gameObject == __instance.chest || __instance.eid.target.GetVelocity().magnitude > 18f))
                {
                    if (!InvincibleEnemies.Enabled && !__instance.eid.blessed)
                    {
                        num *= 2f;
                    }
                    MonoSingleton<NewMovement>.Instance.Parry(__instance.eid, "");
                }
            }
            if (!__instance.eid.blessed && !InvincibleEnemies.Enabled)
            {
                //this.health -= num;
            }
            if (__instance.eid.hitter != "fire" && num > 0f)
            {
                if (__instance.eid.hitter == "hammer")
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Head, __instance.eid, fromExplosion);
                }
                else if (num >= 1f || __instance.health <= 0f)
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Body, __instance.eid, fromExplosion);
                }
                else
                {
                    gameObject = __instance.bsm.GetGore(GoreType.Small, __instance.eid, fromExplosion);
                }
            }
            if (__instance.health <= 0f && target.gameObject == __instance.chest && __instance.eid.hitter != "fire")
            {
                if (__instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "hammerzone" || __instance.eid.hitter == "sawblade")
                {
                    __instance.chestHP = 0f;
                }
                else
                {
                    __instance.chestHP -= num;
                }
                if (__instance.chestHP <= 0f && __instance.eid.hitter != "harpoon")
                {
                    __instance.ChestExplosion(__instance.eid.hitter == "sawblade", fromExplosion);
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
                if (__instance.eid.hitter != "sawblade" && target.GetComponentInParent<Rigidbody>() != null)
                {
                    target.GetComponentInParent<Rigidbody>().AddForce(force * 10f);
                }
            }
        }
        if (gameObject != null)
        {
            AccessTools.Method(typeof(Zombie), "GetGoreZone").Invoke(__instance, null);
            gameObject.transform.position = target.transform.position;
            if (__instance.eid.hitter == "drill")
            {
                gameObject.transform.localScale *= 2f;
            }
            if (__instance.gz != null && __instance.gz.goreZone != null)
            {
                gameObject.transform.SetParent(__instance.gz.goreZone, true);
            }
            Bloodsplatter component = gameObject.GetComponent<Bloodsplatter>();
            if (component)
            {
                ParticleSystem.CollisionModule collision = component.GetComponent<ParticleSystem>().collision;
                if (__instance.eid.hitter == "shotgun" || __instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "explosion")
                {
                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                    {
                        collision.enabled = false;
                    }
                    component.hpAmount = 3;
                }
                else if (__instance.eid.hitter == "nail")
                {
                    component.hpAmount = 1;
                    component.GetComponent<AudioSource>().volume *= 0.8f;
                }
                if (!__instance.noheal)
                {
                    component.GetReady();
                }
            }
        }
        if (__instance.health <= 0f)
        {
            if (__instance.eid.hitter == "sawblade")
            {
                __instance.Cut(target);
            }
            else if (__instance.eid.hitter != "harpoon" && __instance.eid.hitter != "fire")
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
                                characterJoint.transform.SetParent(__instance.gz.transform);
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
                        child.SetParent(__instance.gz.transform, true);
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
        if (__instance.health > 0f && !__instance.limp && __instance.hurtSounds.Length != 0 && !__instance.eid.blessed && __instance.eid.hitter != "blocked")
        {
            __instance.aud.clip = __instance.hurtSounds[UnityEngine.Random.Range(0, __instance.hurtSounds.Length)];
            __instance.aud.volume = __instance.hurtSoundVol;
            __instance.aud.pitch = UnityEngine.Random.Range(0.85f, 1.35f);
            __instance.aud.priority = 12;
            __instance.aud.Play();
        }
        if (__instance.eid == null)
        {
            __instance.eid = __instance.GetComponent<EnemyIdentifier>();
        }
        if (multiplier == 0f || __instance.eid.puppet)
        {
            flag2 = false;
        }
        if (flag2 && __instance.eid.hitter != "enemy")
        {
            if (__instance.scalc == null)
            {
                __instance.scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            if (__instance.health <= 0f)
            {
                flag = true;
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
            if (__instance.eid.hitter != "secret" && __instance.scalc)
            {
                __instance.scalc.HitCalculator(__instance.eid.hitter, "zombie", hitLimb, flag, __instance.eid, sourceWeapon);
            }
            if (flag && __instance.eid.hitter != "fire")
            {
                Flammable componentInChildren = __instance.GetComponentInChildren<Flammable>();
                if (componentInChildren && componentInChildren.burning && __instance.scalc)
                {
                    __instance.scalc.shud.AddPoints(50, "ultrakill.finishedoff", sourceWeapon, __instance.eid, -1, "", "");
                }
            }
        }

        return;
    }
}