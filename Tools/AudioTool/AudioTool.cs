#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Experimental.EditorVR.Proxies;
using UnityEditor.Experimental.EditorVR.Utilities;
using UnityEngine;
using UnityEngine.InputNew;

namespace UnityEditor.Experimental.EditorVR.Tools
{
    public enum AudioComponentType
    {
        Source,
        ReverbZone,
        ReverbFilter,
        HighPassFilter,
        LowPassFilter,
        ChorusFilter,
        DistortionFilter,
        EchoFilter
    }

    public enum AudioCreationState
    {
        Idle,
        Modifying,
        End
    }

    [MainMenuItem("Audio", "Create", "Place & edit scene audio")]
    sealed class AudioTool : MonoBehaviour, ITool, IStandardActionMap, IConnectInterfaces, IInstantiateMenuUI,
        IUsesRayOrigin, IUsesSpatialHash, IUsesViewerScale, ISelectTool, IIsHoveringOverUI, IIsMainMenuVisible,
        IRayVisibilitySettings, IMenuIcon, IRequestFeedback, IUsesNode, IUsesDirectSelection
    {
        [SerializeField]
        CreatePrimitiveMenu m_MenuPrefab;

        [SerializeField]
        AudioToolMenu m_AudioMenuPrefab;

        [SerializeField]
        Sprite m_Icon;

        const float k_DrawDistance = 0.075f;

        GameObject m_ToolMenu;

        [SerializeField]
        AudioComponentType m_SelectedComponentType = AudioComponentType.ReverbZone; // new

        PrimitiveType m_SelectedPrimitiveType = PrimitiveType.Cube;
        bool m_Freeform;

        GameObject m_CurrentGameObject;

        Vector3 m_StartPoint = Vector3.zero;
        Vector3 m_EndPoint = Vector3.zero;

        PrimitiveCreationStates m_State = PrimitiveCreationStates.StartPoint;

        public Transform rayOrigin { get; set; }
        public Node node { get; set; }

        public Sprite icon { get { return m_Icon; } }

        enum PrimitiveCreationStates
        {
            StartPoint,
            EndPoint,
            Freeform
        }

        void Start()
        {
            // Clear selection so we can't manipulate things
            Selection.activeGameObject = null;

            m_ToolMenu = this.InstantiateMenuUI(rayOrigin, m_AudioMenuPrefab);
            var audioToolMenu = m_ToolMenu.GetComponent<AudioToolMenu>();
            this.ConnectInterfaces(audioToolMenu, rayOrigin);

            audioToolMenu.selectComponent = SetSelectedComponent;
            audioToolMenu.close = Close;

            var controls = new BindingDictionary();
            InputUtils.GetBindingDictionaryFromActionMap(standardActionMap, controls);

            foreach (var control in controls)
            {
                foreach (var id in control.Value)
                {
                    var request = (ProxyFeedbackRequest)this.GetFeedbackRequestObject(typeof(ProxyFeedbackRequest));
                    request.node = node;
                    request.control = id;
                    request.tooltipText = "Audio Tool Init ??";
                    this.AddFeedbackRequest(request);
                }
            }
        }

        public void ProcessInput(ActionMapInput input, ConsumeControlDelegate consumeControl)
        {
            if (!IsActive())
                return;

            var standardInput = (Standard)input;

            switch (m_State)
            {
                case PrimitiveCreationStates.StartPoint:
                {
                    HandleStartPoint(standardInput, consumeControl);
                    break;
                }
                case PrimitiveCreationStates.EndPoint:
                {
                    UpdatePositions();
                    CheckForTriggerRelease(standardInput, consumeControl);
                    break;
                }
                case PrimitiveCreationStates.Freeform:
                {
                    UpdatePositions();
                    CheckForTriggerRelease(standardInput, consumeControl);
                    break;
                }
            }

            if (m_State == PrimitiveCreationStates.StartPoint && this.IsHoveringOverUI(rayOrigin))
                this.RemoveRayVisibilitySettings(rayOrigin, this);
            else
                this.AddRayVisibilitySettings(rayOrigin, this, false, true);
        }

        void SetSelectedComponent(AudioComponentType type)
        {
            m_SelectedComponentType = type;
            Debug.Log("set selected comp type: " + type);
        }

        GameObject TryGetRayDirectSelection()
        {
            GameObject raySelection = null;
            var selections = this.GetDirectSelection();
            if (selections != null)
                selections.TryGetValue(rayOrigin, out raySelection);

            return raySelection;
        }

        void HandleStartPoint(Standard standardInput, ConsumeControlDelegate consumeControl)
        {
            m_CurrentGameObject = null;
            if (standardInput.action.wasJustPressed)
            {
                consumeControl(standardInput.action);

                Debug.Log("standard action press in audio tool!");
                m_CurrentGameObject = Selection.activeGameObject;

                if (m_CurrentGameObject == null)
                {
                    Debug.Log("no object selected to add audio components to!");
                    return;
                }
                else
                {
                    Debug.Log("selected object found!", m_CurrentGameObject);
                    AddSelectedComponent();
                    return;
                }

                /*
                Undo.RegisterCreatedObjectUndo(m_CurrentGameObject, "Create Primitive");

                // Set starting minimum scale (don't allow zero scale object to be created)
                const float kMinScale = 0.0025f;
                var viewerScale = this.GetViewerScale();
                m_CurrentGameObject.transform.localScale = Vector3.one * kMinScale * viewerScale;
                m_StartPoint = rayOrigin.position + rayOrigin.forward * k_DrawDistance * viewerScale;
                m_CurrentGameObject.transform.position = m_StartPoint;

                m_State = m_Freeform ? PrimitiveCreationStates.Freeform : PrimitiveCreationStates.EndPoint;

                this.AddToSpatialHash(m_CurrentGameObject);

                consumeControl(standardInput.action);
                Selection.activeGameObject = m_CurrentGameObject;
                */
            }
        }

        void AddSelectedComponent()
        {
            switch (m_SelectedComponentType)
            {
                case AudioComponentType.Source:
                    AddSource(m_CurrentGameObject);
                    break;
                case AudioComponentType.ReverbZone:
                    AddReverbZone(m_CurrentGameObject);
                    break;
                case AudioComponentType.ReverbFilter:
                    AddReverbFilter(m_CurrentGameObject);
                    break;
                case AudioComponentType.LowPassFilter:
                    AddLowPassFilter(m_CurrentGameObject);
                    break;
                case AudioComponentType.HighPassFilter:
                    AddHighPassFilter(m_CurrentGameObject);
                    break;
                case AudioComponentType.ChorusFilter:
                    AddChorusFilter(m_CurrentGameObject);
                    break;
                case AudioComponentType.DistortionFilter:
                    AddDistortionFilter(m_CurrentGameObject);
                    break;
                case AudioComponentType.EchoFilter:
                    AddEchoFilter(m_CurrentGameObject);
                    break;
            }
        }

        AudioSource AddSource(GameObject go)
        {
            return go.TryAddComponent<AudioSource>();
        }

        AudioReverbZone AddReverbZone(GameObject go)
        {
            return go.TryAddComponent<AudioReverbZone>();
        }

        AudioReverbFilter AddReverbFilter(GameObject go)
        {
            return TryAddFilter<AudioReverbFilter>(go);
        }

        AudioLowPassFilter AddLowPassFilter(GameObject go)
        {
            return TryAddFilter<AudioLowPassFilter>(go);
        }

        AudioHighPassFilter AddHighPassFilter(GameObject go)
        {
            return TryAddFilter<AudioHighPassFilter>(go);
        }

        AudioChorusFilter AddChorusFilter(GameObject go)
        {
            return TryAddFilter<AudioChorusFilter>(go);
        }

        AudioDistortionFilter AddDistortionFilter(GameObject go)
        {
            return TryAddFilter<AudioDistortionFilter>(go);
        }

        AudioEchoFilter AddEchoFilter(GameObject go)
        {
            return TryAddFilter<AudioEchoFilter>(go);
        }

        T TryAddFilter<T>(GameObject go) where T: Component
        {
            if (HasSourceOrListener(go))
            {
                return go.TryAddComponent<T>();
            }
            else
            {
                Debug.LogWarning("Filters need an Source or Listener on the object!");
                return null;
            }
        }

        bool HasSourceOrListener(GameObject go)
        {
            if (go.GetComponent<AudioSource>() != null)
                return true;
            else if (go.GetComponent<AudioListener>() != null)
                return true;

            return false;
        }

        void UpdatePositions()
        {
            m_EndPoint = rayOrigin.position + rayOrigin.forward * k_DrawDistance * this.GetViewerScale();
            m_CurrentGameObject.transform.position = (m_StartPoint + m_EndPoint) * 0.5f;
        }

        void CheckForTriggerRelease(Standard standardInput, ConsumeControlDelegate consumeControl)
        {
            // Ready for next object to be created
            if (standardInput.action.wasJustReleased)
            {
                m_State = PrimitiveCreationStates.StartPoint;
                Undo.IncrementCurrentGroup();

                consumeControl(standardInput.action);
            }
        }

        bool IsActive()
        {
            return !this.IsMainMenuVisible(rayOrigin);
        }

        void Close()
        {
            this.SelectTool(rayOrigin, GetType());
        }

        void OnDestroy()
        {
            ObjectUtils.Destroy(m_ToolMenu);

            if (rayOrigin == null)
                return;

            this.RemoveRayVisibilitySettings(rayOrigin, this);
            this.ClearFeedbackRequests();
        }

        public void OnResetDirectSelectionState()
        {
            // do nothing for now
        }

        public ActionMap standardActionMap { private get; set; }
    }
}
#endif
