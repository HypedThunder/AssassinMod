using System;
using EntityStates.Commando;
using EntityStates.ClaymanMonster;
using EntityStates.HermitCrab;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Assassin.Weapon;
using EntityStates.RoboBallBoss.Weapon;

namespace EntityStates.Assassin.Weapon4
{
    // Token: 0x02000006 RID: 6
    public class SweepingSlash : BaseState
    {
        // Token: 0x06000017 RID: 23 RVA: 0x00002CA0 File Offset: 0x00000EA0
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlayScaledSound(SwipeForward.attackString, base.gameObject, 0.5f);
            Ray aimRay = base.GetAimRay();
            this.modelTransform = base.GetModelTransform();
            bool flag = this.modelTransform;
            if (flag)
            {
                this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
            }
            bool flag2 = this.characterModel;
            if (flag2)
            {
                this.characterModel.invisibilityCount++;
            }
            this.animator = base.GetModelAnimator();
            ChildLocator component = this.animator.GetComponent<ChildLocator>();
            bool flag3 = base.isAuthority && base.inputBank && base.characterDirection;
            if (flag3)
            {
                this.forwardDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
            }
            Vector3 rhs = base.characterDirection ? base.characterDirection.forward : this.forwardDirection;
            Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
            float value = Vector3.Dot(this.forwardDirection, rhs);
            float value2 = Vector3.Dot(this.forwardDirection, rhs2);
            this.animator.SetFloat("forwardSpeed", value, 0.4f, Time.fixedDeltaTime);
            this.animator.SetFloat("rightSpeed", value2, 0.4f, Time.fixedDeltaTime);
            bool flag4 = SlashCombo.hitEffectPrefab;
            if (flag4)
            {
                Transform exists = base.GetModelTransform();
                bool flag5 = exists;
                if (flag5)
                {
                    EffectManager.SimpleMuzzleFlash(SlashCombo.hitEffectPrefab, base.gameObject, "DaggerLeft", false);
                }
            }
            this.RecalculateSpeed();
            bool flag6 = base.characterMotor && base.characterDirection;
            if (flag6)
            {
                CharacterMotor characterMotor = base.characterMotor;
                characterMotor.velocity.y = characterMotor.velocity.y * 0.2f;
                base.characterMotor.velocity = this.forwardDirection * this.rollSpeed;
            }
            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00002EFB File Offset: 0x000010FB
        private void RecalculateSpeed()
        {
            this.rollSpeed = (2f + 0.5f * this.moveSpeedStat) * Mathf.Lerp(SweepingSlash.initialSpeedCoefficient, SweepingSlash.finalSpeedCoefficient, base.fixedAge / this.duration);
        }

        // Token: 0x06000019 RID: 25 RVA: 0x00002F34 File Offset: 0x00001134
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.RecalculateSpeed();
            bool flag = base.cameraTargetParams;
            if (flag)
            {
                base.cameraTargetParams.fovOverride = Mathf.Lerp(DodgeState.dodgeFOV, 60f, base.fixedAge / this.duration);
            }
            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            bool flag2 = base.characterMotor && base.characterDirection && normalized != Vector3.zero;
            if (flag2)
            {
                Vector3 vector = normalized * this.rollSpeed;
                float y = vector.y;
                vector.y = 0f;
                float d = Mathf.Max(Vector3.Dot(vector, this.forwardDirection), 0f);
                vector = this.forwardDirection * d;
                vector.y += Mathf.Max(y, 0f);
                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;
            bool flag3 = base.fixedAge >= this.duration && base.isAuthority;
            if (flag3)
            {
                this.outer.SetNextStateToMain();
            }
        }

        // Token: 0x0600001A RID: 26 RVA: 0x00003084 File Offset: 0x00001284
        public override void OnExit()
        {
            bool flag = base.cameraTargetParams;
            if (flag)
            {
                base.cameraTargetParams.fovOverride = -1f;
            }
            this.modelTransform = base.GetModelTransform();
            bool flag2 = this.modelTransform;
            if (flag2)
            {
                TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 1.25f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matMercEnergized");
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }
            bool flag3 = this.characterModel;
            if (flag3)
            {
                this.characterModel.invisibilityCount--;
            }
            bool flag4 = FireMortar.mortarMuzzleflashEffect;
            if (flag4)
            {
                Transform exists = base.GetModelTransform();
                bool flag5 = exists;
                if (flag5)
                {
                    EffectManager.SimpleMuzzleFlash(SlashCombo.hitEffectPrefab, base.gameObject, "SwingCenter", false);
                }
            }
            this.blastAttack = new BlastAttack
            {
                attacker = base.gameObject,
                inflictor = base.gameObject,
                teamIndex = base.teamComponent.teamIndex,
                baseForce = 0f,
                bonusForce = Vector3.zero,
                position = base.transform.position,
                radius = 16f,
                falloffModel = BlastAttack.FalloffModel.None,
                crit = base.RollCrit(),
                baseDamage = 8f * this.damageStat,
                procCoefficient = 0.8f,
                impactEffect = SlashCombo.hitEffectPrefab.GetComponent<EffectComponent>().effectIndex
            };

            blastAttack.damageType |= DamageType.CrippleOnHit;
            blastAttack.Fire();
            Util.PlayScaledSound(SwipeForward.attackString, base.gameObject, 0.75f);
            this.blastAttack.teamIndex = TeamComponent.GetObjectTeam(this.blastAttack.attacker);
            base.OnExit();
        }

        // Token: 0x0600001B RID: 27 RVA: 0x0000327D File Offset: 0x0000147D
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.forwardDirection);
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00003295 File Offset: 0x00001495
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.forwardDirection = reader.ReadVector3();
        }

        // Token: 0x0600001D RID: 29 RVA: 0x000032AC File Offset: 0x000014AC
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        // Token: 0x0400001F RID: 31
        [SerializeField]
        public float duration = 0.4f;

        // Token: 0x04000020 RID: 32
        public static GameObject dodgeEffect;

        // Token: 0x04000021 RID: 33
        public static float initialSpeedCoefficient = 12f;

        // Token: 0x04000022 RID: 34
        public static float finalSpeedCoefficient = 0.4f;

        // Token: 0x04000023 RID: 35
        private float rollSpeed;

        // Token: 0x04000024 RID: 36
        private Vector3 forwardDirection;

        // Token: 0x04000025 RID: 37
        private Animator animator;

        // Token: 0x04000026 RID: 38
        private Vector3 previousPosition;

        // Token: 0x04000027 RID: 39
        private Transform modelTransform;

        // Token: 0x04000028 RID: 40
        private CharacterModel characterModel;

        private BlastAttack blastAttack;
    }
}