using System.IO;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Simulation.SMath;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Reflection;
using Il2CppSystem.Runtime.Serialization;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace UltimateCrosspathing.Loaders
{
	public class MonkeyVillageLoader : ModByteLoader<TowerModel> {
	
		BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static; 
		BinaryReader br = null;
	
		// NOTE: was a collection per type but it prevented inheriance e.g list of Products would required class type id
		protected override string BytesFileName => "MonkeyVillages.bytes";
		object[] m;
		int mIndex = 1; // first element is null
		#region Read array
	
		private void LinkArray<T>() where T : Il2CppObjectBase {
			var setCount = br.ReadInt32();
			for (var i = 0; i < setCount; i++) {
				var arrIndex = br.ReadInt32();
				var arr = (Il2CppReferenceArray<T>)m[arrIndex];
				for (var j = 0; j < arr.Length; j++) {
					arr[j] = (T) m[br.ReadInt32()];
				}
			}
		}
		private void LinkList<T>() where T : Il2CppObjectBase {
			var setCount = br.ReadInt32();
			for (var i = 0; i < setCount; i++) {
				var arrIndex = br.ReadInt32();
				var arr = (List<T>)m[arrIndex];
				for (var j = 0; j < arr.Capacity; j++) {
					arr.Add( (T) m[br.ReadInt32()] );
				}
			}
		}
		private void LinkDictionary<T>() where T : Il2CppObjectBase {
			var setCount = br.ReadInt32();
			for (var i = 0; i < setCount; i++) {
				var arrIndex = br.ReadInt32();
				var arr = (Dictionary<string, T>)m[arrIndex];
				var arrCount = br.ReadInt32();
				for (var j = 0; j < arrCount; j++) {
					var key = br.ReadString();
					var valueIndex = br.ReadInt32();
					arr[key] = (T) m[valueIndex];
				}
			}
		}
		private void LinkModelDictionary<T>() where T : Assets.Scripts.Models.Model {
			var setCount = br.ReadInt32();
			for (var i = 0; i < setCount; i++) {
				var arrIndex = br.ReadInt32();
				var arr = (Dictionary<string, T>)m[arrIndex];
				var arrCount = br.ReadInt32();
				for (var j = 0; j < arrCount; j++) {
					var valueIndex = br.ReadInt32();
					var obj = (T)m[valueIndex];
					arr[obj.name] = obj;
				}
			}
		}
		private void Read_a_Int32_Array() {
			var arrSetCount = br.ReadInt32();
			var count = arrSetCount;
			for (var i = 0; i < count; i++) {
				var arrCount = br.ReadInt32();
				var arr = new Il2CppStructArray<int>(arrCount);
				for (var j = 0; j < arr.Length; j++) {
					arr[j] = br.ReadInt32();
				}
				m[mIndex++] = arr;
			}
		}
		private void Read_a_Single_Array() {
			var arrSetCount = br.ReadInt32();
			var count = arrSetCount;
			for (var i = 0; i < count; i++) {
				var arrCount = br.ReadInt32();
				var arr = new Il2CppStructArray<float>(arrCount);
				for (var j = 0; j < arr.Length; j++) {
					arr[j] = br.ReadSingle();
				}
				m[mIndex++] = arr;
			}
		}
		private void Read_a_String_Array() {
			var arrSetCount = br.ReadInt32();
			var count = arrSetCount;
			for (var i = 0; i < count; i++) {
				var arrCount = br.ReadInt32();
				var arr = new Il2CppStringArray(arrCount);
				for (var j = 0; j < arr.Length; j++) {
					arr[j] = br.ReadBoolean() ? null : br.ReadString();
				}
				m[mIndex++] = arr;
			}
		}
		private void Read_a_TargetType_Array() {
			var arrSetCount = br.ReadInt32();
			var count = arrSetCount;
			for (var i = 0; i < count; i++) {
				var arrCount = br.ReadInt32();
				var arr = new Il2CppReferenceArray<Assets.Scripts.Models.Towers.TargetType>(arrCount);
				for (var j = 0; j < arr.Length; j++) {
					arr[j] = new Assets.Scripts.Models.Towers.TargetType {id = br.ReadString(), isActionable = br.ReadBoolean()};
				}
				m[mIndex++] = arr;
			}
		}
		private void Read_a_AreaType_Array() {
			var arrSetCount = br.ReadInt32();
			var count = arrSetCount;
			for (var i = 0; i < count; i++) {
				var arrCount = br.ReadInt32();
				var arr = new Il2CppStructArray<Assets.Scripts.Models.Map.AreaType>(arrCount);
				for (var j = 0; j < arr.Length; j++) {
					arr[j] = (Assets.Scripts.Models.Map.AreaType)br.ReadInt32();
				}
				m[mIndex++] = arr;
			}
		}
		#endregion
	
		#region Read object records
	
		private void CreateArraySet<T>() where T : Il2CppObjectBase {
			var arrCount = br.ReadInt32();
			for(var i = 0; i < arrCount; i++) {
				m[mIndex++] = new Il2CppReferenceArray<T>(br.ReadInt32());;
			}
		}
	
		private void CreateListSet<T>() where T : Il2CppObjectBase {
			var arrCount = br.ReadInt32();
			for (var i = 0; i < arrCount; i++) {
				m[mIndex++] = new List<T>(br.ReadInt32()); // set capactity
			}
		}
	
		private void CreateDictionarySet<K, T>() {
			var arrCount = br.ReadInt32();
			for (var i = 0; i < arrCount; i++) {
				m[mIndex++] = new Dictionary<K, T>(br.ReadInt32());// set capactity
			}
		}
	
		private void Create_Records<T>() where T : Il2CppObjectBase {
			var count = br.ReadInt32();
			var t = Il2CppType.Of<T>();
			for (var i = 0; i < count; i++) {
				m[mIndex++] = FormatterServices.GetUninitializedObject(t).Cast<T>();
			}
		}
		#endregion
	
		#region Link object records
	
		private void Set_v_Model_Fields(int start, int count) {
			var t = Il2CppType.Of<Assets.Scripts.Models.Model>();
			var _nameField = t.GetField("_name", bindFlags);
			var childDependantsField = t.GetField("childDependants", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Model)m[i+start];
				_nameField.SetValue(v,br.ReadBoolean() ? null : String.Intern(br.ReadString()));
				childDependantsField.SetValue(v,(List<Assets.Scripts.Models.Model>) m[br.ReadInt32()]);
			}
		}
	
		private void Set_v_TowerModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.TowerModel>();
			var towerSizeField = t.GetField("towerSize", bindFlags);
			var cachedThrowMarkerHeightField = t.GetField("cachedThrowMarkerHeight", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.TowerModel)m[i+start];
				v.display = br.ReadBoolean() ? null : br.ReadString();
				v.baseId = br.ReadBoolean() ? null : br.ReadString();
				v.cost = br.ReadSingle();
				v.radius = br.ReadSingle();
				v.radiusSquared = br.ReadSingle();
				v.range = br.ReadSingle();
				v.ignoreBlockers = br.ReadBoolean();
				v.isGlobalRange = br.ReadBoolean();
				v.tier = br.ReadInt32();
				v.tiers = (Il2CppStructArray<int>) m[br.ReadInt32()];
				v.towerSet = br.ReadBoolean() ? null : br.ReadString();
				v.areaTypes = (Il2CppStructArray<Assets.Scripts.Models.Map.AreaType>) m[br.ReadInt32()];
				v.icon = (Assets.Scripts.Utils.SpriteReference) m[br.ReadInt32()];
				v.portrait = (Assets.Scripts.Utils.SpriteReference) m[br.ReadInt32()];
				v.instaIcon = (Assets.Scripts.Utils.SpriteReference) m[br.ReadInt32()];
				v.mods = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Mods.ApplyModModel>) m[br.ReadInt32()];
				v.ignoreTowerForSelection = br.ReadBoolean();
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Model>) m[br.ReadInt32()];
				v.footprint = (Assets.Scripts.Models.Towers.Behaviors.FootprintModel) m[br.ReadInt32()];
				v.dontDisplayUpgrades = br.ReadBoolean();
				v.emoteSpriteSmall = (Assets.Scripts.Utils.SpriteReference) m[br.ReadInt32()];
				v.emoteSpriteLarge = (Assets.Scripts.Utils.SpriteReference) m[br.ReadInt32()];
				v.doesntRotate = br.ReadBoolean();
				v.upgrades = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>) m[br.ReadInt32()];
				v.appliedUpgrades = (Il2CppStringArray) m[br.ReadInt32()];
				v.targetTypes = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.TargetType>) m[br.ReadInt32()];
				v.paragonUpgrade = (Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel) m[br.ReadInt32()];
				v.isSubTower = br.ReadBoolean();
				v.isBakable = br.ReadBoolean();
				v.powerName = br.ReadBoolean() ? null : br.ReadString();
				v.showPowerTowerBuffs = br.ReadBoolean();
				v.animationSpeed = br.ReadSingle();
				v.towerSelectionMenuThemeId = br.ReadBoolean() ? null : br.ReadString();
				v.ignoreCoopAreas = br.ReadBoolean();
				v.canAlwaysBeSold = br.ReadBoolean();
				v.isParagon = br.ReadBoolean();
				v.sellbackModifierAdd = br.ReadSingle();
				towerSizeField.SetValue(v,br.ReadInt32().ToIl2Cpp());
				cachedThrowMarkerHeightField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			}
		}
	
		private void Set_ar_Sprite_Fields(int start, int count) {
			Set_v_AssetReference_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Utils.AssetReference<UnityEngine.Sprite>)m[i+start];
			}
		}
	
		private void Set_v_AssetReference_Fields(int start, int count) {
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Utils.AssetReference)m[i+start];
			}
		}
	
		private void Set_v_SpriteReference_Fields(int start, int count) {
			Set_ar_Sprite_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Utils.SpriteReference>();
			var guidRefField = t.GetField("guidRef", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Utils.SpriteReference)m[i+start];
				guidRefField.SetValue(v,br.ReadBoolean() ? null : br.ReadString());
			}
		}
	
		private void Set_v_ApplyModModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Mods.ApplyModModel)m[i+start];
				v.mod = br.ReadBoolean() ? null : br.ReadString();
				v.target = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_TowerBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.TowerBehaviorModel)m[i+start];
			}
		}
	
		private void Set_v_CreateSoundOnTowerPlaceModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnTowerPlaceModel)m[i+start];
				v.sound1 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.heroSound1 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.heroSound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_SoundModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Audio.SoundModel)m[i+start];
				v.assetId = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_BlankSoundModel_Fields(int start, int count) {
			Set_v_SoundModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Audio.BlankSoundModel)m[i+start];
			}
		}
	
		private void Set_v_CreateSoundOnUpgradeModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnUpgradeModel)m[i+start];
				v.sound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound1 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound3 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound4 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound5 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound6 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound7 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound8 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_CreateSoundOnSellModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnSellModel)m[i+start];
				v.sound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_CreateEffectOnUpgradeModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnUpgradeModel)m[i+start];
				v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_EffectModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Effects.EffectModel)m[i+start];
				v.assetId = br.ReadBoolean() ? null : br.ReadString();
				v.scale = br.ReadSingle();
				v.lifespan = br.ReadSingle();
				v.fullscreen = br.ReadBoolean();
				v.useCenterPosition = br.ReadBoolean();
				v.useTransformPosition = br.ReadBoolean();
				v.useTransfromRotation = br.ReadBoolean();
				v.destroyOnTransformDestroy = br.ReadBoolean();
				v.alwaysUseAge = br.ReadBoolean();
				v.useRoundTime = br.ReadBoolean();
			}
		}
	
		private void Set_v_CreateEffectOnPlaceModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnPlaceModel)m[i+start];
				v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_CreateEffectOnSellModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnSellModel)m[i+start];
				v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_AttackModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel)m[i+start];
				v.weapons = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Weapons.WeaponModel>) m[br.ReadInt32()];
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Model>) m[br.ReadInt32()];
				v.range = br.ReadSingle();
				v.targetProvider = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetSupplierModel) m[br.ReadInt32()];
				v.offsetX = br.ReadSingle();
				v.offsetY = br.ReadSingle();
				v.offsetZ = br.ReadSingle();
				v.attackThroughWalls = br.ReadBoolean();
				v.fireWithoutTarget = br.ReadBoolean();
				v.framesBeforeRetarget = br.ReadInt32();
				v.addsToSharedGrid = br.ReadBoolean();
				v.sharedGridRange = br.ReadSingle();
			}
		}
	
		private void Set_v_AttackBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.AttackBehaviorModel)m[i+start];
			}
		}
	
		private void Set_v_AttackFilterModel_Fields(int start, int count) {
			Set_v_AttackBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.AttackFilterModel)m[i+start];
				v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Filters.FilterModel>) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_FilterModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Filters.FilterModel)m[i+start];
			}
		}
	
		private void Set_v_FilterInvisibleModel_Fields(int start, int count) {
			Set_v_FilterModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Filters.FilterInvisibleModel)m[i+start];
				v.isActive = br.ReadBoolean();
				v.ignoreBroadPhase = br.ReadBoolean();
			}
		}
	
		private void Set_v_WeaponModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
			var rateField = t.GetField("rate", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.WeaponModel)m[i+start];
				v.animation = br.ReadInt32();
				v.animationOffset = br.ReadSingle();
				v.animationOffsetFrames = br.ReadInt32();
				v.emission = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionModel) m[br.ReadInt32()];
				v.ejectX = br.ReadSingle();
				v.ejectY = br.ReadSingle();
				v.ejectZ = br.ReadSingle();
				v.projectile = (Assets.Scripts.Models.Towers.Projectiles.ProjectileModel) m[br.ReadInt32()];
				v.rateFrames = br.ReadInt32();
				v.fireWithoutTarget = br.ReadBoolean();
				v.fireBetweenRounds = br.ReadBoolean();
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel>) m[br.ReadInt32()];
				rateField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.useAttackPosition = br.ReadBoolean();
				v.startInCooldown = br.ReadBoolean();
				v.customStartCooldown = br.ReadSingle();
				v.customStartCooldownFrames = br.ReadInt32();
				v.animateOnMainAttack = br.ReadBoolean();
			}
		}
	
		private void Set_v_EmissionModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionModel)m[i+start];
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionBehaviorModel>) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_SingleEmmisionTowardsTargetModel_Fields(int start, int count) {
			Set_v_EmissionModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.SingleEmmisionTowardsTargetModel)m[i+start];
				v.offset = br.ReadSingle();
			}
		}
	
		private void Set_v_ProjectileModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.ProjectileModel)m[i+start];
				v.display = br.ReadBoolean() ? null : br.ReadString();
				v.id = br.ReadBoolean() ? null : br.ReadString();
				v.maxPierce = br.ReadSingle();
				v.pierce = br.ReadSingle();
				v.scale = br.ReadSingle();
				v.ignoreBlockers = br.ReadBoolean();
				v.usePointCollisionWithBloons = br.ReadBoolean();
				v.canCollisionBeBlockedByMapLos = br.ReadBoolean();
				v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Filters.FilterModel>) m[br.ReadInt32()];
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Model>) m[br.ReadInt32()];
				v.collisionPasses = (Il2CppStructArray<int>) m[br.ReadInt32()];
				v.canCollideWithBloons = br.ReadBoolean();
				v.radius = br.ReadSingle();
				v.vsBlockerRadius = br.ReadSingle();
				v.hasDamageModifiers = br.ReadBoolean();
				v.dontUseCollisionChecker = br.ReadBoolean();
				v.checkCollisionFrames = br.ReadInt32();
				v.ignoreNonTargetable = br.ReadBoolean();
				v.ignorePierceExhaustion = br.ReadBoolean();
				v.saveId = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_ProjectileBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.ProjectileBehaviorModel)m[i+start];
				v.collisionPass = br.ReadInt32();
			}
		}
	
		private void Set_v_DamageModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModel)m[i+start];
				v.damage = br.ReadSingle();
				v.maxDamage = br.ReadSingle();
				v.distributeToChildren = br.ReadBoolean();
				v.overrideDistributeBlocker = br.ReadBoolean();
				v.createPopEffect = br.ReadBoolean();
				v.immuneBloonProperties = (BloonProperties) (br.ReadInt32());
			}
		}
	
		private void Set_v_RetargetOnContactModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.RetargetOnContactModel>();
			var delayField = t.GetField("delay", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.RetargetOnContactModel)m[i+start];
				v.distance = br.ReadSingle();
				v.maxBounces = br.ReadInt32();
				delayField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.delayFrames = br.ReadInt32();
				v.targetType.id = br.ReadString();
				v.targetType.actionOnCreate = br.ReadBoolean();
				v.expireIfNoTargetFound = br.ReadBoolean();
			}
		}
	
		private void Set_v_TravelStraitModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelStraitModel>();
			var lifespanField = t.GetField("lifespan", bindFlags);
			var speedField = t.GetField("speed", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelStraitModel)m[i+start];
				lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.lifespanFrames = br.ReadInt32();
				speedField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.speedFrames = br.ReadSingle();
			}
		}
	
		private void Set_v_DamageModifierModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.DamageModifierModel)m[i+start];
			}
		}
	
		private void Set_v_DamageModifierForTagModel_Fields(int start, int count) {
			Set_v_DamageModifierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModifierForTagModel)m[i+start];
				v.tag = br.ReadBoolean() ? null : br.ReadString();
				v.tags = (Il2CppStringArray) m[br.ReadInt32()];
				v.damageMultiplier = br.ReadSingle();
				v.damageAddative = br.ReadSingle();
				v.mustIncludeAllTags = br.ReadBoolean();
				v.applyOverMaxDamage = br.ReadBoolean();
			}
		}
	
		private void Set_v_CollideExtraPierceReductionModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CollideExtraPierceReductionModel)m[i+start];
				v.bloonTag = br.ReadBoolean() ? null : br.ReadString();
				v.extraAmount = br.ReadInt32();
				v.destroyProjectileIfPierceNotEnough = br.ReadBoolean();
			}
		}
	
		private void Set_v_DisplayModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.GenericBehaviors.DisplayModel)m[i+start];
				v.display = br.ReadBoolean() ? null : br.ReadString();
				v.layer = br.ReadInt32();
				v.positionOffset = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				v.scale = br.ReadSingle();
				v.ignoreRotation = br.ReadBoolean();
				v.animationChanges = (List<Assets.Scripts.Models.GenericBehaviors.AnimationChange>) m[br.ReadInt32()];
				v.delayedReveal = br.ReadSingle();
			}
		}
	
		private void Set_v_TargetSupplierModel_Fields(int start, int count) {
			Set_v_AttackBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetSupplierModel)m[i+start];
				v.isOnSubTower = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetLastModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLastModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetCloseModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetCloseModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetStrongModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetFirstModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_RotateToTargetModel_Fields(int start, int count) {
			Set_v_AttackBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RotateToTargetModel)m[i+start];
				v.onlyRotateDuringThrow = br.ReadBoolean();
				v.useThrowMarkerHeight = br.ReadBoolean();
				v.rotateOnlyOnThrow = br.ReadBoolean();
				v.additionalRotation = br.ReadInt32();
				v.rotateTower = br.ReadBoolean();
				v.useMainAttackRotation = br.ReadBoolean();
			}
		}
	
		private void Set_v_SupportModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.SupportModel)m[i+start];
				v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.TowerFilters.TowerFilterModel>) m[br.ReadInt32()];
				v.isGlobal = br.ReadBoolean();
				v.isCustomRadius = br.ReadBoolean();
				v.customRadius = br.ReadSingle();
				v.appliesToOwningTower = br.ReadBoolean();
				v.showBuffIcon = br.ReadBoolean();
				v.buffLocsName = br.ReadBoolean() ? null : br.ReadString();
				v.buffIconName = br.ReadBoolean() ? null : br.ReadString();
				v.maxStackSize = br.ReadInt32();
				v.onlyShowBuffIfMutated = br.ReadBoolean();
			}
		}
	
		private void Set_v_RangeSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.RangeSupportModel)m[i+start];
				v.multiplier = br.ReadSingle();
				v.additive = br.ReadSingle();
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
				v.isUnique = br.ReadBoolean();
			}
		}
	
		private void Set_v_BuffIndicatorModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.GenericBehaviors.BuffIndicatorModel>();
			var _fullNameField = t.GetField("_fullName", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.GenericBehaviors.BuffIndicatorModel)m[i+start];
				v.buffName = br.ReadBoolean() ? null : br.ReadString();
				v.iconName = br.ReadBoolean() ? null : br.ReadString();
				v.stackable = br.ReadBoolean();
				v.maxStackSize = br.ReadInt32();
				v.globalRange = br.ReadBoolean();
				v.onlyShowBuffIfMutated = br.ReadBoolean();
				_fullNameField.SetValue(v,br.ReadBoolean() ? null : br.ReadString());
			}
		}
	
		private void Set_v_RateSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.RateSupportModel)m[i+start];
				v.multiplier = br.ReadSingle();
				v.isUnique = br.ReadBoolean();
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
				v.priority = br.ReadInt32();
			}
		}
	
		private void Set_v_VisibilitySupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.VisibilitySupportModel)m[i+start];
				v.isUnique = br.ReadBoolean();
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_AddBehaviorToBloonInZoneModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.AddBehaviorToBloonInZoneModel)m[i+start];
				v.zoneRadius = br.ReadSingle();
				v.mutationId = br.ReadBoolean() ? null : br.ReadString();
				v.isUnique = br.ReadBoolean();
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Bloons.BloonBehaviorModel>) m[br.ReadInt32()];
				v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Filters.FilterModel>) m[br.ReadInt32()];
				v.overlays = (Dictionary<System.String, Assets.Scripts.Models.Effects.AssetPathModel>) m[br.ReadInt32()];
				v.overlayLayer = br.ReadInt32();
			}
		}
	
		private void Set_v_BloonBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Bloons.BloonBehaviorModel)m[i+start];
			}
		}
	
		private void Set_v_GrowBlockModel_Fields(int start, int count) {
			Set_v_BloonBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Bloons.Behaviors.GrowBlockModel)m[i+start];
			}
		}
	
		private void Set_v_FilterWithTagModel_Fields(int start, int count) {
			Set_v_FilterModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Filters.FilterWithTagModel)m[i+start];
				v.moabTag = br.ReadBoolean();
				v.camoTag = br.ReadBoolean();
				v.growTag = br.ReadBoolean();
				v.fortifiedTag = br.ReadBoolean();
				v.tag = br.ReadBoolean() ? null : br.ReadString();
				v.inclusive = br.ReadBoolean();
			}
		}
	
		private void Set_v_AssetPathModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Effects.AssetPathModel)m[i+start];
				v.assetPath = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_PierceSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.PierceSupportModel)m[i+start];
				v.pierce = br.ReadSingle();
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
				v.isUnique = br.ReadBoolean();
			}
		}
	
		private void Set_v_TowerFilterModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.TowerFilters.TowerFilterModel)m[i+start];
			}
		}
	
		private void Set_v_FilterInSetModel_Fields(int start, int count) {
			Set_v_TowerFilterModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.TowerFilters.FilterInSetModel)m[i+start];
				v.towerSets = (Il2CppStringArray) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_ProjectileSpeedSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.ProjectileSpeedSupportModel)m[i+start];
				v.multiplier = br.ReadSingle();
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
				v.isUnique = br.ReadBoolean();
			}
		}
	
		private void Set_v_FreeUpgradeSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.FreeUpgradeSupportModel)m[i+start];
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
				v.upgrade = br.ReadInt32();
			}
		}
	
		private void Set_v_AbilityCooldownScaleSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.AbilityCooldownScaleSupportModel)m[i+start];
				v.isUnique = br.ReadBoolean();
				v.abilityCooldownSpeedScale = br.ReadSingle();
				v.affectsOnlyWater = br.ReadBoolean();
				v.mutatorPriority = br.ReadInt32();
			}
		}
	
		private void Set_v_DamageTypeSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.DamageTypeSupportModel)m[i+start];
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
				v.immuneBloonProperties = (BloonProperties) (br.ReadInt32());
				v.isUnique = br.ReadBoolean();
			}
		}
	
		private void Set_v_SupportRemoveFilterOutTagModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.SupportRemoveFilterOutTagModel)m[i+start];
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
				v.removeScriptWithSupportMutatorId = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_AbilityModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityModel>();
			var cooldownSpeedScaleField = t.GetField("cooldownSpeedScale", bindFlags);
			var animationOffsetField = t.GetField("animationOffset", bindFlags);
			var cooldownField = t.GetField("cooldown", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityModel)m[i+start];
				v.displayName = br.ReadBoolean() ? null : br.ReadString();
				v.description = br.ReadBoolean() ? null : br.ReadString();
				v.icon = (Assets.Scripts.Utils.SpriteReference) m[br.ReadInt32()];
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Model>) m[br.ReadInt32()];
				v.activateOnPreLeak = br.ReadBoolean();
				v.activateOnLeak = br.ReadBoolean();
				v.addedViaUpgrade = br.ReadBoolean() ? null : br.ReadString();
				v.cooldownFrames = br.ReadInt32();
				v.livesCost = br.ReadInt32();
				v.maxActivationsPerRound = br.ReadInt32();
				v.animation = br.ReadInt32();
				v.animationOffsetFrames = br.ReadInt32();
				v.enabled = br.ReadBoolean();
				v.canActivateBetweenRounds = br.ReadBoolean();
				v.resetCooldownOnTierUpgrade = br.ReadBoolean();
				v.disabledByAnotherTower = br.ReadBoolean();
				v.activateOnLivesLost = br.ReadBoolean();
				v.sharedCooldown = br.ReadBoolean();
				v.dontShowStacked = br.ReadBoolean();
				v.animateOnMainAttackDisplay = br.ReadBoolean();
				v.restrictAbilityAfterMaxRoundTimer = br.ReadBoolean();
				cooldownSpeedScaleField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				animationOffsetField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				cooldownField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			}
		}
	
		private void Set_v_AbilityBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityBehaviorModel)m[i+start];
			}
		}
	
		private void Set_v_CreateSoundOnAbilityModel_Fields(int start, int count) {
			Set_v_AbilityBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateSoundOnAbilityModel)m[i+start];
				v.sound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.heroSound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.heroSound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_CallToArmsModel_Fields(int start, int count) {
			Set_v_AbilityBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CallToArmsModel>();
			var lifespanField = t.GetField("lifespan", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CallToArmsModel)m[i+start];
				v.multiplier = br.ReadSingle();
				v.useRadius = br.ReadBoolean();
				v.buffLocsName = br.ReadBoolean() ? null : br.ReadString();
				v.buffIconName = br.ReadBoolean() ? null : br.ReadString();
				v.lifespanFrames = br.ReadInt32();
				lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			}
		}
	
		private void Set_v_CreateEffectOnAbilityModel_Fields(int start, int count) {
			Set_v_AbilityBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateEffectOnAbilityModel)m[i+start];
				v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
				v.randomRotation = br.ReadBoolean();
				v.centerEffect = br.ReadBoolean();
				v.destroyOnEnd = br.ReadBoolean();
				v.useAttackTransform = br.ReadBoolean();
				v.canSave = br.ReadBoolean();
			}
		}
	
		private void Set_v_FootprintModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.FootprintModel)m[i+start];
				v.doesntBlockTowerPlacement = br.ReadBoolean();
				v.ignoresPlacementCheck = br.ReadBoolean();
				v.ignoresTowerOverlap = br.ReadBoolean();
			}
		}
	
		private void Set_v_CircleFootprintModel_Fields(int start, int count) {
			Set_v_FootprintModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CircleFootprintModel)m[i+start];
				v.radius = br.ReadSingle();
			}
		}
	
		private void Set_v_UpgradePathModel_Fields(int start, int count) {
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
			var towerField = t.GetField("tower", bindFlags);
			var upgradeField = t.GetField("upgrade", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel)m[i+start];
				towerField.SetValue(v,br.ReadBoolean() ? null : br.ReadString());
				upgradeField.SetValue(v,br.ReadBoolean() ? null : br.ReadString());
			}
		}
	
		private void Set_v_TowerBehaviorBuffModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.TowerBehaviorBuffModel)m[i+start];
				v.buffLocsName = br.ReadBoolean() ? null : br.ReadString();
				v.buffIconName = br.ReadBoolean() ? null : br.ReadString();
				v.maxStackSize = br.ReadInt32();
				v.isGlobalRange = br.ReadBoolean();
			}
		}
	
		private void Set_v_DiscountZoneModel_Fields(int start, int count) {
			Set_v_TowerBehaviorBuffModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.DiscountZoneModel)m[i+start];
				v.discountMultiplier = br.ReadSingle();
				v.stackLimit = br.ReadInt32();
				v.stackName = br.ReadBoolean() ? null : br.ReadString();
				v.groupName = br.ReadBoolean() ? null : br.ReadString();
				v.affectSelf = br.ReadBoolean();
				v.tierCap = br.ReadInt32();
			}
		}
	
		private void Set_v_ShowCashIconInsteadModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.ShowCashIconInsteadModel)m[i+start];
			}
		}
	
		private void Set_v_AddBehaviorToTowerSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.AddBehaviorToTowerSupportModel)m[i+start];
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.TowerBehaviorModel>) m[br.ReadInt32()];
				v.mutationId = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_CashIncreaseModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CashIncreaseModel)m[i+start];
				v.increase = br.ReadSingle();
				v.multiplier = br.ReadSingle();
			}
		}
	
		private void Set_v_MonkeyCityModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.MonkeyCityModel)m[i+start];
				v.roundsTillMultiplier = br.ReadInt32();
				v.towerId = br.ReadBoolean() ? null : br.ReadString();
				v.multiplier = br.ReadSingle();
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_MonkeyCityIncomeSupportModel_Fields(int start, int count) {
			Set_v_SupportModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.MonkeyCityIncomeSupportModel)m[i+start];
				v.isUnique = br.ReadBoolean();
				v.incomeModifier = br.ReadSingle();
			}
		}
	
		private void Set_v_MonkeyopolisModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.MonkeyopolisModel)m[i+start];
				v.filterTower = br.ReadBoolean() ? null : br.ReadString();
				v.valueRequiredForCrate = br.ReadInt32();
				v.cashFromCrate = br.ReadInt32();
			}
		}
	
		private void Set_v_PickupModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.PickupModel)m[i+start];
				v.collectRadius = br.ReadSingle();
				v.delay = br.ReadSingle();
				v.delayFrames = br.ReadSingle();
			}
		}
	
		private void Set_v_CashModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CashModel)m[i+start];
				v.minimum = br.ReadSingle();
				v.maximum = br.ReadSingle();
				v.bonusMultiplier = br.ReadSingle();
				v.salvage = br.ReadSingle();
				v.noTransformCash = br.ReadBoolean();
				v.distributeSalvage = br.ReadBoolean();
			}
		}
	
		private void Set_v_AgeModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.AgeModel>();
			var lifespanField = t.GetField("lifespan", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.AgeModel)m[i+start];
				v.rounds = br.ReadInt32();
				v.lifespanFrames = br.ReadInt32();
				v.useRoundTime = br.ReadBoolean();
				lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.endOfRoundClearBypassModel = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.EndOfRoundClearBypassModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_CreateTextEffectModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateTextEffectModel)m[i+start];
				v.assetId = br.ReadBoolean() ? null : br.ReadString();
				v.lifespan = br.ReadSingle();
				v.useTowerPosition = br.ReadBoolean();
			}
		}
	
		private void Set_v_ScaleProjectileModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.ScaleProjectileModel)m[i+start];
				v.samples = (Il2CppStructArray<float>) m[br.ReadInt32()];
				v.curve = (Assets.Scripts.Simulation.Towers.Projectiles.Behaviors.Curve) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_Curve_Fields(int start, int count) {
			var t = Il2CppType.Of<Assets.Scripts.Simulation.Towers.Projectiles.Behaviors.Curve>();
			var samplesField = t.GetField("samples", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Simulation.Towers.Projectiles.Behaviors.Curve)m[i+start];
				v.samples = (Il2CppStructArray<float>) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_Assets_Scripts_Models_Towers_Projectiles_Behaviors_CreateEffectOnExpireModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnExpireModel)m[i+start];
				v.assetId = br.ReadBoolean() ? null : br.ReadString();
				v.lifespan = br.ReadSingle();
				v.fullscreen = br.ReadBoolean();
				v.randomRotation = br.ReadBoolean();
				v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_ArriveAtTargetModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.ArriveAtTargetModel)m[i+start];
				v.timeToTake = br.ReadSingle();
				v.curveSamples = (Il2CppStructArray<float>) m[br.ReadInt32()];
				v.filterCollisionWhileMoving = br.ReadBoolean();
				v.expireOnArrival = br.ReadBoolean();
				v.altSpeed = br.ReadSingle();
				v.stopOnTargetReached = br.ReadBoolean();
				v.curve = (Assets.Scripts.Simulation.Towers.Projectiles.Behaviors.Curve) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_HeightOffsetProjectileModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.HeightOffsetProjectileModel)m[i+start];
				v.samples = (Il2CppStructArray<float>) m[br.ReadInt32()];
				v.curve = (Assets.Scripts.Simulation.Towers.Projectiles.Behaviors.Curve) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_WeaponBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel)m[i+start];
			}
		}
	
		private void Set_v_EmissionsPerRoundFilterModel_Fields(int start, int count) {
			Set_v_WeaponBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.EmissionsPerRoundFilterModel)m[i+start];
				v.count = br.ReadInt32();
				v.allowSpawnOnInitialise = br.ReadBoolean();
				v.ignoreInSandbox = br.ReadBoolean();
			}
		}
	
		private void Set_v_WeaponRateMinModel_Fields(int start, int count) {
			Set_v_WeaponBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.WeaponRateMinModel)m[i+start];
				v.min = br.ReadSingle();
			}
		}
	
		private void Set_v_RandomPositionBasicModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RandomPositionBasicModel)m[i+start];
				v.minRadius = br.ReadSingle();
				v.maxRadius = br.ReadSingle();
				v.mapBorder = br.ReadSingle();
				v.useTerrainHeight = br.ReadBoolean();
			}
		}
	
		private void Set_v_MonkeyopolisUpgradeCostModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.MonkeyopolisUpgradeCostModel)m[i+start];
				v.costPerFarm = br.ReadInt32();
				v.path = br.ReadInt32();
				v.towerFilter = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		#endregion
	
		protected override TowerModel Load(byte[] bytes) {
			using (var s = new MemoryStream(bytes)) {
				using (var reader = new BinaryReader(s)) {
					this.br = reader;
					var totalCount = br.ReadInt32();
					m = new object[totalCount];
				
					//##  Step 1: create empty collections
					CreateArraySet<Assets.Scripts.Models.Model>();
					Read_a_Int32_Array();
					Read_a_AreaType_Array();
					CreateArraySet<Assets.Scripts.Models.Towers.Mods.ApplyModModel>();
					CreateArraySet<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
					CreateArraySet<Assets.Scripts.Models.Towers.Filters.FilterModel>();
					Read_a_String_Array();
					CreateArraySet<Assets.Scripts.Models.Bloons.BloonBehaviorModel>();
					CreateArraySet<Assets.Scripts.Models.Towers.TowerFilters.TowerFilterModel>();
					CreateArraySet<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
					Read_a_TargetType_Array();
					CreateArraySet<Assets.Scripts.Models.Towers.TowerBehaviorModel>();
					Read_a_Single_Array();
					CreateArraySet<Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel>();
					CreateListSet<Assets.Scripts.Models.Model>();
					CreateDictionarySet<System.String, Assets.Scripts.Models.Effects.AssetPathModel>();
				
					//##  Step 2: create empty objects
					Create_Records<Assets.Scripts.Models.Towers.TowerModel>();
					Create_Records<Assets.Scripts.Utils.SpriteReference>();
					Create_Records<Assets.Scripts.Models.Towers.Mods.ApplyModModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnTowerPlaceModel>();
					Create_Records<Assets.Scripts.Models.Audio.SoundModel>();
					Create_Records<Assets.Scripts.Models.Audio.BlankSoundModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnUpgradeModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnSellModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnUpgradeModel>();
					Create_Records<Assets.Scripts.Models.Effects.EffectModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnPlaceModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnSellModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.AttackFilterModel>();
					Create_Records<Assets.Scripts.Models.Towers.Filters.FilterInvisibleModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.SingleEmmisionTowardsTargetModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.ProjectileModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.RetargetOnContactModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelStraitModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModifierForTagModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CollideExtraPierceReductionModel>();
					Create_Records<Assets.Scripts.Models.GenericBehaviors.DisplayModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLastModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetCloseModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RotateToTargetModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.RangeSupportModel>();
					Create_Records<Assets.Scripts.Models.GenericBehaviors.BuffIndicatorModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.RateSupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.VisibilitySupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.AddBehaviorToBloonInZoneModel>();
					Create_Records<Assets.Scripts.Models.Bloons.Behaviors.GrowBlockModel>();
					Create_Records<Assets.Scripts.Models.Towers.Filters.FilterWithTagModel>();
					Create_Records<Assets.Scripts.Models.Effects.AssetPathModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.PierceSupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.TowerFilters.FilterInSetModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.ProjectileSpeedSupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.FreeUpgradeSupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.AbilityCooldownScaleSupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.DamageTypeSupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.SupportRemoveFilterOutTagModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateSoundOnAbilityModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CallToArmsModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateEffectOnAbilityModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CircleFootprintModel>();
					Create_Records<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.DiscountZoneModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.ShowCashIconInsteadModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.AddBehaviorToTowerSupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CashIncreaseModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.MonkeyCityModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.MonkeyCityIncomeSupportModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.MonkeyopolisModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.PickupModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CashModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.AgeModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateTextEffectModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.ScaleProjectileModel>();
					Create_Records<Assets.Scripts.Simulation.Towers.Projectiles.Behaviors.Curve>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnExpireModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.ArriveAtTargetModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.HeightOffsetProjectileModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.EmissionsPerRoundFilterModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.WeaponRateMinModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RandomPositionBasicModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.MonkeyopolisUpgradeCostModel>();
				
					Set_v_TowerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SpriteReference_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ApplyModModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnTowerPlaceModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SoundModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_BlankSoundModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnUpgradeModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnSellModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateEffectOnUpgradeModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EffectModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateEffectOnPlaceModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateEffectOnSellModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AttackModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AttackFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterInvisibleModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_WeaponModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SingleEmmisionTowardsTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ProjectileModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DamageModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_RetargetOnContactModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TravelStraitModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DamageModifierForTagModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CollideExtraPierceReductionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DisplayModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetLastModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetCloseModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetStrongModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetFirstModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_RotateToTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_RangeSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_BuffIndicatorModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_RateSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_VisibilitySupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AddBehaviorToBloonInZoneModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_GrowBlockModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterWithTagModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AssetPathModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_PierceSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterInSetModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ProjectileSpeedSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FreeUpgradeSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AbilityCooldownScaleSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DamageTypeSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SupportRemoveFilterOutTagModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AbilityModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnAbilityModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CallToArmsModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateEffectOnAbilityModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CircleFootprintModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_UpgradePathModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DiscountZoneModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ShowCashIconInsteadModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AddBehaviorToTowerSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CashIncreaseModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_MonkeyCityModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_MonkeyCityIncomeSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_MonkeyopolisModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_PickupModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CashModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AgeModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateTextEffectModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ScaleProjectileModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_Curve_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_Assets_Scripts_Models_Towers_Projectiles_Behaviors_CreateEffectOnExpireModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ArriveAtTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_HeightOffsetProjectileModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EmissionsPerRoundFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_WeaponRateMinModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_RandomPositionBasicModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_MonkeyopolisUpgradeCostModel_Fields(br.ReadInt32(), br.ReadInt32());
				
					//##  Step 4: link object collections e.g Product[]. Note: requires object data e.g dictionary<string, value> where string = model.name
					LinkArray<Assets.Scripts.Models.Model>();
					LinkArray<Assets.Scripts.Models.Towers.Mods.ApplyModModel>();
					LinkArray<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
					LinkArray<Assets.Scripts.Models.Towers.Filters.FilterModel>();
					LinkArray<Assets.Scripts.Models.Bloons.BloonBehaviorModel>();
					LinkArray<Assets.Scripts.Models.Towers.TowerFilters.TowerFilterModel>();
					LinkArray<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
					LinkArray<Assets.Scripts.Models.Towers.TowerBehaviorModel>();
					LinkArray<Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel>();
					LinkList<Assets.Scripts.Models.Model>();
					LinkDictionary<Assets.Scripts.Models.Effects.AssetPathModel>();
				
					var resIndex = br.ReadInt32();
					UnityEngine.Debug.Assert(br.BaseStream.Position == br.BaseStream.Length);
					return (Assets.Scripts.Models.Towers.TowerModel) m[resIndex];
				}
			}
		}
	}
}
