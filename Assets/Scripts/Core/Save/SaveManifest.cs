using System;
using System.Collections.Generic;

namespace Santa.Core.Save
{
    [Serializable]
    public class SaveManifest
    {
        public List<BackupEntry> Backups = new List<BackupEntry>();
    }

    [Serializable]
    public struct BackupEntry
    {
        public string Key;
        public long Timestamp;
    }
}
