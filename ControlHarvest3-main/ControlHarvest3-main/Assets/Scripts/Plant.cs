using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public int cost, profit, growthDelay, nutritionalValueMax;
    public int nutritionalValue;
    public bool colectable;
    public SpriteRenderer spriteRenderer;
    public Sprite[] spriteArray;

    // Start is called before the first frame update
    void Start() {
        colectable = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        //start the evolution routine
        StartCoroutine(evolution());
        
    }

    // Update is called once per frame
    void Update() {
    }
    //method to change de situation of the plant.
    IEnumerator evolution()
    {
        float time = growthDelay / 3;
        nutritionalValue = nutritionalValueMax / 4;
        spriteRenderer.sprite = spriteArray[0];

        yield return new WaitForSeconds(time);
        nutritionalValue += nutritionalValueMax / 4;
        spriteRenderer.sprite = spriteArray[1];

        yield return new WaitForSeconds(time);
        nutritionalValue += nutritionalValueMax / 4;
        spriteRenderer.sprite = spriteArray[2];

        yield return new WaitForSeconds(time);
        colectable = true;
        nutritionalValue = nutritionalValueMax;
        spriteRenderer.sprite = spriteArray[3];
    }
}
