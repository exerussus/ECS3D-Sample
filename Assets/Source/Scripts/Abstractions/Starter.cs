using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.UnityEditor;
using Source.Scripts.Data;
using UnityEngine;

namespace Source.Scripts.Abstractions
{
    public abstract class Starter : MonoBehaviour
    {
        private EcsWorld _world;
        private IEcsSystems _initSystems;
        private IEcsSystems _updateSystems;
        private IEcsSystems _fixedUpdateSystems;
        private IEcsSystems _lateUpdateSystems;
        [SerializeField] private GameData gameData;
        
#if UNITY_EDITOR
        IEcsSystems _editorSystems;
#endif

        private void Start() 
        {        
            DontDestroyOnLoad(gameObject);
            _world = new EcsWorld();

            PrepareInitSystems();
            PrepareUpdateSystems();
            PrepareFixedUpdateSystems();
            PrepareLateUpdateSystems();
        }

        protected abstract void SetInitSystems(IEcsSystems initSystems);
        protected abstract void SetUpdateSystems(IEcsSystems updateSystems);
        protected abstract void SetFixedUpdateSystems(IEcsSystems fixedUpdateSystems);
        protected abstract void SetLateUpdateSystems(IEcsSystems lateUpdateSystems);
        
        private void Update() 
        {
            _updateSystems?.Run();
#if UNITY_EDITOR
            // Выполняем обновление состояния отладочных систем. 
            _editorSystems?.Run ();
#endif
        }

        private void FixedUpdate()
        {
            _fixedUpdateSystems?.Run();
        }
        private void LateUpdate()
        {
            _lateUpdateSystems?.Run();
        }

        private void PrepareInitSystems()
        {
            _initSystems = new EcsSystems(_world, gameData);
#if UNITY_EDITOR
            _editorSystems = new EcsSystems (_initSystems.GetWorld ());
            _editorSystems
                .Add (new EcsWorldDebugSystem ())
                .Inject()
                .Init ();
#endif
            SetInitSystems(_initSystems);
#if UNITY_EDITOR
            _initSystems.Add(new EcsWorldDebugSystem())
#endif
                .Inject();
            _initSystems.Init();
        }
        
        private void PrepareUpdateSystems()
        {
            _updateSystems = new EcsSystems(_world, gameData);
            SetUpdateSystems(_updateSystems);
            _updateSystems.Inject();
            _updateSystems.Init();
        }
        
        private void PrepareFixedUpdateSystems()
        {
            _fixedUpdateSystems = new EcsSystems(_world, gameData);
            SetFixedUpdateSystems(_fixedUpdateSystems);
            _fixedUpdateSystems.Inject();
            _fixedUpdateSystems.Init();
        }
        
        private void PrepareLateUpdateSystems()
        {
            _lateUpdateSystems = new EcsSystems(_world, gameData);
            SetLateUpdateSystems(_lateUpdateSystems);
            _lateUpdateSystems.Inject();
            _lateUpdateSystems.Init();
        }
        
        private void OnDestroy() 
        {
#if UNITY_EDITOR
            if (_editorSystems != null) {
                _editorSystems.Destroy ();
                _editorSystems = null;
            }
#endif
            _initSystems.Destroy();
        }
    }
}
