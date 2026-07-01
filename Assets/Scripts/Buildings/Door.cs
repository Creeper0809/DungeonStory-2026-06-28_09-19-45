using UnityEngine;

public class Door : BuildableObject
{
    public override void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        base.Initialization(buildingSO, buildPos);
        GetComponent<BoxCollider2D>().size = new Vector2(3,1);
        GetComponent<BoxCollider2D>().offset = new Vector2(2, 0.5f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Character"))
        {
            Character character = collision.GetComponent<Character>();
            if (character != null)
            {
                character.ChangeLayer("OutsideObject");
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Character"))
        {
            Character character = collision.GetComponent<Character>();
            if (character != null)
            {
                character.ChangeLayer("Default");
            }
        }
    }
}
