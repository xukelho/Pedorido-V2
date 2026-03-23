using UnityEngine;
using UnityEngine.UI;

public class FadeIn : FadeController
{    
    private float _elapsedTime = 0f;
    private bool _fadeStarted = false;

    void FixedUpdate()
    {
        if (FadeFinished)
            return;

        if (Image != null)
        {
            _elapsedTime += Time.fixedDeltaTime;

            if (!_fadeStarted && _elapsedTime >= StartAfterSeconds)
            {
                _fadeStarted = true;
            }

            if (_fadeStarted)
            {
                Color color = Image.color;
                color.a -= Time.fixedDeltaTime / FadeDuration;
                color.a = Mathf.Clamp01(color.a);
                Image.color = color;

                if (color.a >= 1f || color.a <= FadeLimit)
                {
                    FadeFinished = true;
                }
            }
        }
    }

    public override void StartFade()
    {
        FadeFinished = false;
    }
}
