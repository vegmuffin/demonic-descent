using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    [HideInInspector] public bool inUse = false;

    [SerializeField] private Sprite defaultCursor = default;
    private Vector2 defaultPivot;
    [SerializeField] private Sprite attackCursor = default;
    private Vector2 attackPivot;
    [SerializeField] private Sprite moveCursor = default;
    private Vector2 movePivot;
    [SerializeField] private Sprite castCursor = default;
    private Vector2 castPivot;
    [SerializeField] private Sprite cannotCursor = default;
    private Vector2 cannotPivot;
    [SerializeField] private Sprite pickupCursor = default;
    private Vector2 pickupPivot;
    [SerializeField] private Sprite shootCursor = default;
    private Vector2 shootPivot;

    private Camera mainCamera;
    private Image realCursor;
    private RectTransform rectTransform;
    [HideInInspector] public Transform combatPointsIndicator;
    private TMP_Text combatPointsIndicatorTMP;
    private RectTransform combatPointsIndicatorRect;
    private RectTransform cursorRect;
    private Vector2 baseAnchor;
    private int showingPoints;

    public enum CursorStates
    {
        DEFAULT,
        ATTACK,
        MOVE,
        CAST,
        CANNOT,
        PICKUP,
        SHOOT
    }

    public CursorStates currentState;

    private void Awake()
    {
        instance = this;
        currentState = CursorStates.DEFAULT;
        mainCamera = Camera.main;
        Transform child = transform.Find("CursorManager");
        realCursor = child.GetComponent<Image>();
        rectTransform = transform.GetComponent<RectTransform>();
        cursorRect = child.GetComponent<RectTransform>();
        combatPointsIndicator = transform.Find("CPNumber");
        combatPointsIndicatorTMP = combatPointsIndicator.GetComponent<TMP_Text>();
        combatPointsIndicatorRect = combatPointsIndicator.GetComponent<RectTransform>();
        baseAnchor = combatPointsIndicatorRect.anchoredPosition;

        defaultPivot = new Vector2(0.05f, 0.95f);
        attackPivot = new Vector2(0.9f, 0.5f);
        movePivot = new Vector2(0.05f, 0.95f);
        castPivot = new Vector2(0.25f, 0.75f);
        cannotPivot = new Vector2(0.05f, 0.95f);
        pickupPivot = new Vector2(0.5f, 0.75f);
        shootPivot = Vector2.zero;
    }

    private void Start()
    {
        Cursor.visible = false;
        realCursor.sprite = defaultCursor;
    }

    private void Update()
    {
        UpdateCursor();
        UpdateHoveringPoints();
    }

    public bool CheckState(string state)
    {
        if(state == "DEFAULT" && currentState != CursorStates.DEFAULT)
            return false;
        else if(state == "ATTACK" && currentState != CursorStates.ATTACK)
            return false;
        else if(state == "CAST" && currentState != CursorStates.CAST)
            return false;
        else if(state == "MOVE" && currentState != CursorStates.MOVE)
            return false;
        else if(state == "CANNOT" && currentState != CursorStates.CANNOT)
            return false;
        else if(state == "PICKUP" && currentState != CursorStates.PICKUP)
            return false;
        else if(state == "SHOOT" && currentState != CursorStates.SHOOT)
            return false;

        // Current state is the one we are checking for if we have reached this far.
        return true;
    }

    public Vector3Int GetTileBelowCursor(ref Vector3 precisePos)
    {
        precisePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.z));
        Vector2 offsetPos = new Vector2(precisePos.x - 1f, precisePos.y - 1f);

        int tileX = Mathf.CeilToInt(offsetPos.x);
        int tileY = Mathf.CeilToInt(offsetPos.y);
        Vector3Int tilePos = new Vector3Int(tileX, tileY, 0);

        return tilePos;
    }

    private void UpdateCursor()
    {
        Cursor.visible = false;
        rectTransform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
    }

    public void SetCursor(string whichCursor, string dir)
    {
        if(whichCursor == "DEFAULT")
        {
            realCursor.sprite = defaultCursor;
            cursorRect.pivot = defaultPivot;
            cursorRect.eulerAngles = Vector3.zero;
            currentState = CursorStates.DEFAULT;

            combatPointsIndicatorRect.anchoredPosition = baseAnchor;
        } else if(whichCursor == "ATTACK")
        {
            realCursor.sprite = attackCursor;
            cursorRect.pivot = attackPivot;
            currentState = CursorStates.ATTACK;
            if(realCursor.sprite != defaultCursor)
            {
                switch(dir)
                {
                    case "bottom":
                        cursorRect.eulerAngles = new Vector3(0, 0, 90f);
                        combatPointsIndicatorRect.anchoredPosition = new Vector2(baseAnchor.x + 15f, baseAnchor.y);
                        break;
                    case "left":
                        cursorRect.eulerAngles = Vector3.zero;
                        combatPointsIndicatorRect.anchoredPosition = new Vector2(baseAnchor.x - 30f, baseAnchor.y + 15f);
                        break;
                    case "top":
                        cursorRect.eulerAngles = new Vector3(0, 0, -90f);
                        combatPointsIndicatorRect.anchoredPosition = new Vector2(baseAnchor.x - 15f, baseAnchor.y + 50f);
                        break;
                    case "right":
                        cursorRect.eulerAngles = new Vector3(0, 0, 180f);
                        combatPointsIndicatorRect.anchoredPosition = new Vector2(baseAnchor.x + 33f, baseAnchor.y + 14f);
                        break;
                }
            }
        } else if(whichCursor == "MOVE")
        {
            realCursor.sprite = moveCursor;
            cursorRect.pivot = movePivot;
            cursorRect.eulerAngles = Vector3.zero;
            currentState = CursorStates.MOVE;

            combatPointsIndicatorRect.anchoredPosition = baseAnchor;
        } else if(whichCursor == "CAST")
        {
            realCursor.sprite = castCursor;
            cursorRect.pivot = castPivot;
            cursorRect.eulerAngles = Vector3.zero;
            currentState = CursorStates.CAST;

            combatPointsIndicatorRect.anchoredPosition = baseAnchor;
        } else if(whichCursor == "CANNOT")
        {
            realCursor.sprite = cannotCursor;
            cursorRect.pivot = cannotPivot;
            cursorRect.eulerAngles = Vector3.zero;
            currentState = CursorStates.CANNOT;
            
            combatPointsIndicatorRect.anchoredPosition = baseAnchor;
        } else if(whichCursor == "PICKUP")
        {
            realCursor.sprite = pickupCursor;
            cursorRect.pivot = pickupPivot;
            cursorRect.eulerAngles = Vector3.zero;
            currentState = CursorStates.PICKUP;

            combatPointsIndicatorRect.anchoredPosition = baseAnchor;
        } else if(whichCursor == "SHOOT")
        {
            realCursor.sprite = shootCursor;
            cursorRect.pivot = shootPivot;
            cursorRect.eulerAngles = Vector3.zero;
            currentState = CursorStates.SHOOT;

            combatPointsIndicatorRect.anchoredPosition = baseAnchor;
        }
    }

    private void UpdateHoveringPoints()
    {
        if(GameStateManager.instance.CheckState("COMBAT") && !CombatManager.instance.initiatingCombatState)
        {
            if(CombatManager.instance.whoseTurn == "Player")
            {
                int hoveringCombatPoints = CombatManager.instance.currentUnit.hoveringCombatPoints;
                if(hoveringCombatPoints != showingPoints)
                {
                    combatPointsIndicatorTMP.text = hoveringCombatPoints.ToString();
                    showingPoints = hoveringCombatPoints;
                }
            }
        }
    }

    public void DisableHoveringPoints()
    {
        combatPointsIndicator.gameObject.SetActive(false);
    }

    public void EnableHoveringPoints()
    {
        combatPointsIndicator.gameObject.SetActive(true);
    }

    public string GetMouseEnemyTriangle(Bounds enemyBounds, Vector2 mousePos)
    {
        float xMin = enemyBounds.min.x;
        float xMax = enemyBounds.max.x;
        float yMin = enemyBounds.min.y;
        float yMax = enemyBounds.max.y;

        Vector2 center = enemyBounds.center;
        Vector2 bottomLeft = new Vector2(xMin, yMin);
        Vector2 topleft = new Vector2(xMin, yMax);
        Vector2 topRight = new Vector2(xMax, yMax);
        Vector2 bottomRight = new Vector2(xMax, yMin);

        // Bottom triangle
        if(IsPointInTriangle(mousePos, center, bottomLeft, bottomRight))
            return "bottom";

        // Left triangle
        if(IsPointInTriangle(mousePos, center, bottomLeft, topleft))
            return "left";

        // Top triangle
        if(IsPointInTriangle(mousePos, center, topleft, topRight))
            return "top";

        // Right triangle
        if(IsPointInTriangle(mousePos, center, topRight, bottomRight))
            return "right";

        // Something went wrong if we got here.
        return "failure";
    }

    // I have no idea what the fuck is going on here, but it works. I didn't study much math to understand.
    private bool IsPointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
        var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

        if ((s < 0) != (t < 0))
            return false;

        var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

        return A < 0 ? (s <= 0 && s + t >= A) : (s >= 0 && s + t <= A);
    }
    
}
