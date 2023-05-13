using System;
using System.Collections.Generic;
using System.Linq;

using Modding;

using Osmi;
using Osmi.Utils;

using UnityEngine;

using UnityEngine.SceneManagement;

using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace NoParticles;

public sealed partial class NoParticles : Mod {
	private static readonly Dictionary<string, string[]> excludedInSceneGameObjects = new() {
		[""] = new[] {
			"Gas Attack" // Various atk
		},
		["DontDestroyOnLoad"] = new[] {
			"GlobalPool"
		},
		["GG_Ghost_Galien"] = new[] {
			"Summon Pt"
		},
		["GG_Radiance"] = new[] {
			"Shot Charge 2" // AbsRad Orbs antic
		}
	};

	private static readonly string[] excludedPooledGameObjects = new[] {
		"Electro Zap(Clone)", // Volt Twister atk
		"Flame Trail(Clone)", // NKG Dive Dash trails
		"Gas Projectile(Clone)", // Fungoon atk
		"Mega Jelly Zap(Clone)", // Uumuu Chase
		"Grimm UP Ball(Clone)", // TMG Dash Uppercut fireballs
		"Nightmare UP Ball(Clone)", // NKG Dash Uppercut fireballs
		"particle_orange blood(Clone)", // Flukemarm Spawn
		"Grimm_flare_pillar(Clone)", // NKG Flame Pillars
		"Shot Markoth Nail(Clone)"
	};

	private static readonly Lazy<string> version = AssemblyUtil
#if DEBUG
		.GetMyDefaultVersionWithHash();
#else
		.GetMyDefaultVersion();
#endif

	public override string GetVersion() => version.Value;

	public override void Initialize() {
		OsmiHooks.GameInitializedHook += DisableStartupParticleSystems;
		OsmiHooks.SceneChangeHook += DisableNewSceneParticleSystems;
		ModHooks.ObjectPoolSpawnHook += DisableSpawnedObjectParticleSystems;
		On.HeroController.Start += DisableHeroParticleSystems;
	}

	private static void DisableStartupParticleSystems() {
		DisableParticleSystemsInScene(USceneManager.GetActiveScene());
		DisableParticleSystemsInScene(Ref.DDOL);
	}

	private static void DisableNewSceneParticleSystems(Scene prev, Scene next) =>
		DisableParticleSystemsInScene(next);

	private static GameObject DisableSpawnedObjectParticleSystems(GameObject go) {
		if (!excludedPooledGameObjects.Contains(go.name)) {
			DisableParticleSystems(go.transform);
		}

		return go;
	}

	private static void DisableHeroParticleSystems(On.HeroController.orig_Start orig, HeroController self) {
		orig(self);
		DisableParticleSystems(self.transform);
		DisableParticleSystems(self.takeHitDoublePrefab.transform);
	}

	private static void DisableParticleSystemsInScene(Scene scene) =>
		scene.GetRootGameObjects().ForEach(go => DisableParticleSystems(go.transform));

	private static void DisableParticleSystems(Transform transform) {
		if (transform.gameObject.scene.name is string scene
			&& excludedInSceneGameObjects.TryGetValue(scene, out string[] names)) {
			DisableParticleSystems(transform, name => names.Contains(name) || excludedInSceneGameObjects[""].Contains(name));
		} else {
			DisableParticleSystems(transform, excludedInSceneGameObjects[""].Contains);
		}
	}

	private static void DisableParticleSystems(Transform transform, Func<string, bool> predicate) {
		if (predicate.Invoke(transform.name)) {
			return;
		}

		foreach (ParticleSystem particle in transform.GetComponents<ParticleSystem>()) {
			ParticleSystem.EmissionModule emission = particle.emission;
			emission.enabled = false;
			emission.rateOverTimeMultiplier = 0;
		}

		foreach (Transform tf in transform) {
			DisableParticleSystems(tf, predicate);
		}
	}
}
