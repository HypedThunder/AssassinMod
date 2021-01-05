using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Assassin.Weapon2;
using EntityStates.Assassin.Weapon3;
using EntityStates.Assassin.Weapon4;
using RoR2.Projectile;
using RealAssassin;
using EntityStates.Assassin5;
using EntityStates.Assassin.Weapon5;
using AssassinAssets;
using EntityStates.Assassin.Weapon1;

namespace Assassin
{
    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin(MODUID, "Assassin", "0.1.0")]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(LanguageAPI), nameof(BuffAPI), nameof(EffectAPI))]

    public class AssassinPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Ruxbieno.Assassin";

        internal static AssassinPlugin instance;

        public static GameObject myCharacter;
        public static GameObject characterDisplay;
        public static GameObject doppelganger;

        public static GameObject assassinCrosshair;

        private static readonly Color CHAR_COLOR = new Color(0.5f, 0.1f, 0.5f);
        private static readonly Color HEAL_COLOR = new Color(0.5f, 0.1f, 0.5f);

        private static ConfigEntry<float> baseHealth;
        private static ConfigEntry<float> healthGrowth;
        private static ConfigEntry<float> baseArmor;
        private static ConfigEntry<float> baseDamage;
        private static ConfigEntry<float> damageGrowth;
        private static ConfigEntry<float> baseRegen;
        private static ConfigEntry<float> regenGrowth;
        private static ConfigEntry<float> baseSpeed;

        public void Awake()
        {
            instance = this;

            ReadConfig();
            RegisterStates();
            RegisterCharacter();
            Skins.RegisterSkins();
            CreateMaster();
        }

        private void ReadConfig()
        {
            baseHealth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Health"), 100f, new ConfigDescription("Base health", null, Array.Empty<object>()));
            healthGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Health growth"), 27f, new ConfigDescription("Health per level", null, Array.Empty<object>()));
            baseArmor = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Armor"), 20f, new ConfigDescription("Base armor", null, Array.Empty<object>()));
            baseDamage = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Damage"), 15f, new ConfigDescription("Base damage", null, Array.Empty<object>()));
            damageGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Damage growth"), 2.5f, new ConfigDescription("Damage per level", null, Array.Empty<object>()));
            baseRegen = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Regen"), 1.2f, new ConfigDescription("Base HP regen", null, Array.Empty<object>()));
            regenGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Regen growth"), 0.5f, new ConfigDescription("HP regen per level", null, Array.Empty<object>()));
            baseSpeed = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Speed"), 9f, new ConfigDescription("Base speed", null, Array.Empty<object>()));
        }

        private void RegisterCharacter()
        {
            //create a clone of the grovetender prefab
            myCharacter = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/AssassinBody"), "Prefabs/CharacterBodies/RuxAssassinBody", true);
            //create a display prefab
            characterDisplay = PrefabAPI.InstantiateClone(myCharacter.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "AssassinDisplay", true);

            //add custom menu animation script
            characterDisplay.AddComponent<MenuAnim>();

            var component1 = myCharacter.AddComponent<SetStateOnHurt>();
            component1.canBeHitStunned = false;
            component1.canBeStunned = true;
            component1.canBeFrozen = true;


            CharacterBody charBody = myCharacter.GetComponent<CharacterBody>();
            charBody.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;

            //swap to generic mainstate to fix clunky controls
            myCharacter.GetComponent<EntityStateMachine>().mainStateType = new SerializableEntityStateType(typeof(GenericCharacterMain));

            myCharacter.GetComponentInChildren<Interactor>().maxInteractionDistance = 5f;

            charBody.portraitIcon = Assets.charPortrait;

            //crosshair stuff
            charBody.SetSpreadBloom(0, false);
            charBody.spreadBloomCurve = Resources.Load<GameObject>("Prefabs/CharacterBodies/BanditBody").GetComponent<CharacterBody>().spreadBloomCurve;
            charBody.spreadBloomDecayTime = Resources.Load<GameObject>("Prefabs/CharacterBodies/BanditBody").GetComponent<CharacterBody>().spreadBloomDecayTime;

            charBody.hullClassification = HullClassification.Human;




            characterDisplay.transform.localScale = Vector3.one * 1f;
            characterDisplay.AddComponent<NetworkIdentity>();

            //create the custom crosshair
            assassinCrosshair = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Crosshair/BanditCrosshair"), "AssassinCrosshair", true);
            assassinCrosshair.AddComponent<NetworkIdentity>();

            //networking

            if (myCharacter) PrefabAPI.RegisterNetworkPrefab(myCharacter);
            if (characterDisplay) PrefabAPI.RegisterNetworkPrefab(characterDisplay);
            if (doppelganger) PrefabAPI.RegisterNetworkPrefab(doppelganger);
            if (assassinCrosshair) PrefabAPI.RegisterNetworkPrefab(assassinCrosshair);



            string desc = "Assassin is a swift melee-hybrid that rapidly phases in and out of combat and delivers swift and fatal blows.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Deep Wound is good for dealing with aerial enemies." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Extra stacks of Blade Rain increase the amount of projectiles thrown out at once." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Resupply is a good way of keeping a steady source of damage on targets." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Volatile Scrap is great for clearing out crowds easily.</color>" + Environment.NewLine;

            LanguageAPI.Add("ASSASSIN_NAME", "Assassin");
            LanguageAPI.Add("ASSASSIN_DESCRIPTION", desc);
            LanguageAPI.Add("ASSASSIN_SUBTITLE", "Deadly Droid");
            LanguageAPI.Add("ASSASSIN_OUTRO_FLAVOR", "...and so he left, less than an empty husk.");


            charBody.name = "RuxAssassinBody";
            charBody.baseNameToken = "ASSASSIN_NAME";
            charBody.subtitleNameToken = "ASSASSIN_SUBTITLE";
            charBody.crosshairPrefab = assassinCrosshair;

            charBody.baseMaxHealth = baseHealth.Value;
            charBody.levelMaxHealth = healthGrowth.Value;
            charBody.baseRegen = baseRegen.Value;
            charBody.levelRegen = regenGrowth.Value;
            charBody.baseDamage = baseDamage.Value;
            charBody.levelDamage = damageGrowth.Value;
            charBody.baseArmor = baseArmor.Value;
            charBody.levelArmor = 0;
            charBody.baseCrit = 1;

            charBody.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CharacterBody>().preferredPodPrefab;


            //create a survivordef for our grovetender
            SurvivorDef survivorDef = new SurvivorDef
            {
                name = "ASSASSIN_NAME",
                unlockableName = "",
                descriptionToken = "ASSASSIN_DESCRIPTION",
                primaryColor = CHAR_COLOR,
                bodyPrefab = myCharacter,
                displayPrefab = characterDisplay,
                outroFlavorToken = "ASSASSIN_OUTRO_FLAVOR"
            };


            SurvivorAPI.AddSurvivor(survivorDef);


            SkillSetup();


            //add it to the body catalog
            BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(myCharacter);
            };
        }

        private void RegisterStates()
        {
            LoadoutAPI.AddSkill(typeof(KnifeAttack));
            LoadoutAPI.AddSkill(typeof(ComboAttack));
            LoadoutAPI.AddSkill(typeof(KnifeBurst));
            LoadoutAPI.AddSkill(typeof(BounceKnife));
            LoadoutAPI.AddSkill(typeof(WarpDash));
            LoadoutAPI.AddSkill(typeof(SweepingSlash));
        }

        private void SkillSetup()
        {
            foreach (GenericSkill obj in myCharacter.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }

            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }

        private void PassiveSetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            LanguageAPI.Add("ASSASSIN_PASSIVE_NAME", "Nimble Chassis");
            LanguageAPI.Add("ASSASSIN_PASSIVE_DESCRIPTION", "All of your attacks are <style=cIsUtility>Agile</style>, you gain an <style=cIsUtility>extra jump and speed</style>.");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = "ASSASSIN_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "ASSASSIN_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.iconP;
        }

        private void PrimarySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Swiftly slash into your enemies, dealing <style=cIsDamage>" + KnifeAttack.damageCoefficient * 100f + "% damage</style>. Successful aerial hits <style=cIsUtility>reduce the cooldown of Sweeping Slash</style>.";

            LanguageAPI.Add("ASSASSIN_PRIMARY_CUT_NAME", "Deep Wound");
            LanguageAPI.Add("ASSASSIN_PRIMARY_CUT_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(KnifeAttack));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon1;
            mySkillDef.skillDescriptionToken = "ASSASSIN_PRIMARY_CUT_DESCRIPTION";
            mySkillDef.skillName = "ASSASSIN_PRIMARY_CUT_NAME";
            mySkillDef.skillNameToken = "ASSASSIN_PRIMARY_CUT_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);
            component.primary = myCharacter.AddComponent<GenericSkill>();
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(skillFamily);
            Reflection.SetFieldValue<SkillFamily>(component.primary, "_skillFamily", skillFamily);
            SkillFamily skillFamily2 = component.primary.skillFamily;
            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }


        private void SecondarySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Throw out a burst of 5 <style=cIsUtility>weakening knives</style> that deal <style=cIsDamage>" + KnifeBurst.damageCoefficient * 100f + "% damage</style>.";

            LanguageAPI.Add("ASSASSIN_SECONDARY_KNIFE_NAME", "Blade Rain");
            LanguageAPI.Add("ASSASSIN_SECONDARY_KNIFE_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(KnifeBurst));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 2;
            mySkillDef.baseRechargeInterval = 3f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon2;
            mySkillDef.skillDescriptionToken = "ASSASSIN_SECONDARY_KNIFE_DESCRIPTION";
            mySkillDef.skillName = "ASSASSIN_SECONDARY_KNIFE_NAME";
            mySkillDef.skillNameToken = "ASSASSIN_SECONDARY_KNIFE_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.secondary = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily2 = component.secondary.skillFamily;

            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            LanguageAPI.Add("ASSASSIN_SECONDARY2_BOUNCE_NAME", "Bouncing Knife");
            LanguageAPI.Add("ASSASSIN_SECONDARY2_BOUNCE_DESCRIPTION", "Throw out a bouncing knife, dealing <style=cIsDamage>400% damage</style>.");
            SkillDef skillDef2 = ScriptableObject.CreateInstance<SkillDef>();
            skillDef2.activationState = new SerializableEntityStateType(typeof(BounceKnife));
            skillDef2.activationStateMachineName = "Weapon";
            skillDef2.baseMaxStock = 1;
            skillDef2.baseRechargeInterval = 4f;
            skillDef2.beginSkillCooldownOnSkillEnd = true;
            skillDef2.canceledFromSprinting = false;
            skillDef2.noSprint = false;
            skillDef2.fullRestockOnAssign = true;
            skillDef2.interruptPriority = InterruptPriority.Any;
            skillDef2.isBullets = true;
            skillDef2.isCombatSkill = true;
            skillDef2.mustKeyPress = true;
            skillDef2.rechargeStock = 1;
            skillDef2.requiredStock = 1;
            skillDef2.shootDelay = 0f;
            skillDef2.stockToConsume = 1;
            skillDef2.icon = Assets.icon2;
            skillDef2.skillDescriptionToken = "ASSASSIN_SECONDARY2_BOUNCE_DESCRIPTION";
            skillDef2.skillName = "ASSASSIN_SECONDARY2_BOUNCE_NAME";
            skillDef2.skillNameToken = "ASSASSIN_SECONDARY2_BOUNCE_NAME";
            LoadoutAPI.AddSkillDef(skillDef2);
            Array.Resize<SkillFamily.Variant>(ref skillFamily2.variants, skillFamily2.variants.Length + 1);
            skillFamily2.variants[skillFamily2.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef2,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef2.skillNameToken, false, null)
            };
        }


        private void UtilitySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "<style=cIsUtility>Warp</style> in the direction you are facing. Each use <style=cIsUtility>refreshes a stock of Blade Rain</style>.";

            LanguageAPI.Add("ASSASSIN_UTILITY_WARP_NAME", "Resupply");
            LanguageAPI.Add("ASSASSIN_UTILITY_WARP_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(WarpDash));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseRechargeInterval = 6;
            mySkillDef.baseMaxStock = 2;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon3;
            mySkillDef.skillDescriptionToken = "ASSASSIN_UTILITY_WARP_DESCRIPTION";
            mySkillDef.skillName = "ASSASSIN_UTILITY_WARP_NAME";
            mySkillDef.skillNameToken = "ASSASSIN_UTILITY_WARP_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.utility = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void SpecialSetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "<style=cIsUtility>Warp</style> in the direction you’re moving and deliver a radial slash, dealing <style=cIsDamage>800% damage</style> in an area around you and slowing enemies hit.";

            LanguageAPI.Add("ASSASSIN_SPECIAL_LUNGE_NAME", "Sweeping Slash");
            LanguageAPI.Add("ASSASSIN_SPECIAL_LUNGE_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(SweepingSlash));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 7;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon4;
            mySkillDef.skillDescriptionToken = "ASSASSIN_SPECIAL_LUNGE_DESCRIPTION";
            mySkillDef.skillName = "ASSASSIN_SPECIAL_LUNGE_NAME";
            mySkillDef.skillNameToken = "ASSASSIN_SPECIAL_LUNGE_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.special = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily2 = component.special.skillFamily;

            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void CreateMaster()
        {
            //create the doppelganger, uses commando ai bc i can't be bothered writing my own
            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), "AssassinMonsterMaster", true);

            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(doppelganger);
            };

            CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
            component.bodyPrefab = myCharacter;
        }

        public class MenuAnim : MonoBehaviour
        {
            //animates him in character select
            internal void OnEnable()
            {
                bool flag = base.gameObject.transform.parent.gameObject.name == "CharacterPad";
                if (flag)
                {
                    base.StartCoroutine(this.SpawnAnim());
                }
            }

            private IEnumerator SpawnAnim()
            {
                Animator animator = base.GetComponentInChildren<Animator>();
                Transform effectTransform = base.gameObject.transform;

                ChildLocator component = base.gameObject.GetComponentInChildren<ChildLocator>();

                if (component) effectTransform = component.FindChild("Root");

                GameObject.Instantiate<GameObject>(EntityStates.HermitCrab.SpawnState.burrowPrefab, effectTransform.position, Quaternion.identity);


                PlayAnimation("Body", "Spawn", "Spawn.playbackRate", 3, animator);

                yield break;
            }


            private void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration, Animator animator)
            {
                int layerIndex = animator.GetLayerIndex(layerName);
                animator.SetFloat(playbackRateParam, 1f);
                animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
                animator.Update(0f);
                float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
                animator.SetFloat(playbackRateParam, length / duration);
            }
        }
    }
}