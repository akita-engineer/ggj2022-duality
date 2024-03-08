using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// TODO
// Should consider using a custom projection matrix the entire time
// Due to above, there appears to be a little jolt in transitions. the math for the custom matrix should be good now though.
// Height lerping might have some issues that need resolving.

/// <summary>
/// Allows splitting and rejoining of perspectives with the new input system. Completely independent from the existing CameraSplit.
/// </summary>
public class SplitablePerspective : MonoBehaviour
{
    public enum SplitState
    {
        Joined,
        Split
    };

    public enum ScreenSide
    {
        Left,
        Right
    };

    public enum TransitionMode
    {
        Animated,
        InstantOneShot,
        Instant
    };

    public enum DebugThing
    {
        None,
        Experimental,
        Reset,
        Scissors,
        MatOverride,
        CustomProj
    };

    public GameObject otherPerspectiveRoot;
    public PlayerConfigScriptableObject playerConfig;
    public ScreenSide screenSide = ScreenSide.Left;
    public SplitState splitState = SplitState.Split;
    public TransitionMode transitionMode = TransitionMode.Animated;

    // for other animations like UI... 
    [Tooltip("Only invoked once, for the player sending the action. Invoked when perspective split starts specifically. (spaghetti for dinner?)")]
    public UnityEvent<float> onSplitStart;
    [Tooltip("Only invoked once, for the player sending the action. (spaghetti for dinner?)")]
    public UnityEvent<float> onRejoinStart;

    private static readonly Rect leftViewPort = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
    private static readonly Rect rightViewPort = new Rect(0.5f, 0.0f, 1.0f, 1.0f);
    private static readonly Rect leftRenderCut = new Rect(-1.0f, -1.0f, 1.0f, 1.0f);
    private static readonly Rect rightRenderCut = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
    private static readonly Rect fullRenderCut = new Rect(-1.0f, -1.0f, 2.0f, 2.0f);

    // returner 
    //public float returningDuration = 1.0f; // (see playerConfig instead)
    //private Quaternion returningStart;

    // splitter
    //public float splitDuration = 1.0f; // (see playerConfig instead)
    private float splitCounter = -1.0f;
    private Quaternion splitTarget;

    private Camera perspectiveCamera;
    private Camera otherPerspectiveCamera;
    private SplitablePerspective otherSplitablePerspective;

    private Rect splitViewPort;
    private Rect splitRenderCut;
    private float splitRotationDirection = 0.0f;

    private bool isTransitioning = false;
    private FirstPersonController fps;

    [Header("DEBUG NOT USED IN RUNTIME")]
    public RawImage rawImg;
    private RenderTexture rt;

    public Matrix4x4 mat;
    public float asp;

    private void InitializeScreenValues()
    {
        if (screenSide == ScreenSide.Left)
        {
            splitViewPort = leftViewPort;
            splitRenderCut = leftRenderCut;
            splitRotationDirection = -1.0f;
        }
        else if (screenSide == ScreenSide.Right)
        {
            splitViewPort = rightViewPort;
            splitRenderCut = rightRenderCut;
            splitRotationDirection = 1.0f;
        }
        else
        {
            Debug.LogError("Cannot initialize screen values. Unknown screen side.");
        }
    }

    private void Awake()
    {
        // Time.timeScale = 0.25f;
        perspectiveCamera = GetComponentInChildren<Camera>();
        Debug.Assert(perspectiveCamera != null, "Camera on other object was not found.");

        otherPerspectiveCamera = otherPerspectiveRoot.GetComponentInChildren<Camera>();
        Debug.Assert(otherPerspectiveCamera != null, "Camera on other object was not found.");

        otherSplitablePerspective = otherPerspectiveRoot.GetComponentInChildren<SplitablePerspective>();

        fps = GetComponent<FirstPersonController>();

        // rt = new RenderTexture(perspectiveCamera.pixelWidth, perspectiveCamera.pixelHeight, 16);
        // perspectiveCamera.targetTexture = rt;
        // rawImg.texture = rt;
        
        InitializeScreenValues();
    }

    /// <summary>
    /// Is this perspective hidden?
    /// eg. the inactive perspective when rejoined.
    /// </summary>
    public bool IsHidden()
    {
        return perspectiveCamera.enabled == false;
    }

    /// <summary>
    /// Splits or rejoins the perspectives, depending on the current state.
    /// </summary>
    public void Activate()
    {
        if (splitState == SplitState.Joined)
        {
            StartCoroutine(SplitFlow());
        }
        else
        {
            StartCoroutine(RejoinFlow());
        }

        // cleanup
        if (transitionMode == TransitionMode.InstantOneShot)
        {
            transitionMode = TransitionMode.Animated;
        }

        //return;

        //returningStart = perspectiveCamera.transform.rotation;
        //var fps = GetComponent<FirstPersonController>();
        //fps.enabled = false;

        //return;

        //if (splitState == SplitState.Joined)
        //{
        //    Vector3 forward = new Vector3(perspectiveCamera.transform.forward.x, 0, perspectiveCamera.transform.forward.z).normalized;
        //    Vector3 leftForward = Quaternion.Euler(0, -playerConfig.fov / 4, 0) * forward;
        //    Vector3 rightForward = Quaternion.Euler(0, playerConfig.fov / 4, 0) * forward; // instead of world up we need relative up
        //    Rect leftViewPort = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
        //    Rect rightViewPort = new Rect(0.5f, 0.0f, 1.0f, 1.0f);

        //    Vector3 thisForward, otherForward;
        //    Rect thisViewPort, otherViewPort;
        //    if (screenSide == ScreenSide.Left)
        //    {
        //        thisForward = leftForward;
        //        otherForward = rightForward;
        //        thisViewPort = leftViewPort;
        //        otherViewPort = rightViewPort;
        //    } 
        //    else
        //    {
        //        thisForward = rightForward;
        //        otherForward = leftForward;
        //        thisViewPort = rightViewPort;
        //        otherViewPort = leftViewPort;
        //    }

        //    CharacterController thisCC = GetComponent<CharacterController>();
        //    thisCC.enabled = false;
        //    Debug.Log(playerConfig.fov);
        //    perspectiveCamera.rect = thisViewPort;
        //    perspectiveCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(playerConfig.fov / 2, perspectiveCamera.aspect);
        //    Debug.Log(perspectiveCamera.fieldOfView);
        //    transform.rotation = Quaternion.LookRotation(thisForward, Vector3.up);
        //    splitState = SplitState.Split;
        //    thisCC.enabled = true;


        //    otherPerspectiveCamera.rect = otherViewPort;
        //    otherPerspectiveCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(playerConfig.fov / 2, otherPerspectiveCamera.aspect);
        //    otherPerspectiveCamera.enabled = true;
        //    // unity is fucking trolling
        //    CharacterController otherCC = otherPerspectiveRoot.GetComponent<CharacterController>();
        //    otherCC.enabled = false;
        //    otherPerspectiveRoot.transform.rotation = Quaternion.LookRotation(otherForward, Vector3.up);
        //    otherPerspectiveRoot.transform.position = transform.position;
        //    otherPerspectiveCamera.transform.localRotation = perspectiveCamera.transform.localRotation;
        //    //Quaternion otherOrientation = Quaternion.LookRotation(otherForward, Vector3.up);
        //    //Vector3 otherOrientationEuler = otherOrientation.eulerAngles;
        //    //otherPerspectiveRoot.transform.rotation = Quaternion.Euler(new Vector3(0, otherOrientationEuler.y, otherOrientationEuler.z));
        //    //otherPerspectiveCamera.transform.rotation = Quaternion.Euler(new Vector3(otherOrientationEuler.x, 0, 0));

        //    otherCC.enabled = true;
        //    if (otherSplitablePerspective != null) splitState = SplitState.Split;
        //}
        //else
        //{
        //    perspectiveCamera.fieldOfView = playerConfig.fov;
        //    perspectiveCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        //    splitState = SplitState.Joined;

        //    otherPerspectiveCamera.enabled = false;
        //}
    }

    // Received via messages
    public void OnSplitJoin(InputValue value)
    {
        Debug.Log("Split join");
        if (enabled && !isTransitioning)
        {
            Activate();
        }
        // otherSplitablePerspective.Activate();
    }

    // Flow Controllers
    // There is always one host for each transition (split/rejoin) which coodrinates the flow as it knows the position and forward direction

    private IEnumerator SplitFlow()
    {
        // step 0. update tracking information
        isTransitioning = true;
        otherSplitablePerspective.isTransitioning = true;

        // TODO
        fps.enabled = false;
        otherSplitablePerspective.fps.enabled = false;

        // step 1. return to horizon
        StartCoroutine(otherSplitablePerspective.ReturnToHorizonInstantTransition(transform.position, transform.rotation));
        yield return StartCoroutine(ReturnToHorizonTransition());

        // step 2. split both perspectives
        onSplitStart.Invoke(transitionMode == TransitionMode.Animated ? playerConfig.splitDuration : 0.0f);
        Vector3 forward = new Vector3(perspectiveCamera.transform.forward.x, 0, perspectiveCamera.transform.forward.z).normalized;
        StartCoroutine(otherSplitablePerspective.MoveToSplitTransition(forward));
        yield return StartCoroutine(MoveToSplitTransition(forward));

        // TODO enable cameras
        perspectiveCamera.enabled = true;
        otherPerspectiveCamera.enabled = true;
        fps.enabled = true;
        otherSplitablePerspective.fps.enabled = true;

        // step 3. update tracking information
        splitState = SplitState.Split;
        otherSplitablePerspective.splitState = SplitState.Split;
        isTransitioning = false;
        otherSplitablePerspective.isTransitioning = false;
    }

    private IEnumerator RejoinFlow()
    {
        isTransitioning = true;
        otherSplitablePerspective.isTransitioning = true;

        Vector3 forward = new Vector3(perspectiveCamera.transform.forward.x, 0, perspectiveCamera.transform.forward.z).normalized;
        Vector3 targetForward = Quaternion.Euler(0, -splitRotationDirection * playerConfig.splitFov / 2.0f, 0) * forward; // TODO: might want to change 2.0 back to 4.0. not 100% sure what's up here. 

        onRejoinStart.Invoke(transitionMode == TransitionMode.Animated ? playerConfig.rejoinDuration : 0.0f);
        StartCoroutine(otherSplitablePerspective.RejoinTransition(targetForward, transform.position, false));
        yield return StartCoroutine(RejoinTransition(targetForward, transform.position, true));

        splitState = SplitState.Joined;
        otherSplitablePerspective.splitState = SplitState.Joined;
        isTransitioning = false;
        otherSplitablePerspective.isTransitioning = false;

        yield return null;
    }

    private IEnumerator ReturnToHorizonTransition()
    {
        var pitch = fps.GetCameraPitch();
        // just some shifting to ensure this value can go negative (which the controller expects)
        if (pitch > 180.0f) pitch -= 360.0f;

        var returningCounter = (transitionMode == TransitionMode.Animated) ? 0.0f : playerConfig.returnToHorizonDuration;
        do
        {
            returningCounter = Mathf.Min(returningCounter + Time.deltaTime, playerConfig.returnToHorizonDuration);
            float progress = playerConfig.returnToHorizonCurve.Evaluate(returningCounter / playerConfig.returnToHorizonDuration);
            fps.SetCameraPitch(Mathf.Lerp(pitch, 0, progress));
            yield return null;
        } while (returningCounter < playerConfig.returnToHorizonDuration);
    }

    private IEnumerator ReturnToHorizonInstantTransition(Vector3 position, Quaternion rootRotation)
    {
        transform.position = position;
        transform.rotation = rootRotation;
        fps.SetCameraPitch(0);

        yield return null;
    }

    private static Rect BoundRect(Rect r)
    {
        if (r.x < 0)
        {
            r.width += r.x;
            r.x = 0;
        }

        if (r.y < 0)
        {
            r.height += r.y;
            r.y = 0;
        }

        r.width = Mathf.Min(1 - r.x, r.width);
        r.height = Mathf.Min(1 - r.y, r.height);

        return r;
    }

    /// <summary>
    /// Extension of https://answers.unity.com/questions/1709397/urp-cameras-create-a-custom-projection-matrix-give.html
    /// Imagine rendering normally, but only rendering a cut of it which is then stretch/squished to another area.
    /// 
    /// JK THIS DOESN"T WORK. do it fucking properly here https://gamedev.stackexchange.com/questions/83191/offset-a-camera-render-without-changing-perspective
    /// </summary>
    /// <param name="cam">Camera with the desired projection to update.</param>
    /// <param name="rectRender">The "cut" of the screen to take. Ranges from -1 to 1.</param>
    /// <param name="rectViewPort">Where to place the taken "cut" on the screen. Ranges from 0 to 1</param>
    public void RemapCameraContents(Camera cam, Rect rectRender, Rect rectViewPort)
    {
        cam.rect = new Rect(0, 0, 1, 1); // required
        cam.ResetProjectionMatrix();
        Matrix4x4 matOriginalProjection = cam.projectionMatrix;
        Matrix4x4 matTranslateToCenterOfCut = Matrix4x4.Translate(new Vector3(-rectRender.center.x, -rectRender.center.y, 0));
        Matrix4x4 matScaleToFill = Matrix4x4.Scale(new Vector3(2.0f / rectRender.width, 2.0f / rectRender.height, 1.0f));
        cam.projectionMatrix = matOriginalProjection * matTranslateToCenterOfCut * matScaleToFill;
        // cam.rect = rectViewPort;
    }

    public float fieldOfView = 60f;
    public float aspect = 1920.0f / 1080.0f;

    /// <summary>
    /// Extension of https://answers.unity.com/questions/1709397/urp-cameras-create-a-custom-projection-matrix-give.html
    /// Imagine rendering normally, but only rendering a cut of it which is then stretch/squished to another area.
    /// 
    /// This is general perspective frustrum equation http://www.songho.ca/opengl/gl_projectionmatrix.html 
    /// with a bit of fov and aspect addons for convenience. (http://www.songho.ca/opengl/gl_transform.html#example2)
    /// </summary>
    /// <param name="leftPercent">The left edge of the frustrum, as a percent of the unmodified frustrum size.</param>
    /// <param name="rightPercent">The right edge of the frustrum, as a percent of the unmodified frustrum size.</param>
    /// <param name="bottomPercent">The bottom edge of the frustrum, as a percent of the unmodified frustrum size.</param>
    /// <param name="topPercent">The top edge of the frustrum, as a percent of the unmodified frustrum size.</param>
    /// <param name="near">The near plane distance.</param>
    /// <param name="far">The far plane distance.</param>
    /// <param name="fovH">The horizontal field of view.</param>
    /// <param name="aspect">The aspect ratio (of the viewport).</param>
    /// <returns>Projection matrix for a camera.</returns>
    public static Matrix4x4 OffsetProjectionMatrix(float leftPercent, float rightPercent, float bottomPercent, float topPercent, float near, float far, float fovH, float aspect)
    {
        float tangent = Mathf.Tan(fovH / 2.0f * Mathf.Deg2Rad);
        // horizontal fov
        float halfWidth = near * tangent;
        float halfHeight = halfWidth / aspect;

        // vertical fov (standard)
        //float halfHeight = near * tangent;
        //float halfWidth = halfHeight * aspect;

        float left = Mathf.Lerp(-halfWidth, halfWidth, leftPercent);
        float right = Mathf.Lerp(-halfWidth, halfWidth, rightPercent);
        float bottom = Mathf.Lerp(-halfHeight, halfHeight, bottomPercent);
        float top = Mathf.Lerp(-halfHeight, halfHeight, topPercent);

        return new Matrix4x4(
            new Vector4(2.0f * near / (right - left), 0, 0, 0),
            new Vector4(0, 2.0f * near / (top - bottom), 0, 0),
            new Vector4((right + left) / (right - left), (top + bottom) / (top - bottom), -(far + near) / (far - near), -1),
            new Vector4(0, 0, (-2.0f * far * near) / (far - near), 0)
        );
    }

    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }

    public AnimationCurve splitCurve;

    private IEnumerator MoveToSplitTransition(Vector3 originalForward)
    {
        // Assumes pitch is 0 

        Vector3 thisForward = Quaternion.Euler(0, splitRotationDirection * playerConfig.splitFov / 2, 0) * originalForward; // TODO /4 

        Quaternion targetRotation = Quaternion.LookRotation(thisForward, Vector3.up);
        Quaternion currentRotataion = transform.rotation;

        float targetFov = playerConfig.splitFov;
        float currentFov = playerConfig.fov;
        float aspect = perspectiveCamera.aspect;

        perspectiveCamera.enabled = true;
        //perspectiveCamera.ResetProjectionMatrix();
        perspectiveCamera.rect = splitViewPort;

        CharacterController thisCC = GetComponent<CharacterController>();
        thisCC.enabled = false;

        float left, leftEnd, right, rightEnd;
        if (screenSide == ScreenSide.Left)
        {
            left = 0.0f;
            right = 0.5f;
        } 
        else
        {
            left = 0.5f;
            right = 1.0f;
        }

        var counter = (transitionMode == TransitionMode.Animated) ? 0.0f : playerConfig.splitDuration;
        do
        {
            counter = Mathf.Min(counter + Time.deltaTime, playerConfig.splitDuration);
            float progress = playerConfig.splitCurve.Evaluate(counter / playerConfig.splitDuration);

            // Note: There's some math here to do this properly (given a start and end FOV) but I'm too lazy to figure it out
            var progress2 = playerConfig.splitCurve2.Evaluate(progress);
            //var renderCut = new Rect(
            //    Mathf.Lerp(splitRenderCut.x, fullRenderCut.x, progress2),
            //    Mathf.Lerp(splitRenderCut.y, fullRenderCut.y, progress2),
            //    Mathf.Lerp(splitRenderCut.width, fullRenderCut.width, progress2),
            //    Mathf.Lerp(splitRenderCut.height, fullRenderCut.height, progress2)
            //);
            //RemapCameraContents(perspectiveCamera, renderCut, splitViewPort);
            var projectionMat = OffsetProjectionMatrix(
                Mathf.Lerp(left, 0.0f, progress2), // this might look counter intuitive but we start with rendering half of the full fov (and then all of the half fov)
                Mathf.Lerp(right, 1.0f, progress2),
                0,
                1,
                perspectiveCamera.nearClipPlane,
                perspectiveCamera.farClipPlane,
                Mathf.Lerp(currentFov, targetFov, progress),
                Mathf.Lerp(aspect, aspect / 2.0f, progress));
            // Matrix4x4.identity; 
            //CustomProjectionMatrix(Mathf.Lerp(left, 0.0f, progress2), Mathf.Lerp(right, 1.0f, progress2), 0, 1, 0.3f, 100.0f);
            perspectiveCamera.projectionMatrix = projectionMat;
            //perspectiveCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(Mathf.Lerp(currentFov, targetFov, progress), perspectiveCamera.aspect);
            transform.rotation = Quaternion.Lerp(currentRotataion, targetRotation, progress);
            yield return null;
        } while (counter < playerConfig.splitDuration);

        //perspectiveCamera.aspect = aspect / 2.0f;
        Debug.Log(playerConfig.fov);
        Debug.Log(perspectiveCamera.fieldOfView);

        thisCC.enabled = true;
    }

    // unlike the split transition, the other half could be located far away so we don't really need to ensure they start at horizon,
    // but it does need to end on the horizon for the same issues as splitting.
    private IEnumerator RejoinTransition(Vector3 targetForward, Vector3 targetPosition, bool isHost)
    {
        // pitch may not be 0, but assume targetForward will have 0 pitch
        Quaternion targetRotation = Quaternion.LookRotation(targetForward, Vector3.up);
        Quaternion currentRotataion = transform.rotation;

        Vector3 currentPosition = transform.position;

        float currentFov = playerConfig.splitFov;
        float targetFov = playerConfig.fov;
        float aspect = perspectiveCamera.aspect;
        float targetAspect = aspect * 2.0f;
        var pitch = fps.GetCameraPitch();
        // just some shifting to ensure this value can go negative (which the controller expects)
        if (pitch > 180.0f) pitch -= 360.0f;

        perspectiveCamera.enabled = true;
        //perspectiveCamera.ResetProjectionMatrix();
        perspectiveCamera.rect = splitViewPort;

        CharacterController thisCC = GetComponent<CharacterController>();
        thisCC.enabled = false;

        float left, right;
        if (screenSide == ScreenSide.Left)
        {
            left = 0.0f;
            right = 0.5f;
        }
        else
        {
            left = 0.5f;
            right = 1.0f;
        }

        var counter = (transitionMode == TransitionMode.Animated) ? 0.0f : playerConfig.rejoinDuration;
        do
        {
            counter = Mathf.Min(counter + Time.deltaTime, playerConfig.rejoinDuration);
            float progress = playerConfig.splitCurve.Evaluate(counter / playerConfig.rejoinDuration);

            // Note: There's some math here to do this properly (given a start and end FOV) but I'm too lazy to figure it out
            var progress2 = playerConfig.splitCurve2.Evaluate(progress);
            var projectionMat = OffsetProjectionMatrix(
                Mathf.Lerp(0.0f, left, progress2),
                Mathf.Lerp(1.0f, right, progress2),
                0,
                1,
                perspectiveCamera.nearClipPlane,
                perspectiveCamera.farClipPlane,
                Mathf.Lerp(currentFov, targetFov, progress),
                Mathf.Lerp(aspect, targetAspect, progress));
            perspectiveCamera.projectionMatrix = projectionMat;

            transform.rotation = Quaternion.Lerp(currentRotataion, targetRotation, progress);
            transform.position = Vector3.Lerp(currentPosition, targetPosition, progress);
            fps.SetCameraPitch(Mathf.Lerp(pitch, 0, progress));
            yield return null;
        } while (counter < playerConfig.rejoinDuration);

        // both perspectives now have the exact same view, but are only rendering one half of it.
        // now we reset the projection so both have the same view and render the same portion. (and below we disable the non-host side)
        // TODO: we should seriously consider just using a custom projection matrix the entire time.

        //perspectiveCamera.aspect = targetAspect;
        perspectiveCamera.rect = new Rect(0, 0, 1, 1);
        //perspectiveCamera.ResetProjectionMatrix();

        perspectiveCamera.projectionMatrix = OffsetProjectionMatrix(
            0,
            1,
            0,
            1,
            perspectiveCamera.nearClipPlane,
            perspectiveCamera.farClipPlane,
            targetFov,
            targetAspect);

        thisCC.enabled = true;
        splitState = SplitState.Joined;

        // TODO: Move this out?
        if (!isHost)
        {
            this.perspectiveCamera.enabled = false;
        }

        yield return null;
    }

    public Rect rect1, rect2;

    public void SetScissorRectTest(Camera cam)
    {
        Rect r = rect1;
        Rect r2 = rect2;

        if (r.x < 0)
        {
            r.width += r.x;
            r.x = 0;
        }

        if (r.y < 0)
        {
            r.height += r.y;
            r.y = 0;
        }

        r.width = Mathf.Min(1 - r.x, r.width);
        r.height = Mathf.Min(1 - r.y, r.height);

        cam.rect = new Rect(0, 0, 1, 1); // (?) this line is required even though we overwrite it shortly below
        cam.ResetProjectionMatrix();
        Matrix4x4 mP = cam.projectionMatrix;
        cam.rect = r2;

        bool renderWithOffset = true;

        if (renderWithOffset)
        {
            Matrix4x4 m2 = Matrix4x4.TRS(new Vector3((1 / r.width - 1), (1 / r.height - 1), 0), Quaternion.identity, new Vector3(1 / r.width, 1 / r.height, 1));
            Matrix4x4 m3 = Matrix4x4.TRS(new Vector3(-r2.x * 2 / r2.width, -r2.y * 2 / r2.height, 0), Quaternion.identity, Vector3.one);
            cam.projectionMatrix = m3 * m2 * mP;
        }
        else
        {
            Matrix4x4 m2 = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1 / r.width, 1 / r.height, 1));
            Matrix4x4 m3 = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
            cam.projectionMatrix = m3 * m2 * mP;
        }
    }

    public DebugThing doThing = DebugThing.None;
    public Rect rect3;

    public float left, right, bottom, top, near, far;

    private void LateUpdate()
    {
        if (doThing == DebugThing.CustomProj)
        {
            var mat = OffsetProjectionMatrix(left, right, bottom, top, perspectiveCamera.nearClipPlane, perspectiveCamera.farClipPlane, fieldOfView, aspect);
            // CustomProjectionMatrix(left, right, bottom, top, near, far);
            perspectiveCamera.projectionMatrix = mat;
        }

        mat = perspectiveCamera.projectionMatrix;
        asp = perspectiveCamera.aspect;
    }

    private void Update()
    {
        if (doThing == DebugThing.None)
        {
            mat = perspectiveCamera.projectionMatrix;
        }
        else if (doThing == DebugThing.Experimental)
        {
            //SetScissorRectTest(perspectiveCamera);
            RemapCameraContents(perspectiveCamera, rect1, rect2);
        }
        else if (doThing == DebugThing.Reset)
        {
            perspectiveCamera.ResetProjectionMatrix();
            perspectiveCamera.rect = rect3;
        }
        else if (doThing == DebugThing.Scissors)
        {
            SetScissorRectTest(perspectiveCamera);
        }
        else if (doThing == DebugThing.MatOverride)
        {
            perspectiveCamera.projectionMatrix = mat;
        }
        else if (doThing == DebugThing.CustomProj)
        {
        }

        //if (returningCounter >= 0.0f)
        //{
        //    // phase 1 - return to horizon
        //    returningCounter = Mathf.Min(returningCounter + Time.deltaTime, returningDuration);

        //    // var forward = perspectiveCamera.transform.forward;
        //    //forward.y = 0;
        //    //var flatForward = forward.normalized;

        //    StarterAssets.FirstPersonController cc = GetComponent<StarterAssets.FirstPersonController>();
        //    var pitch = perspectiveCamera.transform.localRotation.eulerAngles.x;
        //    // just some shifting to ensure this value can go negative (which the controller expects)
        //    if (pitch > 180.0f) pitch -= 360.0f;
        //    Debug.Log(pitch);
        //    cc.SetCameraPitch(Mathf.Lerp(pitch, 0, returningCounter / returningDuration));

        //    //cc.enabled = false;
        //    //perspectiveCamera.transform.rotation = Quaternion.Lerp(perspectiveCamera.transform.rotation, Quaternion.LookRotation(flatForward, Vector3.up), returningCounter / returningDuration);
        //    //cc.enabled = true;

        //    // going to need to troll, see above Activate()
        //    if (returningCounter >= returningDuration)
        //    {
        //        returningCounter = -1.0f;
        //        var fps = GetComponent<FirstPersonController>();
        //        fps.enabled = true;
        //    }
        //} 

        //if (splitCounter >= 0.0f)
        //{
        //    splitCounter += Mathf.Min(splitCounter + Time.deltaTime, splitDuration);


        //}
    }

    private void OnDrawGizmos()
    {
        if (perspectiveCamera == null) return;
        Vector3 forward = new Vector3(perspectiveCamera.transform.forward.x, 0, perspectiveCamera.transform.forward.z).normalized;
        Vector3 targetForward = Quaternion.Euler(0, -splitRotationDirection * playerConfig.splitFov / 2.0f, 0) * forward;
        Debug.DrawLine(transform.position, transform.position + targetForward * 10.0f, Color.magenta);
    }
}
