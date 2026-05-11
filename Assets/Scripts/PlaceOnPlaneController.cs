using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceOnPlaneController : MonoBehaviour
{
    #region Fields
    [Header("AR managers")]
    [SerializeField] ARPlaneManager ArPlaneManager;
    [SerializeField] ARRaycastManager ArRaycastManager;

    [Header("Placement")]
    [SerializeField] GameObject PrefabToPlace;

    [Header("Preview")]
    [SerializeField] GameObject PreviewPositionValid;
    [SerializeField] GameObject PreviewPositionInvalid;

    static List<ARRaycastHit> _arRaycastHits = new List<ARRaycastHit>();

    public bool ValidPosition = false;

    #endregion //Fields

    #region Unity

    private void Start()
    {
        if (MainUiNavigation.Instance != null && MainUiNavigation.Instance.InstantiateArObjBtn != null)
        {
            var btn = MainUiNavigation.Instance.InstantiateArObjBtn;
            // Remove any previous runtime registration to avoid duplicates, then add.
            btn.onClick.RemoveListener(InstantiateObject);
            btn.onClick.AddListener(InstantiateObject);
        }
    }

    private void OnDestroy()
    {
        // Remove the listener when this controller is destroyed/unloaded to avoid dangling references.
        if (MainUiNavigation.Instance != null && MainUiNavigation.Instance.InstantiateArObjBtn != null)
        {
            MainUiNavigation.Instance.InstantiateArObjBtn.onClick.RemoveListener(InstantiateObject);
        }
    }

    void Update()
    {
        PlacePreviewObjectAndCheckIfPreviewPositionIsValid();

        MainUiNavigation.Instance.InstantiateArObjBtn.interactable = ValidPosition;
        MainUiNavigation.Instance.TextHelpGuideUsersAr.enabled = !ValidPosition;
    }
    #endregion //Unity

    #region Methods
    public void InstantiateObject()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (ArRaycastManager != null && ArRaycastManager.Raycast(screenCenter, _arRaycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = _arRaycastHits[0].pose;

            var prefabToPlace = GetPrefabToPlace();

            var newObj = Instantiate(prefabToPlace, hitPose.position, hitPose.rotation);
            newObj.SetActive(true);
            newObj.tag = "ArObject";
        }
        else
        {
            Debug.Log("Nenhum plano atingido pelo raycast.");
        }
    }

    private GameObject GetPrefabToPlace()
    {
        var previousUi = MainUiNavigation.Instance.UiMenus[MainUiNavigation.Instance.UiMenus.Count - 2];

        if (previousUi == MainUiNavigation.Instance.UiPraia)
            return MainUiNavigation.Instance.PrefabObj3dPraiaDosTesos;
        if (previousUi == MainUiNavigation.Instance.UiPonteVelha)
            return MainUiNavigation.Instance.PrefabObj3dPonteVelha;
        if (previousUi == MainUiNavigation.Instance.UiEstatua)
            return MainUiNavigation.Instance.PrefabObj3dEstatuaDosMineiros;
        if (previousUi == MainUiNavigation.Instance.UiLocomotiva)
            return MainUiNavigation.Instance.PrefabObj3dLocomotiva;
        if (previousUi == MainUiNavigation.Instance.UiIgreja)
            return MainUiNavigation.Instance.PrefabObj3dIgrejaPedorido;
        if (previousUi == MainUiNavigation.Instance.UiAerodromo)
            return MainUiNavigation.Instance.PrefabObj3dAerodromo;
        if (previousUi == MainUiNavigation.Instance.UiMinasPocoDeGermundeII)
            return MainUiNavigation.Instance.PrefabObj3dPocoGermundeII;
        if (previousUi == MainUiNavigation.Instance.UiCapelaSaoDomingos)
            return MainUiNavigation.Instance.PrefabObj3dMonteSaoDomingos;

        return null;
    }

    bool HasAnyPlanes()
    {
        if (ArPlaneManager == null || ArPlaneManager.trackables.count == 0)
            return false;

        return true;
    }

    private void PlacePreviewObjectAndCheckIfPreviewPositionIsValid()
    {
        ValidPosition = false;

        var hasPlanes = HasAnyPlanes();

        if (PreviewPositionValid == null || !hasPlanes)
            return;

        if (_arRaycastHits.Count == 0)
        {
            PreviewPositionValid.SetActive(false);
            PreviewPositionInvalid.SetActive(false);
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (ArRaycastManager != null && ArRaycastManager.Raycast(screenCenter, _arRaycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose? hitPose = GetPosePoitingUp(_arRaycastHits);

            if (hitPose == null)
            {
                PreviewPositionValid.SetActive(false);
                PreviewPositionInvalid.SetActive(true);

                hitPose = _arRaycastHits[0].pose;

                PreviewPositionInvalid.transform.position = hitPose.Value.position;
                PreviewPositionInvalid.transform.rotation = hitPose.Value.rotation;

                return;
            }

            PreviewPositionValid.SetActive(true);
            PreviewPositionInvalid.SetActive(false);

            PreviewPositionValid.transform.position = hitPose.Value.position;
            PreviewPositionValid.transform.rotation = hitPose.Value.rotation;

            ValidPosition = true;
        }
    }

    private Pose? GetPosePoitingUp(List<ARRaycastHit> arRaycastHits)
    {
        // Tolerance: allow a small margin from exact up (in degrees).
        // Adjust this value if you want a stricter/looser "pointing up" test.
        const float maxAngleFromUpDegrees = 20f;

        for (int i = 0; i < arRaycastHits.Count; i++)
        {
            Pose p = arRaycastHits[i].pose;
            // Get the 'up' direction of the hit's pose
            Vector3 poseUp = p.rotation * Vector3.up;

            // Angle between the pose's up and the world up vector
            float angle = Vector3.Angle(poseUp, Vector3.up);

            // If the pose is within the allowed angle from up, return it
            if (angle <= maxAngleFromUpDegrees)
                return p;
        }

        // None of the hits were pointing (approximately) up
        return null;
    }

    #endregion //Methods
}
