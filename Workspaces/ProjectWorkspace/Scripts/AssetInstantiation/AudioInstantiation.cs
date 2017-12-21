using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.EditorVR.Data;

namespace UnityEditor.Experimental.EditorVR.Utilities
{
    internal static class AudioInstantiation
    {
        const string k_AudioClipAttachUndoLabel = "Attach Audio Clip";
        const string k_AudioClipObjectUndoLabel = "Create Audio Object";

        internal static GameObject PlaceAudioObject(AssetData data, Vector3 position)
        {
            var go = new GameObject("Audio: " + data.asset.name);
            go.transform.position = position;

            var source = go.AddComponent<AudioSource>();
            source.clip = (AudioClip)data.asset;

            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, k_AudioClipObjectUndoLabel);
            return go;
        }

        internal static AudioSource AttachAudioClip(GameObject go, AssetData data)
        {
            Undo.RecordObject(go, k_AudioClipAttachUndoLabel);

            var source = go.GetComponent<AudioSource>();
            if (source == null)
                source = go.AddComponent<AudioSource>();

            source.clip = (AudioClip)data.asset;

            Selection.activeGameObject = go;
            Undo.IncrementCurrentGroup();

            return source;
        }

    }
}
