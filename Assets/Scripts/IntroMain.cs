using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public List<FadeIn> FadeInControllers = new List<FadeIn>();
    public List<FadeOut> FadeOutControllers = new List<FadeOut>();

    private bool _hasUserAlreadyTouchedScreen = false;

    private void Start()
    {
        foreach (var controller in FadeInControllers)
        {
            controller.StartFade();
        }
    }

    void Update()
    {
        ListenForUserClick();
        ListenForEnvironmentIsReadyToLoadNextScene();
    }

    private void ListenForEnvironmentIsReadyToLoadNextScene()
    {
        if (!_hasUserAlreadyTouchedScreen)
            return;

        var controllersThatStillHaveTransparency = FadeOutControllers.Where(controller => controller.Image.color.a != 1);

        if (controllersThatStillHaveTransparency.Any())
            return;

        LoadMenuScene();
    }

    private void ListenForUserClick()
    {
        var userTouchedScreen = IsUserTouchedScreen();
        var mouseClicked = IsMouseClicked();

        if (userTouchedScreen || mouseClicked)
        {
            _hasUserAlreadyTouchedScreen = true;

            foreach (var controller in FadeOutControllers)
            {
                controller.StartFade();
            }
        }
    }

    private bool IsUserTouchedScreen()
    {
        return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
    }

    private bool IsMouseClicked()
    {
        var isMouseClicked = Mouse.current.leftButton.wasPressedThisFrame 
            || Mouse.current.rightButton.wasPressedThisFrame 
            || Mouse.current.middleButton.wasPressedThisFrame;

        var isMouseInScreenBounds = false;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        if (mousePos.x >= 0 && mousePos.x <= Screen.width &&
            mousePos.y >= 0 && mousePos.y <= Screen.height)
        {
            isMouseInScreenBounds = true;
        }

        return isMouseClicked && isMouseInScreenBounds;
    }

    private void LoadMenuScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu UI");
    }
}
