using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    GameObject _player;
    ThirdPersonController tpc;
    //camera states for 'cinematic dialogue'
    public Vector3 zoomedOutPos, zoomedOutRot, boatPos, boatRot;
    public float zoomedOutFOV, zoomedOutClip, boatFOV, boatClip;
    public bool zoomedOut = true, inBoat;
    //camera ref
    Camera mainCam;
    float origActDistance;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        tpc = _player.GetComponent<ThirdPersonController>();
        mainCam = GetComponent<Camera>();
    }

    void Update()
    {
        //normal follow player 
        if (zoomedOut)
        {
            transform.SetParent(null);
            transform.position = new Vector3(_player.transform.position.x + zoomedOutPos.x,
            _player.transform.position.y + zoomedOutPos.y, _player.transform.position.z + zoomedOutPos.z);
            transform.localEulerAngles = zoomedOutRot;
            mainCam.fieldOfView = zoomedOutFOV;
            mainCam.nearClipPlane = zoomedOutClip;
        }
        // camera view while in boat
        if (inBoat)
        {
            transform.position = new Vector3(tpc.boat.transform.position.x + boatPos.x,
            tpc.boat.transform.position.y + boatPos.y, tpc.boat.transform.position.z + boatPos.z);
            transform.localEulerAngles = boatRot;
            mainCam.nearClipPlane = boatClip;
            mainCam.fieldOfView = boatFOV;
        }
           

    }


}
