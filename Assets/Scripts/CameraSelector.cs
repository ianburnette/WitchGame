using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSelector : MonoBehaviour {

    [SerializeField] Behaviour[] camScripts;

    [SerializeField] CamState currentCamState;
    [SerializeField] Behaviour[] toDisableInFirstPerson;
    public enum CamState { Follow, Target, FirstPerson, Free, Nodes }

    public CamState CurrentCamState
    {
        get { return currentCamState; }
        set {
            currentCamState = value;
            for (var i = 0; i <= LengthOfCamEnum(); i++)
                camScripts[i].enabled = i == (int)value;
            foreach (var behaviour in toDisableInFirstPerson)
                behaviour.enabled = currentCamState != CamState.FirstPerson;
        }
    }

    void OnEnable() {
        PlayerInput.OnCameraFollow += SetCamFollow;
        PlayerInput.OnCameraTarget += SetCamTarget;
        PlayerInput.OnCameraFree += SetCamFree;
        PlayerInput.OnCameraNodes += SetCamNodes;
        PlayerInput.OnCameraFirstPerson += SetCamFirstPerson;
    }

    void OnDisable() {
        PlayerInput.OnCameraFollow -= SetCamFollow;
        PlayerInput.OnCameraTarget -= SetCamTarget;
        PlayerInput.OnCameraFree -= SetCamFree;
        PlayerInput.OnCameraNodes -= SetCamNodes;
        PlayerInput.OnCameraFirstPerson -= SetCamFirstPerson;
    }

    void SetCamFollow() => CurrentCamState = CamState.Follow;
    void SetCamTarget() => CurrentCamState = CamState.Target;
    void SetCamFree() => CurrentCamState = CamState.Free;
    void SetCamNodes() => CurrentCamState = CamState.Nodes;
    void SetCamFirstPerson() => CurrentCamState = CamState.FirstPerson;

    int LengthOfCamEnum() => camScripts.Length - 1;//System.Enum.GetValues(typeof(CamState)).Length;
}
