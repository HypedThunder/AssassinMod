using System;
using RoR2;
using UnityEngine;
using EntityStates.NullifierMonster;
using EntityStates.Commando.CommandoWeapon;

namespace EntityStates.Assassin.Weapon6
{
	// Token: 0x02000AB9 RID: 2745
	public class Sniper : BaseState
	{
		// Token: 0x06003E81 RID: 16001 RVA: 0x001053D8 File Offset: 0x001035D8
		public override void OnEnter()
		{
			base.OnEnter();
			base.AddRecoil(-1f * FireShotgun.recoilAmplitude, -2f * FireShotgun.recoilAmplitude, -0.5f * FireShotgun.recoilAmplitude, 0.5f * FireShotgun.recoilAmplitude);
			this.maxDuration = FireShotgun.baseMaxDuration / this.attackSpeedStat;
			this.minDuration = FireShotgun.baseMinDuration / this.attackSpeedStat;
			Ray aimRay = base.GetAimRay();
			base.StartAimMode(aimRay, 2f, false);
			Util.PlaySound(FirePistol2.firePistolSoundString, base.gameObject);
			base.PlayAnimation("Gesture, Additive", "SlashP1", "SlashCombo.playbackRate", this.maxDuration * 0.8f);
			base.PlayAnimation("Gesture, Override", "SlashP1", "SlashCombo.playbackRate", this.maxDuration * 0.8f);
			string muzzleName = "DaggerLeft";
			if (FirePistol2.hitEffectPrefab)
			{
				EffectManager.SimpleMuzzleFlash(FirePistol2.hitEffectPrefab, base.gameObject, muzzleName, false);
			}
			if (base.isAuthority)
			{
				new BulletAttack
				{
					owner = base.gameObject,
					weapon = base.gameObject,
					origin = aimRay.origin,
					aimVector = aimRay.direction,
					minSpread = 0f,
					maxSpread = base.characterBody.spreadBloomAngle,
					bulletCount = 1,
					procCoefficient = 1f,
					damage = 3f * this.damageStat,
					force = 180f,
					falloffModel = BulletAttack.FalloffModel.DefaultBullet,
					tracerEffectPrefab = FireShotgun.tracerEffectPrefab,
					muzzleName = muzzleName,
					hitEffectPrefab = FireLightsOut.hitEffectPrefab,
					isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
					HitEffectNormal = false,
					radius = 1f,
					stopperMask = LayerIndex.world.collisionMask,
				}.Fire();
			}
			base.characterBody.AddSpreadBloom(FireShotgun.spreadBloomValue);
		}

		// Token: 0x06003E82 RID: 16002 RVA: 0x00032FA7 File Offset: 0x000311A7
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x06003E83 RID: 16003 RVA: 0x001055E0 File Offset: 0x001037E0
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.buttonReleased |= !base.inputBank.skill1.down;
			if (base.fixedAge >= this.maxDuration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		// Token: 0x06003E84 RID: 16004 RVA: 0x00105635 File Offset: 0x00103835
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			if (this.buttonReleased && base.fixedAge >= this.minDuration)
			{
				return InterruptPriority.Any;
			}
			return InterruptPriority.Skill;
		}

		// Token: 0x040039DC RID: 14812
		public static GameObject effectPrefab;

		// Token: 0x040039DD RID: 14813
		public static GameObject hitEffectPrefab;

		// Token: 0x040039DE RID: 14814
		public static GameObject tracerEffectPrefab;

		// Token: 0x040039DF RID: 14815
		public static float damageCoefficient;

		// Token: 0x040039E0 RID: 14816
		public static float force;

		// Token: 0x040039E1 RID: 14817
		public static int bulletCount;

		// Token: 0x040039E2 RID: 14818
		public static float baseMaxDuration = 2f;

		// Token: 0x040039E3 RID: 14819
		public static float baseMinDuration = 0.5f;

		// Token: 0x040039E4 RID: 14820
		public static string attackSoundString;

		// Token: 0x040039E5 RID: 14821
		public static float recoilAmplitude;

		// Token: 0x040039E6 RID: 14822
		public static float spreadBloomValue = 0.3f;

		// Token: 0x040039E7 RID: 14823
		private float maxDuration;

		// Token: 0x040039E8 RID: 14824
		private float minDuration;

		// Token: 0x040039E9 RID: 14825
		private bool buttonReleased;
	}
}