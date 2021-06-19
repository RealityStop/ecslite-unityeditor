using Leopotam.EcsLite.UnityEditor;
using UnityEngine;

namespace Leopotam.EcsLite
{
	public struct UnityDebugName
	{
		public string DisplayName;

		public bool changed;
	}
	
	public static class UnityDebugNamingExt
	{
		/// <summary>
		/// Creates a new entity.  If running inside the unity editor, the passed name will be used
		/// in the debug hierarchy.  (In builds, this is ignored)
		/// </summary>
		public static int NewEntity(this EcsWorld world, string entityName)
		{
			int result = world.NewEntity();
#if UNITY_EDITOR
			ref UnityDebugName nameComponent = ref world.GetPool<UnityDebugName>().Add(result);
			nameComponent.DisplayName = entityName;
			nameComponent.changed = true;
#endif
			return result;
		}
		
		/// <summary>
		/// Sets the debug name shown for an entity in the debug hierarchy, if running
		/// inside the unity editor. (In builds, this is ignored)
		/// </summary>
		public static void UpdateEntityName(this EcsWorld world, int entity, string entityName)
		{
#if UNITY_EDITOR
			var pool = world.GetPool<UnityDebugName>();
			ref UnityDebugName nameComponent = ref pool.GetOrCreate(entity);
			nameComponent.DisplayName = entityName;
			nameComponent.changed = true;
#endif
		}
		
	
		private static ref T GetOrCreate<T>(this EcsPool<T> self, int entity) where T : struct
		{
			if (!self.Has(entity))
			{
				return ref self.Add(entity);
			}

			return ref self.Get(entity);
		}
	}
	
	/// <summary>
	/// Handles integrating entity names into the Unity Hierarchy.  Should only be added
	/// under a `#if UNITY_EDITOR` context.
	/// </summary>
	public class UnityDebugEntityNameUpdater : IEcsRunSystem, IEcsInitSystem
	{
		private readonly EcsWorldObserver _observer;
		private EcsWorld _world;
		private EcsPool<UnityDebugName> _namePool;
		private EcsFilter _filter;

		
		/// <summary>
		/// Creates the name updater for a particular world.  Takes an EcsWorldObserver, but
		/// most of the time the simply provide the output of the `EcsWorldObserver.Create` call.
		/// </summary>
		public UnityDebugEntityNameUpdater(EcsWorldObserver observer)
		{
			_observer = observer;
		}
		
		/// <summary>
		/// Creates the name updater for a particular world.  Takes the output of the
		/// `EcsWorldObserver.Create` call. 
		/// </summary>
		public UnityDebugEntityNameUpdater(GameObject observer) : this(observer.GetComponent<EcsWorldObserver>())
		{
		}
			
		public void Init(EcsSystems systems)
		{
			_world = systems.GetWorld();
			_namePool = _world.GetPool<UnityDebugName>();
			_filter = _world.Filter<UnityDebugName>().End();
		}
			
		public void Run(EcsSystems systems)
		{
			foreach (var entity in _filter)
			{
				ref UnityDebugName nameComponent = ref _namePool.Get(entity);
				if (nameComponent.changed)
					_observer.EntityGameObjects[entity].name = nameComponent.DisplayName;
			}
		}
	}
}