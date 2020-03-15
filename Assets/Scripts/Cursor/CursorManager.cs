using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    [SerializeField] private Sprite defaultCursor = default;
    [SerializeField] private Sprite attackCursor = default;
    [SerializeField] private Sprite moveCursor = default;
    [SerializeField] private Sprite castCursor = default;

    private Camera mainCamera;
    private SpriteRenderer realCursor;

    public enum CursorStates
    {
        DEFAULT,
        ATTACK,
        MOVE,
        CAST
    }

    public CursorStates currentState;

    private void Awake()
    {
        instance = this;
        currentState = CursorStates.DEFAULT;
        mainCamera = Camera.main;
        realCursor = transform.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Cursor.visible = false;
        realCursor.sprite = defaultCursor;

    }

    private void Update()
    {
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        Cursor.visible = false;
        transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
    }

    public void SetCursor(CursorStates state)
    {
        currentState = state;
        //
    }
    
}
