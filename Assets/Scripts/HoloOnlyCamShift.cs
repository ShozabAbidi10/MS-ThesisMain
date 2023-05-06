using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OpenCVForUnity.RectangleTrack;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.ImgprocModule;
using Rect = OpenCVForUnity.CoreModule.Rect;
using HoloLensWithOpenCVForUnity.UnityUtils.Helper;
using HoloLensCameraStream;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Input;
using OpenCVForUnity.VideoModule;
using OpenCVForUnity.TrackingModule;
using HoloTracking;
using UnityEngine.Assertions;
using OpenCVForUnityExample;
using JetBrains.Annotations;

namespace HoloLensWithOpenCVForUnityExample
{
    /// <summary>
    /// HoloLens Face Detection Example
    /// An example of detecting face using OpenCVForUnity on Hololens.
    /// Referring to https://github.com/Itseez/opencv/blob/master/modules/objdetect/src/detection_based_tracker.cpp.
    /// </summary>
    [RequireComponent(typeof(HLCameraStreamToMatHelper), typeof(ImageOptimizationHelper))]
    public class HoloOnlyCamShift : MonoBehaviour
    {
        // Added my own header like Debug header below.
        [HeaderAttribute("Detection")]

        /// <summary>
        /// Determines if enables the detection.
        /// </summary>
        public bool enableDetection = true;

        /// <summary>
        /// Determines if enable downscale.
        /// </summary>
        public bool enableDownScale;

        /// <summary>
        /// The enable downscale toggle.
        /// </summary>
        public Toggle enableDownScaleToggle;

        // By default it will be false.
        /// <summary>
        /// Determines if uses separate detection. [MIGHT NEED TO REMOVE THIS]
        /// </summary>
        public bool useSeparateDetection = false;

        /// <summary>
        /// The use separate detection toggle.
        /// </summary>
        public Toggle useSeparateDetectionToggle;

        /// <summary>
        /// Determines if displays camera image.
        /// </summary>
        public bool displayCameraImage = false;

        /// <summary>
        /// The display camera image toggle. [MIGHT NEED TO REMOVE THIS]
        /// </summary>
        public Toggle displayCameraImageToggle;

        /// <summary>
        /// The min detection size ratio. [MIGHT NEED TO REMOVE THIS] We are not using detection.
        /// </summary>
        public float minDetectionSizeRatio = 0.07f;  

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        HLCameraStreamToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The image optimization helper.
        /// </summary>
        ImageOptimizationHelper imageOptimizationHelper;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The cascade.  [MIGHT NEED TO REMOVE THIS]
        /// </summary>
        CascadeClassifier cascade;

        /// <summary>
        /// The quad renderer.
        /// </summary>
        Renderer quad_renderer;

        /// <summary>
        /// The detection result.
        /// </summary>
        List<Rect> detectionResult = new List<Rect>();

        // Scalar is opencv made data type
        // I think they are using it for color values as scalar as can easily convert to/from Matrix. 
        // I think the fourth element of scaler here is refering to the opacity of the color which is for both white and
        // gray color is full (255). 
        readonly Scalar COLOR_WHITE = new Scalar(255, 255, 255, 255);

        Mat grayMat4Thread;

        #region RaycastParams

        bool RayCastRequested = false;


        [HeaderAttribute("UI Tap")]

        [SerializeField]
        [NotNull] private GameObject _UIObject;

        // For creating labels:
        private List<GameObject> _createdObjects = new List<GameObject>();

        [HeaderAttribute("AirTap")]

        [SerializeField]
        [NotNull] private GameObject _labelObject;
        [SerializeField]
        private string _labelText = "Drone";


        // Drone Model
        [SerializeField]
        [NotNull] private GameObject _drone_model;

        /// <summary>
        /// rayCastAndLabel class object.
        /// </summary>
        [SerializeField]
        private GameObject RayCastAndLabelHandler;
        RayCastAndLabel rayCastAndLabelHandler;

        /// <summary>
        /// RosSubscriber class object.
        /// </summary>
        [SerializeField]
        private GameObject RosSubcriberHandler;
        RosSubcriber rosSubscriberHandler;

        private static int _meshPhysicsLayer = 0;

        private IMixedRealityInputSystem inputSystem;

        #endregion

        // this is for face detection.
        CascadeClassifier cascade4Thread;

        // Making a queue of "Action" which task to be performe one by one.
        // I think this will be use for multiple face detections. Not sure.
        readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

        // Making evi object to use it as key to access "_isThreadRunning" variable. 
        // Most like this variable will be shared among threads. 
        System.Object sync = new System.Object();

        bool _isThreadRunning = false;
        bool isThreadRunning
        {
            get
            {
                lock (sync)
                    return _isThreadRunning;
            }
            set
            {
                lock (sync)
                    _isThreadRunning = value;
            }
        }

        // Ohhh this is the tracking we are using in this script.
        // Need to study more about it.
        RectangleTracker rectangleTracker;

        // Not sure what does this mean. Tracking window size. 
        // Need to see how this variable is being use below.
        float coeffTrackingWindowSize = 2.0f;

        // Hmm same, some how related to tracking now sure how. 
        float coeffObjectSizeToTrack = 0.85f;

        // straight forward.
        List<Rect> detectedObjectsInRegions = new List<Rect>();

        List<Rect> resultObjects = new List<Rect>();

        bool _isDetecting = false;
        bool isDetecting
        {
            get
            {
                lock (sync)
                    return _isDetecting;
            }
            set
            {
                lock (sync)
                    _isDetecting = value;
            }
        }

        // This var will be use to check if the program has updated the latest detection results or not. 
        // It would be very cool to see how it is actually working.
        bool _hasUpdatedDetectionResult = false;
        bool hasUpdatedDetectionResult
        {
            get
            {
                lock (sync)
                    return _hasUpdatedDetectionResult;
            }
            set
            {
                lock (sync)
                    _hasUpdatedDetectionResult = value;
            }
        }


        bool _isDetectingInFrameArrivedThread = false;
        bool isDetectingInFrameArrivedThread
        {
            get
            {
                lock (sync)
                    return _isDetectingInFrameArrivedThread;
            }
            set
            {
                lock (sync)
                    _isDetectingInFrameArrivedThread = value;
            }
        }

        // Nice, this is cool Check out spector tab of HLFaceDection Game object in editor. 
        // There will be a heading of Debug befor these text fields.
        [HeaderAttribute("Debug")]

        public Text renderFPS;
        public Text videoFPS;
        public Text trackFPS;
        public Text debugStr;
        public Text OnAirTapAcquiredCheck;
        public Text CallingLabelObjectsCheck;
        public Text CallingRayCastCheck;
        public Text TelloBattery;


        #region camshift

        // Initialize the KCF tracker
        TrackerKCF tracker;

        Rect initialRect;
        Rect currentTrackObjectRect;
        bool regionOfInterestSelected = false;
        bool camShiftInitialization = false;

        /// <summary>
        /// The termination.
        /// </summary>
        TermCriteria termination;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        /// <summary>
        /// The hsv mat.
        /// </summary>
        Mat hsvMat;

        /// <summary>
        /// The roi hist mat.
        /// </summary>
        Mat roiHistMat;


        /// <summary>
        /// The roi rect.
        /// </summary>
        Rect roiRect;

        double widthRatio = 1;
        double heigthRatio = 1;

        #endregion

        Vector3 handGazePosition;
        public float RayCastDistance;

        // Use this for initialization
        // This is where the program starts..
        protected void Start()
        {
            termination = new TermCriteria(TermCriteria.EPS | TermCriteria.COUNT, 10, 1);

            initialRect = new Rect(0, 0, 0, 0);

            OnAirTapAcquiredCheck.text = string.Format("initialized");
            CallingLabelObjectsCheck.text = string.Format("initialized");
            CallingRayCastCheck.text = string.Format("initialized");
            TelloBattery.text = string.Format("initialized");

            // Setting the default values of all toggle fields. [NOT REALLY USING THIS]
            enableDownScaleToggle.isOn = enableDownScale;
            useSeparateDetectionToggle.isOn = useSeparateDetection;
            displayCameraImageToggle.isOn = displayCameraImage;

            inputSystem = CoreServices.InputSystem;

            // Assigning these two dependencies there constructors in this script. 
            imageOptimizationHelper = gameObject.GetComponent<ImageOptimizationHelper>();
            webCamTextureToMatHelper = gameObject.GetComponent<HLCameraStreamToMatHelper>();
            rayCastAndLabelHandler = RayCastAndLabelHandler.GetComponent<RayCastAndLabel>();
            rosSubscriberHandler = RosSubcriberHandler.GetComponent<RosSubcriber>();

#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
            // I know what this is..
            // They are setting the "OnFrameMatAcquired" method which belongs to this class 
            // as a subscriber of "framMatAcquired" event. When this event is fire this method will be called.
            webCamTextureToMatHelper.frameMatAcquired += OnFrameMatAcquired;
            rayCastAndLabelHandler.AirTapAcquired += OnAirTapAcquired;
#else
            rayCastAndLabelHandler.AirTapAcquired += OnAirTapAcquired;
#endif
            // setting the camera output frame color format in grayscale.
            //webCamTextureToMatHelper.outputColorFormat = WebCamTextureToMatHelper.ColorFormat.GRAY;

            // Let's convert this to RGB
            webCamTextureToMatHelper.outputColorFormat = WebCamTextureToMatHelper.ColorFormat.BGRA;

            // Calling the "Initialize" method of HLCameraStreamToMatHelper class which is inheriting from WebCamTextureToMatHelper
            webCamTextureToMatHelper.Initialize();

            // initializing the tracker. We are using this tracker. 
            rectangleTracker = new RectangleTracker();
        }

        public void OnAirTapAcquired()
        {
#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API

            if (!_UIObject.activeInHierarchy)
            {
                if (regionOfInterestSelected)
                {
                    regionOfInterestSelected = false;
                    camShiftInitialization = false;
                    OnAirTapAcquiredCheck.text = string.Format("Recieved AirTap Gesture. regionOfInterestSelected {0}", regionOfInterestSelected);
                    //Debug.Log($"Recieved AirTap Gesture. regionOfInterestSelected {regionOfInterestSelected}");
                }
                else
                {
                    regionOfInterestSelected = true;
                    camShiftInitialization = true;
                    OnAirTapAcquiredCheck.text = string.Format("Recieved AirTap Gesture. regionOfInterestSelected {0}", regionOfInterestSelected);
                    //Debug.Log($"Recieved AirTap Gesture. regionOfInterestSelected {regionOfInterestSelected}");
                }
            }
            
            //LabelObjects(CopyCameraTransForm());
            
#else
            if (!_UIObject.activeInHierarchy)
            {
                if (regionOfInterestSelected)
                {
                    regionOfInterestSelected = false;
                    camShiftInitialization = false;
                    OnAirTapAcquiredCheck.text = string.Format("Recieved AirTap Gesture. regionOfInterestSelected {0}", regionOfInterestSelected);
                    //Debug.Log($"Recieved AirTap Gesture. regionOfInterestSelected {regionOfInterestSelected}");
                }
                else
                {
                    regionOfInterestSelected = true;
                    camShiftInitialization = true;
                    OnAirTapAcquiredCheck.text = string.Format("Recieved AirTap Gesture. regionOfInterestSelected {0}", regionOfInterestSelected);
                    //Debug.Log($"Recieved AirTap Gesture. regionOfInterestSelected {regionOfInterestSelected}");
                }
            }
            //Debug.Log($"Recieved AirTap Gesture. Creating Label.");
            //LabelObjects(CopyCameraTransForm());
#endif
        }

        // Whenever HLCameraStreamToMatHelper class have a gray matrix of the acquired frame they will trigger this function.
        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            // retriving the gray matrix form of the acquired frame.
            Mat grayMat = webCamTextureToMatHelper.GetMat();

            hsvMat = new Mat(grayMat.rows(), grayMat.cols(), CvType.CV_8UC3);

            // from the acquired graymatrix they are making a 2D texture.
            texture = new Texture2D(grayMat.cols(), grayMat.rows(), TextureFormat.Alpha8, false);

            // Don't know what is happening here.
            texture.wrapMode = TextureWrapMode.Clamp;

            // Using GetComponent<Renderer>() to access the Renderer component of the GameObject atatched to this script.
            quad_renderer = gameObject.GetComponent<Renderer>();

            // Assigning a new texture to the material of this GameObj
            quad_renderer.sharedMaterial.SetTexture("_MainTex", texture);
            quad_renderer.sharedMaterial.SetVector("_VignetteOffset", new Vector4(0, 0));

            //Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            DebugUtils.AddDebugStr(webCamTextureToMatHelper.outputColorFormat.ToString() + " " + webCamTextureToMatHelper.GetWidth() + " x " + webCamTextureToMatHelper.GetHeight() + " : " + webCamTextureToMatHelper.GetFPS());

            // Need to understand what downscale script is doing
            if (enableDownScale)
                DebugUtils.AddDebugStr("enableDownScale = true: " + imageOptimizationHelper.downscaleRatio + " / " + webCamTextureToMatHelper.GetWidth() / imageOptimizationHelper.downscaleRatio + " x " + webCamTextureToMatHelper.GetHeight() / imageOptimizationHelper.downscaleRatio);


            Matrix4x4 projectionMatrix;
#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
            
            // This returns the projection matrix at the time the frame was captured, if location data is available.
            // If it's not, that is probably an indication that the HoloLens is not tracking and its location is not known.
            // It could also mean the VideoCapture stream is not running.
            // If location data is unavailable then the projection matrix will be set to the identity matrix.
            projectionMatrix = webCamTextureToMatHelper.GetProjectionMatrix();
            
            // Applying projection matrix on the texture.
            quad_renderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);
#else
            // This value is obtained from PhotoCapture's TryGetProjectionMatrix() method. I do not know whether this method is good.
            // Please see the discussion of this thread. Https://forums.hololens.com/discussion/782/live-stream-of-locatable-camera-webcam-in-unity
            projectionMatrix = Matrix4x4.identity;
            projectionMatrix.m00 = 2.31029f;
            projectionMatrix.m01 = 0.00000f;
            projectionMatrix.m02 = 0.09614f;
            projectionMatrix.m03 = 0.00000f;
            projectionMatrix.m10 = 0.00000f;
            projectionMatrix.m11 = 4.10427f;
            projectionMatrix.m12 = -0.06231f;
            projectionMatrix.m13 = 0.00000f;
            projectionMatrix.m20 = 0.00000f;
            projectionMatrix.m21 = 0.00000f;
            projectionMatrix.m22 = -1.00000f;
            projectionMatrix.m23 = 0.00000f;
            projectionMatrix.m30 = 0.00000f;
            projectionMatrix.m31 = 0.00000f;
            projectionMatrix.m32 = -1.00000f;
            projectionMatrix.m33 = 0.00000f;
            quad_renderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);
#endif

            quad_renderer.sharedMaterial.SetFloat("_VignetteScale", 0.0f);

            // This is face detection happening.
            // FOR UNITY EDITOR
            cascade = new CascadeClassifier();
            cascade.load(Utils.getFilePath("objdetect/lbpcascade_frontalface.xml"));
#if !WINDOWS_UWP || UNITY_EDITOR
            // "empty" method is not working on the UWP platform.
            if (cascade.empty())
            {
                Debug.LogError("cascade file is not loaded. Please copy from “OpenCVForUnity/StreamingAssets/objdetect/” to “Assets/StreamingAssets/objdetect/” folder. ");
            }
#endif

            grayMat4Thread = new Mat();
            // FOR UWP a.k.a HOLOLENS
            cascade4Thread = new CascadeClassifier();
            //cascade4Thread.load(Utils.getFilePath("objdetect/haarcascade_frontalface_alt.xml"));
            cascade4Thread.load(Utils.getFilePath("objdetect/lbpcascade_frontalface.xml"));
#if !WINDOWS_UWP || UNITY_EDITOR
            // "empty" method is not working on the UWP platform.
            if (cascade4Thread.empty())
            {
                Debug.LogError("cascade file is not loaded. Please copy from “OpenCVForUnity/StreamingAssets/objdetect/” to “Assets/StreamingAssets/objdetect/” folder. ");
            }
#endif
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API

            while (isDetectingInFrameArrivedThread)
            {
                //Wait detecting stop
            }
#endif

            StopThread();
            lock (ExecuteOnMainThread)
            {
                ExecuteOnMainThread.Clear();
            }
            hasUpdatedDetectionResult = false;
            isDetecting = false;

            if (cascade != null)
                cascade.Dispose();

            if (grayMat4Thread != null)
                grayMat4Thread.Dispose();

            if (cascade4Thread != null)
                cascade4Thread.Dispose();

            rectangleTracker.Reset();

            if (debugStr != null)
            {
                debugStr.text = string.Empty;
            }
            DebugUtils.ClearDebugStr();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // This is only for when its UWP and hololensStream is ON.
#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API

        // This method is subscriber of "webCamTextureToMatHelper.frameMatAcquired" event.
        public void OnFrameMatAcquired(Mat grayMat, Matrix4x4 projectionMatrix, Matrix4x4 cameraToWorldMatrix, CameraIntrinsics cameraIntrinsics)
        {
            isDetectingInFrameArrivedThread = true;
            DebugUtils.VideoTick();

            Mat downScaleMat = null;
            float DOWNSCALE_RATIO;
            if (enableDownScale)
            {
                downScaleMat = imageOptimizationHelper.GetDownScaleMat(grayMat);
                DOWNSCALE_RATIO = imageOptimizationHelper.downscaleRatio;
            }
            else
            {
                downScaleMat = grayMat;
                DOWNSCALE_RATIO = 1.0f;
            }

            // The first parameter is input and the second param is output.
            // improves the contrast in an image and make the region with higher pixel intensities clearer.
            Imgproc.equalizeHist(downScaleMat, downScaleMat);
            
            // Detecting is STARTING

            // Initially this TRUE and isDetecting is FALSE
            if (enableDetection && !isDetecting)
            {
                isDetecting = true;

                // Copying the values of "downScaleMat" matrix in "grayMat4Thread" values.
                downScaleMat.copyTo(grayMat4Thread);

                System.Threading.Tasks.Task.Run(() =>
                {
                    isThreadRunning = true;

                    // Here, object is being detected. detectionResult is list of rects.
                    // Here I have to change the detection result based on my desired region input.
                    // Need to implement this function which will keep the track of all the regions in the frame
                    // that user has select to track.
                    // THIS IS MY METHOD: GetTrackRegions(grayMat4Thread, out currentTrackRegions);
                    
                    // Here I can run CamShift Algorithm once the region of interest is selected which I can control through bool variable.

                    // DOING IT OUTSIDE TREAD:

                    initialRect.width = (int)Math.Floor(100*widthRatio);
                    initialRect.height = (int)Math.Floor(50*heigthRatio);
                    initialRect.x = (hsvMat.width() / 2) - (initialRect.width / 2);
                    initialRect.y = (hsvMat.height() / 2) - (initialRect.height / 2);

                    if (regionOfInterestSelected)
                    {
                        //Debug.Log($"Tracking Started..");
                        //Debug.Log($"Mat recieved of type {grayMat4Thread.channels()}");
                        Imgproc.cvtColor(grayMat4Thread, hsvMat, Imgproc.COLOR_BGR2HSV);

                        //hsvMat = grayMat4Thread;
                        if (camShiftInitialization)
                        {
                            camShiftInitialization = false;

                            //Debug.Log($"Initializing Camshift..");
                            if (initialRect != null)
                            {
                                roiRect = initialRect;
                            }
                            else
                                Debug.LogError("initialRect is Null");


                            // Using KCF

                            /*if (tracker != null)
                            {
                                tracker.Dispose();
                                tracker = null;
                            }
                            
                            // Initialize the KCF tracker
                            tracker = TrackerKCF.create(new TrackerKCF_Params());
                            tracker.init(grayMat4Thread, roiRect);
                            Debug.Log($"Initialized KCF..");*/

                            // Here we can start camshift algorithm. As we have the rect roi. 
                            if (roiHistMat != null)
                            {
                                roiHistMat.Dispose();
                                roiHistMat = null;
                            }
                            roiHistMat = new Mat();

                            using (Mat roiHSVMat = new Mat(hsvMat, roiRect))
                            using (Mat maskMat = new Mat())
                            {
                                Imgproc.calcHist(new List<Mat>(new Mat[] { roiHSVMat }), new MatOfInt(0), maskMat, roiHistMat, new MatOfInt(16), new MatOfFloat(0, 180));
                                Core.normalize(roiHistMat, roiHistMat, 0, 255, Core.NORM_MINMAX);
                            }
                        }
                        else 
                        {
                            // update the tracker
                          /*tracker.update(grayMat4Thread, roiRect);

                            if (detectionResult.Count > 0)
                                detectionResult[0] = roiRect;
                            else
                                detectionResult.Add(roiRect);
                            Debug.Log($"Current KCF Track Rect: {roiRect}, RectListLen: {detectionResult.Count}");*/

                            using (Mat backProj = new Mat())
                            {
                                Imgproc.calcBackProject(new List<Mat>(new Mat[] { grayMat4Thread }), new MatOfInt(0), roiHistMat, backProj, new MatOfFloat(0, 256), 1.0);
                                RotatedRect rotatedRect = Video.CamShift(backProj, roiRect, termination);

                                if (detectionResult.Count > 0)
                                    detectionResult[0] = rotatedRect.boundingRect();
                                else
                                    detectionResult.Add(rotatedRect.boundingRect());
                                //Debug.Log($"Current Track Rect: {rotatedRect.boundingRect()}, RectListLen: {detectionResult.Count}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"Tracking not started...");
                    }

                    isThreadRunning = false;
                    // In this method we are setting "hasUpdatedDetectionResult" to TRUE
                    // and isDetecting to FALSE
                    OnDetectionDone();
                });
            }
            
            // Need to understand what "useSeparateDetection" really do.
            if (!useSeparateDetection)
            {
                if (hasUpdatedDetectionResult)
                {
                    hasUpdatedDetectionResult = false;

                    lock (rectangleTracker)
                    {
                        rectangleTracker.UpdateTrackedObjects(detectionResult);
                    }
                }

                lock (rectangleTracker)
                {
                    rectangleTracker.GetObjects(resultObjects, true);
                }

                if (displayCameraImage)
                {
                    Imgproc.putText(grayMat, "W:" + grayMat.width() + " H:" + grayMat.height(), new Point(5, grayMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                }
                else
                {
                    // fill all black.
                    Imgproc.rectangle(grayMat, new Point(0, 0), new Point(grayMat.width(), grayMat.height()), new Scalar(0, 0, 0, 0), -1);
                }

                // draw face rect.
                if(regionOfInterestSelected)
                {
                    DrawDownScaleFaceRects(grayMat, detectionResult.ToArray(), DOWNSCALE_RATIO, COLOR_WHITE, 2);
                }
                else
                {
                    DrawDownScaleFaceRects(grayMat, new Rect[] { initialRect }, DOWNSCALE_RATIO, COLOR_WHITE, 2);
                }
            }
 /*           else
            {
                Rect[] rectsWhereRegions;

                if (hasUpdatedDetectionResult)
                {
                    hasUpdatedDetectionResult = false;

                    //Enqueue(() =>
                    //{
                    //    Debug.Log("process: get rectsWhereRegions were got from detectionResult");
                    //});

                    lock (rectangleTracker)
                    {
                        rectsWhereRegions = detectionResult.ToArray();
                    }
                }
                else
                {
                    //Enqueue(() =>
                    //{
                    //    Debug.Log("process: get rectsWhereRegions from previous positions");
                    //});

                    lock (rectangleTracker)
                    {
                        rectsWhereRegions = rectangleTracker.CreateCorrectionBySpeedOfRects();
                    }
                }

                detectedObjectsInRegions.Clear();
                int len = rectsWhereRegions.Length;
                for (int i = 0; i < len; i++)
                {
                    DetectInRegion(downScaleMat, rectsWhereRegions[i], detectedObjectsInRegions, cascade);
                }

                lock (rectangleTracker)
                {
                    rectangleTracker.UpdateTrackedObjects(detectedObjectsInRegions);
                    rectangleTracker.GetObjects(resultObjects, true);
                }

                if (displayCameraImage)
                {
                    Imgproc.putText(grayMat, "W:" + grayMat.width() + " H:" + grayMat.height(), new Point(5, grayMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                }
                else
                {
                    // fill all black.
                    Imgproc.rectangle(grayMat, new Point(0, 0), new Point(grayMat.width(), grayMat.height()), new Scalar(0, 0, 0, 0), -1);
                }

                // draw previous rect.
                DrawDownScaleFaceRects(grayMat, rectsWhereRegions, DOWNSCALE_RATIO, COLOR_GRAY, 2);

                // draw face rect.
                DrawDownScaleFaceRects(grayMat, resultObjects.ToArray(), DOWNSCALE_RATIO, COLOR_WHITE, 2);
            }*/

            DebugUtils.TrackTick();

            Enqueue(() =>
            {
                if (!webCamTextureToMatHelper.IsPlaying()) return;

                Utils.matToTexture2D(grayMat, texture);
                grayMat.Dispose();

                Matrix4x4 worldToCameraMatrix = cameraToWorldMatrix.inverse;

                quad_renderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", worldToCameraMatrix);
                quad_renderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);

                // Position the canvas object slightly in front
                // of the real world web camera.
                Vector3 position = cameraToWorldMatrix.GetColumn(3) - cameraToWorldMatrix.GetColumn(2) * 2.2f;

                // Rotate the canvas object so that it faces the user.
                Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));


                // Here, check if the object has been selected or not. Then set the position of the quad 
                // to where the user is seeing in the space.

                gameObject.transform.SetPositionAndRotation(position, rotation);

            });

            isDetectingInFrameArrivedThread = false;
        }

        private void Update()
        {
            lock (ExecuteOnMainThread)
            {
                while (ExecuteOnMainThread.Count > 0)
                {
                    ExecuteOnMainThread.Dequeue().Invoke();
                }
            }
            
            //GetHandRayPosition();
        }

        private void Enqueue(Action action)
        {
            lock (ExecuteOnMainThread)
            {
                ExecuteOnMainThread.Enqueue(action);
            }
        }

#else

        // Update is called once per frame
        void Update()
        {
            lock (ExecuteOnMainThread)
            {
                while (ExecuteOnMainThread.Count > 0)
                {
                    ExecuteOnMainThread.Dequeue().Invoke();
                }
            }

            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                DebugUtils.VideoTick();

                Mat grayMat = webCamTextureToMatHelper.GetMat();

                Mat downScaleMat = null;
                float DOWNSCALE_RATIO;
                if (enableDownScale)
                {
                    downScaleMat = imageOptimizationHelper.GetDownScaleMat(grayMat);
                    DOWNSCALE_RATIO = imageOptimizationHelper.downscaleRatio;
                }
                else
                {
                    downScaleMat = grayMat;
                    DOWNSCALE_RATIO = 1.0f;
                }

                Imgproc.equalizeHist(downScaleMat, downScaleMat);

                if (enableDetection && !isDetecting)
                {
                    isDetecting = true;

                    //Debug.Log($"grayMat size: {grayMat.size()}");
                    downScaleMat.copyTo(grayMat4Thread);
                    StartThread(ThreadWorker);

                    // DOING IT OUTSIDE TREAD:
                    initialRect.width = (int)Math.Floor(100*widthRatio);
                    initialRect.height = (int)Math.Floor(50*heigthRatio);
                    initialRect.x = (hsvMat.width() / 2) - (initialRect.width / 2);
                    initialRect.y = (hsvMat.height() / 2) - (initialRect.height / 2);

                    //DetectObject(grayMat4Thread, out detectionResult, cascade4Thread);
                    // Here I can run CamShift Algorithm once the region of interest is selected which I can control through bool variable.

                    if (regionOfInterestSelected)
                    {
                        Debug.Log($"Tracking Started..");
                        
                        Imgproc.cvtColor(grayMat4Thread, hsvMat, Imgproc.COLOR_BGR2HSV);

                        //hsvMat = grayMat4Thread;
                        if (camShiftInitialization)
                        {
                            camShiftInitialization = false;

                            //Debug.Log($"Initializing Camshift..");
                            if (initialRect != null)
                            {
                                roiRect = initialRect;
                            }
                            else
                                Debug.LogError("initialRect is Null");


                            // Using KCF

                            /*if (tracker != null)
                            {
                                tracker.Dispose();
                                tracker = null;
                            }

                            // Initialize the KCF tracker
                            tracker = TrackerKCF.create(new TrackerKCF_Params());
                            tracker.init(grayMat4Thread, roiRect);
                            Debug.Log($"Initialized KCF..");*/

                            // Here we can start camshift algorithm. As we have the rect roi. 
                            if (roiHistMat != null)
                            {
                                roiHistMat.Dispose();
                                roiHistMat = null;
                            }
                            roiHistMat = new Mat();

                            using (Mat roiHSVMat = new Mat(hsvMat, roiRect))
                            using (Mat maskMat = new Mat())
                            {
                                Imgproc.calcHist(new List<Mat>(new Mat[] { roiHSVMat }), new MatOfInt(0), maskMat, roiHistMat, new MatOfInt(16), new MatOfFloat(0, 180));
                                Core.normalize(roiHistMat, roiHistMat, 0, 255, Core.NORM_MINMAX);
                            }
                        }
                        else 
                        {
                            /*// update the tracker
                            tracker.update(grayMat4Thread, roiRect);

                            if (detectionResult.Count > 0)
                                detectionResult[0] = roiRect;
                            else
                                detectionResult.Add(roiRect);
                            Debug.Log($"Current KCF Track Rect: {roiRect}, RectListLen: {detectionResult.Count}");*/

                            using (Mat backProj = new Mat())
                            {
                                Imgproc.calcBackProject(new List<Mat>(new Mat[] { grayMat4Thread }), new MatOfInt(0), roiHistMat, backProj, new MatOfFloat(0, 256), 1.0);
                                RotatedRect rotatedRect = Video.CamShift(backProj, roiRect, termination);

                                if (detectionResult.Count > 0)
                                    detectionResult[0] = rotatedRect.boundingRect();
                                else
                                    detectionResult.Add(rotatedRect.boundingRect());
                                Debug.Log($"Current Track Rect: {rotatedRect.boundingRect()}, Rect centre: {rotatedRect.center}");
                                double diff_x = rotatedRect.center.x - grayMat4Thread.width()/2;
                                double diff_y = rotatedRect.center.y - grayMat4Thread.height() / 2;
                                Debug.Log($"(diff_x, diff_y) = ({diff_x}, {diff_y})");
                                double drone_pos_x = gameObject.transform.position.x + diff_x;
                                double drone_pos_y = gameObject.transform.position.y + diff_y;
                                _drone_model.transform.position = new Vector3((float)drone_pos_x, (float)drone_pos_y, gameObject.transform.position.z);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"Please adjust Bounding Box and close the UI tap to started tracking.");
                    }
                }

                if (!useSeparateDetection)
                {
                    if (hasUpdatedDetectionResult)
                    {
                        hasUpdatedDetectionResult = false;
                        rectangleTracker.UpdateTrackedObjects(detectionResult);
                    }
                    rectangleTracker.GetObjects(resultObjects, true);

                    if (displayCameraImage)
                    {
                        Imgproc.putText(grayMat, "W:" + grayMat.width() + " H:" + grayMat.height(), new Point(5, grayMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                    else
                    {
                        // fill all black.
                        Imgproc.rectangle(grayMat, new Point(0, 0), new Point(grayMat.width(), grayMat.height()), new Scalar(0, 0, 0, 0), -1);
                    }
                    
                    if(regionOfInterestSelected)
                    {
                        DrawDownScaleFaceRects(grayMat, detectionResult.ToArray(), DOWNSCALE_RATIO, COLOR_WHITE, 2);
                    }
                    else
                    {
                        //Imgproc.rectangle(grayMat, new Point(grayMat.width() / 2, grayMat.height() / 2), 5, COLOR_WHITE, 4);
                        DrawDownScaleFaceRects(grayMat, new Rect[] { initialRect }, DOWNSCALE_RATIO, COLOR_WHITE, 2);
                    }
                }
/*                else
                {
                    Rect[] rectsWhereRegions;

                    if (hasUpdatedDetectionResult)
                    {
                        hasUpdatedDetectionResult = false;

                        //Debug.Log("process: get rectsWhereRegions were got from detectionResult");
                        rectsWhereRegions = detectionResult.ToArray();
                    }
                    else
                    {
                        //Debug.Log("process: get rectsWhereRegions from previous positions");
                        rectsWhereRegions = rectangleTracker.CreateCorrectionBySpeedOfRects();
                    }

                    detectedObjectsInRegions.Clear();
                    int len = rectsWhereRegions.Length;
                    for (int i = 0; i < len; i++)
                    {
                        DetectInRegion(downScaleMat, rectsWhereRegions[i], detectedObjectsInRegions, cascade);
                    }

                    rectangleTracker.UpdateTrackedObjects(detectedObjectsInRegions);
                    rectangleTracker.GetObjects(resultObjects, true);

                    if (displayCameraImage)
                    {
                        Imgproc.putText(grayMat, "W:" + grayMat.width() + " H:" + grayMat.height(), new Point(5, grayMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                    else
                    {
                        // fill all black.
                        Imgproc.rectangle(grayMat, new Point(0, 0), new Point(grayMat.width(), grayMat.height()), new Scalar(0, 0, 0, 0), -1);
                    }

                    // draw previous rect.
                    DrawDownScaleFaceRects(grayMat, rectsWhereRegions, DOWNSCALE_RATIO, COLOR_GRAY, 1);

                    // draw face rect.
                    DrawDownScaleFaceRects(grayMat, resultObjects.ToArray(), DOWNSCALE_RATIO, COLOR_WHITE, 6);
                }*/

                DebugUtils.TrackTick();

                Utils.matToTexture2D(grayMat, texture);
            }

            if (webCamTextureToMatHelper.IsPlaying())
            {

                Matrix4x4 cameraToWorldMatrix = Camera.main.cameraToWorldMatrix;
                Matrix4x4 worldToCameraMatrix = cameraToWorldMatrix.inverse;

                quad_renderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", worldToCameraMatrix);
                
                // Position the canvas object slightly in front
                // of the real world web camera.
                Vector3 position = cameraToWorldMatrix.GetColumn(3) - cameraToWorldMatrix.GetColumn(2) * 2.2f;

                // Rotate the canvas object so that it faces the user.
                Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

                gameObject.transform.SetPositionAndRotation(position, rotation);
            }

            //GetHandRayPosition();
        }
#endif

        private void StartThread(Action action)
        {
#if WINDOWS_UWP || (!UNITY_WSA_10_0 && (NET_4_6 || NET_STANDARD_2_0))
            System.Threading.Tasks.Task.Run(() => action());
#else
            ThreadPool.QueueUserWorkItem(_ => action());
#endif
        }

        private void StopThread()
        {
            if (!isThreadRunning)
                return;

            while (isThreadRunning)
            {
                //Wait threading stop
            }
        }

        private void ThreadWorker()
        {
            isThreadRunning = true;

            lock (ExecuteOnMainThread)
            {
                if (ExecuteOnMainThread.Count == 0)
                {
                    ExecuteOnMainThread.Enqueue(() =>
                    {
                        OnDetectionDone();
                    });
                }
            }

            isThreadRunning = false;
        }

        private void DetectObject(Mat img, out List<Rect> detectedObjects, CascadeClassifier cascade)
        {
            int d = Mathf.Min(img.width(), img.height());
            d = (int)Mathf.Round(d * minDetectionSizeRatio);

            MatOfRect objects = new MatOfRect();
            if (cascade != null)
                cascade.detectMultiScale(img, objects, 1.1, 2, Objdetect.CASCADE_SCALE_IMAGE, new Size(d, d), new Size());

            detectedObjects = objects.toList();
        }

        private void OnDetectionDone()
        {
            hasUpdatedDetectionResult = true;

            isDetecting = false;
        }

        private void DetectInRegion(Mat img, Rect region, List<Rect> detectedObjectsInRegions, CascadeClassifier cascade)
        {
            Rect r0 = new Rect(new Point(), img.size());
            Rect r1 = new Rect(region.x, region.y, region.width, region.height);
            Rect.inflate(r1, (int)((r1.width * coeffTrackingWindowSize) - r1.width) / 2,
                (int)((r1.height * coeffTrackingWindowSize) - r1.height) / 2);
            r1 = Rect.intersect(r0, r1);

            if ((r1.width <= 0) || (r1.height <= 0))
            {
                Debug.Log("detectInRegion: Empty intersection");
                return;
            }

            int d = Math.Min(region.width, region.height);
            d = (int)Math.Round(d * coeffObjectSizeToTrack);

            using (MatOfRect tmpobjects = new MatOfRect())
            using (Mat img1 = new Mat(img, r1)) //subimage for rectangle -- without data copying
            {
                cascade.detectMultiScale(img1, tmpobjects, 1.1, 2, 0 | Objdetect.CASCADE_DO_CANNY_PRUNING | Objdetect.CASCADE_SCALE_IMAGE | Objdetect.CASCADE_FIND_BIGGEST_OBJECT, new Size(d, d), new Size());

                Rect[] tmpobjectsArray = tmpobjects.toArray();
                int len = tmpobjectsArray.Length;
                for (int i = 0; i < len; i++)
                {
                    Rect tmp = tmpobjectsArray[i];
                    Rect rx = new Rect(new Point(tmp.x + r1.x, tmp.y + r1.y), tmp.size());
                    detectedObjectsInRegions.Add(rx);
                }
            }
        }

        private void DrawDownScaleFaceRects(Mat img, Rect[] rects, float downscaleRatio, Scalar color, int thickness)
        {
            int len = rects.Length;
            for (int i = 0; i < len; i++)
            {
                Rect rect = new Rect(
                    (int)(rects[i].x * downscaleRatio),
                    (int)(rects[i].y * downscaleRatio),
                    (int)(rects[i].width * downscaleRatio),
                    (int)(rects[i].height * downscaleRatio)
                );
                Imgproc.rectangle(img, rect, color, thickness);
            }
        }

        void LateUpdate()
        {
            DebugUtils.RenderTick();
            float renderDeltaTime = DebugUtils.GetRenderDeltaTime();
            float videoDeltaTime = DebugUtils.GetVideoDeltaTime();
            float trackDeltaTime = DebugUtils.GetTrackDeltaTime();

            if (renderFPS != null)
            {
                renderFPS.text = string.Format("Render: {0:0.0} ms ({1:0.} fps)", renderDeltaTime, 1000.0f / renderDeltaTime);
            }
            if (videoFPS != null)
            {
                videoFPS.text = string.Format("Video: {0:0.0} ms ({1:0.} fps)", videoDeltaTime, 1000.0f / videoDeltaTime);
            }
            if (trackFPS != null)
            {
                trackFPS.text = string.Format("Track:   {0:0.0} ms ({1:0.} fps)", trackDeltaTime, 1000.0f / trackDeltaTime);
            }
            if (debugStr != null)
            {
                if (DebugUtils.GetDebugStrLength() > 0)
                {
                    if (debugStr.preferredHeight >= debugStr.rectTransform.rect.height)
                        debugStr.text = string.Empty;

                    debugStr.text += DebugUtils.GetDebugStr();
                    DebugUtils.ClearDebugStr();
                }
            }
            if (TelloBattery != null)
            {
                float tello_battery = rosSubscriberHandler.getTelloBattary();
                TelloBattery.text = $"Tello Battery : {tello_battery}";
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            rayCastAndLabelHandler.AirTapAcquired -= OnAirTapAcquired;

#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
            webCamTextureToMatHelper.frameMatAcquired -= OnFrameMatAcquired;
#endif
            webCamTextureToMatHelper.Dispose();
            imageOptimizationHelper.Dispose();

            if (rectangleTracker != null)
                rectangleTracker.Dispose();
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void DecreaseBBHeigth()
        {
            heigthRatio -= 0.1;
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void IncreaseBBHeigth()
        {

            heigthRatio += 0.1;
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void DecreaseBBWidth()
        {
            widthRatio -= 0.1;
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void IncreaseBBWidth()
        {
            widthRatio += 0.1;
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing();
        }

        /// <summary>
        /// Raises the enable downscale toggle value changed event.
        /// </summary>
        public void OnEnableDownScaleToggleValueChanged()
        {
            enableDownScale = enableDownScaleToggle.isOn;

            if (rectangleTracker != null)
            {
                lock (rectangleTracker)
                {
                    rectangleTracker.Reset();
                }
            }
        }

        /// <summary>>
        /// Raises the use separate detection toggle value changed event.
        /// </summary>
        public void OnUseSeparateDetectionToggleValueChanged()
        {
            useSeparateDetection = useSeparateDetectionToggle.isOn;

            if (rectangleTracker != null)
            {
                lock (rectangleTracker)
                {
                    rectangleTracker.Reset();
                }
            }
        }

        /// <summary>
        /// Raises the display camera image toggle value changed event.
        /// </summary>
        public void OnDisplayCameraImageToggleValueChanged()
        {
            displayCameraImage = displayCameraImageToggle.isOn;
        }


        private void GetHandRayPosition()
        {
            foreach (var source in CoreServices.InputSystem.DetectedInputSources)
            {
                // Ignore anything that is not a hand because we want articulated hands
                if (source.SourceType == InputSourceType.Hand)
                {
                    foreach (var p in source.Pointers)
                    {
                        if (p is IMixedRealityNearPointer)
                        {
                            // Ignore near pointers, we only want the rays
                            continue;
                        }
                        if (p.Result != null)
                        {
                            var startPoint = p.Position;
                            handGazePosition = p.Result.Details.Point;
                            Debug.Log($"handGazePosition : {handGazePosition}");
                        }
                    }
                }
            }
        }

        //public void LabelObjects(Mat imageMatrix, Transform cameraTransform)
        public void LabelObjects(Transform cameraTransform)
        {
            CallingLabelObjectsCheck.text = string.Format("Inside LabelObjects..");
            ClearLabels();
            ////var heightFactor = imageMatrix.height() / imageMatrix.width();
            /*var topCorner = cameraTransform.position + cameraTransform.forward -
                            cameraTransform.right / 2f +
                            cameraTransform.up * heightFactor / 2f;*/

            //var recognizedPos = topCorner + cameraTransform.right * imageCenter.x -
            //                                cameraTransform.up * imageCenter.y * heightFactor;


            CallingLabelObjectsCheck.text = string.Format("handGazePosition {0}", handGazePosition);
            var recognizedPos = cameraTransform.position + handGazePosition;
            //var recognizedPos = cameraTransform.position + gameObject.transform.position;

            /*#if UNITY_EDITOR
                        _createdObjects.Add(CreateLabel(_labelText, handGazePosition));
            #endif*/
            CallingRayCastCheck.text = string.Format("CallingRayCastCheck: Calling..");
            var labelPos = DoRaycastOnSpatialMap(cameraTransform, recognizedPos);
            if (labelPos != null)
            {
                Debug.Log($"LabelPos : {labelPos.Value}");
                _createdObjects.Add(CreateLabel(_labelText, labelPos.Value));
            }
            else
            {
                CallingRayCastCheck.text = $"LabelPos is NULL.";
                Debug.Log("LabelPos is NULL");
            }

            Destroy(cameraTransform.gameObject);
        }

        /// <summary>
        /// Making a camera transform
        /// </summary>
        /// <returns></returns>
        public Transform CopyCameraTransForm()
        {
            //Debug.Log("Inside CopyCameraTransForm()");
            GameObject g = new GameObject();
            //Debug.Log("Accessing CameraCache.");
            g.transform.SetPositionAndRotation(CameraCache.Main.transform.position, CameraCache.Main.transform.rotation);
            g.transform.localScale = CameraCache.Main.transform.localScale;
            return g.transform;
        }


        public Vector3? DoRaycastOnSpatialMap(Transform cameraTransform, Vector3 recognitionCenterPos)
        {
            //Debug.Log("Inside DoRaycastOnSpatialMap()");
            RaycastHit hitInfo;

            //if (Physics.Raycast(cameraTransform.position, (recognitionCenterPos - cameraTransform.position), out hitInfo, 15f, GetSpatialMeshMask()))
            if (Physics.Raycast(cameraTransform.position, (handGazePosition - cameraTransform.position).normalized, out hitInfo, 15f, GetSpatialMeshMask()))
            {
                //return hitPoint ?? CalculatePositionDeadAhead(15f);
                CallingRayCastCheck.text = ($"Dist: {hitInfo.distance}");
                RayCastDistance = hitInfo.distance;
                return hitInfo.point;
            }
            else
            {
                //Debug.DrawRay(cameraTransform.position, (recognitionCenterPos - cameraTransform.position)* 1000, Color.white);
                Debug.Log("Did not Hit");
            }
            return null;
        }

        public void ClearLabels()
        {
            //CallingRayCastCheck.text = "Inside ClearLabels..";
            foreach (var label in _createdObjects)
            {
                Destroy(label);
            }
            _createdObjects.Clear();
        }

        public static int GetSpatialMeshMask()
        {
            //Debug.Log("Inside GetSpatialMeshMask()");
            if (_meshPhysicsLayer == 0)
            {
                var spatialMappingConfig = CoreServices.SpatialAwarenessSystem.ConfigurationProfile as
                    MixedRealitySpatialAwarenessSystemProfile;
                if (spatialMappingConfig != null)
                {
                    foreach (var config in spatialMappingConfig.ObserverConfigurations)
                    {
                        var observerProfile = config.ObserverProfile
                            as MixedRealitySpatialAwarenessMeshObserverProfile;
                        if (observerProfile != null)
                        {
                            _meshPhysicsLayer |= 1 << observerProfile.MeshPhysicsLayer;
                        }
                    }
                }
            }

            return _meshPhysicsLayer;
        }

        public GameObject CreateLabel(string text, Vector3 location)
        {
            //Debug.Log("Inside CreateLabel()");
            var labelObject = Instantiate(_labelObject);
            var toolTip = labelObject.GetComponent<ToolTip>();
            //toolTip.ShowOutline = false;
            toolTip.ShowBackground = true;
            toolTip.ToolTipText = $"{text} {RayCastDistance}";
            toolTip.transform.position = location + Vector3.up * 0.05f;
            //toolTip.transform.parent = _labelContainer.transform;
            toolTip.AttachPointPosition = location;
            //toolTip.ContentParentTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            var connector = toolTip.GetComponent<ToolTipConnector>();
            connector.PivotDirectionOrient = ConnectorOrientType.OrientToCamera;
            connector.Target = labelObject;
            return labelObject;
        }

        public static Vector3 CalculatePositionDeadAhead(float distance = 15)
        {
            return CameraCache.Main.transform.position +
                   CameraCache.Main.transform.forward.normalized * distance;
        }

    }
}