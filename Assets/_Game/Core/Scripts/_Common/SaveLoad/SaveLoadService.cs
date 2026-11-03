using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core.SaveLoad
{
    public abstract class SaveLoadService : ISaveLoadService
    {
        public SaveData Data => _saveData;
        public bool IsSaveDataLoaded { get; private set; }
        public bool HasSaveData { get; private set; }
        public bool IsBusy => _pendingOperations > 0;
        public virtual bool CanUseInEditMode => true;
        public abstract string StorageDescription { get; }
        public Exception LastError { get; private set; }

        protected SaveData _saveData = new();
        private UniTask _operationTail = UniTask.CompletedTask;
        private int _pendingOperations;

        // Gameplay can request saves without awaiting them. Startup and tools use
        // the async API to observe completion and handle failures.
        public void LoadData() => LoadDataAsync().Forget(Debug.LogException);
        public void SaveData() => SaveDataAsync().Forget(Debug.LogException);

        public UniTask LoadDataAsync() => ExecuteAsync(async () =>
        {
            IsSaveDataLoaded = false;
            var data = await ReadDataAsync();
            _saveData = data ?? new SaveData();
            HasSaveData = data != null;
            IsSaveDataLoaded = true;
        });

        public UniTask SaveDataAsync() => ExecuteAsync(async () =>
        {
            EnsureLoaded();
            await WriteDataAsync(_saveData);
            HasSaveData = true;
        });

        public UniTask DeleteDataAsync() => ExecuteAsync(async () =>
        {
            await RemoveDataAsync();
            _saveData = new SaveData();
            HasSaveData = false;
            IsSaveDataLoaded = true;
        });

        public void ResetData()
        {
            EnsureLoaded();
            if (IsBusy)
                throw new InvalidOperationException("Wait for the current storage operation before resetting data.");
            _saveData = new SaveData();
        }

        protected abstract UniTask<SaveData> ReadDataAsync();
        protected abstract UniTask WriteDataAsync(SaveData data);
        protected abstract UniTask RemoveDataAsync();

        private void EnsureLoaded()
        {
            if (!IsSaveDataLoaded)
                throw new InvalidOperationException("Save data must be loaded successfully before it can be changed or saved.");
        }

        private async UniTask ExecuteAsync(Func<UniTask> operation)
        {
            // Serialize reads, writes and deletes to keep storage operations in order.
            var previous = _operationTail;
            var completion = new UniTaskCompletionSource();
            _operationTail = completion.Task;
            _pendingOperations++;
            try
            {
                await previous;
                LastError = null;
                await operation();
            }
            catch (Exception error)
            {
                LastError = error;
                throw;
            }
            finally
            {
                _pendingOperations--;
                completion.TrySetResult();
            }
        }

        public virtual FieldInfo GetFieldInfo(object name) => typeof(SaveData).GetField(
            name?.ToString() ?? string.Empty, BindingFlags.Public | BindingFlags.Instance);

        public virtual object GetProp(string name) => GetFieldInfo(name)?.GetValue(_saveData);

        public virtual void SetProp(string name, object value, bool toSave = true)
        {
            var field = GetFieldInfo(name);
            if (field == null) return;
            field.SetValue(_saveData, value);
            if (toSave) SaveData();
        }
    }
}
