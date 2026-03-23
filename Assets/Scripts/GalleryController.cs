using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GalleryController : MonoBehaviour
{
    #region Fields
    public MainUiNavigation MainUiNavigation;

    public List<Image> Images;

    private Image _currentImage;
    #endregion //Fields

    #region Public
    public void ShowLeft()
    {
        var imageIndex = Images.IndexOf(_currentImage);
        var previousIndex = imageIndex - 1;

        if (imageIndex == 0)
            previousIndex = Images.Count - 1;

        _currentImage.gameObject.SetActive(false);
        Images[previousIndex].gameObject.SetActive(true);

        _currentImage = Images[previousIndex];
    }

    public void ShowRight()
    {
        var imageIndex = Images.IndexOf(_currentImage);
        var nextIndex = imageIndex + 1;

        if (imageIndex >= Images.Count - 1)
            nextIndex = 0;

        _currentImage.gameObject.SetActive(false);
        Images[nextIndex].gameObject.SetActive(true);

        _currentImage = Images[nextIndex];
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    public void LoadGalleryPraiaByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryPraia();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryPonteVelhaByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryPonteVelha();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryEstatuaDosMineirosByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryEstatuaDosMineiros();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryLocomotivaByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryLocomotiva();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryIgrejaPedoridoByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryIgrejaPedorido();
        LoadImageByReference(imageRef);
    }

    public void LoadGalleryAerodromoByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryAerodromo();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryPocoGermundeIIByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryMinasPocoGermundeII();
        LoadImageByReference(imageRef);
    }

    public void LoadGalleryCampoFutebolByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryCampoFutebol();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryPenedoDoLastraoByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryPenedoDoLastrao();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryPassadicoByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryPassadico();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryCasaDaMaltaByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryCasaDaMalta();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryCapelaSenhoraDasAmorasByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryCapelaSenhoraDasAmoras();

        LoadImageByReference(imageRef);
    }

    public void LoadGalleryMonteSaoDomingosByImageReference(Image imageRef)
    {
        MainUiNavigation.LoadGalleryCapelaSaoDomingos();
        LoadImageByReference(imageRef);
    }
    #endregion //Public

    #region Private
    private void LoadImageByReference(Image imageRef)
    {
        foreach (var img in Images)
        {
            img.gameObject.SetActive(false);
        }

        if (!this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(true);
        }

        _currentImage = Images.First(i => i.mainTexture == imageRef.mainTexture);

        _currentImage.gameObject.SetActive(true);
    }
    #endregion //Private
}
