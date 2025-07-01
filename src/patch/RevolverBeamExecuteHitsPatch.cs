using UnityEngine;
using HarmonyLib;

namespace StyleGoRound;
/*
[HarmonyPatch(typeof(RevolverBeam), nameof(RevolverBeam.ExecuteHits))]
public static class RevolverBeamExecuteHitsPatch
{
    public static void Prefix(ref RaycastHit currentHit, RevolverBeam __instance)
    {
        Transform transform = currentHit.transform;
		if (transform != null)
		{
			GameObject gameObject = transform.gameObject;
			Breakable breakable;
			if (transform.TryGetComponent<Breakable>(out breakable) && !breakable.specialCaseOnly && (__instance.strongAlt || __instance.beamType == BeamType.Railgun || breakable.weak))
			{
				if (breakable.interrupt)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.interruption", __instance.sourceWeapon, null, -1, "", "");
					MonoSingleton<TimeController>.Instance.ParryFlash();
					if (__instance.canHitProjectiles)
					{
						breakable.breakParticle = MonoSingleton<DefaultReferenceManager>.Instance.superExplosion;
					}
					if (breakable.interruptEnemy && !breakable.interruptEnemy.blessed)
					{
						breakable.interruptEnemy.Explode(true);
					}
				}
				breakable.Break();
			}
			if (SceneHelper.IsStaticEnvironment(currentHit))
			{
				MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(currentHit, Mathf.RoundToInt(3f * this.damage), this.damage);
			}
			Glass glass;
			if (gameObject.TryGetComponent<Glass>(out glass) && !glass.broken && this.beamType == BeamType.Enemy)
			{
				glass.Shatter();
			}
			Projectile projectile;
			if (this.canHitProjectiles && gameObject.layer == 14 && gameObject.TryGetComponent<Projectile>(out projectile) && (projectile.speed != 0f || projectile.turnSpeed != 0f || projectile.decorative))
			{
				Object.Instantiate<GameObject>((!this.hasHitProjectile) ? MonoSingleton<DefaultReferenceManager>.Instance.superExplosion : projectile.explosionEffect, projectile.transform.position, Quaternion.identity);
				Object.Destroy(projectile.gameObject);
				if (!this.hasHitProjectile)
				{
					MonoSingleton<TimeController>.Instance.ParryFlash();
				}
				this.hasHitProjectile = true;
			}
			Bleeder bleeder;
			if (gameObject.TryGetComponent<Bleeder>(out bleeder))
			{
				if (this.beamType == BeamType.Railgun || this.strongAlt)
				{
					bleeder.GetHit(currentHit.point, GoreType.Head, false);
				}
				else
				{
					bleeder.GetHit(currentHit.point, GoreType.Body, false);
				}
			}
			SandboxProp sandboxProp;
			if (gameObject.TryGetComponent<SandboxProp>(out sandboxProp) && currentHit.rigidbody != null)
			{
				currentHit.rigidbody.AddForceAtPosition(base.transform.forward * (float)this.bulletForce * 0.005f, this.hit.point, ForceMode.VelocityChange);
			}
			Coin coin;
			if (transform.TryGetComponent<Coin>(out coin) && this.beamType == BeamType.Revolver)
			{
				if (this.quickDraw)
				{
					coin.quickDraw = true;
				}
				coin.DelayedReflectRevolver(currentHit.point, null);
			}
			if (gameObject.CompareTag("Enemy") || gameObject.CompareTag("Body") || gameObject.CompareTag("Limb") || gameObject.CompareTag("EndLimb") || gameObject.CompareTag("Head"))
			{
				EnemyIdentifier eid = transform.GetComponentInParent<EnemyIdentifierIdentifier>().eid;
				if (eid && !this.deflected && (this.beamType == BeamType.MaliciousFace || this.beamType == BeamType.Enemy) && (eid.enemyType == this.ignoreEnemyType || eid.immuneToFriendlyFire || EnemyIdentifier.CheckHurtException(this.ignoreEnemyType, eid.enemyType, this.target)))
				{
					this.enemiesPierced++;
					return;
				}
				if (this.beamType != BeamType.Enemy)
				{
					if (this.hitAmount > 1)
					{
						this.cc.CameraShake(1f * this.screenshakeMultiplier);
					}
					else
					{
						this.cc.CameraShake(0.5f * this.screenshakeMultiplier);
					}
				}
				if (eid && !eid.dead && this.quickDraw && !eid.blessed && !eid.puppet)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.quickdraw", this.sourceWeapon, eid, -1, "", "");
					this.quickDraw = false;
				}
				string text = "";
				if (this.beamType == BeamType.Revolver)
				{
					text = "revolver";
				}
				else if (this.beamType == BeamType.Railgun)
				{
					text = "railcannon";
				}
				else if (this.beamType == BeamType.MaliciousFace || this.beamType == BeamType.Enemy)
				{
					text = "enemy";
				}
				if (eid)
				{
					eid.hitter = text;
					if (this.attributes != null && this.attributes.Length != 0)
					{
						foreach (HitterAttribute item in this.attributes)
						{
							eid.hitterAttributes.Add(item);
						}
					}
					if (!eid.hitterWeapons.Contains(text + this.gunVariation.ToString()))
					{
						eid.hitterWeapons.Add(text + this.gunVariation.ToString());
					}
				}
				float critMultiplier = 1f;
				if (this.beamType != BeamType.Revolver)
				{
					critMultiplier = 0f;
				}
				if (this.critDamageOverride != 0f || this.strongAlt)
				{
					critMultiplier = this.critDamageOverride;
				}
				float num = (this.enemyDamageOverride != 0f) ? this.enemyDamageOverride : this.damage;
				if (eid && this.deflected)
				{
					if (this.beamType == BeamType.MaliciousFace && eid.enemyType == EnemyType.MaliciousFace)
					{
						num = 999f;
					}
					else if (this.beamType == BeamType.Enemy)
					{
						num *= 2.5f;
					}
					if (!this.chargeBacked)
					{
						this.chargeBacked = true;
						if (!eid.blessed)
						{
							MonoSingleton<StyleHUD>.Instance.AddPoints(400, "ultrakill.chargeback", this.sourceWeapon, eid, -1, "", "");
						}
					}
				}
				bool tryForExplode = false;
				if (this.strongAlt)
				{
					tryForExplode = true;
				}
				if (eid)
				{
					eid.DeliverDamage(gameObject, (transform.position - base.transform.position).normalized * (float)this.bulletForce, currentHit.point, num, tryForExplode, critMultiplier, this.sourceWeapon, false, false);
				}
				if (this.beamType != BeamType.MaliciousFace && this.beamType != BeamType.Enemy)
				{
					if (eid && !eid.dead && this.beamType == BeamType.Revolver && !eid.blessed && gameObject.CompareTag("Head"))
					{
						this.gc.headshots++;
						this.gc.headShotComboTime = 3f;
					}
					else if (this.beamType == BeamType.Railgun || !gameObject.CompareTag("Head"))
					{
						this.gc.headshots = 0;
						this.gc.headShotComboTime = 0f;
					}
					if (this.gc.headshots > 1 && eid && !eid.blessed)
					{
						StyleHUD instance = MonoSingleton<StyleHUD>.Instance;
						int points = this.gc.headshots * 20;
						string pointID = "ultrakill.headshotcombo";
						int i = this.gc.headshots;
						instance.AddPoints(points, pointID, this.sourceWeapon, eid, i, "", "");
					}
				}
				if (this.enemyHitSound)
				{
					Object.Instantiate<GameObject>(this.enemyHitSound, currentHit.point, Quaternion.identity);
					return;
				}
			}
			else if (gameObject.layer == 10)
			{
				Grenade componentInParent = transform.GetComponentInParent<Grenade>();
				if (componentInParent != null)
				{
					if (this.beamType != BeamType.Enemy || !componentInParent.enemy || componentInParent.playerRiding)
					{
						MonoSingleton<TimeController>.Instance.ParryFlash();
					}
					if ((this.beamType == BeamType.Railgun && this.hitAmount == 1) || this.beamType == BeamType.MaliciousFace)
					{
						this.maliciousIgnorePlayer = true;
						componentInParent.Explode(componentInParent.rocket, false, !componentInParent.rocket, 2f, true, this.sourceWeapon, false);
						return;
					}
					componentInParent.Explode(componentInParent.rocket, false, !componentInParent.rocket, 1f, false, this.sourceWeapon, false);
					return;
				}
				else
				{
					Cannonball componentInParent2 = transform.GetComponentInParent<Cannonball>();
					if (componentInParent2)
					{
						MonoSingleton<TimeController>.Instance.ParryFlash();
						componentInParent2.Explode();
						return;
					}
				}
			}
			else if (this.beamType == BeamType.Enemy && !currentHit.collider.isTrigger && gameObject.CompareTag("Player"))
			{
				if (this.enemyHitSound)
				{
					Object.Instantiate<GameObject>(this.enemyHitSound, currentHit.point, Quaternion.identity);
				}
				if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
				{
					MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(this.damage * 10f), true, 1f, false, false, 0.35f, false);
					return;
				}
				MonoSingleton<PlatformerMovement>.Instance.Explode(false);
				return;
			}
			else
			{
				if (this.gc)
				{
					this.gc.headshots = 0;
					this.gc.headShotComboTime = 0f;
				}
				if (gameObject.CompareTag("Armor"))
				{
					GameObject gameObject2 = Object.Instantiate<GameObject>(base.gameObject, currentHit.point, base.transform.rotation);
					gameObject2.transform.forward = Vector3.Reflect(base.transform.forward, currentHit.normal);
					RevolverBeam component = gameObject2.GetComponent<RevolverBeam>();
					component.noMuzzleflash = true;
					component.alternateStartPoint = Vector3.zero;
					component.aimAssist = true;
					GameObject gameObject3 = Object.Instantiate<GameObject>(this.ricochetSound, currentHit.point, Quaternion.identity);
					gameObject3.SetActive(false);
					gameObject2.SetActive(false);
					MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject2, 0.1f);
					MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject3, 0.1f);
				}
			}
		}
    }
}*/