using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public Image Image;
    public float FadeDuration = 1f;
    public float StartAfterSeconds = 0f;
    [Range(0f, 1f)]
    public float FadeLimit = 0;

    public bool FadeFinished { get; protected set; } = true;

    public virtual void StartFade()
    {

    }
}
