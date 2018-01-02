#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Experimental.EditorVR.Proxies;
using UnityEditor.Experimental.EditorVR.Utilities;
using UnityEngine;
using UnityEngine.InputNew;


namespace UnityEditor.Experimental.EditorVR.Tools
{
    public enum AudioCreationState
    {
        Evaluating,
        Executing,
        SuccessFeedback,
        FailureFeedback
    }

    [MainMenuItem("Audio", "Create", "Place & edit scene audio")]
    sealed class AudioTool : MonoBehaviour, ITool, IStandardActionMap, IConnectInterfaces, IInstantiateMenuUI,
        IUsesRayOrigin, IUsesViewerScale, ISelectTool, IIsHoveringOverUI, IIsMainMenuVisible,
        IRayVisibilitySettings, IMenuIcon, IRequestFeedback, IUsesNode, IUsesDirectSelection, IGetPreviewOrigin
    {
        [SerializeField]
        AudioToolMenu m_AudioMenuPrefab;

        [SerializeField]
        Sprite m_Icon;

        [SerializeField]
        AudioComponentType m_SelectedComponentType = AudioComponentType.Source;

        GameObject m_ToolMenu;

        GameObject m_CurrentObject;

        GameObject m_PreviewObject;
        MeshRenderer m_PreviewRenderer;

        AudioCreationState m_ActionState = AudioCreationState.Evaluating;

        public Transform rayOrigin { get; set; }
        public Node node { get; set; }

        public Sprite icon { get { return m_Icon; } }

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

            InitTempPreviewCube();
        }

        static readonly Vector3 k_PreviewRotation = new Vector3(1f, 3f, 0f);

        public void ProcessInput(ActionMapInput input, ConsumeControlDelegate consumeControl)
        {
            if (!IsActive())
                return;

            var standardInput = (Standard)input;

            switch (m_ActionState)
            {
                case AudioCreationState.Evaluating:
                {
                    HandleStartPoint(standardInput, consumeControl);
                    break;
                }
                case AudioCreationState.Executing:
                {
                    CheckForTriggerRelease(standardInput, consumeControl);
                    m_PreviewObject.transform.Rotate(k_PreviewRotation, Space.Self);
                    break;
                }
            }

            if (m_ActionState == AudioCreationState.Evaluating && this.IsHoveringOverUI(rayOrigin))
                this.RemoveRayVisibilitySettings(rayOrigin, this);
            else
                this.AddRayVisibilitySettings(rayOrigin, this, false, true);
        }

        void SetSelectedComponent(AudioComponentType type)
        {
            m_SelectedComponentType = type;
            Debug.Log("set selected type: " + type);
        }

        GameObject TryGetRayDirectSelection()
        {
            GameObject raySelection = null;
            var selections = this.GetDirectSelection();
            if (selections != null)
                selections.TryGetValue(rayOrigin, out raySelection);

            return raySelection;
        }

        // this will be removed, it just shows 
        // a spinning cube in place of the preview icon
        void InitTempPreviewCube()
        {
            var previewOrigin = this.GetPreviewOriginForRayOrigin(rayOrigin);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale *= this.GetViewerScale() / 20f;
            cube.transform.position = previewOrigin.position;
            cube.transform.rotation = previewOrigin.rotation;
            cube.transform.SetParent(previewOrigin);

            m_PreviewObject = cube;
            m_PreviewRenderer = cube.GetComponent<MeshRenderer>();
            m_PreviewRenderer.enabled = false;
        }

        void HandleStartPoint(Standard standardInput, ConsumeControlDelegate consumeControl)
        {
            if (standardInput.action.wasJustPressed)
            {
                m_CurrentObject = null;
                consumeControl(standardInput.action);
                m_ActionState = AudioCreationState.Executing;
                m_PreviewRenderer.enabled = true;
            }
        }

        void AddSelectedComponent()
        {
            if (m_CurrentObject != null)
                AudioComponentUtils.Add(m_CurrentObject, m_SelectedComponentType);
        }

        void CheckForTriggerRelease(Standard standardInput, ConsumeControlDelegate consumeControl)
        {
            // Ready for next object to be created
            if (standardInput.action.wasJustReleased)
            {
                m_ActionState = AudioCreationState.Evaluating;

                Undo.IncrementCurrentGroup();

                var selection = TryGetRayDirectSelection();
                if (selection != null)
                {
                    m_CurrentObject = selection;
                    AddSelectedComponent();

                    Debug.Log("successful component add");
                }

                if (m_PreviewRenderer != null)
                {
                    // we should animate this, temporary
                    m_PreviewRenderer.enabled = false;
                }

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
            // do nothing
        }

        public ActionMap standardActionMap { private get; set; }
    }
}
#endif
