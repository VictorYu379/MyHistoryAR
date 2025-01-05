using System;
using System.Collections;
using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using Google.XR.ARCoreExtensions.Samples.Geospatial;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Scripts
{
    public class GeospatialControllerLogic : MonoBehaviour
    {

        /// <summary>
        /// The XROrigin used in the sample.
        /// </summary>
        public XROrigin Origin;

        /// <summary>
        /// The ARSession used in the sample.
        /// </summary>
        public ARSession Session;

        /// <summary>
        /// The ARCoreExtensions used in the sample.
        /// </summary>
        public ARCoreExtensions ARCoreExtensions;

        /// <summary>
        /// The AREarthManager used in the sample.
        /// </summary>
        public AREarthManager EarthManager;
        
        /// <summary>
        /// The key name used in PlayerPrefs which stores geospatial anchor history data.
        /// The earliest one will be deleted once it hits storage limit.
        /// </summary>
        private const string _persistentGeospatialAnchorsStorageKey = "PersistentGeospatialAnchors";
        
        /// <summary>
        /// The key name used in PlayerPrefs which indicates whether the privacy prompt has
        /// displayed at least one time.
        /// </summary>
        private const string _hasDisplayedPrivacyPromptKey = "HasDisplayedGeospatialPrivacyPrompt";


        private bool _waitingForLocationService = false;
        private IEnumerator _startLocationService = null;
        private bool _isReturning;
        private bool _enablingGeospatial;
        private bool _streetscapeGeometryVisibility;
        private AnchorType _anchorType = AnchorType.Geospatial;

        /// <summary>
        /// Determines if streetscape geometry should be removed from the scene.
        /// </summary>
        private bool _clearStreetscapeGeometryRenderObjects = false;

        private float _localizationPassedTime = 0f;
        private bool _isLocalizing = false;
        
        private GeospatialAnchorHistoryCollection _historyCollection = null;
        private bool _shouldResolvingHistory = false;
        private bool _isARViewOn = false;
        private IEnumerator _asyncCheck = null;

        public void Awake()
        {
            if (Origin == null)
            {
                Debug.LogError("Cannot find XROrigin.");
            }

            if (Session == null)
            {
                Debug.LogError("Cannot find ARSession.");
            }

            if (ARCoreExtensions == null)
            {
                Debug.LogError("Cannot find ARCoreExtensions.");
            }
        }

        public void OnEnable()
        {
            _startLocationService = StartLocationService();
            StartCoroutine(_startLocationService);

            _isReturning = false;
            _enablingGeospatial = false;

            _localizationPassedTime = 0f;
            _isLocalizing = true;
            
            LoadGeospatialAnchorHistory();
            _shouldResolvingHistory = _historyCollection.Collection.Count > 0;
            
            TurnOnARView(PlayerPrefs.HasKey(_hasDisplayedPrivacyPromptKey));
        }

        public void Update()
        {
        }

    internal AREarthManager getAREarthManager()
        {
            return EarthManager;
        }

        internal void SetStreetscapeGeometryVisibility(bool enabled)
        {
            _streetscapeGeometryVisibility = enabled;
            if (!_streetscapeGeometryVisibility)
            {
                _clearStreetscapeGeometryRenderObjects = true;
            }
        }

        internal void SetAnchorType(AnchorType anchorType)
        {
            _anchorType = anchorType;
        }

        internal bool IsARViewOn()
        {
            return _isARViewOn;
        }
        
        private IEnumerator StartLocationService()
        {
            _waitingForLocationService = true;
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("Requesting the fine location permission.");
                Permission.RequestUserPermission(Permission.FineLocation);
                yield return new WaitForSeconds(3.0f);
            }
#endif

            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("Location service is disabled by the user.");
                _waitingForLocationService = false;
                yield break;
            }

            Debug.Log("Starting location service.");
            Input.location.Start();

            while (Input.location.status == LocationServiceStatus.Initializing)
            {
                yield return null;
            }

            _waitingForLocationService = false;
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarningFormat(
                    "Location service ended with {0} status.", Input.location.status);
                Input.location.Stop();
            }
        }
        
        private void LoadGeospatialAnchorHistory()
        {
            if (PlayerPrefs.HasKey(_persistentGeospatialAnchorsStorageKey))
            {
                _historyCollection = JsonUtility.FromJson<GeospatialAnchorHistoryCollection>(
                    PlayerPrefs.GetString(_persistentGeospatialAnchorsStorageKey));

                // Remove all records created more than 24 hours and update stored history.
                DateTime current = DateTime.Now;
                _historyCollection.Collection.RemoveAll(
                    data => current.Subtract(data.CreatedTime).Days > 0);
                PlayerPrefs.SetString(_persistentGeospatialAnchorsStorageKey,
                    JsonUtility.ToJson(_historyCollection));
                PlayerPrefs.Save();
            }
            else
            {
                _historyCollection = new GeospatialAnchorHistoryCollection();
            }
        }

        private void TurnOnARView(bool enable)
        {
            _isARViewOn = enable;
            Origin.gameObject.SetActive(enable);
            Session.gameObject.SetActive(enable);
            ARCoreExtensions.gameObject.SetActive(enable);
            
            if (enable && _asyncCheck == null)
            {
                _asyncCheck = AvailabilityCheck();
                StartCoroutine(_asyncCheck);
            }
        }
        
        private IEnumerator AvailabilityCheck()
        {
            if (ARSession.state == ARSessionState.None)
            {
                yield return ARSession.CheckAvailability();
            }

            // Waiting for ARSessionState.CheckingAvailability.
            yield return null;

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                yield return ARSession.Install();
            }

            // Waiting for ARSessionState.Installing.
            yield return null;
#if UNITY_ANDROID

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("Requesting camera permission.");
                Permission.RequestUserPermission(Permission.Camera);
                yield return new WaitForSeconds(3.0f);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                // User has denied the request.
                Debug.LogWarning(
                    "Failed to get the camera permission. VPS availability check isn't available.");
                yield break;
            }
#endif

            while (_waitingForLocationService)
            {
                yield return null;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning(
                    "Location services aren't running. VPS availability check is not available.");
                yield break;
            }

            // Update event is executed before coroutines so it checks the latest error states.
            if (_isReturning)
            {
                yield break;
            }

            var location = Input.location.lastData;
            var vpsAvailabilityPromise =
                AREarthManager.CheckVpsAvailabilityAsync(location.latitude, location.longitude);
            yield return vpsAvailabilityPromise;

            Debug.LogFormat("VPS Availability at ({0}, {1}): {2}",
                location.latitude, location.longitude, vpsAvailabilityPromise.Result);
            // VPSCheckCanvas.SetActive(vpsAvailabilityPromise.Result != VpsAvailability.Available);
        }
    }
}