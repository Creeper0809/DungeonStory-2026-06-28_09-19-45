using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BackgroundManager : MonoBehaviour
{
    public SpriteRenderer background;
    public Light2D light2D;
    public GameData gameData;
    private void Awake()
    {
        gameData.day.OnValueChange += (_) =>
        {
            DOTween.Sequence()
            .Insert(10f, background.DOColor(new Color32(25, 25, 112, 255), 15f))
            .Insert(50f, background.DOColor(new Color32(135, 206, 250, 255), 40f))
            .Insert(127f, background.DOColor(new Color32(253, 184, 39, 255), 8f))
            .Insert(143.5f, background.DOColor(new Color32(255, 126, 95, 255), 1.5f))
            .Insert(146f, background.DOColor(new Color32(54, 33, 89, 255), 9f))
            .Insert(155.5f, background.DOColor(Color.black, 10f))
            .SetUpdate(UpdateType.Normal)
            .Play();
        };
        gameData.day.OnValueChange += (_) =>
        {
            DOTween.Sequence()
                .Insert(15f, DOVirtual.Float(0.5f, 1f, 45f, value =>
                {
                    light2D.intensity = value;
                }))
                .Insert(127f, DOVirtual.Float(1f, 0.5f, 20f, value =>
                {
                    light2D.intensity = value;
                }))
                .SetUpdate(UpdateType.Normal)
                .Play();
        };
    }
    void Start()
    {
        
    }
}
