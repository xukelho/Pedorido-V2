using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MainUiNavigation : MonoBehaviour
{
    #region Fields

    [Header("Debug")]
    public List<GameObject> _uiMenus = new List<GameObject>();

    [Header("UI - Menus")]
    public GameObject UiMainMenu;
    public GameObject UiPraia;
    public GameObject UiPonteVelha;
    public GameObject UiEstatua;
    public GameObject UiLocomotiva;
    public GameObject UiIgreja;
    public GameObject UiAerodromo;
    public GameObject UiMinasPocoDeGermundeII;
    public GameObject UiCampoFutebol;
    public GameObject UiPassadico;
    public GameObject UiCasaDaMalta;
    public GameObject UiPenedoDoLastrao;
    public GameObject UiCapelaSenhoraDasAmoras;
    public GameObject UiCapelaSaoDomingos;
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

    [Header("3D Objects")]
    public GameObject Obj3dPraiaDosTesos;
    public GameObject Obj3dPonteVelha;
    public GameObject Obj3dEstatuaDosMineiros;
    public GameObject Obj3dLocomotiva;
    public GameObject Obj3dIgrejaPedorido;
    public GameObject Obj3dAerodromo;
    public GameObject Obj3dPocoGermundeII;
    public GameObject Obj3dMonteSaoDomingos;

    [Header("Camera")]
    public Camera MainCamera;
    public CameraTouchOrbitController CameraController;

    private GameObject _currentUi;
    #endregion

    #region Unity
    private void Start()
    {
        if (MainCamera == null)
        {
            MainCamera = Camera.main;
        }

        if (_uiMenus.Count == 0)
        {
            _uiMenus.Add(UiMainMenu);
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
            if (_uiMenus != null && _uiMenus.Count > 1)
            {
                Return();
            }
        }
    }
    #endregion //Unity

    #region UI Navigation

    private void LoadUi()
    {
        _uiMenus.Add(_currentUi);

        ShowCurrentUiMenu();
    }

    private void ShowCurrentUiMenu()
    {
        for (int i = 0; i < _uiMenus.Count - 1; i++)
        {
            var uiMenu = _uiMenus[i];
            uiMenu.SetActive(false);
        }

        var lastUiMenu = _uiMenus[_uiMenus.Count - 1];
        lastUiMenu.SetActive(true);
    }

    public void Return()
    {
        var lastUiMenu = _uiMenus[_uiMenus.Count - 1];
        lastUiMenu.SetActive(false);

        _uiMenus.Remove(lastUiMenu);

        ShowCurrentUiMenu();

        if (lastUiMenu == Obj3dPraiaDosTesos)
        {
            CameraController.enabled = false;
            UiPraia3dObj.SetActive(false);
        }
        else if (lastUiMenu == Obj3dPonteVelha)
        {
            CameraController.enabled = false;
            UiPonteVelha3dObj.SetActive(false);
        }
        else if (lastUiMenu == Obj3dEstatuaDosMineiros)
        {
            CameraController.enabled = false;
            UiEstatuaDosMineirosdObj.SetActive(false);
        }
        else if (lastUiMenu == Obj3dLocomotiva)
        {
            CameraController.enabled = false;
            UiLocomotiva3dObj.SetActive(false);
        }
        else if (lastUiMenu == Obj3dIgrejaPedorido)
        {
            CameraController.enabled = false;
            UiIgrejaPedorido3dObj.SetActive(false);
        }
        else if (lastUiMenu == Obj3dAerodromo)
        {
            CameraController.enabled = false;
            UiAerodromo3dObj.SetActive(false);
        }
        else if (lastUiMenu == Obj3dPocoGermundeII)
        {
            CameraController.enabled = false;
            UiPocoGermundeII3dObj.SetActive(false);
        }
        else if (lastUiMenu == Obj3dMonteSaoDomingos)
        {
            CameraController.enabled = false;
            UiMonteSaoDomingos3dObj.SetActive(false);
        }
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

    public void LoadCampoFutebolUi()
    {
        _currentUi = UiCampoFutebol;

        LoadUi();
    }

    public void LoadPassadicoUi()
    {
        _currentUi = UiPassadico;

        LoadUi();
    }

    public void LoadCasaDaMaltaUi()
    {
        _currentUi = UiCasaDaMalta;

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
    //--- Galleries ---
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
    #endregion //UI Navigation

    public void Load3dObjectPraiaDosTesos()
    {
        _currentUi = Obj3dPraiaDosTesos;
        LoadUi();

        CameraController.enabled = true;
        UiPraia3dObj.SetActive(true);
    }

    public void Load3dObjectPonteVelha()
    {
        _currentUi = Obj3dPonteVelha;
        LoadUi();

        CameraController.enabled = true;
        UiPonteVelha3dObj.SetActive(true);
    }

    public void Load3dObjectEstatuaDosMineiros()
    {
        _currentUi = Obj3dEstatuaDosMineiros;
        LoadUi();

        CameraController.enabled = true;
        UiEstatuaDosMineirosdObj.SetActive(true);
    }

    public void Load3dObjectLocomotiva()
    {
        _currentUi = Obj3dLocomotiva;
        LoadUi();

        CameraController.enabled = true;
        UiLocomotiva3dObj.SetActive(true);
    }

    public void Load3dObjectIgrejaPedorido()
    {
        _currentUi = Obj3dIgrejaPedorido;
        LoadUi();

        CameraController.enabled = true;
        UiIgrejaPedorido3dObj.SetActive(true);
    }

    public void Load3dObjectAerodromo()
    {
        _currentUi = Obj3dAerodromo;
        LoadUi();

        CameraController.enabled = true;
        UiAerodromo3dObj.SetActive(true);
    }

    public void Load3dObjectPocoGermundeII()
    {
        _currentUi = Obj3dPocoGermundeII;
        LoadUi();

        CameraController.enabled = true;
        UiPocoGermundeII3dObj.SetActive(true);
    }

    public void Load3dObjectMonteSaoDomingos()
    {
        _currentUi = Obj3dMonteSaoDomingos;
        LoadUi();

        CameraController.enabled = true;
        UiMonteSaoDomingos3dObj.SetActive(true);
    }
}
