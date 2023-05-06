using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggler : MonoBehaviour
{
    public GameObject UIUnityGameObject;
    private bool IsUnityUIActive = true;

    public void UnityUIToggle()
    {
        if(IsUnityUIActive)
        {
            UIUnityGameObject.SetActive(false);
            IsUnityUIActive = false;
        }
        else
        {
            UIUnityGameObject.SetActive(true);
            IsUnityUIActive = true;
        }
    }

    
}
