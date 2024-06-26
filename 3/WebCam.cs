using UnityEngine;
using System;
using System.Collections;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
namespace SimHub
{

    public class WebCam : MonoBehaviour
    {
#if UNITY_IOS || UNITY_WEBGL
        private bool CheckPermissionAndRaiseCallbackIfGranted(UserAuthorization authenticationType, Action authenticationGrantedAction)
        {
            if (Application.HasUserAuthorization(authenticationType))
            {
                if (authenticationGrantedAction != null)
                    authenticationGrantedAction();

                return true;
            }
            return false;
        }

        private IEnumerator AskForPermissionIfRequired(UserAuthorization authenticationType, Action authenticationGrantedAction)
        {
            if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
            {
                yield return Application.RequestUserAuthorization(authenticationType);
                if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
                    Debug.LogWarning($"Permission {authenticationType} Denied");
            }
        }
#elif UNITY_ANDROID
    private void PermissionCallbacksPermissionGranted(string permissionName)
    {
        StartCoroutine(DelayedCameraInitialization());
    }

    private IEnumerator DelayedCameraInitialization()
    {
        yield return null;
        InitializeCamera();
    }

    private void PermissionCallbacksPermissionDenied(string permissionName)
    {
        Debug.LogWarning($"Permission {permissionName} Denied");
    }

    private void AskCameraPermission()
    {
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionDenied += PermissionCallbacksPermissionDenied;
        callbacks.PermissionGranted += PermissionCallbacksPermissionGranted;
        Permission.RequestUserPermission(Permission.Camera, callbacks);
    }
#endif

        void Start()
        {
#if UNITY_IOS || UNITY_WEBGL
            StartCoroutine(AskForPermissionIfRequired(UserAuthorization.WebCam, () => { InitializeCamera(); }));
            return;
#elif UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            AskCameraPermission();
            return;
        }
        InitializeCamera();
#endif
        }

        private void InitializeCamera()
        {
            WebCamTexture webcamTexture = new WebCamTexture();
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = webcamTexture;
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length > 1)
            {
                webcamTexture.deviceName = devices[1].name;
                webcamTexture.Play();
            }
        }
    }
}
