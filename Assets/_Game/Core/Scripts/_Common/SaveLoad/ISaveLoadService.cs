using System;
using Cysharp.Threading.Tasks;

namespace Game.Core.SaveLoad
{
    public interface ISaveLoadService
    {
        SaveData Data { get; }
        // Loaded includes a successful read with no existing save (new player).
        bool IsSaveDataLoaded { get; }
        bool HasSaveData { get; }
        bool IsBusy { get; }
        bool CanUseInEditMode { get; }
        string StorageDescription { get; }
        Exception LastError { get; }

        void LoadData();
        void SaveData();
        UniTask LoadDataAsync();
        UniTask SaveDataAsync();
        UniTask DeleteDataAsync();
        void ResetData();
    }
}
