using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using System;

public class LeapMotionControllerCursorBehaviour : CursorPositioningController
{
    public CursorHandPosition handPosition;

    public LeapServiceProvider leapService;
    public GameObject leapMotionObjects;
    public HandModelManager leapHandModelManager;
    public Transform leapMotionOffset;

    Vector3 lastCursorPosition;

    bool isCalibrating;
    Action finishCalibrationCallback;

    const float offsetPositionCalibrationStep = 0.001f;
    const float offsetAngleCalibrationStep = 0.5f;

    bool isTrackingHand;
    int detectedHandID;

    private void Update()
    {
        Leap.Frame frame = leapService.CurrentFrame;
        isTrackingHand = frame.Hands.Count > 0;

        if (isTrackingHand)
        {
            if (handPosition == CursorHandPosition.HandTop)
            {
                lastCursorPosition = frame.Hands[0].GetIndex().TipPosition.ToVector3();
            }
            else
            {
                lastCursorPosition = frame.Hands[0].PalmPosition.ToVector3();
            }
            detectedHandID = frame.Hands[0].Id;
        }
        else
        {
            detectedHandID = -1;
        }

        if (isCalibrating)
        {
            Vector3 pos = leapMotionOffset.position;
            Vector3 rotation = leapMotionOffset.rotation.eulerAngles;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                FinishCalibration();
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                pos.x -= offsetPositionCalibrationStep;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                pos.x += offsetPositionCalibrationStep;
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                pos.y += offsetPositionCalibrationStep;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                pos.y -= offsetPositionCalibrationStep;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                pos.z += offsetPositionCalibrationStep;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                pos.z -= offsetPositionCalibrationStep;
            }
            else if (Input.GetKey(KeyCode.O))
            {
                leapMotionOffset.rotation = Quaternion.Euler(new Vector3(rotation.x - offsetAngleCalibrationStep, rotation.y, rotation.z));
            }
            else if (Input.GetKey(KeyCode.L))
            {
                leapMotionOffset.rotation = Quaternion.Euler(new Vector3(rotation.x + offsetAngleCalibrationStep, rotation.y, rotation.z));
            }
            leapMotionOffset.position = pos;
        }
    }

    private void OnEnable()
    {
        // Enable LeapMotion related services
        leapService.enabled = true;
        leapMotionObjects.SetActive(true);
    }

    private void OnDisable()
    {
        // Disable LeapMotion related services
        leapService.enabled = false;
        leapMotionObjects.SetActive(false);
    }

    public override string GetDeviceName()
    {
        return "LeapMotionController";
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return lastCursorPosition;
    }

    public override int GetTrackedHandId()
    {
        return detectedHandID;
    }

    public void StartLeapMotionCalibration(Action callback)
    {
        Debug.Log("LeapMotionControllerCursorBehaviour: calibrating...");
        isCalibrating = true;
        finishCalibrationCallback = callback;
        leapHandModelManager.EnableGroup("Rigged Hands");
    }

    void FinishCalibration()
    {
        isCalibrating = false;
        finishCalibrationCallback?.Invoke();
        finishCalibrationCallback = null;
        leapHandModelManager.DisableGroup("Rigged Hands");
        Debug.Log("LeapMotionControllerCursorBehaviour: finished calibration.");
    }
}
