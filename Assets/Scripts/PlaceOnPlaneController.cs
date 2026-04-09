using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;

public class PlaceOnPlaneController : MonoBehaviour
{
    [Header("AR managers")]
    [SerializeField] ARPlaneManager ArPlaneManager;
    [SerializeField] ARRaycastManager ArRaycastManager;

    [Header("Placement")]
    [SerializeField] GameObject PrefabToPlace;

    [Header("Preview")]
    [SerializeField] GameObject PreviewPositionValid;
    [SerializeField] GameObject PreviewPositionInvalid;

    static List<ARRaycastHit> _arRaycastHits = new List<ARRaycastHit>();

    void Update()
    {
        PlacePreviewObject();
    }

    public void InstantiateObject()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (ArRaycastManager != null && ArRaycastManager.Raycast(screenCenter, _arRaycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = _arRaycastHits[0].pose;

            if (PrefabToPlace != null)
            {
                var newObj = Instantiate(PrefabToPlace, hitPose.position, hitPose.rotation);
                newObj.SetActive(true);
            }
            else
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                primitive.name = "PlacedCube";
                primitive.transform.position = hitPose.position;
                primitive.transform.rotation = hitPose.rotation;
                
                primitive.transform.localScale = Vector3.one * 0.2f;
            }
        }
        else
        {
            Debug.Log("Nenhum plano atingido pelo raycast.");
        }
    }

    bool HasAnyPlanes()
    {
        if (ArPlaneManager == null || ArPlaneManager.trackables.count == 0)
            return false;

        return true;
    }

    private void PlacePreviewObject()
    {
        var hasPlanes = HasAnyPlanes();

        if (PreviewPositionValid == null || !hasPlanes)
            return;
        
        if(_arRaycastHits.Count == 0)
        {
            PreviewPositionValid.SetActive(false);
            PreviewPositionInvalid.SetActive(false);
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (ArRaycastManager != null && ArRaycastManager.Raycast(screenCenter, _arRaycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose? hitPose = GetPosePoitingUp(_arRaycastHits);

            if(hitPose == null)
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
}
