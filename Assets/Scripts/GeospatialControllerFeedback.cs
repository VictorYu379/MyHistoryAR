using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts
{
    public class GeospatialControllerFeedback : MonoBehaviour
    {
        /// <summary>
        /// Text displaying in a snack bar at the bottom of the screen.
        /// </summary>
        public Text SnackBarText;
        
        /// <summary>
        /// Text displaying debug information, only activated in debug build.
        /// </summary>
        public Text DebugText;
        
        /// <summary>
        /// UI element containing all AR view contents.
        /// </summary>
        public GameObject ARViewCanvas;
        
        /// <summary>
        /// UI element showing privacy prompt.
        /// </summary>
        public GameObject PrivacyPromptCanvas;
        
        /// <summary>
        /// UI element showing VPS availability notification.
        /// </summary>
        public GameObject VPSCheckCanvas;
        
        /// <summary>
        /// Help message shown while localizing.
        /// </summary>
        private const string _localizingMessage = "Localizing your device to set anchor.";
        
        private GeospatialControllerLogic _geospatialControllerLogic;
        private bool _isInARView = false;
        
        public void Awake()
        {
            // Lock screen to portrait.
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.Portrait;
            
            // Enable geospatial sample to target 60fps camera capture frame rate
            // on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;
        }

        public void OnEnable()
        {
            _geospatialControllerLogic = GetComponent<GeospatialControllerLogic>();
            DebugText.gameObject.SetActive(Debug.isDebugBuild
                                           && _geospatialControllerLogic.getAREarthManager() != null);
            
            SnackBarText.text = _localizingMessage;
        }

        public void Start()
        {
            
        }

        public void Update()
        {
            if (_isInARView != _geospatialControllerLogic.IsARViewOn())
            {
                _isInARView = _geospatialControllerLogic.IsARViewOn();
                SwitchToARView(_isInARView);
            }
        }
        
        private void SwitchToARView(bool enable)
        {
            _isInARView = enable;
            ARViewCanvas.SetActive(enable);
            PrivacyPromptCanvas.SetActive(!enable);
            VPSCheckCanvas.SetActive(false);
        }
    }
}