﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// SceneBlendRenderer provides volumetric rendering and lighting of the real environment, as seen by the sensor's color camera.
    /// </summary>
    public class SceneBlendRenderer : MonoBehaviour
    {
        [Tooltip("Added depth distance between the real environment and the virtual environment, in meters.")]
        [Range(-0.5f, 0.5f)]
        public float depthDistance = 0.1f;

        [Tooltip("Index of the depth sensor that generates the color camera background. 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("Depth value in meters, used for invalid depth points.")]
        public float invalidDepthValue = 5f;

        [Tooltip("Whether to maximize the rendered object on the screen, or not.")]
        private bool maximizeOnScreen = true;

        [Tooltip("Whether to apply per-pixel lighting on the foreground, or not.")]
        public bool applyLighting = false;

        [Tooltip("Camera used to scale the mesh, to fill the camera's background. If left empty, it will default to the main camera in the scene.")]
        public Camera foregroundCamera;

        [Tooltip("Background image (if any) that needs to be overlayed by this blend renderer.")]
        public UnityEngine.UI.RawImage backgroundImage;

        [Tooltip("Reference to a background removal manager, if one is available in the scene.")]
        public BackgroundRemovalManager backgroundRemovalManager = null;


        // references to KM and data
        private KinectManager kinectManager = null;
        private KinectInterop.SensorData sensorData = null;
        private Material matRenderer = null;

        // depth image buffer (in depth camera resolution)
        private ComputeBuffer depthImageBuffer = null;

        // textures
        private Texture alphaTex = null;
        private Texture colorTex = null;

        // lighting
        private FragmentLighting lighting = new FragmentLighting();

        // saved screen width & height
        private int lastScreenW = 0;
        private int lastScreenH = 0;
        private int lastColorW = 0;
        private int lastColorH = 0;
        private Vector3 initialScale = Vector3.one;


        void Start()
        {
            kinectManager = KinectManager.Instance;
            initialScale = transform.localScale;

            Renderer meshRenderer = GetComponent<Renderer>();
            if (meshRenderer)
            {
                Shader blendShader = Shader.Find("Kinect/ForegroundBlendShader");
                if (blendShader != null)
                {
                    matRenderer = new Material(blendShader);
                    meshRenderer.material = matRenderer;
                }
            }

            if (kinectManager && kinectManager.IsInitialized())
            {
                // get sensor data
                sensorData = kinectManager.GetSensorData(sensorIndex);
            }

            if(foregroundCamera == null)
            {
                foregroundCamera = Camera.main;
            }

            // find scene lights
            Light[] sceneLights = GameObject.FindObjectsOfType<Light>();
            lighting.SetLightsAndBounds(sceneLights, transform.position, new Vector3(20f, 20f, 20f));
        }


        void OnDestroy()
        {
            if (sensorData != null && sensorData.colorDepthBuffer != null)
            {
                sensorData.colorDepthBuffer.Release();
                sensorData.colorDepthBuffer = null;
            }

            if (depthImageBuffer != null)
            {
                //depthImageCopy = null;

                depthImageBuffer.Release();
                depthImageBuffer = null;
            }

            // release lighting resources
            lighting.ReleaseResources();
        }


        void Update()
        {
            if (matRenderer == null || sensorData == null)
                return;

            if(alphaTex == null || alphaTex.width != sensorData.colorImageWidth || alphaTex.height != sensorData.colorImageHeight)
            {
                // alpha texture
                alphaTex = backgroundRemovalManager != null ? backgroundRemovalManager.GetAlphaTex() : null;

                if (alphaTex != null)
                {
                    matRenderer.SetTexture("_AlphaTex", alphaTex);
                }
            }

            if(colorTex == null || colorTex.width != sensorData.colorImageWidth || colorTex.height != sensorData.colorImageHeight)
            {
                // color texture
                colorTex = sensorData.colorImageTexture;
                if(backgroundRemovalManager != null)
                {
                    colorTex = !backgroundRemovalManager.computeAlphaMaskOnly ? backgroundRemovalManager.GetForegroundTex() : alphaTex;
                }

                if (colorTex != null)
                {
                    matRenderer.SetInt("_TexResX", colorTex.width);
                    matRenderer.SetInt("_TexResY", colorTex.height);

                    matRenderer.SetTexture("_ColorTex", colorTex);
                }
            }

            if (colorTex == null)
                return;

            int bufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight / 2;
            if (sensorData.colorDepthBuffer == null || sensorData.colorDepthBuffer.count != bufferLength)
            {
                sensorData.colorDepthBuffer = new ComputeBuffer(bufferLength, sizeof(uint));
                matRenderer.SetBuffer("_DepthMap", sensorData.colorDepthBuffer);
                //Debug.Log("Created colorDepthBuffer with len: " + bufferLength);
            }

            matRenderer.SetFloat("_DepthDistance", depthDistance);
            matRenderer.SetFloat("_InvDepthVal", invalidDepthValue);

            int curScreenW = foregroundCamera ? foregroundCamera.pixelWidth : Screen.width;
            int curScreenH = foregroundCamera ? foregroundCamera.pixelHeight : Screen.height;
            if (lastScreenW != curScreenW || lastScreenH != curScreenH || lastColorW != sensorData.colorImageWidth || lastColorH != sensorData.colorImageHeight)
            {
                ScaleRendererTransform(colorTex, curScreenW, curScreenH);
            }

            // update lighting parameters
            lighting.UpdateLighting(matRenderer, applyLighting);
        }

        // scales the renderer's transform properly
        private void ScaleRendererTransform(Texture colorTex, int curScreenW, int curScreenH)
        {
            lastScreenW = curScreenW;
            lastScreenH = curScreenH;
            lastColorW = sensorData.colorImageWidth;
            lastColorH = sensorData.colorImageHeight;

            Vector3 localScale = Vector3.one;  // transform.localScale;

            if (maximizeOnScreen && foregroundCamera)
            {
                float objectZ = transform.position.z;
                float screenW = foregroundCamera.pixelWidth;
                float screenH = foregroundCamera.pixelHeight;

                if(backgroundImage)
                {
                    PortraitBackground portraitBack = backgroundImage.gameObject.GetComponent<PortraitBackground>();

                    if (portraitBack != null)
                    {
                        Rect backRect = portraitBack.GetBackgroundRect();
                        screenW = backRect.width;
                        screenH = backRect.height;
                    }
                }

                Vector3 vLeft = foregroundCamera.ScreenToWorldPoint(new Vector3(0f, screenH / 2f, objectZ));
                Vector3 vRight = foregroundCamera.ScreenToWorldPoint(new Vector3(screenW, screenH / 2f, objectZ));
                float distLeftRight = (vRight - vLeft).magnitude;

                Vector3 vBottom = foregroundCamera.ScreenToWorldPoint(new Vector3(screenW / 2f, 0f, objectZ));
                Vector3 vTop = foregroundCamera.ScreenToWorldPoint(new Vector3(screenW / 2f, screenH, objectZ));
                float distBottomTop = (vTop - vBottom).magnitude;

                localScale.x = distLeftRight / initialScale.x;
                localScale.y = distBottomTop / initialScale.y;
                //Debug.Log("SceneRenderer scale: " + localScale + ", screenW: " + screenW + ", screenH: " + screenH);
            }

            // scale according to color-tex resolution
            //localScale.y = localScale.x * colorTex.height / colorTex.width;

            // apply color image scale
            Vector3 colorImageScale = kinectManager.GetColorImageScale(sensorIndex);
            if (colorImageScale.x < 0f)
                localScale.x = -localScale.x;
            if (colorImageScale.y < 0f)
                localScale.y = -localScale.y;

            transform.localScale = localScale;
        }

    }
}
