using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private Camera mainCamera;

    private Vector3 moveDir = Vector2.zero;
    private Rect mapBounds;

    [SerializeField] private float cameraSpeed = default;
    [SerializeField] private GameObject playerMissingIndicator = default;
    private RectTransform playerMissingImage;
    private RectTransform playerMissingArrow;
    [SerializeField] private float indicatorScreenOffset = default;
    [SerializeField] private float indicatorScreenOffsetTop = default; // bit weird

    private void Awake()
    {
        instance = this;
        mainCamera = transform.GetComponent<Camera>();
        mainCamera.orthographicSize = 8; // *visible confusion*
        playerMissingImage = playerMissingIndicator.GetComponent<RectTransform>();
        playerMissingArrow = playerMissingIndicator.transform.Find("Arrow").GetComponent<RectTransform>();
    }

    private void Update()
    {
        LogInput();
        MoveCamera();
    }

    private void LogInput()
    {
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);
        bool up = Input.GetKey(KeyCode.W);
        bool down = Input.GetKey(KeyCode.S);
        float vecX = 0f;
        float vecY = 0f;

        if (left)
            vecX -= 1f;
        if (right)
            vecX += 1f;
        if (up)
            vecY += 1f;
        if (down)
            vecY -= 1f;

        moveDir = new Vector2(vecX, vecY);
        
    }

    private void MoveCamera()
    {
        if (moveDir == Vector3.zero)
            return;

        float relSpeed = cameraSpeed * Time.deltaTime;
        Vector3 newPos = mainCamera.transform.position + moveDir * relSpeed;
        if (CanMove(newPos))
        {
            mainCamera.transform.position = newPos;
            Vector3Int roundedPos = PlayerController.instance.playerPos;
            Vector2 worldPos = new Vector2(roundedPos.x + 0.5f, roundedPos.y + 0.5f);
            Vector2 playerPos = mainCamera.WorldToScreenPoint(new Vector2(worldPos.x, worldPos.y));

            // Right now cameraPos is not the bottom/left corner from which we start drawing our rect. Let's fix that.
            Vector2 cameraPos = mainCamera.ViewportToScreenPoint(Vector2.zero);
            Rect r = new Rect(cameraPos, mainCamera.ViewportToScreenPoint(Vector2.one));

            Debug.DrawLine(new Vector3(r.xMin, r.yMin, 10f), new Vector3(r.xMin, r.yMax, 10f));

            if (!r.Contains(playerPos))
            {
                if (!playerMissingIndicator.activeSelf)
                    playerMissingIndicator.SetActive(true);
                ShowIndicator(playerPos, r, worldPos);
            }
            else if (playerMissingIndicator.activeSelf)
                playerMissingIndicator.SetActive(false);
        }
    }

    private bool CanMove(Vector3 pos)
    {
        if (pos.x > mapBounds.xMax || pos.x < mapBounds.xMin || pos.y > mapBounds.yMax || pos.y < mapBounds.yMin)
            return false;
        return true;
    }

    private void ShowIndicator(Vector2 playerPos, Rect cameraRect, Vector2 worldPos)
    {
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(worldPos);
        Vector2 anchorPos = Vector2.zero;
        Vector2 arrowPos = Vector2.zero;
        Vector3 arrowRot = Vector3.zero;

        if (playerPos.y < cameraRect.yMax - indicatorScreenOffsetTop && playerPos.y > cameraRect.yMin + indicatorScreenOffset)
        {
            if (playerPos.x > cameraRect.xMax - indicatorScreenOffset)
            {
                // Player is to the right of the camera.
                anchorPos = new Vector2(Screen.width - indicatorScreenOffset, screenPoint.y);
                arrowPos = new Vector2(30f, 0);
                arrowRot = new Vector3(0, 0, -90f);
            }
            else if (playerPos.x < cameraRect.xMin + indicatorScreenOffset)
            {
                // Player is to the left of the camera.
                anchorPos = new Vector2(indicatorScreenOffset, screenPoint.y);
                arrowPos = new Vector2(-40f, 0);
                arrowRot = new Vector3(0, 0, 90f);
            }
        }
        else if (playerPos.x < cameraRect.xMax - indicatorScreenOffset && playerPos.x > cameraRect.xMin + indicatorScreenOffset)
        {
            if (playerPos.y > cameraRect.yMax - indicatorScreenOffsetTop)
            {
                // Player is to the top of the camera.
                anchorPos = new Vector2(screenPoint.x, Screen.height - indicatorScreenOffsetTop);
                arrowPos = new Vector2(0, 40f);
                arrowRot = Vector3.zero;
            }
            else if (playerPos.y < cameraRect.yMin + indicatorScreenOffset)
            {
                // Player is to the bottom of the camera.
                anchorPos = new Vector2(screenPoint.x, indicatorScreenOffset);
                arrowPos = new Vector2(0, -40f);
                arrowRot = new Vector3(0, 0, 180f);
            }
        }
        else if (playerPos.x > cameraRect.xMax - indicatorScreenOffset && playerPos.y > cameraRect.yMax - indicatorScreenOffsetTop)
        {
            // Player is to the top-right of the camera.
            anchorPos = new Vector2(Screen.width - indicatorScreenOffset, Screen.height - indicatorScreenOffsetTop);
            arrowPos = new Vector2(30f, 30f);
            arrowRot = new Vector3(0, 0, -45f);
        }
        else if (playerPos.x > cameraRect.xMax - indicatorScreenOffset && playerPos.y < cameraRect.yMin + indicatorScreenOffset)
        {
            // Player is to the bottom-right of the camera.
            anchorPos = new Vector2(Screen.width - indicatorScreenOffset, indicatorScreenOffset);
            arrowPos = new Vector2(30f, -30f);
            arrowRot = new Vector3(0, 0, -135f);
        }
        else if (playerPos.x < cameraRect.xMin + indicatorScreenOffset && playerPos.y > cameraRect.yMax - indicatorScreenOffsetTop)
        {
            // Player is to the top-left of the camera.
            anchorPos = new Vector2(indicatorScreenOffset, Screen.height - indicatorScreenOffsetTop);
            arrowPos = new Vector2(-40f, 40f);
            arrowRot = new Vector3(0, 0, 45f);
        }
        else if (playerPos.x < cameraRect.xMin + indicatorScreenOffset && playerPos.y < cameraRect.yMin + indicatorScreenOffset)
        {
            // Player is to the bottom-left of the camera.
            anchorPos = new Vector2(indicatorScreenOffset, indicatorScreenOffset);
            arrowPos = new Vector2(-40f, -40f);
            arrowRot = new Vector3(0, 0, 135f);
        }

        playerMissingImage.anchoredPosition = anchorPos;
        playerMissingArrow.anchoredPosition = arrowPos;
        playerMissingArrow.eulerAngles = arrowRot;
    }


    public void CameraShake(float angle, float magnitude)
    {
        Vector3 startPos = transform.position;

        angle *= Mathf.Deg2Rad;
        float x = startPos.x;
        float y = startPos.y;
        x += magnitude * Mathf.Cos(angle);
        y += magnitude * Mathf.Sin(angle);
        
        Vector3 endPos = new Vector3(x, y, startPos.z);
        StartCoroutine(Shake(startPos, endPos));
    }

    private IEnumerator Shake(Vector3 startPos, Vector3 endPos)
    {
        float timer = 0f;
        AnimationCurve curveRef = CombatManager.instance.cameraShakeSpeedCurve;
        while(timer <= 1f)
        {
            if(timer > 0.5f)
                transform.position = Vector3.Lerp(endPos, startPos, timer);
            else
                transform.position = Vector3.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * curveRef.Evaluate(timer);

            if(timer >= 1f)
            {
                transform.position = startPos;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        float distX = maxX - minX;
        float distY = maxY - minY;
        mapBounds = new Rect(minX, minY, distX, distY);
    }

    public bool IsPointInsideMap(Vector3 pos)
    {
        if (mapBounds.Contains(pos))
            return true;
        return false;
    }
}
