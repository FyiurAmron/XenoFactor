namespace UnityTemplateProjects {

using UnityEngine;
using UnityEngine.InputSystem;
using Util;

public class SimpleCameraController : MonoBehaviour {

    private const float k_MouseSensitivityMultiplier = 0.01f;

    [Header( "Movement Settings" )]
    [Tooltip( "Exponential boost factor on translation, controllable by mouse wheel." )]
    public float boost = 3.5f;

    [Tooltip( "Time it takes to interpolate camera position 99% of the way to the target." )]
    [Range( 0.001f, 1f )]
    public float positionLerpTime = 0.2f;

    [Header( "Rotation Settings" )]
    [Tooltip( "Multiplier for the sensitivity of the rotation." )]
    public float mouseSensitivity = 60.0f;

    [Tooltip( "X = Change in mouse position.\nY = Multiplicative factor for camera rotation." )]
    public AnimationCurve mouseSensitivityCurve =
        new( new Keyframe( 0f, 0.5f, 0f, 5f ), new Keyframe( 1f, 2.5f, 0f, 0f ) );

    [Tooltip( "Time it takes to interpolate camera rotation 99% of the way to the target." )]
    [Range( 0.001f, 1f )]
    public float rotationLerpTime = 0.01f;

    public bool invertX = false;
    public bool invertY = false;

    public Vector3Range allowedPositionRange;

    public float tiltDampeningFactor = 0.99f;

    private CameraState interpolatingCameraState;

    private InputAction lookAction;
    private bool mouseRightButtonPressed;

    private InputAction movementAction;
    private CameraState targetCameraState;
    private InputAction verticalMovementAction;
    private InputAction zoomAction;

    private bool RightMouseButtonDown
        => Mouse.current?.rightButton.isPressed ?? true;

    private bool EscapePressed
        => Keyboard.current?.escapeKey.isPressed ?? false;

    private void Start() {
        InputActionMap map = new( "Simple Camera Controller" );

        lookAction = map.AddAction( "look", binding: "<Mouse>/delta" );
        movementAction = map.AddAction( "move", binding: "<Gamepad>/leftStick" );
        verticalMovementAction = map.AddAction( "Vertical Movement" );
        zoomAction = map.AddAction( "Zoom", binding: "<Mouse>/scroll" );

        lookAction.AddBinding( "<Gamepad>/rightStick" ).WithProcessor( "scaleVector2(x=15, y=15)" );
        movementAction.AddCompositeBinding( "Dpad" )
                      .With( "Up", "<Keyboard>/w" )
                      .With( "Up", "<Keyboard>/upArrow" )
                      .With( "Down", "<Keyboard>/s" )
                      .With( "Down", "<Keyboard>/downArrow" )
                      .With( "Left", "<Keyboard>/a" )
                      .With( "Left", "<Keyboard>/leftArrow" )
                      .With( "Right", "<Keyboard>/d" )
                      .With( "Right", "<Keyboard>/rightArrow" );
        verticalMovementAction.AddCompositeBinding( "Dpad" )
                              .With( "Up", "<Keyboard>/e" )
                              .With( "Down", "<Keyboard>/q" )
                              .With( "Up", "<Gamepad>/rightshoulder" )
                              .With( "Down", "<Gamepad>/leftshoulder" );
        zoomAction.AddBinding( "<Gamepad>/Dpad" ).WithProcessor( "scaleVector2(x=1, y=4)" );

        movementAction.Enable();
        lookAction.Enable();
        verticalMovementAction.Enable();
        zoomAction.Enable();
    }

    private void Update() {
        if ( EscapePressed ) {
            Application.Quit();
        }

        if ( RightMouseButtonDown ) {
            Vector2 mouseMovement = k_MouseSensitivityMultiplier * mouseSensitivity * GetInputLookRotation();

            if ( invertX ) {
                mouseMovement.x *= -1;
            }

            if ( invertY ) {
                mouseMovement.y *= -1;
            }

            float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate( mouseMovement.magnitude );

            targetCameraState.eulerAngles.y += mouseMovement.x * mouseSensitivityFactor;
            targetCameraState.eulerAngles.x -= mouseMovement.y * mouseSensitivityFactor;
        }

        targetCameraState.eulerAngles.y *= tiltDampeningFactor;
        targetCameraState.eulerAngles.x *= tiltDampeningFactor;

        targetCameraState.eulerAngles.z -=
            ( IsBoostPressed() ? 5 : 1 ) * verticalMovementAction.ReadValue<Vector2>().y;

        Vector2 moveDelta = movementAction.ReadValue<Vector2>();
        Vector3 direction = new(
            moveDelta.x,
            moveDelta.y,
            0.01f * zoomAction.ReadValue<Vector2>().y
        );

        Vector3 translation = direction * Time.deltaTime;

        if ( IsBoostPressed() ) {
            translation *= 10.0f;
        }

        translation *= Mathf.Pow( 2.0f, boost );

        targetCameraState.Translate( translation );
        targetCameraState.pos = allowedPositionRange.clamp( targetCameraState.pos );
        
        // targetCameraState.UpdateTransform();

        float positionLerpPct = 1f - Mathf.Exp( ( Mathf.Log( 0.01f ) / positionLerpTime ) * Time.deltaTime );
        float rotationLerpPct = 1f - Mathf.Exp( ( Mathf.Log( 0.01f ) / rotationLerpTime ) * Time.deltaTime );
        interpolatingCameraState.LerpTowards( targetCameraState, positionLerpPct, rotationLerpPct );

        interpolatingCameraState.UpdateTransform();
    }

    private void OnEnable() {
        targetCameraState = new( transform );
        interpolatingCameraState = new( transform );
    }

    private Vector2 GetInputLookRotation() {
        // try to compensate the diff between the two input systems by multiplying with empirical values
        Vector2 delta = lookAction.ReadValue<Vector2>();
        delta *= 0.5f; // Account for scaling applied directly in Windows code by old input system.
        delta *= 0.1f; // Account for sensitivity setting on old Mouse X and Y axes.
        return delta;
    }

    private bool IsBoostPressed() {
        bool boost = Keyboard.current?.leftShiftKey.isPressed ?? false;
        boost |= Gamepad.current?.xButton.isPressed ?? false;
        return boost;
    }

    private class CameraState {
        public Vector3 eulerAngles; // yaw, pitch, roll
        public Vector3 pos;

        private readonly Transform transform;

        public CameraState( Transform transform ) {
            this.transform = transform;
            eulerAngles = transform.localEulerAngles;
            pos = transform.localPosition;
        }
        
        public void UpdateTransform() {
            Quaternion q = eulerAngles.asEulerToQuaternion( EulerAxis.X, EulerAxis.Y, EulerAxis.Z );

            transform.localRotation = q;
            transform.localPosition = pos;
        }

        public void Translate( Vector3 translation ) {
            Vector3 rotPos = Quaternion.Euler( 0, 0, eulerAngles.z )
                * new Vector3( translation.x, translation.y, 0 );
            pos += rotPos;
            pos.z += translation.z;
        }

        public void LerpTowards( CameraState target, float positionLerpPct, float rotationLerpPct ) {
            eulerAngles = Vector3.Lerp( eulerAngles, target.eulerAngles, rotationLerpPct );
            pos = Vector3.Lerp( pos, target.pos, positionLerpPct );
        }
    }
}

}
