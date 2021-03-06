// ----------------------------------------------------------------------------
// The MIT License
// UnityEditor integration https://github.com/Leopotam/ecslite-unityeditor
// for LeoECS Lite https://github.com/Leopotam/ecslite
// Copyright (c) 2021 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Leopotam.EcsLite.UnityEditor {
    public sealed class EcsWorldDebugSystem : IEcsPreInitSystem, IEcsRunSystem, IEcsWorldEventListener {
        readonly string _worldName;
        readonly GameObject _rootGO;
        readonly Transform _entitiesRoot;
        readonly bool _bakeComponentsInName;
        EcsWorld _world;
        EcsEntityDebugView[] _entities;
        Dictionary<int, byte> _dirtyEntities;
        Type[] _typesCache;
        private EcsPool<UnityDebugName> _namePool;
        private EcsFilter _debugNameFilter;

        public EcsWorldDebugSystem (string worldName = null, bool bakeComponentsInName = true) {
            _bakeComponentsInName = bakeComponentsInName;
            _worldName = worldName;
            _rootGO = new GameObject (_worldName != null ? $"[ECS-WORLD {_worldName}]" : "[ECS-WORLD]");
            Object.DontDestroyOnLoad (_rootGO);
            _rootGO.hideFlags = HideFlags.NotEditable;
            _entitiesRoot = new GameObject ("Entities").transform;
            _entitiesRoot.gameObject.hideFlags = HideFlags.NotEditable;
            _entitiesRoot.SetParent (_rootGO.transform, false);
        }

        public void PreInit (EcsSystems systems) {
            _world = systems.GetWorld (_worldName);
            if (_world == null) { throw new Exception ("Cant find required world."); }
            _entities = new EcsEntityDebugView [_world.GetWorldSize ()];
            _dirtyEntities = new Dictionary<int, byte> (_entities.Length);
            _world.AddEventListener (this);
            if (!_bakeComponentsInName)
            {
                _namePool = _world.GetPool<UnityDebugName>();
                _debugNameFilter = _world.Filter<UnityDebugName>().End();
            }
        }

        public void Run (EcsSystems systems) {
            if (_bakeComponentsInName)
            {
                foreach (var pair in _dirtyEntities)
                {
                    var entity = pair.Key;
                    var entityName = entity.ToString("X8");
                    if (_world.GetEntityGen(entity) > 0)
                    {
                        var count = _world.GetComponentTypes(entity, ref _typesCache);
                        for (var i = 0; i < count; i++)
                        {
                            entityName = $"{entityName}:{EditorExtensions.GetCleanGenericTypeName(_typesCache[i])}";
                        }
                    }

                    _entities[entity].name = entityName;
                }

                _dirtyEntities.Clear();
            }
            else
            {
                foreach (var entity in _debugNameFilter)
                {
                    ref UnityDebugName nameComponent = ref _namePool.Get(entity);
                    if (nameComponent.changed)
                        _entities[entity].name = nameComponent.DisplayName;
                }
            }
        }

        public void OnEntityCreated (int entity) {
            if (!_entities[entity]) {
                var go = new GameObject ();
                go.transform.SetParent (_entitiesRoot, false);
                var entityObserver = go.AddComponent<EcsEntityDebugView> ();
                entityObserver.Entity = entity;
                entityObserver.World = _world;
                _entities[entity] = entityObserver;
                if (_bakeComponentsInName) {
                    _dirtyEntities[entity] = 1;
                } else {
                    go.name = entity.ToString ("X8");
                }
            }
            _entities[entity].gameObject.SetActive (true);
        }

        public void OnEntityDestroyed (int entity) {
            if (_entities[entity]) {
                _entities[entity].gameObject.SetActive (false);
            }
        }

        public void OnEntityChanged (int entity) {
            if (_bakeComponentsInName) {
                _dirtyEntities[entity] = 1;
            }
        }

        public void OnFilterCreated (EcsFilter filter) { }

        public void OnWorldResized (int newSize) {
            Array.Resize (ref _entities, newSize);
        }

        public void OnWorldDestroyed (EcsWorld world) {
            _world.RemoveEventListener (this);
            Object.Destroy (_rootGO);
        }
    }
}