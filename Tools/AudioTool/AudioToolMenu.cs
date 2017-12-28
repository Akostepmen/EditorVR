#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.EditorVR.Menus;
using UnityEditor.Experimental.EditorVR.Utilities;
using UnityEngine;

namespace UnityEditor.Experimental.EditorVR.Tools
{
    sealed class AudioToolMenu : MonoBehaviour, IMenu
    {
        [SerializeField]
        GameObject[] m_HighlightObjects;

        public Action<AudioComponentType> selectComponent;
        public Action close;

        public Bounds localBounds { get; private set; }
        public int priority { get { return 1; } }

        public MenuHideFlags menuHideFlags
        {
            get { return gameObject.activeSelf ? 0 : MenuHideFlags.Hidden; }
            set { gameObject.SetActive(value == 0); }
        }

        public GameObject menuContent { get { return gameObject; } }

        void Awake()
        {
            localBounds = ObjectUtils.GetBounds(transform);
        }

        public void SelectAudioComponent(int type)
        {
            selectComponent((AudioComponentType)type);
            HighlightSelected(type);
        }

        void HighlightSelected(int index)
        {
            for (var i = 0; i < m_HighlightObjects.Length; i++)
                m_HighlightObjects[i].SetActive(i == index);
        }

        public void SelectFreeformCuboid()
        {
            //selectPrimitive(PrimitiveType.Cube, true);

            foreach (var go in m_HighlightObjects)
                go.SetActive(false);
        }

        public void Close()
        {
            close();
        }
    }
}
#endif
