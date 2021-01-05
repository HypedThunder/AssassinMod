using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Merc;

namespace EntityStates.Assassin5
{
	// Token: 0x02000992 RID: 2450
	public class ComboAttack : BaseState
	{
		// Token: 0x060038D7 RID: 14551 RVA: 0x000E8014 File Offset: 0x000E6214
		public override void OnEnter()
		{
			base.OnEnter();
			this.stopwatch = 0f;
			this.earlyExitDuration = GroundLight.baseEarlyExitDuration / this.attackSpeedStat;
			this.animator = base.GetModelAnimator();
			bool @bool = this.animator.GetBool("isMoving");
			bool bool2 = this.animator.GetBool("isGrounded");
			switch (this.comboState)
			{
				case GroundLight.ComboState.GroundLight1:
					this.attackDuration = GroundLight.baseComboAttackDuration / this.attackSpeedStat;
					this.overlapAttack = base.InitMeleeOverlap(GroundLight.comboDamageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), "DaggerLeft");
					if (@bool || !bool2)
					{
						base.PlayAnimation("Gesture, Additive", "SlashP1", "SlashCombo.playbackRate", this.attackDuration);
						base.PlayAnimation("Gesture, Override", "SlashP1", "SlashCombo.playbackRate", this.attackDuration);
					}
					else
					{
						base.PlayAnimation("Gesture, Additive", "SlashP1", "SlashCombo.playbackRate", this.attackDuration);
						base.PlayAnimation("Gesture, Override", "SlashP1", "SlashCombo.playbackRate", this.attackDuration);
					}
					this.slashChildName = "GroundLight1Slash";
					this.swingEffectPrefab = GroundLight.comboSwingEffectPrefab;
					this.hitEffectPrefab = GroundLight.comboHitEffectPrefab;
					this.attackSoundString = GroundLight.comboAttackSoundString;
					break;
				case GroundLight.ComboState.GroundLight2:
					this.attackDuration = GroundLight.baseComboAttackDuration / this.attackSpeedStat;
					this.overlapAttack = base.InitMeleeOverlap(GroundLight.comboDamageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), "DaggerLeft");
					if (@bool || !bool2)
					{
						base.PlayAnimation("Gesture, Additive", "SlashP1", "SlashCombo.playbackRate", this.attackDuration);
						base.PlayAnimation("Gesture, Override", "SlashP1", "SlashCombo.playbackRate", this.attackDuration);
					}
					else
					{
						base.PlayAnimation("Gesture, Additive", "SlashP1", "SlashCombo.playbackRate", this.attackDuration);
						base.PlayAnimation("Gesture, Override", "SlashP1", "SlashCombo.playbackRate", this.attackDuration);
					}
					this.slashChildName = "GroundLight2Slash";
					this.swingEffectPrefab = GroundLight.comboSwingEffectPrefab;
					this.hitEffectPrefab = GroundLight.comboHitEffectPrefab;
					this.attackSoundString = GroundLight.comboAttackSoundString;
					break;
				case GroundLight.ComboState.GroundLight3:
					this.attackDuration = GroundLight.baseFinisherAttackDuration / this.attackSpeedStat;
					this.overlapAttack = base.InitMeleeOverlap(GroundLight.finisherDamageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), "DaggerLeft");
					if (@bool || !bool2)
					{
						base.PlayAnimation("Gesture, Additive", "SlashP2", "SlashCombo.playbackRate", this.attackDuration);
						base.PlayAnimation("Gesture, Override", "SlashP2", "SlashCombo.playbackRate", this.attackDuration);
					}
					else
					{
						base.PlayAnimation("Gesture, Additive", "SlashP2", "SlashCombo.playbackRate", this.attackDuration);
						base.PlayAnimation("Gesture, Override", "SlashP2", "SlashCombo.playbackRate", this.attackDuration);
					}
					this.slashChildName = "GroundLight3Slash";
					this.swingEffectPrefab = GroundLight.finisherSwingEffectPrefab;
					this.hitEffectPrefab = GroundLight.finisherHitEffectPrefab;
					this.attackSoundString = GroundLight.finisherAttackSoundString;
					break;
			}
			base.characterBody.SetAimTimer(this.attackDuration + 1f);
			this.overlapAttack.hitEffectPrefab = this.hitEffectPrefab;
		}

		// Token: 0x060038D8 RID: 14552 RVA: 0x00032FA7 File Offset: 0x000311A7
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x060038D9 RID: 14553 RVA: 0x000E82F0 File Offset: 0x000E64F0
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.hitPauseTimer -= Time.fixedDeltaTime;
			if (base.isAuthority)
			{
				bool flag = base.FireMeleeOverlap(this.overlapAttack, this.animator, "DaggerLeft", GroundLight.forceMagnitude, true);
				this.hasHit = (this.hasHit || flag);
				if (flag)
				{
					Util.PlaySound(GroundLight.hitSoundString, base.gameObject);
					if (!this.isInHitPause)
					{
						this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "SlashCombo.playbackRate");
						this.hitPauseTimer = GroundLight.hitPauseDuration / this.attackSpeedStat;
						this.isInHitPause = true;
					}
				}
				if (this.hitPauseTimer <= 0f && this.isInHitPause)
				{
					base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
					this.isInHitPause = false;
				}
			}
			if (this.animator.GetFloat("DaggerLeft") > 0f && !this.hasSwung)
			{
				Util.PlayScaledSound(this.attackSoundString, base.gameObject, GroundLight.slashPitch);
				HealthComponent healthComponent = base.characterBody.healthComponent;
				CharacterDirection component = base.characterBody.GetComponent<CharacterDirection>();
				if (healthComponent)
				{
					healthComponent.TakeDamageForce(GroundLight.selfForceMagnitude * component.forward, true, false);
				}
				this.hasSwung = true;
				EffectManager.SimpleMuzzleFlash(this.swingEffectPrefab, base.gameObject, this.slashChildName, false);
			}
			if (!this.isInHitPause)
			{
				this.stopwatch += Time.fixedDeltaTime;
			}
			else
			{
				base.characterMotor.velocity = Vector3.zero;
				this.animator.SetFloat("SlashCombo.playbackRate", 0f);
			}
			if (base.isAuthority && this.stopwatch >= this.attackDuration - this.earlyExitDuration)
			{
				if (!this.hasSwung)
				{
					this.overlapAttack.Fire(null);
				}
				if (base.inputBank.skill1.down && this.comboState != GroundLight.ComboState.GroundLight3)
				{
					GroundLight groundLight = new GroundLight();
					groundLight.comboState = this.comboState + 1;
					this.outer.SetNextState(groundLight);
					return;
				}
				if (this.stopwatch >= this.attackDuration)
				{
					this.outer.SetNextStateToMain();
					return;
				}
			}
		}

		// Token: 0x060038DA RID: 14554 RVA: 0x0000CFF7 File Offset: 0x0000B1F7
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		// Token: 0x060038DB RID: 14555 RVA: 0x000E852B File Offset: 0x000E672B
		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write((byte)this.comboState);
		}

		// Token: 0x060038DC RID: 14556 RVA: 0x000E8541 File Offset: 0x000E6741
		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			this.comboState = (GroundLight.ComboState)reader.ReadByte();
		}

		// Token: 0x04003179 RID: 12665
		public static float baseComboAttackDuration;

		// Token: 0x0400317A RID: 12666
		public static float baseFinisherAttackDuration;

		// Token: 0x0400317B RID: 12667
		public static float baseEarlyExitDuration;

		// Token: 0x0400317C RID: 12668
		public static string comboAttackSoundString;

		// Token: 0x0400317D RID: 12669
		public static string finisherAttackSoundString;

		// Token: 0x0400317E RID: 12670
		public static float comboDamageCoefficient;

		// Token: 0x0400317F RID: 12671
		public static float finisherDamageCoefficient;

		// Token: 0x04003180 RID: 12672
		public static float forceMagnitude;

		// Token: 0x04003181 RID: 12673
		public static GameObject comboHitEffectPrefab;

		// Token: 0x04003182 RID: 12674
		public static GameObject finisherHitEffectPrefab;

		// Token: 0x04003183 RID: 12675
		public static GameObject comboSwingEffectPrefab;

		// Token: 0x04003184 RID: 12676
		public static GameObject finisherSwingEffectPrefab;

		// Token: 0x04003185 RID: 12677
		public static float hitPauseDuration;

		// Token: 0x04003186 RID: 12678
		public static float selfForceMagnitude;

		// Token: 0x04003187 RID: 12679
		public static string hitSoundString;

		// Token: 0x04003188 RID: 12680
		public static float slashPitch;

		// Token: 0x04003189 RID: 12681
		private float stopwatch;

		// Token: 0x0400318A RID: 12682
		private float attackDuration;

		// Token: 0x0400318B RID: 12683
		private float earlyExitDuration;

		// Token: 0x0400318C RID: 12684
		private Animator animator;

		// Token: 0x0400318D RID: 12685
		private OverlapAttack overlapAttack;

		// Token: 0x0400318E RID: 12686
		private float hitPauseTimer;

		// Token: 0x0400318F RID: 12687
		private bool isInHitPause;

		// Token: 0x04003190 RID: 12688
		private bool hasSwung;

		// Token: 0x04003191 RID: 12689
		private bool hasHit;

		// Token: 0x04003192 RID: 12690
		private GameObject swingEffectInstance;

		// Token: 0x04003193 RID: 12691
		public GroundLight.ComboState comboState;

		// Token: 0x04003194 RID: 12692
		private Vector3 characterForward;

		// Token: 0x04003195 RID: 12693
		private string slashChildName;

		// Token: 0x04003196 RID: 12694
		private BaseState.HitStopCachedState hitStopCachedState;

		// Token: 0x04003197 RID: 12695
		private GameObject swingEffectPrefab;

		// Token: 0x04003198 RID: 12696
		private GameObject hitEffectPrefab;

		// Token: 0x04003199 RID: 12697
		private string attackSoundString;

		// Token: 0x02000993 RID: 2451
		public enum ComboState
		{
			// Token: 0x0400319B RID: 12699
			GroundLight1,
			// Token: 0x0400319C RID: 12700
			GroundLight2,
			// Token: 0x0400319D RID: 12701
			GroundLight3
		}

		// Token: 0x02000994 RID: 2452
		private struct ComboStateInfo
		{
			// Token: 0x0400319E RID: 12702
			private string mecanimStateName;

			// Token: 0x0400319F RID: 12703
			private string mecanimPlaybackRateName;
		}
	}
}