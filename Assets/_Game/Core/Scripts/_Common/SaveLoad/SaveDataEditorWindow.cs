#if UNITY_EDITOR
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Core.SaveLoad
{
    public class SaveDataEditorWindow : EditorWindow
    {
        private const string ProviderPreference = "Game.SaveDataEditor.Provider";
        private ISaveLoadService _saveService;
        private Type[] _providerTypes;
        private string[] _providerNames;
        private int _providerIndex;
        private PropertyTree _tree;
        private SaveData _displayedData;
        private Vector2 _scroll;
        private bool _hasChanges;
        private bool _working;
        private bool _ownsProvider;
        private int _connectionVersion;
        private string _error;

        [MenuItem("Game/Save Data Viewer")]
        public static void Open() => GetWindow<SaveDataEditorWindow>("Save Data");

        private void OnEnable()
        {
            _providerTypes = TypeCache.GetTypesDerivedFrom<ISaveLoadService>()
                .Where(type => type.IsClass && type.IsPublic && !type.IsAbstract
                    && !type.ContainsGenericParameters && type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => type.Name).ToArray();
            _providerNames = _providerTypes.Select(type => type.Name).ToArray();
            string preferred = EditorPrefs.GetString(ProviderPreference, nameof(SaveServiceBinary));
            _providerIndex = Math.Max(0, Array.FindIndex(_providerTypes, type => type.Name == preferred));
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Connect();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            ReleaseProvider();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            ReleaseProvider();
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
                Connect();
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying && !_working)
            {
                var live = LifetimeScope.Find<RootLifetimeScope>()?.Container?.Resolve<ISaveLoadService>();
                if (live != null && !ReferenceEquals(live, _saveService))
                {
                    ReleaseProvider();
                    _saveService = live;
                }
            }
            Repaint();
        }

        private void ReleaseProvider()
        {
            _connectionVersion++;
            _working = false;
            _tree?.Dispose();
            _tree = null;
            _displayedData = null;
            // The container owns the live service. Only dispose standalone providers.
            if (_ownsProvider && _saveService is IDisposable disposable)
                disposable.Dispose();
            _ownsProvider = false;
            _saveService = null;
            _hasChanges = false;
            _error = null;
        }

        private void Connect()
        {
            ReleaseProvider();
            try
            {
                if (Application.isPlaying)
                    _saveService = LifetimeScope.Find<RootLifetimeScope>()?.Container?.Resolve<ISaveLoadService>();
                else if (_providerTypes.Length > 0)
                {
                    _saveService = (ISaveLoadService)Activator.CreateInstance(_providerTypes[_providerIndex]);
                    _ownsProvider = true;
                }

                if (_saveService != null && !Application.isPlaying && _saveService.CanUseInEditMode
                    && !_saveService.IsSaveDataLoaded)
                    RunOperation(_saveService.LoadDataAsync).Forget();
            }
            catch (Exception error)
            {
                _error = error.GetBaseException().Message;
            }
            Repaint();
        }

        private void OnGUI()
        {
            bool available = _saveService != null
                && (Application.isPlaying || _saveService.CanUseInEditMode);
            bool busy = _working || (_saveService?.IsBusy ?? false);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                using (new EditorGUI.DisabledScope(Application.isPlaying || busy))
                {
                    int index = EditorGUILayout.Popup(_providerIndex, _providerNames, EditorStyles.toolbarPopup);
                    if (index != _providerIndex && ConfirmDiscard())
                    {
                        _providerIndex = index;
                        EditorPrefs.SetString(ProviderPreference, _providerTypes[index].Name);
                        Connect();
                    }
                }
                if (Application.isPlaying)
                    GUILayout.Label(_saveService?.GetType().Name ?? "Waiting for service", EditorStyles.miniLabel);
                using (new EditorGUI.DisabledScope(!available || busy))
                {
                    if (GUILayout.Button("Reload", EditorStyles.toolbarButton) && ConfirmDiscard())
                        RunOperation(_saveService.LoadDataAsync).Forget();
                    using (new EditorGUI.DisabledScope(!_hasChanges || !(_saveService?.IsSaveDataLoaded ?? false)))
                        if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                            RunOperation(_saveService.SaveDataAsync).Forget();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete Save Data", EditorStyles.toolbarButton)
                        && EditorUtility.DisplayDialog("Delete Save Data",
                            "Delete the selected provider's save and reset in-memory data?", "Delete", "Cancel"))
                        RunOperation(_saveService.DeleteDataAsync).Forget();
                }
            }

            if (!string.IsNullOrEmpty(_error ?? _saveService?.LastError?.Message))
                EditorGUILayout.HelpBox(_error ?? _saveService.LastError.Message, MessageType.Error);
            if (_saveService == null)
            {
                EditorGUILayout.HelpBox(Application.isPlaying
                    ? "Waiting for the runtime save service." : "No save provider available.", MessageType.Info);
                return;
            }
            EditorGUILayout.HelpBox(_saveService.StorageDescription, MessageType.Info);
            if (!available)
            {
                EditorGUILayout.HelpBox("This provider requires Play Mode. The viewer uses the active runtime provider in Play Mode.", MessageType.Info);
                return;
            }
            if (busy) EditorGUILayout.LabelField("Storage operation in progress...");
            if (!_saveService.IsSaveDataLoaded)
            {
                EditorGUILayout.HelpBox("Save data has not been loaded successfully. Use Reload to retry.", MessageType.Warning);
                return;
            }

            using (new EditorGUI.DisabledScope(busy))
            {
                if (!_saveService.HasSaveData)
                {
                    EditorGUILayout.HelpBox("No stored save. Showing default data.", MessageType.Info);
                    if (GUILayout.Button("Create Save Data")) RunOperation(_saveService.SaveDataAsync).Forget();
                }
                if (!ReferenceEquals(_displayedData, _saveService.Data))
                {
                    _tree?.Dispose();
                    _displayedData = _saveService.Data;
                    _tree = PropertyTree.Create(_displayedData, SerializationBackend.Odin);
                }
                using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
                {
                    _scroll = scroll.scrollPosition;
                    EditorGUI.BeginChangeCheck();
                    _tree.Draw(false);
                    if (EditorGUI.EndChangeCheck()) _hasChanges = true;
                }
            }
        }

        private bool ConfirmDiscard() => !_hasChanges || EditorUtility.DisplayDialog(
            "Unsaved changes", "Discard unsaved changes in this viewer?", "Discard", "Cancel");

        private async UniTask RunOperation(Func<UniTask> operation)
        {
            _working = true;
            _error = null;
            var service = _saveService;
            int connectionVersion = _connectionVersion;
            try
            {
                await operation();
                if (connectionVersion == _connectionVersion && ReferenceEquals(service, _saveService))
                    _hasChanges = false;
            }
            catch (Exception error)
            {
                if (connectionVersion == _connectionVersion && ReferenceEquals(service, _saveService))
                    _error = error.Message;
            }
            finally
            {
                if (connectionVersion == _connectionVersion) _working = false;
                if (this != null) Repaint();
            }
        }
    }
}
#endif
