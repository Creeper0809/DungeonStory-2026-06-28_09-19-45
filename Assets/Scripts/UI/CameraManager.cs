using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float speed = 5.0f;
    public float edgeThreshold = 10.0f; // 화면 가장자리로부터의 거리
    private GridSystemManager gridSystemManager;
    public Vector2 maxBound;
    public Vector2 minBound;
    private float height;
    private float width;
    private void Awake()
    {
        gridSystemManager = GridSystemManager.Instance;
    }
    private void Start()
    {
        height = Camera.main.orthographicSize;
        width = Camera.main.aspect * height;

        gridSystemManager.OnGridExpand += OnGridExpand;
    }
    void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        // 마우스 포지션을 화면 좌표로 가져옴
        Vector3 mousePosition = Input.mousePosition;

        // 화면의 가장자리를 체크하고, 해당 방향으로 카메라 이동
        if (mousePosition.x < edgeThreshold)
        {
            moveX = -speed;
        }
        else if (mousePosition.x > Screen.width - edgeThreshold)
        {
            moveX = speed;
        }

        if (mousePosition.y < edgeThreshold)
        {
            moveY = -speed;
        }
        else if (mousePosition.y > Screen.height - edgeThreshold)
        {
            moveY = speed;
        }
        // 카메라 이동 (현재 위치에서 moveX, moveY만큼 이동)
        transform.Translate(new Vector3(moveX * Time.deltaTime, moveY * Time.deltaTime, this.transform.position.z) , Space.World);

        float clampX = Mathf.Clamp(transform.position.x, maxBound.x + width, minBound.x - width);
        float clampY = Mathf.Clamp(transform.position.y, minBound.y + height, maxBound.y - height);

        transform.position = new Vector3(clampX, clampY, -10f);
    }
    public void OnGridExpand()
    {
        maxBound = new Vector2(maxBound.x - 4, maxBound.y + 4);
    }
}
