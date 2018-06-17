using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSelector : MonoBehaviour {

    [SerializeField] Behaviour[] camScripts;

    [SerializeField] CamState currentCamState;
    [SerializeField] Behaviour[] toDisableInFirstPerson;
    public enum CamState { Behind, Target, FirstPerson, Free/*, Nodes*/ }

    public CamState CurrentCamState
    {
        get { return currentCamState; }
        set {
            currentCamState = value;
            for (var i = 0; i <= LengthOfCamEnum(); i++)
                camScripts[i].enabled = i == (int)value ? true : false;
            foreach (var behaviour in toDisableInFirstPerson)
                behaviour.enabled = currentCamState != CamState.FirstPerson;
        }
    }

    private void Update() => GetCamState();

    void GetCamState()
    {
        if (Input.GetButtonDown("CamToggle"))
            CurrentCamState = (int)CurrentCamState < LengthOfCamEnum() ? CurrentCamState + 1 : 0;
    }

    int LengthOfCamEnum() => camScripts.Length - 1;//System.Enum.GetValues(typeof(CamState)).Length;
}
