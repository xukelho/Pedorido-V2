using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CameraTouchOrbitController : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // ponto ao redor do qual a câmera irá orbitar. Se nulo, usa Vector3.zero.

    [Header("Distance")]
    public float distance = 5f;

    [Header("Rotation")]
    [Tooltip("Grau por pixel (base).")]
    public float rotationSensitivity = 0.15f;
    
    public bool invertY = false;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Zoom")]
    [Tooltip("Sensibilidade do pinch (multiplicador sobre a variação em pixels).")]
    public float pinchZoomSensitivity = 0.01f;
    [Tooltip("Distância mínima permitida entre câmera e target.")]
    public float minDistance = 1f;
    [Tooltip("Distância máxima permitida entre câmera e target.")]
    public float maxDistance = 20f;

    [Header("Development")]
    [Tooltip("Usar mouse para simular um toque (útil no Editor/PC).")]
    public bool simulateMouse = true;

    // Suavização opcional (pequeno valor para resposta mais direta)
    [Header("Smoothing")]
    [Tooltip("0 = sem suavização. Valores pequenos (ex: 0.05) suavizam movimentos.")]
    public float smoothTime = 0.04f;

    // Estado interno
    private float yaw;
    private float pitch;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 desiredPosition;

    // Mouse simulation state
    private Vector2 lastMousePos;
    private bool mouseDragging;

    // Pinch state
    private bool pinchActive = false;
    private float previousPinchDistance = 0f;

    // Valores originais (serializados para que possam ser salvos no Editor)
    [SerializeField, HideInInspector]
    private Vector3 originalPosition;
    [SerializeField, HideInInspector]
    private Quaternion originalRotation;
    [SerializeField, HideInInspector]
    private float originalYaw;
    [SerializeField, HideInInspector]
    private float originalPitch;
    [SerializeField, HideInInspector]
    private float originalDistance;

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
        // fallback target handled por GetTargetPosition()
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;

        if (target != null)
            distance = Vector3.Distance(transform.position, target.position);

        if (Application.isEditor)
            simulateMouse = true;

        // Captura o estado original no início da execução (Play mode)
        CaptureOriginalFromRuntime();

        UpdateDesiredPositionImmediate();
    }

    void Update()
    {
        bool handled = false;

        // --- Novo Input System (EnhancedTouch) ---
        var touches = Touch.activeTouches;

        // PINCH (dois dedos)
        if (touches.Count == 2)
        {
            var a = touches[0].screenPosition;
            var b = touches[1].screenPosition;
            float currentDistance = Vector2.Distance(a, b);

            if (!pinchActive)
            {
                pinchActive = true;
                previousPinchDistance = currentDistance;
            }
            else
            {
                float pinchDelta = currentDistance - previousPinchDistance;
                // Quando os dedos se afastam (pinchDelta > 0) diminui a distância (zoom in).
                distance -= pinchDelta * pinchZoomSensitivity;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
                previousPinchDistance = currentDistance;

                UpdateDesiredPositionImmediate();
            }

            handled = true;
        }
        else
        {
            // se não há dois dedos, resetar estado de pinch para próximo ciclo
            pinchActive = false;
        }

        // ORBIT (um dedo)
        if (!handled && touches.Count == 1)
        {
            var et = touches[0];
            Vector2 delta = et.delta;
            if (delta != Vector2.zero)
            {
                float dt = Mathf.Max(Time.deltaTime, 0.0001f);
                // cálculo de velocidade não usado para sensibilidade aqui, mas pode ser reativado se desejar
                float dx = delta.x * rotationSensitivity;
                float dy = delta.y * rotationSensitivity;

                yaw += dx;
                pitch += (invertY ? dy : -dy);
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

                UpdateDesiredPositionImmediate();
            }

            handled = true;
        }

        // mouse simulation via novo Input System (apenas para orbitar com 1 botão; não simula pinch)
        if (!handled && simulateMouse && Mouse.current != null)
        {
            var mouse = Mouse.current;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                mouseDragging = true;
                lastMousePos = mouse.position.ReadValue();
            }
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                mouseDragging = false;
            }

            if (mouseDragging)
            {
                Vector2 currentPos = mouse.position.ReadValue();
                Vector2 delta = currentPos - lastMousePos;

                float dx = delta.x * rotationSensitivity;
                float dy = delta.y * rotationSensitivity;

                yaw += dx;
                pitch += (invertY ? dy : -dy);
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

                UpdateDesiredPositionImmediate();

                lastMousePos = currentPos;
            }
        }

        // aplica suavização na posição (opcional)
        if (smoothTime > 0f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
            Quaternion desiredRot = Quaternion.LookRotation((GetTargetPosition() - desiredPosition).normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-20f * Time.deltaTime));
        }
        else
        {
            transform.position = desiredPosition;
            transform.rotation = Quaternion.LookRotation((GetTargetPosition() - transform.position).normalized, Vector3.up);
        }
    }

    private void UpdateDesiredPositionImmediate()
    {
        Vector3 tgt = GetTargetPosition();
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        desiredPosition = tgt + rot * new Vector3(0f, 0f, -Mathf.Max(0.001f, distance));
    }

    private Vector3 GetTargetPosition()
    {
        return target != null ? target.position : Vector3.zero;
    }

    // Public API para o Editor: reseta a câmera para o estado original salvo
    public void ResetToOriginalTransform()
    {
        // Se os originais não foram inicializados, tenta capturar do runtime atual
        if (originalRotation == Quaternion.identity && originalPosition == Vector3.zero && !Application.isPlaying)
        {
            // nada salvo ainda — faz nada
        }

        // Aplica transform original
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Restaura valores internos para que a lógica continue coerente
        yaw = originalYaw;
        pitch = originalPitch;
        distance = originalDistance;
        currentVelocity = Vector3.zero;

        // Atualiza desiredPosition para o novo estado
        UpdateDesiredPositionImmediate();
    }

    // Salva o estado atual como "original" (útil no Editor)
    public void SaveOriginalTransformFromEditor()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // também captura yaw/pitch/distance atuais
        originalYaw = yaw;
        originalPitch = pitch;
        originalDistance = distance;
    }

    // Captura originais no início do Play
    private void CaptureOriginalFromRuntime()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalYaw = yaw;
        originalPitch = pitch;
        originalDistance = distance;
    }

#if UNITY_EDITOR
    // Visual helper no editor para ver o ponto alvo
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target.position, 0.1f);
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
#endif
}