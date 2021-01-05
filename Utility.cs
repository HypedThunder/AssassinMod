using EntityStates.Merc;
using RoR2;
using UnityEngine;
using EntityStates.Huntress;

namespace EntityStates.Assassin.Weapon3
{
	// Token: 0x0200000B RID: 11
	public class WarpDash : BaseState
	{
		// Token: 0x06000048 RID: 72 RVA: 0x00005E40 File Offset: 0x00004040
		public override void OnEnter()
		{
			base.OnEnter();
			Util.PlaySound(BackflipState.dodgeSoundString, base.gameObject);
			this.modelTransform = base.GetModelTransform();
			bool flag = this.modelTransform;
			if (flag)
			{
				this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
				this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
			}
			bool flag2 = this.characterModel;
			if (flag2)
			{
				this.characterModel.invisibilityCount++;
			}
			bool flag3 = this.hurtboxGroup;
			if (flag3)
			{
				HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
				int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
				hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			this.blinkVector = this.GetBlinkVector();
			this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
			bool flag4 = base.skillLocator.secondary.skillDef.skillNameToken == "ASSASSIN_SECONDARY_KNIFE_NAME";
			if (flag4)
			{
				base.skillLocator.secondary.RunRecharge(3f);
			}
			bool flag5 = base.skillLocator.secondary.skillDef.skillNameToken == "ASSASSIN_SECONDARY2_BOUNCE_NAME";
			if (flag5)
			{
				base.skillLocator.primary.RunRecharge(4f);
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00005F7C File Offset: 0x0000417C
		protected virtual Vector3 GetBlinkVector()
		{
			return base.inputBank.aimDirection;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00005F9C File Offset: 0x0000419C
		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(this.blinkVector);
			effectData.origin = origin;
			EffectManager.SpawnEffect(GroundLight.comboHitEffectPrefab, effectData, false);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00005FD8 File Offset: 0x000041D8
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatch += Time.fixedDeltaTime;
			bool flag = base.characterMotor && base.characterDirection;
			if (flag)
			{
				base.characterMotor.velocity = Vector3.zero;
				base.characterMotor.rootMotion += this.blinkVector * (this.moveSpeedStat * this.speedCoefficient * Time.fixedDeltaTime);
			}
			bool flag2 = this.stopwatch >= this.duration && base.isAuthority;
			if (flag2)
			{
				this.outer.SetNextStateToMain();
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00006094 File Offset: 0x00004294
		public override void OnExit()
		{
			bool flag = !this.outer.destroying;
			if (flag)
			{
				Util.PlaySound(BackflipState.dodgeSoundString, base.gameObject);
				this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
				this.modelTransform = base.GetModelTransform();
				bool flag2 = this.modelTransform;
				if (flag2)
				{
					TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
					temporaryOverlay.duration = 0.6f;
					temporaryOverlay.animateShaderAlpha = true;
					temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlay.destroyComponentOnEnd = true;
					temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matMercEnergized");
					temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
				}
			}
			bool flag3 = this.characterModel;
			if (flag3)
			{
				this.characterModel.invisibilityCount--;
			}
			bool flag4 = this.hurtboxGroup;
			if (flag4)
			{
				HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
				int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
				hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			bool isAuthority = base.isAuthority;
			if (isAuthority)
			{
				base.characterBody.isSprinting = true;
			}
			base.OnExit();
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000062B4 File Offset: 0x000044B4
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Frozen;
		}

		// Token: 0x04000071 RID: 113
		private Transform modelTransform;

		// Token: 0x04000072 RID: 114
		public static GameObject blinkPrefab;

		// Token: 0x04000073 RID: 115
		private float stopwatch;

		// Token: 0x04000074 RID: 116
		private Vector3 blinkVector = Vector3.zero;

		// Token: 0x04000069 SEX: YA MOM
		public static float projectileDamageCoefficient = 4f;

		// Token: 0x04000075 RID: 117
		[SerializeField]
		public float duration = 0.2f;

		// Token: 0x04000076 RID: 118
		[SerializeField]
		public float speedCoefficient = 10f;

		// Token: 0x04000077 RID: 119
		[SerializeField]
		public string beginSoundString;

		// Token: 0x04000078 RID: 120
		[SerializeField]
		public string endSoundString;

		// Token: 0x04000079 RID: 121
		private CharacterModel characterModel;

		// Token: 0x0400007A RID: 122
		private HurtBoxGroup hurtboxGroup;
	}
}