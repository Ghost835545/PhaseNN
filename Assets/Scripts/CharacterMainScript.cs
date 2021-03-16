using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMain : MonoBehaviour {

    protected TrajectoryPlayerAndAnimation charact;
    protected PFNN_COMPUTE net;
    protected Transform mainCamera;

    [Range(0.0f, 150.0f)]
    public float camerRotSensitiv = 90.0f;

    [Range(0.0f, 50.0f)]
    public float camerZoomSensitiv = 60.0f;

    private float camerDist;
    private const float camerDistMax = -40.0f;
    private const float camerDistMin = -2.0f;

    private float camerAnglX, camerAnglY;
    private const float camerAnglMaxX = 80.0f;
    private const float camerAnglMinX = 5.0f;

    public Vector3 initWPos;

    // Use this for initialization
    void Start() {

        initWPos = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z);

        net = new PFNN_COMPUTE();

        charact = this.GetComponent<TrajectoryPlayerAndAnimation>();    

        mainCamera = gameObject.transform.GetChild(0);
        mainCamera.LookAt(transform);

        camerAnglX = mainCamera.eulerAngles.x;
        camerAnglY = 0.0f;
        camerDist = mainCamera.position.z;
        MoveCamera();

        ResetCharacter();
    }

    // Update is called once per frame
    protected virtual void Update() {

        charact.UpNetInput(ref net.X);
        net.Compute(charact.phase);
        charact.CreateLocalTrans(net.Y);

         
        charact.VisualTraject();
        charact.VisualBodyEl();

        charact.PostVisualCompute(net.Y);
        charact.UpPhase(net.Y);
    }

    protected void MovCharact(float axisX = 0.0f, float axisY = 0.0f, float rightTrigger = 0.0f, float leftTrigger = 0.0f) {
   
        Vector3 newTargetDirection = Vector3.Normalize(
           new Vector3(mainCamera.forward.x, 0.0f, mainCamera.forward.z));

        Debug.DrawRay(mainCamera.transform.position, newTargetDirection, Color.cyan);        

        charact.UpdateStrafe(leftTrigger);
        charact.UpdateGoalDirAndVel(newTargetDirection, axisX, axisY, rightTrigger);
        charact.UpWalk(rightTrigger);
        charact.ComputeFutureTraject();

        charact.Walls();

        charact.UpRot();
        charact.UpHghts();        
    }

    protected void ResetCharacter() {

        net.Reset();
        charact.ResetPosition(initWPos, net.Y);
    }

    public void UpdateCameraDistance(float value) {

        camerDist += value * camerZoomSensitiv * Time.deltaTime;
        camerDist = Mathf.Clamp(camerDist, camerDistMax, camerDistMin);

        MoveCamera();
    }

    protected void MoveCamera(float speedY = 0.0f, float speedX = 0.0f) {

        camerAnglX += speedX * camerRotSensitiv * Time.deltaTime;
        camerAnglY += speedY * camerRotSensitiv * Time.deltaTime;
        camerAnglX = Mathf.Clamp(camerAnglX, camerAnglMinX, camerAnglMaxX);
  

        Vector3 dir = new Vector3(0, 0, camerDist);
        Quaternion rotation = Quaternion.Euler(camerAnglX, camerAnglY, 0.0f);

        mainCamera.position = transform.position + (rotation * dir);
        mainCamera.LookAt(transform);

        FixCameraAngles();
    }

    protected void FixCameraAngles() {

        if (camerAnglY >= 360.0f || camerAnglY <= -360.0f) {
            camerAnglY = mainCamera.eulerAngles.y;
        }
    }

    protected void CharacterCrouch() {

        charact.Crouch();
    }
    protected void CharacterJump()
    {
        charact.Jumps();
    }


}
