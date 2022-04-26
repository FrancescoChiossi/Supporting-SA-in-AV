using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CameraProjection : MonoBehaviour
{
    //not used & not finished
    //offset not correct
    //for correct display: change m to oblique

    Camera cam;
    
    private void Start()
    {
        cam = GetComponent<Camera>();
    }
 
    void LateUpdate()
    {
        float hOffset;
        if (this.name == "Right Camera") hOffset = -0.15f;
        else hOffset = 0.35f;
        Matrix4x4 m = cam.projectionMatrix;
    }

}
