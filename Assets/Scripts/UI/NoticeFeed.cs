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
        noticePool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 5, 15);
    }
    public virtual void OnTriggerEvent(NoticeFeedEvent e)
    {
        GameObject noticeObject = noticePool.Get();
        noticeObject.transform.parent = this.transform;
        noticeObject.transform.SetAsFirstSibling();
        noticeObject.transform.localScale = new Vector3(1, 1, 1);
        TMP_Text textObject = noticeObject.transform.GetChild(0).GetComponent<TMP_Text>();
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
    private GameObject CreatePooledItem()
    {
        return Instantiate(textPrefab);
    }
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }
    private void OnDestroyPoolObject(GameObject poolGo)
    {
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
