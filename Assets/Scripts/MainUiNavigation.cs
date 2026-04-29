using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEngine.XR.Simulation;
#endif

public class MainUiNavigation : MonoBehaviour
{
    #region Fields

    [Header("Debug")]
    public List<GameObject> UiMenus = new List<GameObject>();

    [Header("UI - Menus")]
    public GameObject UiMainMenu;
    public GameObject UiPraia;
    public GameObject UiPonteVelha;
    public GameObject UiEstatua;
    public GameObject UiLocomotiva;
    public GameObject UiIgreja;
    public GameObject UiAerodromo;
    public GameObject UiMinasPocoDeGermundeII;
    public GameObject UiPenedoDoLastrao;
    public GameObject UiCapelaSenhoraDasAmoras;
    public GameObject UiCapelaSaoDomingos;
    public GameObject UiSobre;
    [Space]
    public GameObject UiPraia3dObj;
    public GameObject UiPonteVelha3dObj;
    public GameObject UiEstatuaDosMineirosdObj;
    public GameObject UiLocomotiva3dObj;
    public GameObject UiIgrejaPedorido3dObj;
    public GameObject UiAerodromo3dObj;
    public GameObject UiPocoGermundeII3dObj;
    public GameObject UiMonteSaoDomingos3dObj;

    [Header("UI - Galleries")]
    public GameObject GalleryPraia;
    public GameObject GalleryPonteVelha;
    public GameObject GalleryEstatuaDosMineiros;
    public GameObject GalleryLocomotiva;
    public GameObject GalleryIgrejaPedorido;
    public GameObject GalleryAerodromo;
    public GameObject GalleryPocoGermundeII;
    public GameObject GalleryPenedoDoLastrao;
    public GameObject GalleryCapelaSenhoraDasAmoras;
    public GameObject GalleryCapelaSaoDomingos;

    [Header("UI - AR")]
    public GameObject UiAr;
    public Button InstantiateArObjBtn;
    public TextMeshProUGUI TextHelpGuideUsersAr;

    [Header("3D Objects Prefabs")]
    public GameObject PrefabObj3dPraiaDosTesos;
    public GameObject PrefabObj3dPonteVelha;
    public GameObject PrefabObj3dEstatuaDosMineiros;
    public GameObject PrefabObj3dLocomotiva;
    public GameObject PrefabObj3dIgrejaPedorido;
    public GameObject PrefabObj3dAerodromo;
    public GameObject PrefabObj3dPocoGermundeII;
    public GameObject PrefabObj3dMonteSaoDomingos;

    [Header("Camera")]
    public Camera MainCamera;
    public CameraTouchOrbitController CameraController;

    [Header("Other")]
    public GameObject EventSystem;

    private GameObject _currentUi;

    private GameObject _current3dObject;

    public static MainUiNavigation Instance { get; private set; }
    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }
    
    private void Start()
    {
        if (MainCamera == null)
        {
            MainCamera = Camera.main;
        }

        if (UiMenus.Count == 0)
        {
            UiMenus.Add(UiMainMenu);
        }
    }

    private void Update()
    {
        bool backPressed = false;
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            backPressed = true;
        }

        if (backPressed)
        {
            if (UiMenus != null && UiMenus.Count > 1)
            {
                Return();
            }
        }
    }
    #endregion //Unity

    #region UI Navigation

    private void LoadUi()
    {
        UiMenus.Add(_currentUi);

        ShowCurrentUiMenu();
    }

    private void ShowCurrentUiMenu()
    {
        for (int i = 0; i < UiMenus.Count - 1; i++)
        {
            var uiMenu = UiMenus[i];
            uiMenu.SetActive(false);
        }

        var lastUiMenu = UiMenus[UiMenus.Count - 1];
        lastUiMenu.SetActive(true);
    }

    public void Return()
    {
        var lastUiMenu = UiMenus[UiMenus.Count - 1];

        var isArScene = lastUiMenu == UiAr;

        lastUiMenu.SetActive(false);

        UiMenus.Remove(lastUiMenu);

        if(_current3dObject  != null)
        {
            Destroy(_current3dObject);
            CameraController.enabled = false;
        }

        ShowCurrentUiMenu();

        if(isArScene)
        {
            UnloadArScene();
        }
    }

    private void UnloadArScene()
    {
        StartCoroutine(UnloadArSceneCoroutine());
    }

    private IEnumerator UnloadArSceneCoroutine()
    {
        var unloadOperations = new List<AsyncOperation>();

        // Request unload of the AR scene
        unloadOperations.Add(SceneManager.UnloadSceneAsync("AR Object View Scene"));

        // Request unload of any simulated environment scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var currentScene = SceneManager.GetSceneAt(i);

            if (currentScene.name.StartsWith("Simulated Environment Scene") && currentScene.IsValid() && currentScene.isLoaded)
            {
                unloadOperations.Add(SceneManager.UnloadSceneAsync(currentScene));
            }
        }

        // Wait for all unload operations to complete
        bool allDone = false;
        while (!allDone)
        {
            allDone = true;
            for (int i = 0; i < unloadOperations.Count; i++)
            {
                if (unloadOperations[i] != null && !unloadOperations[i].isDone)
                {
                    allDone = false;
                    break;
                }
            }

            if (!allDone)
                yield return null;
        }

        // Now that the scenes are unloaded, clean up DontDestroyOnLoad objects
        CleanupPersistentArObjects();

        // Re-enable main camera and event system after cleanup to avoid duplicate listeners
        if (MainCamera != null)
            MainCamera.gameObject.SetActive(true);
        if (EventSystem != null)
            EventSystem.gameObject.SetActive(true);

        yield break;
    }

    private void CleanupPersistentArObjects()
    {
        var xrManagers = Resources.FindObjectsOfTypeAll<XRInteractionManager>();
        foreach (var mgr in xrManagers)
        {
            if (mgr != null)
                Destroy(mgr.gameObject);
        }

#if UNITY_EDITOR
        var simCameras = Resources.FindObjectsOfTypeAll<SimulationCameraPoseProvider>();
        foreach (var obj in simCameras)
        {
            if (obj != null)
                Destroy(obj.gameObject);
        }
#endif

        var debugUpdater = GameObject.Find("[Debug Updater]");
        if (debugUpdater != null)
            Destroy(debugUpdater);

        var arObject = GameObject.FindGameObjectWithTag("ArObject");
        if (arObject != null)
            Destroy(arObject);
    }

    public void LoadPraiaUi()
    {
        _currentUi = UiPraia;   

        LoadUi();
    }

    public void LoadPonteVelhaUi()
    {
        _currentUi = UiPonteVelha;

        LoadUi();
    }

    public void LoadEstatuaUi()
    {
        _currentUi = UiEstatua;

        LoadUi();
    }

    public void LoadLocomotivaUi()
    {
        _currentUi = UiLocomotiva;

        LoadUi();
    }

    public void LoadIgrejaUi()
    {
        _currentUi = UiIgreja;

        LoadUi();
    }

    public void LoadAerodromoUi()
    {
        _currentUi = UiAerodromo;

        LoadUi();
    }

    public void LoadMinasPocoGermundeUi()
    {
        _currentUi = UiMinasPocoDeGermundeII;

        LoadUi();
    }

    public void LoadPenedoDoLastraoUi()
    {
        _currentUi = UiPenedoDoLastrao;

        LoadUi();
    }

    public void LoadCapelaSenhoraDasAmorasUi()
    {
        _currentUi = UiCapelaSenhoraDasAmoras;

        LoadUi();
    }

    public void LoadCapelaSaoDomingosUi()
    {
        _currentUi = UiCapelaSaoDomingos;

        LoadUi();
    }

    public void LoadSobreUi()
    {
        _currentUi = UiSobre;

        LoadUi();
    }

    #region Galleries
    public void LoadGalleryPraia()
    {
        _currentUi = GalleryPraia;

        LoadUi();
    }

    public void LoadGalleryPonteVelha()
    {
        _currentUi = GalleryPonteVelha;

        LoadUi();
    }

    public void LoadGalleryEstatuaDosMineiros()
    {
        _currentUi = GalleryEstatuaDosMineiros;

        LoadUi();
    }

    public void LoadGalleryLocomotiva()
    {
        _currentUi = GalleryLocomotiva;

        LoadUi();
    }

    public void LoadGalleryIgrejaPedorido()
    {
        _currentUi = GalleryIgrejaPedorido;

        LoadUi();
    }

    public void LoadGalleryAerodromo()
    {
        _currentUi = GalleryAerodromo;

        LoadUi();
    }

    public void LoadGalleryMinasPocoGermundeII()
    {
        _currentUi = GalleryPocoGermundeII;

        LoadUi();
    }

    public void LoadGalleryPenedoDoLastrao()
    {
        _currentUi = GalleryPenedoDoLastrao;

        LoadUi();
    }

    public void LoadGalleryCapelaSenhoraDasAmoras()
    {
        _currentUi = GalleryCapelaSenhoraDasAmoras;

        LoadUi();
    }

    public void LoadGalleryCapelaSaoDomingos()
    {
        _currentUi = GalleryCapelaSaoDomingos;

        LoadUi();
    }
    #endregion //Galleries

    #region 3D Objects
    private void Load3dObject(GameObject ui, GameObject prefabToInstantiate)
    {
        _currentUi = ui;

        LoadUi();

        _current3dObject = Instantiate(prefabToInstantiate);
        CameraController.enabled = true;
    }

    public void Load3dObjectPraiaDosTesos()
    {
        Load3dObject(UiPraia3dObj, PrefabObj3dPraiaDosTesos);
    }

    public void Load3dObjectPonteVelha()
    {
        Load3dObject(UiPonteVelha3dObj, PrefabObj3dPonteVelha);
    }

    public void Load3dObjectEstatuaDosMineiros()
    {
        Load3dObject(UiEstatuaDosMineirosdObj, PrefabObj3dEstatuaDosMineiros);
    }

    public void Load3dObjectLocomotiva()
    {
        Load3dObject(UiLocomotiva3dObj, PrefabObj3dLocomotiva);
    }

    public void Load3dObjectIgrejaPedorido()
    {
        Load3dObject(UiIgrejaPedorido3dObj, PrefabObj3dIgrejaPedorido);
    }

    public void Load3dObjectAerodromo()
    {
        Load3dObject(UiAerodromo3dObj, PrefabObj3dAerodromo);
    }

    public void Load3dObjectPocoGermundeII()
    {
        Load3dObject(UiPocoGermundeII3dObj, PrefabObj3dPocoGermundeII);
    }

    public void Load3dObjectMonteSaoDomingos()
    {
        Load3dObject(UiMonteSaoDomingos3dObj, PrefabObj3dMonteSaoDomingos);
    }
    #endregion //3D Objects

    #region AR Objects

    public void LoadArUiAndScene()
    {
        _currentUi = UiAr;

        LoadUi();

        MainCamera.gameObject.SetActive(false);
        EventSystem.gameObject.SetActive(false);

        SceneManager.LoadScene("AR Object View Scene", LoadSceneMode.Additive);
    }

    #endregion

    #endregion //UI Navigation

}
