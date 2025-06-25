using HarmonyLib;
using UnityEngine;

using ULTRAKILL.Cheats;

namespace Only;


/*[HarmonyPatch(typeof(Statue), "GetHurt")]
public static class StatusGetHurtPatch
{
    public static void Prefix(GameObject target,
                              Vector3 force,
                              float multiplier,
                              float critMultiplier,
                              Vector3 hurtPos,
                              GameObject sourceWeapon,
                              bool fromExplosion,
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

        var nohealField = AccessTools.Field(typeof(Statue), "noheal");
        var noheal = nohealField.GetValue(__instance) as bool? ?? false;

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
        if (eid && eid.hitter == "punch")
        {
            bool flag3 = __instance.parryables != null && __instance.parryables.Count > 0 && __instance.parryables.Contains(target.transform);
            if (__instance.parryable || (__instance.partiallyParryable && (flag3 || (this.parryFramesLeft > 0 && this.parryFramesOnPartial))))
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
                this.parryFramesOnPartial = flag3;
                this.parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
            }
        }
        if (flag2 && (num2 >= 1f || (this.eid.hitter == "shotgun" && Random.Range(0f, 1f) > 0.5f) || (this.eid.hitter == "nail" && Random.Range(0f, 1f) > 0.85f)))
        {
            if (this.extraDamageMultiplier >= 2f)
            {
                gameObject = this.bsm.GetGore(GoreType.Head, this.eid, fromExplosion);
            }
            else
            {
                gameObject = this.bsm.GetGore(GoreType.Limb, this.eid, fromExplosion);
            }
            if (gameObject)
            {
                gameObject.transform.position = target.transform.position;
                if (this.gz != null && this.gz.goreZone != null)
                {
                    gameObject.transform.SetParent(this.gz.goreZone, true);
                }
                Bloodsplatter component3 = gameObject.GetComponent<Bloodsplatter>();
                if (component3)
                {
                    ParticleSystem.CollisionModule collision2 = component3.GetComponent<ParticleSystem>().collision;
                    if (this.eid.hitter == "shotgun" || this.eid.hitter == "shotgunzone" || this.eid.hitter == "explosion")
                    {
                        if (Random.Range(0f, 1f) > 0.5f)
                        {
                            collision2.enabled = false;
                        }
                        component3.hpAmount = 3;
                    }
                    else if (this.eid.hitter == "nail")
                    {
                        component3.hpAmount = 1;
                        component3.GetComponent<AudioSource>().volume *= 0.8f;
                    }
                    if (!this.noheal)
                    {
                        component3.GetReady();
                    }
                }
            }
        }
        if (this.health > 0f && this.hurtSounds.Length != 0 && !this.eid.blessed)
        {
            if (this.aud == null)
            {
                this.aud = base.GetComponent<AudioSource>();
            }
            this.aud.clip = this.hurtSounds[Random.Range(0, this.hurtSounds.Length)];
            this.aud.volume = 0.75f;
            this.aud.pitch = Random.Range(0.85f, 1.35f);
            this.aud.priority = 12;
            this.aud.Play();
        }
        if (multiplier == 0f || this.eid.puppet)
        {
            flag = false;
        }
        if (flag && this.eid.hitter != "enemy")
        {
            if (this.scalc == null)
            {
                this.scalc = MonoSingleton<StyleCalculator>.Instance;
            }
            MinosArm component4 = base.GetComponent<MinosArm>();
            if (this.health <= 0f && !component4)
            {
                dead = true;
                if (this.gc && !this.gc.onGround && !this.eid.flying)
                {
                    if (this.eid.hitter == "explosion" || this.eid.hitter == "ffexplosion" || this.eid.hitter == "railcannon")
                    {
                        this.scalc.shud.AddPoints(120, "ultrakill.fireworks", sourceWeapon, this.eid, -1, "", "");
                    }
                    else if (this.eid.hitter == "ground slam")
                    {
                        this.scalc.shud.AddPoints(160, "ultrakill.airslam", sourceWeapon, this.eid, -1, "", "");
                    }
                    else if (this.eid.hitter != "deathzone")
                    {
                        this.scalc.shud.AddPoints(50, "ultrakill.airshot", sourceWeapon, this.eid, -1, "", "");
                    }
                }
            }
            if (this.eid.hitter != "secret" && this.scalc)
            {
                this.scalc.HitCalculator(this.eid.hitter, "spider", hitLimb, dead, this.eid, sourceWeapon);
            }
        }
        if ((this.woundedMaterial || this.woundedModel) && num >= this.originalHealth / 2f && this.health < this.originalHealth / 2f)
        {
            if (this.woundedParticle)
            {
                Object.Instantiate<GameObject>(this.woundedParticle, this.chest.transform.position, Quaternion.identity);
            }
            if (!this.eid.puppet)
            {
                if (this.woundedModel)
                {
                    this.woundedModel.SetActive(true);
                    this.smr.gameObject.SetActive(false);
                    return;
                }
                this.smr.material = this.woundedMaterial;
                EnemySimplifier enemySimplifier;
                if (this.smr.TryGetComponent<EnemySimplifier>(out enemySimplifier))
                {
                    enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, this.woundedMaterial);
                    enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, this.woundedEnrageMaterial);
                }
            }
        }
    }
}*/
