using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class NoticeFeed : MonoBehaviour, UtilEventListener<NoticeFeedEvent>
{
    public GameObject textPrefab;
    public IObjectPool<GameObject> noticePool;

    private void Start()
    {
        EnsurePool();
    }
    public virtual void OnTriggerEvent(NoticeFeedEvent e)
    {
        EnsurePool();
        if (noticePool == null)
        {
            return;
        }

        GameObject noticeObject = noticePool.Get();
        if (noticeObject == null)
        {
            return;
        }

        noticeObject.transform.SetParent(transform, false);
        noticeObject.transform.SetAsFirstSibling();
        noticeObject.transform.localScale = new Vector3(1, 1, 1);
        TMP_Text textObject = noticeObject.GetComponentInChildren<TMP_Text>(true);
        if (textObject == null)
        {
            noticePool.Release(noticeObject);
            return;
        }

        TMPKoreanFont.Apply(textObject);
        textObject.text = e.notice;
        textObject.color = Color.white;
        switch (e.grade)
        {
            case NoticeFeedEvent.Grade.DANGER: textObject.color = Color.red; break;
            case NoticeFeedEvent.Grade.WARNING: textObject.color = Color.yellow; break;
        }
        DOTween.Sequence()
            .PrependInterval(2f)
            .Append(textObject.DOFade(0f, 1f))
            .Append(textObject.DOFade(1f, 0f))
            .OnComplete(() => noticePool.Release(noticeObject))
            .Play();
    }

    private void EnsurePool()
    {
        if (noticePool != null)
        {
            return;
        }

        noticePool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 5, 15);
    }

    private GameObject CreatePooledItem()
    {
        if (textPrefab != null)
        {
            return Instantiate(textPrefab);
        }

        Debug.LogError("NoticeFeed requires textPrefab.");
        return null;
    }
    private void OnTakeFromPool(GameObject poolGo)
    {
        if (poolGo == null) return;

        poolGo.SetActive(true);
    }
    private void OnReturnedToPool(GameObject poolGo)
    {
        if (poolGo == null) return;

        poolGo.SetActive(false);
    }
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        if (poolGo == null) return;

        Destroy(poolGo);
    }
    private void OnEnable()
    {
        this.EventStartListening();
    }
    private void OnDisable()
    {
        this.EventStopListening();
    }
}
public struct NoticeFeedEvent
{
    public string notice;
    public Grade grade;
    public enum Grade
    {
        NONE,
        WARNING,
        DANGER
    }
    public NoticeFeedEvent(string notice, Grade grade)
    {
        this.notice = notice;
        this.grade = grade;
    }
    static NoticeFeedEvent e;
    public static void Trigger(string notice, Grade grade)
    {
        e.notice = notice;
        e.grade = grade;
        EventObserver.TriggerEvent(e);
    }
}
