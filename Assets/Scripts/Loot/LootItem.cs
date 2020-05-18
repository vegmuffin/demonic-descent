using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootItem : MonoBehaviour
{
    [HideInInspector] public bool landed = false;
    [SerializeField] private AnimationCurve flySpeedAnimationCurve = default;

    [HideInInspector] public int customValue;
    [SerializeField] private string description = default;

    private Collider2D col;
    private bool isHovered = false;

    private void Awake()
    {
        col = transform.GetComponent<Collider2D>();
    }

    private void OnMouseOver()
    {
        if(CursorManager.instance.inUse || !landed)
            return;

        CursorManager.instance.SetCursor("DEFAULT");
        LootManager.instance.ShowSingleItem(gameObject);

        isHovered = true;
    }

    private void OnMouseExit()
    {
        if(!landed)
            return;

        LootManager.instance.HideSingleItem(gameObject);

        isHovered = false;
    }

    private void OnMousePress()
    {
        if(Input.GetKey(KeyCode.Mouse0) && isHovered)
            PickUp();
    }

    public void PickUp()
    {
        if(!landed)
            return;
        
        if(gameObject.name.Contains("Potion"))
        {
            GameObject player = MovementManager.instance.player;
            var playerUnit = player.GetComponent<Unit>();
            if(playerUnit.health + customValue > playerUnit.maxHealth)
                playerUnit.health = playerUnit.maxHealth;
            else
                playerUnit.health += customValue;

            int queueIndex = 0;         
            if(GameStateManager.instance.CheckState("COMBAT"))
                queueIndex = CombatManager.instance.GetObjectIndex(player);
            
            UIManager.instance.HealthChange(queueIndex, playerUnit.health, true, true);
        }

        if(gameObject.name.Contains("Gold"))
        {
            LootManager.instance.goldCollected += customValue;
            UIManager.instance.UpdateGoldText(LootManager.instance.goldCollected);
        }

        LootManager.instance.lootDict[gameObject].GetComponent<UIComponent>().DestroyCheck();
        Destroy(LootManager.instance.lootDict[gameObject], 0.1f);
        LootManager.instance.lootDict.Remove(gameObject);
        Destroy(gameObject);
    }

    // Need to simulate height somehow here.
    public IEnumerator Fly(Vector2 startPos, Vector2 endPos)
    {
        float timer = 0f;
        while(timer <= 1f)
        {
            timer += Time.deltaTime * flySpeedAnimationCurve.Evaluate(timer);
            transform.position = Vector2.Lerp(startPos, endPos, timer);

            if(timer >= 1f)
            {
                landed = true;
                AddLootDescription();
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
        yield break;
    }

    private void AddLootDescription()
    {
        // Gold should have variation. Probably will expand on this later.
        int ran = Random.Range(-3, 4);
        customValue += ran;

        Vector2 descPos = new Vector2(transform.position.x, transform.position.y + LootManager.instance.yOffset);
        GameObject lootDescription = Instantiate(LootManager.instance.lootDescription, descPos, Quaternion.identity, LootManager.instance.worldSpaceCanvas);

        if(gameObject.name.Contains("Gold"))
        {
            description += " (" + customValue.ToString() + ")";
        }

        lootDescription.GetComponent<LootDescriptor>().SetValues(description, gameObject);
        
        if(!LootManager.instance.isSpacePressed)
            lootDescription.SetActive(false);

        LootManager.instance.lootDict.Add(gameObject, lootDescription);
    }
}
