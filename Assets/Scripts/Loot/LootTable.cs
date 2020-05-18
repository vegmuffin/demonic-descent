using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootTable : MonoBehaviour
{
    [SerializeField] private List<Loot> lootTable = new List<Loot>();

    public void DropLoot()
    {
        Loot lt = PickLoot();
        GameObject drop = lt.drop;
        
        // If 'l' is null, that means we didn't drop anything. :(
        if(!lt)
            return;

        GameObject item = Instantiate(drop, transform.position, Quaternion.identity);
        item.GetComponent<LootItem>().customValue = lt.customValue;

        float ranX = Random.Range(-0.25f, 0.25f);
        float ranY = Random.Range(-0.25f, 0.25f);

        Vector2 endPos = new Vector2(transform.position.x + ranX, transform.position.y + ranY);
        Vector2 startPos = new Vector2(transform.position.x + 0.5f, transform.position.y + 0.5f);
        
        var lootItem = item.GetComponent<LootItem>();
        StartCoroutine(lootItem.Fly(startPos, endPos));
    }

    private Loot PickLoot()
    {
        // Keep randomizing through the loot table until we succeed in the chance to drop.
        for(int i = 0; i < lootTable.Count; ++i)
        {
            int ran = Random.Range(0, 100);
            if(ran < lootTable[i].chanceToDrop)
            {
                return lootTable[i];
            }
            else
            {
                continue;
            }
        }

        // If we have reached here, we failed to succeed to drop any loot.
        return null;
    }
}
