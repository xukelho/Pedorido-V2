using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceOnPlaneController : MonoBehaviour
{
    [Header("AR managers")]
    [SerializeField] ARPlaneManager arPlaneManager;
    [SerializeField] ARRaycastManager arRaycastManager;

    [Header("Placement")]
    [SerializeField] GameObject prefabToPlace;

    [Header("Preview")]
    [SerializeField] GameObject prefabPreview;

    [Header("UI")]
    [SerializeField] Button placeButton;

    bool planeDetected = false;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        if (placeButton != null)
            placeButton.interactable = false;

        if (placeButton != null)
            placeButton.onClick.AddListener(OnPlaceButtonPressed);
    }

    void OnDestroy()
    {
        if (placeButton != null)
            placeButton.onClick.RemoveListener(OnPlaceButtonPressed);
    }

    // Polling simples e compatível com AR Foundation 6.2.1
    void Update()
    {
        bool hasPlanes = HasAnyPlanes();
        if (hasPlanes != planeDetected)
        {
            planeDetected = hasPlanes;
            if (placeButton != null)
                placeButton.interactable = planeDetected;
        }

        PlacePreviewObject();
    }

    void OnPlaceButtonPressed()
    {
        // Raycast no centro da tela para detectar um plano
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (arRaycastManager != null && arRaycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = s_Hits[0].pose;

            if (prefabToPlace != null)
            {
                Instantiate(prefabToPlace, hitPose.position, hitPose.rotation);
            }
            else
            {
                // Cria um cubo primitivo quando não houver prefab
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                primitive.name = "PlacedCube";
                primitive.transform.position = hitPose.position;
                primitive.transform.rotation = hitPose.rotation;
                // Escala reduzida para AR (ajuste conforme necessário)
                primitive.transform.localScale = Vector3.one * 0.2f;
            }
        }
        else
        {
            Debug.Log("Nenhum plano atingido pelo raycast.");
        }
    }

    // Evita depender de propriedades ambíguas; retorna true se houver pelo menos um plano rastreado
    bool HasAnyPlanes()
    {
        if (arPlaneManager == null)
            return false;

        foreach (var _ in arPlaneManager.trackables)
            return true;

        return false;
    }

    private void PlacePreviewObject()
    {
        if (prefabPreview == null)
            return;
        // Ativa ou desativa o objeto de pré-visualização com base na detecção do plano
        prefabPreview.SetActive(planeDetected);
        if (planeDetected)
        {
            // Raycast no centro da tela para posicionar o objeto de pré-visualização
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (arRaycastManager != null && arRaycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = s_Hits[0].pose;
                prefabPreview.transform.position = hitPose.position;
                prefabPreview.transform.rotation = hitPose.rotation;
            }
        }
    }
}
