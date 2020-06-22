using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIActionPanel : MonoBehaviour
{
    public static UIActionPanel instance;

    [SerializeField] private AnimationCurve logAnimationSpeedCurve = default;

    private Button skipButton;
    private Button waitButton;
    private Button consoleButton;
    private RectTransform consoleArrowRect;
    private RectTransform logBaseRect;
    private RectTransform thisRect;

    private bool consoleState = false;

    private void Awake()
    {
        instance = this;
        skipButton = transform.Find("SkipButtonBase").GetChild(0).GetComponent<Button>();
        waitButton = transform.Find("WaitButtonBase").GetChild(0).GetComponent<Button>();
        consoleButton = transform.Find("ConsoleArrowBase").GetChild(0).GetComponent<Button>();
        consoleArrowRect = consoleButton.transform.GetChild(0).GetComponent<RectTransform>();
        logBaseRect = transform.Find("LogBase").GetComponent<RectTransform>();
        thisRect = transform.GetComponent<RectTransform>();
    }

    public void EnableDisableButtons(bool enable)
    {
        skipButton.interactable = enable;
        waitButton.interactable = enable;
    }

    public void SkipOnClick()
    {
        UIManager.instance.DeselectButton();

        if(!GameStateManager.instance.CheckState("COMBAT"))
            return;
        
        CombatManager.instance.movementTilemap.ClearAllTiles();
        CombatManager.instance.NextTurn();
    }

    public void WaitOnClick()
    {
        if(!GameStateManager.instance.CheckState("COMBAT"))
            return;

        UILog.instance.NewLogEntry("Non existent functionality! :(");
    }

    public void ConsoleOnClick()
    {
        consoleButton.interactable = false;
        consoleState = !consoleState;

        float endX = thisRect.anchoredPosition.x;
        float endY = 0;
        if(consoleState)
        {
            endY = thisRect.anchoredPosition.y + logBaseRect.rect.height;
            consoleArrowRect.rotation = new Quaternion(0, 0, 180f, 0);
        }
        else
        {
            endY = thisRect.anchoredPosition.y - logBaseRect.rect.height;
            consoleArrowRect.rotation = new Quaternion(0, 0, 0, 0);
        }

        Vector2 startPos = thisRect.anchoredPosition;
        Vector2 endPos = new Vector2(endX, endY);

        StartCoroutine(RiseLowerLog(startPos, endPos));
    }

    private IEnumerator RiseLowerLog(Vector2 startPos, Vector2 endPos)
    {
        float timer = 0f;

        while(timer <= 1f)
        {
            timer += Time.deltaTime * logAnimationSpeedCurve.Evaluate(timer);
            
            thisRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer);

            if(timer >= 1f)
            {
                thisRect.anchoredPosition = endPos;
                consoleButton.interactable = true;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }
}
