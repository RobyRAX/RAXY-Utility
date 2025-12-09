using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Utility.Gameplay
{
    public class NamedTransformSet : MonoBehaviour
    {
        [TitleGroup("Entry")]
        [TableList(ShowIndexLabels = true)]
        public List<TransformEntry> entries;
        public Dictionary<string, TransformEntry> EntryDict = new();

        void Awake()
        {
            foreach (var entry in entries)
            {
                EntryDict.Add(entry.entryId, entry);
            }
        }

        public TransformEntry GetEntry(string entryId)
        {
            return EntryDict[entryId];
        }

        public TransformEntry GetEntry(int index)
        {
            return entries[index];
        }
    }

    [Serializable]
    public class TransformEntry
    {
        [TableColumnWidth(150, false)]
        public string entryId;
        public Transform tranform;

        [TableColumnWidth(75, false)]
        [Button]
        void SetId()
        {
            entryId = tranform?.name ?? "";
        }
    }
}
