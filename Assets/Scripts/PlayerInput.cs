using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static CameraSelector;

public class PlayerInput : MonoBehaviour {

    public bool debug;
    public float camSwitchThreshold;
    public CamState lastCameraStateInput;

    public delegate void JumpDelegate();
    public static event JumpDelegate OnJump;

    public delegate void BroomDelegate();
    public static event BroomDelegate OnBroom;

    public delegate void JumpReleaseDelegate();
    public static event JumpReleaseDelegate OnJumpRelease;

    public delegate void MoveDelegate(Vector2 movement);
    public static event MoveDelegate OnMove;

    public delegate void GrabDelegate();
    public static event GrabDelegate OnGrab;

    public delegate void AttackDelegate();
    public static event AttackDelegate OnAttack;

    public delegate void AttackReleaseDelegate();
    public static event AttackReleaseDelegate OnAttackRelease;

    public delegate void MagicDelegate();
    public static event MagicDelegate OnMagic;

    public delegate void RiseDelegate();
    public static event RiseDelegate OnRise;
    public delegate void LowerDelegate();
    public static event LowerDelegate OnLower;

    public delegate void CameraFollowDelegate();
    public static event CameraFollowDelegate OnCameraFollow;
    public delegate void CameraCenterDelegate();
    public static event CameraCenterDelegate OnCameraCenter;
    public delegate void CameraFreeDelegate();
    public static event CameraFreeDelegate OnCameraFree;
    public delegate void CameraNodesDelegate();
    public static event CameraNodesDelegate OnCameraNodes;
    public delegate void CameraFirstPersonDelegate();
    public static event CameraFirstPersonDelegate OnCameraFirstPerson;

    [SerializeField] const float MovementDeadZone = .1f;

    void Update() => GetPlayerInput();

    void GetPlayerInput() {
        if (Input.GetButtonDown("Jump")) OnJump?.Invoke();
        if (Input.GetButtonDown("Broom")) OnBroom?.Invoke();
        if (Input.GetButtonUp("Jump")) OnJumpRelease?.Invoke();
        if (Input.GetButtonDown("Interact")) OnGrab?.Invoke();
        if (Input.GetButtonDown("Attack")) OnAttack?.Invoke();
        if (Input.GetButtonUp("Attack")) OnAttackRelease?.Invoke();
        if (Input.GetButtonDown("Magic")) OnMagic?.Invoke();

        if (Input.GetButton("Rise")) OnRise?.Invoke();
        if (Input.GetButton("Lower")) OnLower?.Invoke();

        if (Input.GetAxisRaw("CameraChangeVertical") > camSwitchThreshold) InvokeFollow(CamState.Follow);
        if (Input.GetAxisRaw("CameraCenter") > camSwitchThreshold)InvokeFollow(CamState.Center);
        if (Input.GetAxisRaw("CameraChangeHorizontal") < -camSwitchThreshold) InvokeFollow(CamState.Free);
        if (Input.GetAxisRaw("CameraChangeHorizontal") > camSwitchThreshold) InvokeFollow(CamState.Nodes);
        if (Input.GetAxisRaw("CameraChangeVertical") < -camSwitchThreshold) InvokeFollow(CamState.FirstPerson);

        print(Input.GetAxis("CameraChangeHorizontal"));

        var movementValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        OnMove?.Invoke(movementValue.magnitude > MovementDeadZone ? movementValue : Vector2.zero);
    }

    void InvokeFollow(CamState inputState) {
        if (lastCameraStateInput != inputState)
            switch (inputState) {
                case CamState.Follow: OnCameraFollow?.Invoke(); break;
                case CamState.Center: OnCameraCenter?.Invoke(); break;
                case CamState.Free: OnCameraFree?.Invoke(); break;
                case CamState.Nodes: OnCameraNodes?.Invoke(); break;
                case CamState.FirstPerson: OnCameraFirstPerson?.Invoke(); break;
            }
        lastCameraStateInput = inputState;
    }

    void OnEnable() {
        OnJump += JumpDebug;
        OnBroom += BroomDebug;
        OnJumpRelease += JumpReleaseDebug;
        OnGrab += GrabDebug;
        OnAttack += AttackDebug;
        OnMagic += MagicDebug;

        OnRise += RiseDebug;
        OnLower += LowerDebug;

        OnCameraFollow += FollowDebug;
        OnCameraCenter += CenterDebug;
        OnCameraFree += FreeDebug;
        OnCameraNodes += NodesDebug;
        OnCameraFirstPerson += FirstPersonDebug;
    }

    void JumpDebug() {if (debug) { print("jump pressed");}}
    void BroomDebug() {if (debug) {  print("broom pressed");}}
    void JumpReleaseDebug() {if (debug) {        print("jump released") ;}}
    void GrabDebug() {if (debug) {     print("grab pressed") ;}}
    void AttackDebug() {if (debug) {   print("attack pressed") ;}}
    void MagicDebug() {if (debug) {  print("magic pressed") ;}}
    void RiseDebug() {if (debug) { print("rise pressed") ;}}
    void LowerDebug() {if (debug) {  print("lower pressed") ;}}
    void FollowDebug() {if (debug) {   print("follow pressed") ;}}
    void CenterDebug() {if (debug) {   print("center pressed") ;}}
    void FreeDebug() {if (debug) {     print("free pressed") ;}}
    void NodesDebug() {if (debug) {  print("nodes pressed");}}
    void FirstPersonDebug() {if (debug) {    print("first person pressed");}}
}
