using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class AirTapGesture : MonoBehaviour
{

    private GestureRecognizer recognizer;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribing to the HoloLens API gesture recognizer to track user gestures
        recognizer = new GestureRecognizer(GestureSettings.Tap);
        recognizer.Start();
    }

    public void OnInputDown(InputEventData eventData)
    {
        Debug.Log($"AirTap action triggered");
    }
}
