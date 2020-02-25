using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D moveCursor;
    [SerializeField] private Texture2D castCursor;

    private Camera mainCamera;

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
        Cursor.visible = false;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        Cursor.visible = false;
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
