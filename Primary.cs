using System;
using EntityStates.ClaymanMonster;
using EntityStates.Merc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Assassin.Weapon;
using EntityStates.RoboBallBoss.Weapon;

namespace EntityStates.Assassin.Weapon1
{
	// Token: 0x0200000A RID: 10
	public class SlashCombo2 : BaseState
	{
		// Token: 0x06000038 RID: 56 RVA: 0x000067E4 File Offset: 0x000049E4
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = 0.6f / this.attackSpeedStat;
			this.modelAnimator = base.GetModelAnimator();
			Transform modelTransform = base.GetModelTransform();
			this.attack = new OverlapAttack();
			this.attack.attacker = base.gameObject;
			this.attack.inflictor = base.gameObject;
			this.attack.teamIndex = TeamComponent.GetObjectTeam(this.attack.attacker);
			this.attack.damage = 2.5f * this.damageStat;
			this.attack.hitEffectPrefab = SlashCombo.hitEffectPrefab;
			this.attack.isCrit = Util.CheckRoll(this.critStat, base.characterBody.master);
			this.animator = base.GetModelAnimator();
			Util.PlaySound(SwipeForward.attackString, base.gameObject);
			string hitboxGroupName = "";
			string animationStateName = "";
			SlashCombo2.SlashComboPermutation slashComboPermutation = this.slashComboPermutation;
			SlashCombo2.SlashComboPermutation slashComboPermutation2 = slashComboPermutation;
			if (slashComboPermutation2 != SlashCombo2.SlashComboPermutation.Slash1)
			{
				if (slashComboPermutation2 == SlashCombo2.SlashComboPermutation.Slash2)
				{
					hitboxGroupName = "DaggerLeft";
					animationStateName = "SlashP2";
				}
			}
			else
			{
				hitboxGroupName = "DaggerLeft";
				animationStateName = "SlashP1";
			}
			bool flag = modelTransform;
			if (flag)
			{
				this.attack.hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxGroupName);
			}
			bool flag2 = this.modelAnimator;
			if (flag2)
			{
				base.PlayAnimation("Gesture, Override", animationStateName, "SlashCombo.playbackRate", this.duration * 0.6f);
				base.PlayAnimation("Gesture, Additive", animationStateName, "SlashCombo.playbackRate", this.duration * 0.6f);
			}
			bool flag3 = base.characterBody;
			if (flag3)
			{
				base.characterBody.SetAimTimer(2f);
			}
			bool flag4 = this.attack.Fire(null);
			if (flag4)
			{
				Util.PlaySound(SwipeForward.attackString, base.gameObject);
				this.hasHopped = (this.hasHopped || flag);
				if (flag4)
				{
					bool flag6 = base.characterMotor && !base.characterMotor.isGrounded;
					if (flag6)
					{
						base.SmallHop(base.characterMotor, 6f);
					}
					if (flag6)
					{

						bool flag8 = base.skillLocator.special.skillDef.skillNameToken == "ASSASSIN_SPECIAL_LUNGE_NAME";
						if (flag8)
						{
							base.skillLocator.special.RunRecharge(1f);
						}
						this.hasHit = true;
						bool flag9 = !this.inHitPause;
						if (flag9)
						{
							this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "SlashCombo.playbackRate");
							this.hitPauseTimer = 2f * GroundLight.hitPauseDuration / this.attackSpeedStat;
							this.inHitPause = true;
						}
					}
				}
			}
		}


		public override void OnExit()
		{
			bool flag1 = this.inHitPause;
			if (flag1)
			{
				base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
				this.inHitPause = false;
			}
		}
		// Token: 0x06000039 RID: 57 RVA: 0x000069B8 File Offset: 0x00004BB8
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			bool flag = NetworkServer.active && this.modelAnimator && this.modelAnimator.GetFloat("SlashCombo.hitBoxActive") > 0.1f;
			if (flag)
			{
				bool flag2 = !this.hasSlashed;
				if (flag2)
				{
					EffectManager.SimpleMuzzleFlash(SlashCombo.swingEffectPrefab, base.gameObject, "SwingCenter", true);
					HealthComponent healthComponent = base.characterBody.healthComponent;
					CharacterDirection component = base.characterBody.GetComponent<CharacterDirection>();
					bool flag3 = healthComponent;
					if (flag3)
					{
						healthComponent.TakeDamageForce(6f * component.forward, true, false);
					}
					this.hasSlashed = true;
				}
				this.attack.forceVector = base.transform.forward * 10f;
				this.attack.Fire(null);
			}
			bool flag4 = base.fixedAge < this.duration || !base.isAuthority;
			if (!flag4)
			{
				bool flag5 = base.inputBank && base.inputBank.skill1.down;
				if (flag5)
				{
					SlashCombo2 slashCombo = new SlashCombo2();
					SlashCombo2.SlashComboPermutation slashComboPermutation = this.slashComboPermutation;
					SlashCombo2.SlashComboPermutation slashComboPermutation2 = slashComboPermutation;
					if (slashComboPermutation2 != SlashCombo2.SlashComboPermutation.Slash1)
					{
						if (slashComboPermutation2 == SlashCombo2.SlashComboPermutation.Slash2)
						{
							slashCombo.slashComboPermutation = SlashCombo2.SlashComboPermutation.Slash1;
						}
					}
					else
					{
						slashCombo.slashComboPermutation = SlashCombo2.SlashComboPermutation.Slash2;
					}
					this.outer.SetNextState(slashCombo);
				}
				else
				{
					this.outer.SetNextStateToMain();
				}
				this.hitPauseTimer -= Time.fixedDeltaTime;
				bool flag6 = this.hitPauseTimer <= 0f && this.inHitPause;
				if (flag6)
				{
					base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
					this.inHitPause = false;
				}
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00006B38 File Offset: 0x00004D38
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		// Token: 0x0400003F RID: 63
		public static float baseDuration = 0.6f;

		// Token: 0x04000040 RID: 64
		public static float damageCoefficient = 2.5f;

		// Token: 0x04000041 RID: 65
		public static float forceMagnitude = 19f;

		// Token: 0x04000042 RID: 66
		public static float selfForceMagnitude = 3f;

		public static float hitHopVelocity = 6f;

		// Token: 0x04000044 RID: 68
		public static GameObject hitEffectPrefab = SwipeForward.hitEffectPrefab;

		// Token: 0x04000045 RID: 69
		public static GameObject swingEffectPrefab = SwipeForward.swingEffectPrefab;

		// Token: 0x04000046 RID: 70
		public static string attackString = SwipeForward.attackString;

		// Token: 0x04000047 RID: 71
		private OverlapAttack attack;

		// Token: 0x04000048 RID: 72
		private Animator modelAnimator;

		// Token: 0x04000049 RID: 73
		private float duration = 0.6f;

		private bool hasHopped;

		// Token: 0x0400004A RID: 74
		private bool hasSlashed;

		// Token: 0x040000BA RID: 186
		private bool inHitPause;

		// Token: 0x040000B8 RID: 184
		private float hitPauseTimer;

		// Token: 0x040000BE RID: 190
		private Animator animator;

		private bool hasHit;

		// Token: 0x040000BF RID: 191
		private BaseState.HitStopCachedState hitStopCachedState;

		// Token: 0x0400004B RID: 75
		public SlashCombo2.SlashComboPermutation slashComboPermutation;

		// Token: 0x0200000E RID: 14
		public enum SlashComboPermutation
		{
			// Token: 0x0400005E RID: 94
			Slash1,
			// Token: 0x0400005F RID: 95
			Slash2,
		}
	}
}