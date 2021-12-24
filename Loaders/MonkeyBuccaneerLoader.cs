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
	public class MonkeyBuccaneerLoader : ModByteLoader<TowerModel> {
	
		BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static; 
		BinaryReader br = null;
	
		// NOTE: was a collection per type but it prevented inheriance e.g list of Products would required class type id
		protected override string BytesFileName => "MonkeyBuccaneers.bytes";
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
		private void Read_a_Vector3_Array() {
			var arrSetCount = br.ReadInt32();
			var count = arrSetCount;
			for (var i = 0; i < count; i++) {
				var arrCount = br.ReadInt32();
				var arr = new Il2CppStructArray<Assets.Scripts.Simulation.SMath.Vector3>(arrCount);
				for (var j = 0; j < arr.Length; j++) {
					arr[j] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
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
	
		private void Set_v_SyncTargetPriorityWithSubTowersModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.SyncTargetPriorityWithSubTowersModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
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
	
		private void Set_v_SoundModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Audio.SoundModel)m[i+start];
				v.assetId = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_CreateSoundOnSellModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnSellModel)m[i+start];
				v.sound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_CreateEffectOnPlaceModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnPlaceModel)m[i+start];
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
	
		private void Set_v_CreateEffectOnSellModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnSellModel)m[i+start];
				v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
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
	
		private void Set_v_BlankSoundModel_Fields(int start, int count) {
			Set_v_SoundModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Audio.BlankSoundModel)m[i+start];
			}
		}
	
		private void Set_v_CreateEffectOnUpgradeModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnUpgradeModel)m[i+start];
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
	
		private void Set_v_ParallelEmissionModel_Fields(int start, int count) {
			Set_v_EmissionModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.ParallelEmissionModel)m[i+start];
				v.spreadLength = br.ReadSingle();
				v.yOffset = br.ReadSingle();
				v.count = br.ReadInt32();
				v.linear = br.ReadBoolean();
				v.offsetStart = br.ReadSingle();
			}
		}
	
		private void Set_v_EmissionBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionBehaviorModel)m[i+start];
			}
		}
	
		private void Set_v_EmissionRotationOffDisplayModel_Fields(int start, int count) {
			Set_v_EmissionBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionRotationOffDisplayModel)m[i+start];
				v.offsetRotation = br.ReadInt32();
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
	
		private void Set_v_ProjectileFilterModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.ProjectileFilterModel)m[i+start];
				v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Filters.FilterModel>) m[br.ReadInt32()];
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
	
		private void Set_v_WeaponBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel)m[i+start];
			}
		}
	
		private void Set_v_FireAlternateWeaponModel_Fields(int start, int count) {
			Set_v_WeaponBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.FireAlternateWeaponModel)m[i+start];
				v.weaponId = br.ReadInt32();
			}
		}
	
		private void Set_v_EmissionRotationOffDisplayOnEmitModel_Fields(int start, int count) {
			Set_v_EmissionBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionRotationOffDisplayOnEmitModel)m[i+start];
				v.offsetRotation = br.ReadInt32();
			}
		}
	
		private void Set_v_FireWhenAlternateWeaponIsReadyModel_Fields(int start, int count) {
			Set_v_WeaponBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.FireWhenAlternateWeaponIsReadyModel)m[i+start];
				v.weaponId = br.ReadInt32();
			}
		}
	
		private void Set_v_FilterTargetAngleFilterModel_Fields(int start, int count) {
			Set_v_WeaponBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.FilterTargetAngleFilterModel)m[i+start];
				v.fieldOfView = br.ReadSingle();
				v.baseTowerRotationOffset = br.ReadSingle();
				v.shareFilterTargets = br.ReadBoolean();
				v.minTimeBetweenFilterTargetsFrames = br.ReadInt32();
			}
		}
	
		private void Set_v_ArcEmissionModel_Fields(int start, int count) {
			Set_v_EmissionModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.Emissions.ArcEmissionModel>();
			var CountField = t.GetField("Count", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.ArcEmissionModel)m[i+start];
				v.angle = br.ReadSingle();
				v.offsetStart = br.ReadSingle();
				v.offset = br.ReadSingle();
				v.sliceSize = br.ReadSingle();
				v.ignoreTowerRotation = br.ReadBoolean();
				v.useProjectileRotation = br.ReadBoolean();
				CountField.SetValue(v,br.ReadInt32().ToIl2Cpp());
			}
		}
	
		private void Set_v_EmissionArcRotationOffDisplayDirectionModel_Fields(int start, int count) {
			Set_v_EmissionBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionArcRotationOffDisplayDirectionModel)m[i+start];
				v.offsetRotation = br.ReadInt32();
			}
		}
	
		private void Set_v_AddBehaviorToBloonModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.AddBehaviorToBloonModel)m[i+start];
				v.mutationId = br.ReadBoolean() ? null : br.ReadString();
				v.lifespan = br.ReadSingle();
				v.layers = br.ReadInt32();
				v.lifespanFrames = br.ReadInt32();
				v.filter = (Assets.Scripts.Models.Towers.Filters.FilterModel) m[br.ReadInt32()];
				v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Filters.FilterModel>) m[br.ReadInt32()];
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Bloons.BloonBehaviorModel>) m[br.ReadInt32()];
				v.overlays = (Dictionary<System.String, Assets.Scripts.Models.Effects.AssetPathModel>) m[br.ReadInt32()];
				v.overlayLayer = br.ReadInt32();
				v.isUnique = br.ReadBoolean();
				v.lastAppliesFirst = br.ReadBoolean();
				v.collideThisFrame = br.ReadBoolean();
				v.cascadeMutators = br.ReadBoolean();
				v.glueLevel = br.ReadInt32();
				v.applyOnlyIfDamaged = br.ReadBoolean();
				v.stackCount = br.ReadInt32();
			}
		}
	
		private void Set_v_BloonBehaviorModelWithTowerTracking_Fields(int start, int count) {
			Set_v_BloonBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Bloons.BloonBehaviorModelWithTowerTracking)m[i+start];
			}
		}
	
		private void Set_v_BloonBehaviorModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Bloons.BloonBehaviorModel)m[i+start];
			}
		}
	
		private void Set_v_DamageOverTimeModel_Fields(int start, int count) {
			Set_v_BloonBehaviorModelWithTowerTracking_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Bloons.Behaviors.DamageOverTimeModel>();
			var intervalField = t.GetField("interval", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Bloons.Behaviors.DamageOverTimeModel)m[i+start];
				v.damage = br.ReadSingle();
				v.payloadCount = br.ReadInt32();
				v.immuneBloonProperties = (BloonProperties) (br.ReadInt32());
				v.intervalFrames = br.ReadInt32();
				intervalField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.displayPath = br.ReadBoolean() ? null : br.ReadString();
				v.displayLifetime = br.ReadSingle();
				v.triggerImmediate = br.ReadBoolean();
				v.rotateEffectWithBloon = br.ReadBoolean();
				v.initialDelay = br.ReadSingle();
				v.initialDelayFrames = br.ReadInt32();
				v.damageOnDestroy = br.ReadBoolean();
				v.overrideDistributionBlocker = br.ReadBoolean();
				v.damageModifierModels = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Projectiles.DamageModifierModel>) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_AssetPathModel_Fields(int start, int count) {
			Set_v_Model_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Effects.AssetPathModel)m[i+start];
				v.assetPath = br.ReadBoolean() ? null : br.ReadString();
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
	
		private void Set_v_TargetCamoModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetCamoModel)m[i+start];
			}
		}
	
		private void Set_v_TargetSupplierModel_Fields(int start, int count) {
			Set_v_AttackBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetSupplierModel)m[i+start];
				v.isOnSubTower = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetFirstPrioCamoModel_Fields(int start, int count) {
			Set_v_TargetCamoModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstPrioCamoModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetLastPrioCamoModel_Fields(int start, int count) {
			Set_v_TargetCamoModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLastPrioCamoModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetClosePrioCamoModel_Fields(int start, int count) {
			Set_v_TargetCamoModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetClosePrioCamoModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetStrongPrioCamoModel_Fields(int start, int count) {
			Set_v_TargetCamoModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongPrioCamoModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_SingleEmissionModel_Fields(int start, int count) {
			Set_v_EmissionModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.SingleEmissionModel)m[i+start];
			}
		}
	
		private void Set_v_CreateTowerModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateTowerModel)m[i+start];
				v.tower = (Assets.Scripts.Models.Towers.TowerModel) m[br.ReadInt32()];
				v.height = br.ReadSingle();
				v.positionAtTarget = br.ReadBoolean();
				v.destroySubTowersOnCreateNewTower = br.ReadBoolean();
				v.useProjectileRotation = br.ReadBoolean();
				v.useParentTargetPriority = br.ReadBoolean();
				v.carryMutatorsFromDestroyedTower = br.ReadBoolean();
			}
		}
	
		private void Set_v_TowerExpireOnParentUpgradedModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.TowerExpireOnParentUpgradedModel)m[i+start];
			}
		}
	
		private void Set_v_TowerExpireOnParentDestroyedModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.TowerExpireOnParentDestroyedModel)m[i+start];
			}
		}
	
		private void Set_v_TowerExpireModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.TowerExpireModel>();
			var lifespanField = t.GetField("lifespan", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.TowerExpireModel)m[i+start];
				v.expireOnRoundComplete = br.ReadBoolean();
				v.expireOnDefeatScreen = br.ReadBoolean();
				v.lifespanFrames = br.ReadInt32();
				lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			}
		}
	
		private void Set_v_CreditPopsToParentTowerModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CreditPopsToParentTowerModel)m[i+start];
			}
		}
	
		private void Set_v_IgnoreTowersBlockerModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.IgnoreTowersBlockerModel)m[i+start];
				v.filteredTowers = (Il2CppStringArray) m[br.ReadInt32()];
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
	
		private void Set_v_SavedSubTowerModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.SavedSubTowerModel)m[i+start];
			}
		}
	
		private void Set_v_EmissionRotationOffBloonDirectionModel_Fields(int start, int count) {
			Set_v_EmissionBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionRotationOffBloonDirectionModel)m[i+start];
				v.useAirUnitPosition = br.ReadBoolean();
				v.dontSetAfterEmit = br.ReadBoolean();
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
	
		private void Set_v_FireFromAirUnitModel_Fields(int start, int count) {
			Set_v_WeaponBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.FireFromAirUnitModel)m[i+start];
			}
		}
	
		private void Set_v_FighterPilotPatternFirstModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FighterPilotPatternFirstModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
				v.offsetDistance = br.ReadSingle();
			}
		}
	
		private void Set_v_FighterPilotPatternCloseModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FighterPilotPatternCloseModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
				v.offsetDistance = br.ReadSingle();
			}
		}
	
		private void Set_v_FighterPilotPatternLastModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FighterPilotPatternLastModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
				v.offsetDistance = br.ReadSingle();
			}
		}
	
		private void Set_v_FighterPilotPatternStrongModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FighterPilotPatternStrongModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
				v.offsetDistance = br.ReadSingle();
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
	
		private void Set_v_CreateSoundOnProjectileCollisionModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateSoundOnProjectileCollisionModel)m[i+start];
				v.sound1 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound3 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound4 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound5 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_CreateEffectOnContactModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnContactModel)m[i+start];
				v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_TrackTargetModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TrackTargetModel>();
			var turnRateField = t.GetField("turnRate", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.TrackTargetModel)m[i+start];
				v.distance = br.ReadSingle();
				v.trackNewTargets = br.ReadBoolean();
				v.constantlyAquireNewTarget = br.ReadBoolean();
				v.maxSeekAngle = br.ReadSingle();
				v.ignoreSeekAngle = br.ReadBoolean();
				v.overrideRotation = br.ReadBoolean();
				v.useLifetimeAsDistance = br.ReadBoolean();
				v.turnRatePerFrame = br.ReadSingle();
				turnRateField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			}
		}
	
		private void Set_v_AccelerateModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.AccelerateModel)m[i+start];
				v.acceleration = br.ReadSingle();
				v.accelerationFrames = br.ReadSingle();
				v.maxSpeed = br.ReadSingle();
				v.maxSpeedFrames = br.ReadSingle();
				v.turnRateChange = br.ReadSingle();
				v.turnRateChangeFrames = br.ReadSingle();
				v.maxTurnRate = br.ReadSingle();
				v.maxTurnRateFrames = br.ReadSingle();
				v.decelerate = br.ReadBoolean();
			}
		}
	
		private void Set_v_CreateProjectileOnContactModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateProjectileOnContactModel)m[i+start];
				v.projectile = (Assets.Scripts.Models.Towers.Projectiles.ProjectileModel) m[br.ReadInt32()];
				v.emission = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionModel) m[br.ReadInt32()];
				v.passOnCollidedWith = br.ReadBoolean();
				v.dontCreateAtBloon = br.ReadBoolean();
				v.passOnDirectionToContact = br.ReadBoolean();
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
	
		private void Set_v_TargetStrongAirUnitModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongAirUnitModel)m[i+start];
				v.isSelectable = br.ReadBoolean();
			}
		}
	
		private void Set_v_AirUnitModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.AirUnitModel)m[i+start];
				v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.TowerBehaviorModel>) m[br.ReadInt32()];
				v.display = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_FighterMovementModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.FighterMovementModel)m[i+start];
				v.maxSpeed = br.ReadSingle();
				v.turningSpeed = br.ReadSingle();
				v.minDistanceToTargetBeforeFlyover = br.ReadSingle();
				v.distanceOfFlyover = br.ReadSingle();
				v.bankAngleMax = br.ReadSingle();
				v.bankSmoothness = br.ReadSingle();
				v.rollTotalTime = br.ReadSingle();
				v.rollRunUpDistance = br.ReadSingle();
				v.rollTimeBeforeNext = br.ReadSingle();
				v.rollChancePerSecondPassed = br.ReadSingle();
				v.loopTotalTime = br.ReadSingle();
				v.loopRunUpDistance = br.ReadSingle();
				v.loopTimeBeforeNext = br.ReadSingle();
				v.loopChancePerSecondPassed = br.ReadSingle();
				v.loopRadius = br.ReadSingle();
				v.loopModelScale = br.ReadSingle();
			}
		}
	
		private void Set_v_SubTowerFilterModel_Fields(int start, int count) {
			Set_v_WeaponBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.SubTowerFilterModel)m[i+start];
				v.baseSubTowerId = br.ReadBoolean() ? null : br.ReadString();
				v.baseSubTowerIds = (Il2CppStringArray) m[br.ReadInt32()];
				v.maxNumberOfSubTowers = br.ReadSingle();
				v.checkForPreExisting = br.ReadBoolean();
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
	
		private void Set_v_FlagshipAttackSpeedIncreaseModel_Fields(int start, int count) {
			Set_v_TowerBehaviorBuffModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.FlagshipAttackSpeedIncreaseModel)m[i+start];
				v.attackSpeedIncrease = br.ReadSingle();
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
	
		private void Set_v_AddMakeshiftAreaModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.AddMakeshiftAreaModel)m[i+start];
				v.points = (Il2CppStructArray<Assets.Scripts.Simulation.SMath.Vector3>) m[br.ReadInt32()];
				v.newAreaType = (Assets.Scripts.Models.Map.AreaType) (br.ReadInt32());
				v.filterInTowerSizes = (Il2CppStringArray) m[br.ReadInt32()];
				v.filterInTowerSets = (Il2CppStringArray) m[br.ReadInt32()];
				v.filterOutSpecificTowers = (Il2CppStringArray) m[br.ReadInt32()];
				v.renderHeightOffset = br.ReadSingle();
				v.ignoreZAxisTowerCollision = br.ReadBoolean();
				v.destroyTowersOnAreaWhenSold = br.ReadBoolean();
				v.dontDestroyTowersWhenAreaChanges = br.ReadBoolean();
			}
		}
	
		private void Set_v_EmissionRotationOffTowerDirectionModel_Fields(int start, int count) {
			Set_v_EmissionBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionRotationOffTowerDirectionModel)m[i+start];
				v.offsetRotation = br.ReadInt32();
			}
		}
	
		private void Set_v_EmissionArcRotationOffTowerDirectionModel_Fields(int start, int count) {
			Set_v_EmissionBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionArcRotationOffTowerDirectionModel)m[i+start];
				v.offsetRotation = br.ReadInt32();
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
	
		private void Set_v_ActivateAttackModel_Fields(int start, int count) {
			Set_v_AbilityBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ActivateAttackModel>();
			var lifespanField = t.GetField("lifespan", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ActivateAttackModel)m[i+start];
				lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.lifespanFrames = br.ReadInt32();
				v.attacks = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>) m[br.ReadInt32()];
				v.processOnActivate = br.ReadBoolean();
				v.cancelIfNoTargets = br.ReadBoolean();
				v.turnOffExisting = br.ReadBoolean();
				v.endOnRoundEnd = br.ReadBoolean();
				v.endOnDefeatScreen = br.ReadBoolean();
				v.isOneShot = br.ReadBoolean();
			}
		}
	
		private void Set_v_GrappleEmissionModel_Fields(int start, int count) {
			Set_v_EmissionModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.GrappleEmissionModel)m[i+start];
			}
		}
	
		private void Set_v_FilterAllExceptTargetModel_Fields(int start, int count) {
			Set_v_FilterModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Filters.FilterAllExceptTargetModel)m[i+start];
			}
		}
	
		private void Set_v_MoabTakedownModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.MoabTakedownModel)m[i+start];
				v.speed = br.ReadSingle();
				v.speedFrames = br.ReadSingle();
				v.increaseMoabBloonWorth = br.ReadBoolean();
				v.multiplier = br.ReadSingle();
				v.additive = br.ReadSingle();
				v.increaseWorthTextEffectModel = (Assets.Scripts.Models.Bloons.Behaviors.IncreaseWorthTextEffectModel) m[br.ReadInt32()];
				v.destroyBloonRadius = br.ReadSingle();
				v.displayAtEjectId = br.ReadBoolean() ? null : br.ReadString();
				v.bloonWorthMutator = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.MoabTakedownModel.BloonWorthMutator) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_IncreaseWorthTextEffectModel_Fields(int start, int count) {
			Set_v_BloonBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Bloons.Behaviors.IncreaseWorthTextEffectModel)m[i+start];
				v.assetId = br.ReadBoolean() ? null : br.ReadString();
				v.lifespan = br.ReadSingle();
				v.displayFullPayout = br.ReadBoolean();
			}
		}
	
		private void Set_v_BehaviorMutator_Fields(int start, int count) {
			var t = Il2CppType.Of<Assets.Scripts.Simulation.Objects.BehaviorMutator>();
			var usesSplitIdField = t.GetField("usesSplitId", bindFlags);
			var idMajorField = t.GetField("idMajor", bindFlags);
			var idMajorMinorField = t.GetField("idMajorMinor", bindFlags);
			var resultCacheField = t.GetField("resultCache", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Simulation.Objects.BehaviorMutator)m[i+start];
				v.id = br.ReadBoolean() ? null : br.ReadString();
				usesSplitIdField.SetValue(v,br.ReadBoolean().ToIl2Cpp());
				idMajorField.SetValue(v,br.ReadBoolean() ? null : br.ReadString());
				idMajorMinorField.SetValue(v,br.ReadBoolean() ? null : br.ReadString());
				v.isExclusiveInMutationList = br.ReadBoolean();
				v.priority = br.ReadInt32();
				v.glueLevel = br.ReadInt32();
				v.isFreeze = br.ReadBoolean();
				v.dontCopy = br.ReadBoolean();
				v.buffIndicator = (Assets.Scripts.Models.GenericBehaviors.BuffIndicatorModel) m[br.ReadInt32()];
				v.includesSubTowers = br.ReadBoolean();
				v.saveId = br.ReadBoolean() ? null : br.ReadString();
				resultCacheField.SetValue(v,(Dictionary<Assets.Scripts.Models.Model, Assets.Scripts.Models.Model>) m[br.ReadInt32()]);
			}
		}
	
		private void Set_v_Assets_Scripts_Models_Towers_Projectiles_Behaviors_MoabTakedownModel_BloonWorthMutator_Fields(int start, int count) {
			Set_v_BehaviorMutator_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.MoabTakedownModel.BloonWorthMutator>();
			var multiplierField = t.GetField("multiplier", bindFlags);
			var additiveField = t.GetField("additive", bindFlags);
			var behaviorField = t.GetField("behavior", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.MoabTakedownModel.BloonWorthMutator)m[i+start];
				multiplierField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				additiveField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				behaviorField.SetValue(v,(Assets.Scripts.Models.Bloons.Behaviors.IncreaseWorthTextEffectModel) m[br.ReadInt32()]);
			}
		}
	
		private void Set_v_CreateRopeEffectModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateRopeEffectModel)m[i+start];
				v.assetId = br.ReadBoolean() ? null : br.ReadString();
				v.endAssetId = br.ReadBoolean() ? null : br.ReadString();
				v.spriteSpacing = br.ReadSingle();
				v.spriteOffset = br.ReadSingle();
				v.spriteRadius = br.ReadSingle();
			}
		}
	
		private void Set_v_TravelTowardsEmitTowerModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelTowardsEmitTowerModel)m[i+start];
				v.lockRotation = br.ReadBoolean();
				v.lifespan = br.ReadSingle();
				v.lifespanFrames = br.ReadInt32();
				v.range = br.ReadSingle();
				v.speed = br.ReadSingle();
				v.speedFrames = br.ReadSingle();
				v.delayedActivation = br.ReadBoolean();
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
	
		private void Set_v_CreateSoundOnProjectileExpireModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateSoundOnProjectileExpireModel)m[i+start];
				v.sound1 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound3 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound4 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
				v.sound5 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			}
		}
	
		private void Set_v_LinearTravelModel_Fields(int start, int count) {
			Set_v_ProjectileBehaviorModel_Fields(start, count);
			var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.LinearTravelModel>();
			var lifespanField = t.GetField("lifespan", bindFlags);
			var speedField = t.GetField("speed", bindFlags);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.LinearTravelModel)m[i+start];
				lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.lifespanFrames = br.ReadInt32();
				speedField.SetValue(v,br.ReadSingle().ToIl2Cpp());
				v.speedFrames = br.ReadSingle();
				v.dontDestroyOnTargetLoss = br.ReadBoolean();
				v.forceCollisionOnSnap = br.ReadBoolean();
			}
		}
	
		private void Set_v_TargetGrapplableModel_Fields(int start, int count) {
			Set_v_TargetSupplierModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetGrapplableModel)m[i+start];
				v.grappleEmissionModel = (Assets.Scripts.Models.Towers.Behaviors.Emissions.GrappleEmissionModel) m[br.ReadInt32()];
				v.isSelectable = br.ReadBoolean();
				v.hooks = br.ReadInt32();
				v.zomgHooksRequired = br.ReadInt32();
				v.badHooksRequired = br.ReadInt32();
			}
		}
	
		private void Set_v_FilterOutTagModel_Fields(int start, int count) {
			Set_v_FilterModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Filters.FilterOutTagModel)m[i+start];
				v.tag = br.ReadBoolean() ? null : br.ReadString();
				v.disableWhenSupportMutatorIDs = (Il2CppStringArray) m[br.ReadInt32()];
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
	
		private void Set_v_RotateToDefaultPositionTowerModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.RotateToDefaultPositionTowerModel)m[i+start];
				v.rotation = br.ReadSingle();
				v.onlyOnReachingTier = br.ReadInt32();
			}
		}
	
		private void Set_v_PerRoundCashBonusTowerModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.PerRoundCashBonusTowerModel)m[i+start];
				v.cashPerRound = br.ReadSingle();
				v.cashRoundBonusMultiplier = br.ReadSingle();
				v.lifespan = br.ReadSingle();
				v.assetId = br.ReadBoolean() ? null : br.ReadString();
				v.distributeCash = br.ReadBoolean();
			}
		}
	
		private void Set_v_TradeEmpireBuffModel_Fields(int start, int count) {
			Set_v_TowerBehaviorBuffModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.TradeEmpireBuffModel)m[i+start];
				v.cashPerRoundPerMechantship = br.ReadSingle();
				v.maxMerchantmanCapBonus = br.ReadInt32();
				v.damageBuff = br.ReadInt32();
				v.ceramicDamageBuff = br.ReadInt32();
				v.moabDamageBuff = br.ReadInt32();
				v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
			}
		}
	
		private void Set_v_CashbackZoneModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.CashbackZoneModel)m[i+start];
				v.cashbackZoneMultiplier = br.ReadSingle();
				v.cashbackMaxPercent = br.ReadSingle();
				v.groupName = br.ReadBoolean() ? null : br.ReadString();
				v.maxStacks = br.ReadInt32();
			}
		}
	
		private void Set_v_MerchantShipModel_Fields(int start, int count) {
			Set_v_TowerBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.MerchantShipModel)m[i+start];
			}
		}
	
		private void Set_v_EmissionWithOffsetsModel_Fields(int start, int count) {
			Set_v_EmissionModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionWithOffsetsModel)m[i+start];
				v.throwMarkerOffsetModels = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel>) m[br.ReadInt32()];
				v.projectileCount = br.ReadInt32();
				v.rotateProjectileWithTower = br.ReadBoolean();
				v.randomRotationCone = br.ReadSingle();
			}
		}
	
		private void Set_v_ThrowMarkerOffsetModel_Fields(int start, int count) {
			Set_v_WeaponBehaviorModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel)m[i+start];
				v.ejectX = br.ReadSingle();
				v.ejectY = br.ReadSingle();
				v.ejectZ = br.ReadSingle();
				v.rotation = br.ReadSingle();
			}
		}
	
		private void Set_v_FilterTargetAngleModel_Fields(int start, int count) {
			Set_v_FilterModel_Fields(start, count);
			for (var i=0; i<count; i++) {
				var v = (Assets.Scripts.Models.Towers.Filters.FilterTargetAngleModel)m[i+start];
				v.fieldOfView = br.ReadSingle();
				v.baseTowerRotationOffset = br.ReadSingle();
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
					CreateArraySet<Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionBehaviorModel>();
					CreateArraySet<Assets.Scripts.Models.Towers.Filters.FilterModel>();
					CreateArraySet<Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel>();
					CreateArraySet<Assets.Scripts.Models.Bloons.BloonBehaviorModel>();
					Read_a_String_Array();
					CreateArraySet<Assets.Scripts.Models.Towers.TowerBehaviorModel>();
					CreateArraySet<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
					Read_a_TargetType_Array();
					Read_a_Vector3_Array();
					CreateArraySet<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>();
					CreateArraySet<Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel>();
					CreateListSet<Assets.Scripts.Models.Model>();
					CreateDictionarySet<System.String, Assets.Scripts.Models.Effects.AssetPathModel>();
				
					//##  Step 2: create empty objects
					Create_Records<Assets.Scripts.Models.Towers.TowerModel>();
					Create_Records<Assets.Scripts.Utils.SpriteReference>();
					Create_Records<Assets.Scripts.Models.Towers.Mods.ApplyModModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.SyncTargetPriorityWithSubTowersModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnUpgradeModel>();
					Create_Records<Assets.Scripts.Models.Audio.SoundModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnSellModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnPlaceModel>();
					Create_Records<Assets.Scripts.Models.Effects.EffectModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnSellModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnTowerPlaceModel>();
					Create_Records<Assets.Scripts.Models.Audio.BlankSoundModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnUpgradeModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.ParallelEmissionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionRotationOffDisplayModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.ProjectileModel>();
					Create_Records<Assets.Scripts.Models.Towers.Filters.FilterInvisibleModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelStraitModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.ProjectileFilterModel>();
					Create_Records<Assets.Scripts.Models.GenericBehaviors.DisplayModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.FireAlternateWeaponModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionRotationOffDisplayOnEmitModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.FireWhenAlternateWeaponIsReadyModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.FilterTargetAngleFilterModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.ArcEmissionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionArcRotationOffDisplayDirectionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.AddBehaviorToBloonModel>();
					Create_Records<Assets.Scripts.Models.Bloons.Behaviors.DamageOverTimeModel>();
					Create_Records<Assets.Scripts.Models.Effects.AssetPathModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.AttackFilterModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RotateToTargetModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstPrioCamoModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLastPrioCamoModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetClosePrioCamoModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongPrioCamoModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.SingleEmissionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateTowerModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.TowerExpireOnParentUpgradedModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.TowerExpireOnParentDestroyedModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.TowerExpireModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreditPopsToParentTowerModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.IgnoreTowersBlockerModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CircleFootprintModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.SavedSubTowerModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionRotationOffBloonDirectionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModifierForTagModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.FireFromAirUnitModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FighterPilotPatternFirstModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FighterPilotPatternCloseModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FighterPilotPatternLastModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FighterPilotPatternStrongModel>();
					Create_Records<Assets.Scripts.Models.Towers.Filters.FilterWithTagModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateSoundOnProjectileCollisionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnContactModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TrackTargetModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.AccelerateModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateProjectileOnContactModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.AgeModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongAirUnitModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.AirUnitModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.FighterMovementModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.SubTowerFilterModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.FlagshipAttackSpeedIncreaseModel>();
					Create_Records<Assets.Scripts.Models.GenericBehaviors.BuffIndicatorModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.AddMakeshiftAreaModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionRotationOffTowerDirectionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors.EmissionArcRotationOffTowerDirectionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ActivateAttackModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.GrappleEmissionModel>();
					Create_Records<Assets.Scripts.Models.Towers.Filters.FilterAllExceptTargetModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.MoabTakedownModel>();
					Create_Records<Assets.Scripts.Models.Bloons.Behaviors.IncreaseWorthTextEffectModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.MoabTakedownModel.BloonWorthMutator>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateRopeEffectModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelTowardsEmitTowerModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnExpireModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateSoundOnProjectileExpireModel>();
					Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.LinearTravelModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetGrapplableModel>();
					Create_Records<Assets.Scripts.Models.Towers.Filters.FilterOutTagModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateSoundOnAbilityModel>();
					Create_Records<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.RotateToDefaultPositionTowerModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.PerRoundCashBonusTowerModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.TradeEmpireBuffModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.CashbackZoneModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.MerchantShipModel>();
					Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionWithOffsetsModel>();
					Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel>();
					Create_Records<Assets.Scripts.Models.Towers.Filters.FilterTargetAngleModel>();
				
					Set_v_TowerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SpriteReference_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ApplyModModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SyncTargetPriorityWithSubTowersModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnUpgradeModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SoundModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnSellModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateEffectOnPlaceModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EffectModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateEffectOnSellModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnTowerPlaceModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_BlankSoundModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateEffectOnUpgradeModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AttackModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_WeaponModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ParallelEmissionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EmissionRotationOffDisplayModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ProjectileModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterInvisibleModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DamageModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TravelStraitModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ProjectileFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DisplayModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FireAlternateWeaponModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EmissionRotationOffDisplayOnEmitModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FireWhenAlternateWeaponIsReadyModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterTargetAngleFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ArcEmissionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EmissionArcRotationOffDisplayDirectionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AddBehaviorToBloonModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DamageOverTimeModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AssetPathModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AttackFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_RotateToTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetFirstPrioCamoModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetLastPrioCamoModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetClosePrioCamoModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetStrongPrioCamoModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SingleEmissionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TowerExpireOnParentUpgradedModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TowerExpireOnParentDestroyedModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TowerExpireModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreditPopsToParentTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_IgnoreTowersBlockerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CircleFootprintModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SavedSubTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EmissionRotationOffBloonDirectionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_DamageModifierForTagModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FireFromAirUnitModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FighterPilotPatternFirstModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FighterPilotPatternCloseModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FighterPilotPatternLastModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FighterPilotPatternStrongModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterWithTagModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnProjectileCollisionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateEffectOnContactModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TrackTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AccelerateModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateProjectileOnContactModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AgeModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetStrongAirUnitModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AirUnitModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FighterMovementModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_SubTowerFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FlagshipAttackSpeedIncreaseModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_BuffIndicatorModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AddMakeshiftAreaModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EmissionRotationOffTowerDirectionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EmissionArcRotationOffTowerDirectionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_AbilityModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ActivateAttackModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_GrappleEmissionModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterAllExceptTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_MoabTakedownModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_IncreaseWorthTextEffectModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_Assets_Scripts_Models_Towers_Projectiles_Behaviors_MoabTakedownModel_BloonWorthMutator_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateRopeEffectModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TravelTowardsEmitTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_Assets_Scripts_Models_Towers_Projectiles_Behaviors_CreateEffectOnExpireModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnProjectileExpireModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_LinearTravelModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TargetGrapplableModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterOutTagModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CreateSoundOnAbilityModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_UpgradePathModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_RotateToDefaultPositionTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_PerRoundCashBonusTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_TradeEmpireBuffModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_CashbackZoneModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_MerchantShipModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_EmissionWithOffsetsModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_ThrowMarkerOffsetModel_Fields(br.ReadInt32(), br.ReadInt32());
					Set_v_FilterTargetAngleModel_Fields(br.ReadInt32(), br.ReadInt32());
				
					//##  Step 4: link object collections e.g Product[]. Note: requires object data e.g dictionary<string, value> where string = model.name
					LinkArray<Assets.Scripts.Models.Model>();
					LinkArray<Assets.Scripts.Models.Towers.Mods.ApplyModModel>();
					LinkArray<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
					LinkArray<Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionBehaviorModel>();
					LinkArray<Assets.Scripts.Models.Towers.Filters.FilterModel>();
					LinkArray<Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel>();
					LinkArray<Assets.Scripts.Models.Bloons.BloonBehaviorModel>();
					LinkArray<Assets.Scripts.Models.Towers.TowerBehaviorModel>();
					LinkArray<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
					LinkArray<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>();
					LinkArray<Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel>();
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
