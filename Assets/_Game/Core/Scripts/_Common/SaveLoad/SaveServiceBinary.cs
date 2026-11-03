using System.IO;
using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine;

namespace Game.Core.SaveLoad
{
    public class SaveServiceBinary : SaveLoadService, ISaveLoadService
    {
        protected const string _saveFolderName = "SaveData/";
        protected const string _saveFileName = "savedata.file";

        public override string StorageDescription => Path.Combine(
            Application.isEditor ? Application.dataPath : Application.persistentDataPath,
            "MMData", _saveFolderName, _saveFileName);

        public SaveServiceBinary()
        {
            LoadData();
        }

        protected override UniTask<SaveData> ReadDataAsync() => UniTask.FromResult(
            (SaveData)MMSaveLoadManager.Load(typeof(SaveData), _saveFileName, _saveFolderName));

        protected override UniTask WriteDataAsync(SaveData data)
        {
            MMSaveLoadManager.Save(data, _saveFileName, _saveFolderName);
            return UniTask.CompletedTask;
        }

        protected override UniTask RemoveDataAsync()
        {
            MMSaveLoadManager.DeleteSave(_saveFileName, _saveFolderName);
            return UniTask.CompletedTask;
        }
    }
}
