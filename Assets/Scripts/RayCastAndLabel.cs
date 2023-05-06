using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.Windows.WebCam;
using UnityEngine;
using System;
using UnityEngine.XR.WSA.Input;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;

namespace HoloTracking
{
    
    public class RayCastAndLabel : BaseInputHandler, IMixedRealityInputHandler
    {
        // delegates are function containers.
        public event Action AirTapAcquired;

        [SerializeField]
        private MixedRealityInputAction myAction = MixedRealityInputAction.None;

        public void OnInputUp(InputEventData eventData)
        {

        }

        public void OnInputDown(InputEventData eventData)
        {
            if (eventData.MixedRealityInputAction == myAction)
            {
                // Invoke custom event for rayCast
                AirTapAcquiredTriggered();
            }
        }

        public void AirTapAcquiredTriggered()
        {
            if (AirTapAcquired != null)
            {
                //Debug.Log($"AirTap action triggered");
                AirTapAcquired?.Invoke();
            }
            else
            {
                Debug.LogError("AirTapAcquired is NULL");
            }
        }

        protected override void RegisterHandlers()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler>(this);
        }

        protected override void UnregisterHandlers()
        {

        }
    }
}
