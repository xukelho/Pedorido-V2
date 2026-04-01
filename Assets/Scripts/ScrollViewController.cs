using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ScrollViewController : MonoBehaviour
{
    #region Fields
    [Header("References")]
    public GalleryController GalleryController;
    public ScrollRect ScrollRect;
    public RectTransform ContentPanel;
    public HorizontalLayoutGroup HorizontalLayoutGroup;

    [Header("Snap Settings")]
    [Tooltip("Percent from center to snap immediately")]
    [SerializeField] private float snapThresholdPercent = 5f;
    [Tooltip("Percent drag of start-centered image required to go to adjacent image")]
    [SerializeField] private float snapThresholdPercentToNextImage = 33f;
    [Tooltip("Lerp duration when snapping")]
    [SerializeField] private float snapLerpDuration = 0.25f;

    public List<RectTransform> ImagesRectTransformList { get; private set; }

    RectTransform _nearestImageToCenterOfScreen;
    int _nearestIndexOfImageClosestToCenterOfScreen = -1;

    RectTransform _secondNearestImageToCenterOfScreen;
    int _secondNearestIndexOfImageClosestToCenterOfScreen = -1;

    bool _wasUserTouching = false;

    Vector2 _screenCenter;

    Coroutine _lerpCoroutine = null;

    // New fields to support threshold-to-next-image behavior
    private RectTransform _startCenteredImage = null;
    private int _startCenteredIndex = -1;
    private float _lastDragPercentForStartImage = 0f;
    private bool _isLerping = false;

    // Cached canvas / camera / viewport to avoid repeated GetComponent calls
    private Canvas _parentCanvas;
    private Camera _cameraForCanvas;
    private RectTransform _viewportRect;

    // track resolution changes
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    #endregion //Fields

    #region Unity
    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        // initial cache and checks
        CacheCanvasCameraAndViewport();
        UpdateScreenCenter();

        ImagesRectTransformList = new List<RectTransform>();
        if (GalleryController != null && GalleryController.Images != null)
        {
            ImagesRectTransformList.Capacity = GalleryController.Images.Count;
            foreach (var image in GalleryController.Images)
            {
                if (image == null) continue;
                RectTransform childRect = image.GetComponent<RectTransform>();
                if (childRect != null)
                    ImagesRectTransformList.Add(childRect);
            }
        }

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }

    void Update()
    {
        // update caches if something changed (canvas/camera/viewport can be null in editor/runtime transitions)
        if (_parentCanvas == null)
            CacheCanvasCameraAndViewport();

        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
            UpdateScreenCenter();

        var touches = Touch.activeTouches;
        var isUserTouchingTheScreen = touches.Count > 0 || (Mouse.current != null && Mouse.current.leftButton.isPressed);

        if (isUserTouchingTheScreen)
        {
            // Touch start
            if (!_wasUserTouching)
            {
                _wasUserTouching = true;

                // Capture which image was centered when the touch began
                GetClosestImageToCenterOfScreen();
                if (_nearestIndexOfImageClosestToCenterOfScreen >= 0 && _nearestIndexOfImageClosestToCenterOfScreen < ImagesRectTransformList.Count)
                {
                    _startCenteredIndex = _nearestIndexOfImageClosestToCenterOfScreen;
                    _startCenteredImage = ImagesRectTransformList[_startCenteredIndex];
                    _lastDragPercentForStartImage = CalculateSignedPercentForRect(_startCenteredImage);
                }
            }
            else
            {
                // Continuing touch: update nearest + track drag percent for the start image
                GetClosestImageToCenterOfScreen();
                if (_startCenteredImage != null)
                {
                    _lastDragPercentForStartImage = CalculateSignedPercentForRect(_startCenteredImage);
                }

                // If user touches while lerping, stop lerp immediately (feel responsive)
                if (_isLerping && _lerpCoroutine != null)
                {
                    StopLerpCoroutine();
                }
            }
        }
        else
        {
            if (_wasUserTouching)
            {
                _wasUserTouching = false;
                LerpToImage();

                // clear start tracking; LerpToImage may use the tracked values before clearing
                _startCenteredImage = null;
                _startCenteredIndex = -1;
                _lastDragPercentForStartImage = 0f;
            }
        }
    }
    #endregion //Unity

    #region Methods
    private void CacheCanvasCameraAndViewport()
    {
        _parentCanvas = GetComponentInParent<Canvas>();
        if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            _cameraForCanvas = _parentCanvas.worldCamera ?? Camera.main;
        else
            _cameraForCanvas = null;

        if (ContentPanel != null)
            _viewportRect = ContentPanel.parent as RectTransform;
        else
            _viewportRect = null;
    }

    private void UpdateScreenCenter()
    {
        _screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }

    private void GetClosestImageToCenterOfScreen()
    {
        // Find which child has its center closest to the screen center.
        if (ImagesRectTransformList == null || ImagesRectTransformList.Count == 0)
        {
            _nearestImageToCenterOfScreen = null;
            _nearestIndexOfImageClosestToCenterOfScreen = -1;
            _secondNearestImageToCenterOfScreen = null;
            _secondNearestIndexOfImageClosestToCenterOfScreen = -1;
            return;
        }

        Vector2 closestToCenterScreenPosition = Vector2.zero;

        float closestToCenterOfScreenSqrDist = float.MaxValue;
        float secondClosestToCenterOfScreenSqrDist = float.MaxValue;

        int closestToCenterOfScreenIndex = -1;
        int secondClosestToCenterOfScreenBestIndex = -1;

        RectTransform closestToCenterOfScreenRect = null;
        RectTransform secondClosestToCenterOfScreenBestRect = null;

        for (int i = 0; i < ImagesRectTransformList.Count; i++)
        {
            var rect = ImagesRectTransformList[i];
            if (rect == null) continue;

            Vector3 worldCenter = rect.TransformPoint(rect.rect.center);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_cameraForCanvas, worldCenter);

            float sqrDist = (screenPoint - _screenCenter).sqrMagnitude;

            if (sqrDist < closestToCenterOfScreenSqrDist)
            {
                // Current best becomes second best
                secondClosestToCenterOfScreenSqrDist = closestToCenterOfScreenSqrDist;
                secondClosestToCenterOfScreenBestIndex = closestToCenterOfScreenIndex;
                secondClosestToCenterOfScreenBestRect = closestToCenterOfScreenRect;

                // New best
                closestToCenterOfScreenSqrDist = sqrDist;
                closestToCenterOfScreenIndex = i;
                closestToCenterOfScreenRect = rect;

                closestToCenterScreenPosition = screenPoint;
            }
            else if (sqrDist < secondClosestToCenterOfScreenSqrDist)
            {
                // New second best (but not better than best)
                secondClosestToCenterOfScreenSqrDist = sqrDist;
                secondClosestToCenterOfScreenBestIndex = i;
                secondClosestToCenterOfScreenBestRect = rect;
            }
        }

        _nearestImageToCenterOfScreen = closestToCenterOfScreenRect;
        _nearestIndexOfImageClosestToCenterOfScreen = closestToCenterOfScreenIndex;

        _secondNearestImageToCenterOfScreen = secondClosestToCenterOfScreenBestRect;
        _secondNearestIndexOfImageClosestToCenterOfScreen = secondClosestToCenterOfScreenBestIndex;
    }

    private static float CalculateDistancePercentageToCenterOfScreen(Vector2 closestToCenterScreenPosition, Vector2 screenCenter)
    {
        Vector2 delta = closestToCenterScreenPosition - screenCenter;
        // Provide X percent relative to half-width
        float xPercent = (Mathf.Abs(delta.x) / (Screen.width * 0.5f)) * 100f;
        xPercent = Mathf.Clamp(xPercent, 0f, 100f);

        if (closestToCenterScreenPosition.x < screenCenter.x)
            return 0 - xPercent;

        return xPercent;
    }

    // Helper to compute signed X percent for a given rect (uses cached camera)
    private float CalculateSignedPercentForRect(RectTransform rect)
    {
        if (rect == null)
            return 0f;

        Vector3 worldCenter = rect.TransformPoint(rect.rect.center);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_cameraForCanvas, worldCenter);
        return CalculateDistancePercentageToCenterOfScreen(screenPoint, _screenCenter);
    }

    private void LerpToImage()
    {
        if (_nearestImageToCenterOfScreen == null || ContentPanel == null || ScrollRect == null)
            return;

        // Decide target image: by default nearest, but if the user dragged the start-centered image past the threshold,
        // choose the adjacent image in the drag direction.
        RectTransform targetRect = _nearestImageToCenterOfScreen;
        int targetIndex = _nearestIndexOfImageClosestToCenterOfScreen;

        if (_startCenteredIndex >= 0 && _startCenteredIndex < ImagesRectTransformList.Count)
        {
            if (Mathf.Abs(_lastDragPercentForStartImage) >= snapThresholdPercentToNextImage)
            {
                int direction = (_lastDragPercentForStartImage < 0f) ? 1 : -1;
                int candidateIndex = _startCenteredIndex + direction;
                if (candidateIndex >= 0 && candidateIndex < ImagesRectTransformList.Count)
                {
                    targetIndex = candidateIndex;
                    targetRect = ImagesRectTransformList[targetIndex];
                    _nearestImageToCenterOfScreen = targetRect;
                    _nearestIndexOfImageClosestToCenterOfScreen = targetIndex;
                }
            }
        }

        // Current world center of the target image
        Vector3 worldCenter = targetRect.TransformPoint(targetRect.rect.center);
        Vector2 imageScreenPoint = RectTransformUtility.WorldToScreenPoint(_cameraForCanvas, worldCenter);

        // Determine viewport rect (cached)
        if (_viewportRect == null)
        {
            _viewportRect = ContentPanel.parent as RectTransform;
            if (_viewportRect == null)
                return;
        }

        // Convert both points to the local space of the viewport.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewportRect, imageScreenPoint, _cameraForCanvas, out Vector2 imageLocalInViewport);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewportRect, _screenCenter, _cameraForCanvas, out Vector2 centerLocalInViewport);

        Vector2 deltaLocal = imageLocalInViewport - centerLocalInViewport;

        float targetX = ContentPanel.localPosition.x - deltaLocal.x;

        // Stop any existing lerp coroutine.
        if (_lerpCoroutine != null)
            StopCoroutine(_lerpCoroutine);

        _lerpCoroutine = StartCoroutine(LerpContentToX(targetX, snapLerpDuration));
    }

    private IEnumerator LerpContentToX(float targetX, float duration)
    {
        _isLerping = true;
        Vector3 startPos = ContentPanel.localPosition;
        Vector3 targetPos = new Vector3(targetX, startPos.y, startPos.z);
        float elapsed = 0f;

        if (_viewportRect == null)
            _viewportRect = ContentPanel.parent as RectTransform;

        if (_viewportRect == null)
        {
            ContentPanel.localPosition = targetPos;
            _isLerping = false;
            _lerpCoroutine = null;
            yield break;
        }

        if (duration <= 0f)
        {
            ContentPanel.localPosition = targetPos;
            _isLerping = false;
            _lerpCoroutine = null;
            yield break;
        }

        while (elapsed < duration)
        {
            // If user starts dragging again, stop lerp immediately.
            var touches = Touch.activeTouches;
            var isUserTouchingTheScreen = touches.Count > 0 || (Mouse.current != null && Mouse.current.leftButton.isPressed);
            if (isUserTouchingTheScreen)
            {
                _isLerping = false;
                _lerpCoroutine = null;
                yield break;
            }

            // Recompute image screen position and percent-to-center each frame so we can snap early.
            if (_nearestImageToCenterOfScreen != null)
            {
                Vector3 worldCenter = _nearestImageToCenterOfScreen.TransformPoint(_nearestImageToCenterOfScreen.rect.center);
                Vector2 imageScreenPoint = RectTransformUtility.WorldToScreenPoint(_cameraForCanvas, worldCenter);

                float percentX = Mathf.Abs(CalculateDistancePercentageToCenterOfScreen(imageScreenPoint, _screenCenter));

                // If the image is within snap threshold, compute the up-to-date target and snap.
                if (percentX <= snapThresholdPercent)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewportRect, imageScreenPoint, _cameraForCanvas, out Vector2 imageLocalInViewport);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewportRect, _screenCenter, _cameraForCanvas, out Vector2 centerLocalInViewport);
                    Vector2 deltaLocal = imageLocalInViewport - centerLocalInViewport;
                    float upToDateTargetX = ContentPanel.localPosition.x - deltaLocal.x;

                    if (ScrollRect != null)
                        ScrollRect.velocity = Vector2.zero;

                    ContentPanel.localPosition = new Vector3(upToDateTargetX, ContentPanel.localPosition.y, ContentPanel.localPosition.z);
                    _isLerping = false;
                    _lerpCoroutine = null;
                    yield break;
                }
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            ContentPanel.localPosition = Vector3.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }

        ContentPanel.localPosition = targetPos;
        _isLerping = false;
        _lerpCoroutine = null;
    }

    private void StopLerpCoroutine()
    {
        if (_lerpCoroutine != null)
        {
            StopCoroutine(_lerpCoroutine);
            _lerpCoroutine = null;
        }
        _isLerping = false;
    }

    public void SnapToImageByReference(Image imageRef)
    {
        // Immediate snap (no lerp). Find the image, compute its screen center and adjust ContentPanel so the image aligns with screen center.
        if (imageRef == null || ContentPanel == null || GalleryController == null)
            return;

        // Ensure camera/viewport cached and screen center up-to-date
        if (_parentCanvas == null)
            CacheCanvasCameraAndViewport();

        UpdateScreenCenter();

        var imageFromImagesList = GalleryController.Images.FirstOrDefault(i => i.mainTexture == imageRef.mainTexture);

        // Validate image exists in gallery
        if (GalleryController.Images == null || imageFromImagesList == null)
        //if (GalleryController.Images == null || !GalleryController.Images.Contains(imageRef))
            return;

        //RectTransform imageRect = imageRef.GetComponent<RectTransform>();
        RectTransform imageRect = imageFromImagesList.GetComponent<RectTransform>();
        if (imageRect == null)
            return;

        // Ensure viewport is available
        if (_viewportRect == null)
            _viewportRect = ContentPanel.parent as RectTransform;
        if (_viewportRect == null)
            return;

        // Stop any existing lerp and reset velocity
        if (_lerpCoroutine != null)
        {
            StopCoroutine(_lerpCoroutine);
            _lerpCoroutine = null;
        }
        _isLerping = false;
        if (ScrollRect != null)
            ScrollRect.velocity = Vector2.zero;

        // Compute world center and screen point for the image
        Vector3 worldCenter = imageRect.TransformPoint(imageRect.rect.center);
        Vector2 imageScreenPoint = RectTransformUtility.WorldToScreenPoint(_cameraForCanvas, worldCenter);

        // Convert both points to the local space of the viewport.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewportRect, imageScreenPoint, _cameraForCanvas, out Vector2 imageLocalInViewport);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewportRect, _screenCenter, _cameraForCanvas, out Vector2 centerLocalInViewport);

        Vector2 deltaLocal = imageLocalInViewport - centerLocalInViewport;

        float targetX = ContentPanel.localPosition.x - deltaLocal.x;

        ContentPanel.localPosition = new Vector3(targetX, ContentPanel.localPosition.y, ContentPanel.localPosition.z);

        // Update nearest tracking fields if possible
        if (ImagesRectTransformList != null)
        {
            int foundIndex = ImagesRectTransformList.IndexOf(imageRect);
            if (foundIndex >= 0)
            {
                _nearestImageToCenterOfScreen = imageRect;
                _nearestIndexOfImageClosestToCenterOfScreen = foundIndex;
                // Optionally update secondary nearest by recalculating
                GetClosestImageToCenterOfScreen();
            }
        }
    }

    #endregion //Methods
}
