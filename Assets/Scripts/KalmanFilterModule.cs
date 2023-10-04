using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.VideoModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;


namespace HoloTracking
{
    public class KalmanFilterModule
    {
        private KalmanFilter kalman;
        private int dynamicParams = 3; // Number of dynamic parameters (x, y, z)
        private Mat measurement; // Matrix to store measurements

        public KalmanFilterModule()
        {
            kalman = new KalmanFilter(dynamicParams, dynamicParams, 0, CvType.CV_32F);
            measurement = new Mat(dynamicParams, 1, CvType.CV_32F);

            Mat initialState = new Mat(dynamicParams, 1, CvType.CV_32F);
            initialState.put(0, 0, 0.0); // x-position
            initialState.put(1, 0, 0.0); // y-position
            initialState.put(2, 0, 2.2); // z-position
                                         // ...
                                         // Assign initial state to Kalman Filter
            kalman.set_statePre(initialState);

            // Initialize state transition matrix (identity for constant velocity model)
            kalman.set_transitionMatrix(Mat.eye(dynamicParams, dynamicParams, CvType.CV_32F));

            Mat processNoiseCov = Mat.eye(dynamicParams, dynamicParams, CvType.CV_32F);
            processNoiseCov.convertTo(processNoiseCov, -1, 1e-2);
            kalman.set_processNoiseCov(processNoiseCov);

            Mat measurementNoiseCov = Mat.eye(dynamicParams, dynamicParams, CvType.CV_32F);
            measurementNoiseCov.convertTo(measurementNoiseCov, -1, 1e-1);
            kalman.set_measurementNoiseCov(measurementNoiseCov);


            // Initialize initial state estimate (zero)
            kalman.set_errorCovPre(Mat.eye(dynamicParams, dynamicParams, CvType.CV_32F));
        }

        public Vector3 ApplyFilter(Vector3 measurementVector)
        {
            // Convert Vector3 to Mat
            measurement.put(0, 0, measurementVector.x);
            measurement.put(1, 0, measurementVector.y);
            measurement.put(2, 0, measurementVector.z);

            // The 'predict' method estimates the next state
            Mat predictionMat = kalman.predict();

            // The 'correct' method computes a new state estimate from a measurement
            Mat correctedMat = kalman.correct(measurement);
            Debug.Log($"correctedMat: {correctedMat.size()}");
            // Convert corrected Mat to Vector3
            Vector3 corrected = new Vector3((float)correctedMat.get(0, 0)[0], (float)correctedMat.get(1, 0)[0], (float)correctedMat.get(2, 0)[0]);

            return corrected;
        }
    }
}