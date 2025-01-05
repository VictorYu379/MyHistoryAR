using Google.XR.ARCoreExtensions.Samples.Geospatial;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts
{
    public class GeospatialControllerInput : MonoBehaviour
    {
        /// <summary>
        /// UI element to display information at runtime.
        /// </summary>
        public GameObject InfoPanel;
        
        /// <summary>
        /// UI element that enables streetscape geometry visibility.
        /// </summary>
        public Toggle GeometryToggle;
        
        /// <summary>
        /// UI element to display or hide the Anchor Settings panel.
        /// </summary>
        public Button AnchorSettingButton;
        
        /// <summary>
        /// UI element for the Anchor Settings panel.
        /// </summary>
        public GameObject AnchorSettingPanel;
        
        /// <summary>
        /// UI element that toggles anchor type to Geometry.
        /// </summary>
        public Toggle GeospatialAnchorToggle;
        
        /// <summary>
        /// UI element that toggles anchor type to Terrain.
        /// </summary>
        public Toggle TerrainAnchorToggle;
        
        /// <summary>
        /// UI element that toggles anchor type to Rooftop.
        /// </summary>
        public Toggle RooftopAnchorToggle;
        
        /// <summary>
        /// UI element for clearing all anchors, including history.
        /// </summary>
        public Button ClearAllButton;

        private GeospatialControllerLogic _geospatialControllerLogic;
        /// <summary>
        /// Determines if the anchor settings panel is visible in the UI.
        /// </summary>
        private bool _showAnchorSettingsPanel = false;
        
        public void Awake()
        {
            
        }

        public void OnEnable()
        {
            _geospatialControllerLogic = GetComponent<GeospatialControllerLogic>();
            
            InfoPanel.SetActive(false);
            GeometryToggle.gameObject.SetActive(false);
            AnchorSettingButton.gameObject.SetActive(false);
            AnchorSettingPanel.gameObject.SetActive(false);
            GeospatialAnchorToggle.gameObject.SetActive(false);
            TerrainAnchorToggle.gameObject.SetActive(false);
            RooftopAnchorToggle.gameObject.SetActive(false);
            ClearAllButton.gameObject.SetActive(false);
            
            GeometryToggle.onValueChanged.AddListener(OnGeometryToggled);
            AnchorSettingButton.onClick.AddListener(OnAnchorSettingButtonClicked);
            GeospatialAnchorToggle.onValueChanged.AddListener(OnGeospatialAnchorToggled);
            TerrainAnchorToggle.onValueChanged.AddListener(OnTerrainAnchorToggled);
            RooftopAnchorToggle.onValueChanged.AddListener(OnRooftopAnchorToggled);
        }
        
        public void Update()
        {
        }
        
        
        /// <summary>
        /// Callback handling "Geometry" toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Streetscape Geometry visibility.</param>
        public void OnGeometryToggled(bool enabled)
        {
            _geospatialControllerLogic.SetStreetscapeGeometryVisibility(enabled);
        }
        
        /// <summary>
        /// Callback handling the  "Anchor Setting" panel display or hide event in AR View.
        /// </summary>
        public void OnAnchorSettingButtonClicked()
        {
            _showAnchorSettingsPanel = !_showAnchorSettingsPanel;
            SetAnchorPanelState(_showAnchorSettingsPanel);
        }
        
        /// <summary>
        /// Callback handling Geospatial anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Geospatial anchors.</param>
        public void OnGeospatialAnchorToggled(bool enabled)
        {
            // GeospatialAnchorToggle.GetComponent<Toggle>().isOn = true;;
            _geospatialControllerLogic.SetAnchorType(AnchorType.Geospatial);
            _showAnchorSettingsPanel = false;
            SetAnchorPanelState(false);
        }
        
        /// <summary>
        /// Callback handling Terrain anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Terrain anchors.</param>
        public void OnTerrainAnchorToggled(bool enabled)
        {
            // TerrainAnchorToggle.GetComponent<Toggle>().isOn = true;
            _geospatialControllerLogic.SetAnchorType(AnchorType.Terrain);
            _showAnchorSettingsPanel = false;
            SetAnchorPanelState(false);
        }
        
        /// <summary>
        /// Callback handling Rooftop anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Rooftop anchors.</param>
        public void OnRooftopAnchorToggled(bool enabled)
        {
            // RooftopAnchorToggle.GetComponent<Toggle>().isOn = true;
            _geospatialControllerLogic.SetAnchorType(AnchorType.Rooftop);
            _showAnchorSettingsPanel = false;
            SetAnchorPanelState(false);
        }
        
        /// <summary>
        /// Activate or deactivate all UI elements on the anchor setting Panel.
        /// </summary>
        /// <param name="state">A bool value to determine if the anchor settings panel is visible.
        private void SetAnchorPanelState(bool state)
        {
            AnchorSettingPanel.gameObject.SetActive(state);
            GeospatialAnchorToggle.gameObject.SetActive(state);
            TerrainAnchorToggle.gameObject.SetActive(state);
            RooftopAnchorToggle.gameObject.SetActive(state);
        }
    }
}