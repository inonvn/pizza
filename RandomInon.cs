
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RandomInon
{
   
    public static System.Random random = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static T rand<T>(this IList<T> list)
    {
        return list[random.Next(list.Count)];
    }
    public static T last<T>(this IList<T> list)
    {
        return list[list.Count - 1];
    }


    public static void Shuffled<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static void FadeIn (this CanvasGroup  canvasGroup)
    {
        canvasGroup.alpha = 1;
        canvasGroup.DOKill();
       
        canvasGroup.DOFade(0, 0.5f).OnComplete(() => { canvasGroup.alpha = 0; canvasGroup.gameObject.SetActive(false);  });

    }
    public static void FadeIn(this CanvasGroup canvasGroup, float alpha,float time)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.DOKill();
        canvasGroup.DOFade(alpha, time).OnComplete(() => { canvasGroup.alpha = 0; canvasGroup.gameObject.SetActive(false); });

    }
    public static void FadeOut(this CanvasGroup canvasGroup)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.5f).OnComplete(() => { canvasGroup.alpha = 1;  });

    }
    public static void FadeOutAndLoad(this CanvasGroup canvasGroup,int NumScene)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.5f).OnComplete(() => { canvasGroup.alpha = 1; SceneManager.LoadScene(NumScene); });
    }
    public static void ButtonSound(this AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public static void AddSound(this Button button, AudioSource audioSource, AudioClip audioClip)
    {
        if (button != null && audioSource != null && audioClip != null)
        {
            button.onClick.AddListener(() =>
            {
                audioSource.PlayOneShot(audioClip);
            });
        }
    }
}
