using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.Settings;



public class GameManager : MonoBehaviour
{
    
    public int initialMoney;

    //grid object
    public Grid grid;


    //audio Variables
    public AudioSource audioSource;
    public AudioClip coins;
    public AudioClip message;
    public AudioClip dying;

    //texts and prefabs objects
    public Text moneyCounter, metaText;
    public int score = 0;
    public TextMeshProUGUI textMenu;
    public GameObject menu;
    public GameObject infomenu;
    public GameObject migrationInfo;
    public GameObject caterPrefab, cicadPrefab, lousePrefab, crickPrefab;
    public GameObject beetlPrefab, ladybPrefab;
    public GameObject cornPrefab, tomatPrefab, kalePrefab, grassPrefab;
    public string plantName = "name";
    
    //create a new localizedString to the text
    public LocalizedString localizedtext;
   

    // User related variables
    private int money,maxMoney;
    private enum buttons {grass, kale, corn, tomato, beetle, ladyb, scythe, net, none};
    private GameObject[] prefabs, plaguesPrefabs;
    private buttons selectedButton;
    private bool canClick, canMeta = true;
    private int timeToMeta, timeToMetaLimit = 30;
    private float timeToPenalty;
    

    //level variable
    private int level;

    //history variables
    private int[] totalPlants;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        audioSource = GetComponent<AudioSource>();
        Debug.Log(LocalizationSettings.SelectedLocale.name);
        Input.simulateMouseWithTouches = true;
        //start array of totalPlants harvested
        totalPlants = new int[4];


        //array with buttons options
        prefabs = new GameObject[6];
        prefabs[(int) buttons.grass] = grassPrefab;
        prefabs[(int) buttons.kale] = kalePrefab;
        prefabs[(int) buttons.corn] = cornPrefab;
        prefabs[(int) buttons.tomato] = tomatPrefab;
        prefabs[(int) buttons.beetle] = beetlPrefab;
        prefabs[(int) buttons.ladyb] = ladybPrefab;

        //array with plagues options
        plaguesPrefabs = new GameObject[4];
        plaguesPrefabs[0] = caterPrefab;
        plaguesPrefabs[1] = cicadPrefab;
        plaguesPrefabs[2] = lousePrefab;
        plaguesPrefabs[3] = crickPrefab;

        //start with the initial money
        money = initialMoney;
        moneyCounter.text = money.ToString();
        maxMoney = money;
        
        //start selectedbutton
        selectedButton = buttons.none;
        //start varial for click control
        canClick = false;

        //initial level
        level = 0;

        //start meta Time, penalty Time;
        timeToMeta = 0;
        timeToPenalty = 0;

        //call migration coroutine
        StartCoroutine(Migration(30));

        //call start timeMeta coroutine
        StartCoroutine(TimeMeta());
    }

    // Update is called once per frame
    void Update() {


        timeToPenalty += Time.deltaTime;

        //check click on game field after selected a button
        if (Input.GetMouseButtonDown(0) && canClick && selectedButton != buttons.none) {
            //Touch touch = Input.GetTouch(0);
            //get click position
            Vector3 place = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Vector3 place = Camera.main.ScreenToWorldPoint(touch.position);

            //click on plant or predator button
            if (selectedButton <= buttons.ladyb) {
                place.z = 0;

                Vector3Int placeInt = grid.WorldToCell(place);
                place = grid.GetCellCenterWorld(placeInt);

                //get mouse position and check grid position object
                Vector2 gridpos = new Vector2(place.x, place.y);
                RaycastHit2D hit = Physics2D.Raycast(gridpos, Vector2.zero,Mathf.Infinity,1 << LayerMask.NameToLayer("Plants"));
                
                //check if have no plant in place
                if (hit.collider == null)
                {
                    ToPlace(place);
                }
                
                
            }
            //scythe or net buttons selected
            else {

                //get mouse position and check click on objects
                Vector2 mousePos = new Vector2(place.x, place.y);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero,Mathf.Infinity, (1 << LayerMask.NameToLayer("Insects") | (1 << LayerMask.NameToLayer("Plants"))));
                if (hit.collider != null) {
                    
                    if (hit.collider.gameObject.transform.parent.gameObject)
                    {
                        GameObject obj = hit.collider.gameObject.transform.parent.gameObject;
                        
                        //if button scythe, check click on plants
                        if (selectedButton == buttons.scythe && obj.GetComponent<Plant>() != null && obj.GetComponent<Plant>().colectable == true)
                        {
                            
                            EarnMoney(obj.GetComponent<Plant>().profit);

                            //register total of plants for each type
                            if (obj.GetComponent<Plant>().tag == "grass") totalPlants[0] += 1;
                            else if (obj.GetComponent<Plant>().tag == "kale") totalPlants[1] += 1;
                            else if (obj.GetComponent<Plant>().tag == "corn") totalPlants[2] += 1;
                            else if (obj.GetComponent<Plant>().tag == "tomato") totalPlants[3] += 1;

                            Destroy(obj, 0);
                            selectedButton = buttons.none;
                        }
                        //if button net, check click on insects
                        //if (selectedButton == buttons.net && obj.GetComponent<Insect>() != null)
                        if (selectedButton == buttons.net && (obj.gameObject.CompareTag("beetle")|| obj.gameObject.CompareTag("ladybug")))    
                        {
                            SpendMoney(obj.GetComponent<Insect>().collectCost);
                            
                            selectedButton = buttons.none;
                            audioSource.PlayOneShot(dying);
                            Destroy(obj);
                        }
                    }
                    //GameObject obj = hit.collider.gameObject.transform.parent.gameObject;
                   

                    
                }
            }
        }

        //check game over condition
        if (timeToPenalty >= 30)
        {
            CheckPenalty();
        }
        if (money < 50 && !FindObjectOfType<Plant>())
        {
            SceneManager.LoadScene("GameOver");
        }
        if (money < 0)
        {
            SceneManager.LoadScene("GameOver");
        }
        
    }

    //method to place plants or predators
    public void ToPlace(Vector3 place)
    {
        GameObject obj = Instantiate(prefabs[(int)selectedButton], place, Quaternion.identity);

        // Plant button
        if (selectedButton <= buttons.tomato)
        {
            //spend money to buy the object
            SpendMoney(prefabs[(int)selectedButton].GetComponent<Plant>().cost);
            timeToMeta = 0;

        }
        // Insect button
        else
        {
            //spend money to buy the object
            SpendMoney(prefabs[(int)selectedButton].GetComponent<Insect>().cost);
        }
        selectedButton = buttons.none;
    }

    //method to increase money
    public void EarnMoney(int value)
    {
        // Get our GlobalVariablesSource
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
        var gScore = source["global"]["score"] as UnityEngine.Localization.SmartFormat.PersistentVariables.IntVariable;
        audioSource.PlayOneShot(coins);
        money += value;
        moneyCounter.text = money.ToString();
        if (maxMoney < money)
        {
            maxMoney = money;

            //global score update
            gScore.Value = maxMoney;
        }

        //check money level
        if (canMeta)
        {
            int actuallevel = (int)money / 1000;
            if (actuallevel > level)
            {
                level = actuallevel;
                canMeta = false;
                StartCoroutine(Meta());
            }
        }
        

        // TODO: loosing money animation
    }

    //method to decrease money
    public void SpendMoney(int value)
    {
        money -= value;
        moneyCounter.text = money.ToString();
        // TODO: earning money animation
    }

    //method to set grass button on selected button
    public void PlantGrass()
    {
        if(grassPrefab.GetComponent<Plant>().cost > money)
        {
            return;
        }

        if(selectedButton != buttons.grass)
        {
            selectedButton = buttons.grass;
        }
        else
        {
            selectedButton = buttons.none;
        }
        // TODO: follow mouse
    }

    //method to set kale button on selected button
    public void PlantKale()
    {
        if(kalePrefab.GetComponent<Plant>().cost > money)
        {
            return;
        }
        
        if(selectedButton != buttons.kale)
        {
            selectedButton = buttons.kale;
        }
        else
        {
            selectedButton = buttons.none;
        }
        // TODO: follow mouse
    }

    //method to set corn button on selected button
    public void PlantCorn()
    {
        if(cornPrefab.GetComponent<Plant>().cost > money)
        {
            return;
        }
        
        if(selectedButton != buttons.corn)
        {
            selectedButton = buttons.corn;
        }
        else
        {
            selectedButton = buttons.none;
        }
        // TODO: follow mouse
    }

    //method to set tomato button on selected button
    public void PlantTomato()
    {
        if(tomatPrefab.GetComponent<Plant>().cost > money)
        {
            return;
        }

        if(selectedButton != buttons.tomato)
        {
            selectedButton = buttons.tomato;
        }
        else
        {
            selectedButton = buttons.none;
        }
        // TODO: follow mouse
    }

    //method to set scythe button on selected button
    public void Harvest()
    {
        if(selectedButton != buttons.scythe)
        {
            selectedButton = buttons.scythe;
        }
        else
        {
            selectedButton = buttons.none;
        }
        // TODO: follow mouse
    }

    //method to set beetle button on selected button
    public void BuyBeetle()
    {
        if(beetlPrefab.GetComponent<Insect>().cost > money)
        {
            return;
        }

        if(selectedButton != buttons.beetle)
        {
            selectedButton = buttons.beetle;
        }
        else
        {
            selectedButton = buttons.none;
        }
        // TODO: follow mouse
    }

    //method to set ladybug button on selected button
    public void BuyLadybug()
    {
        if(ladybPrefab.GetComponent<Insect>().cost > money)
        {
            return;
        }

        if(selectedButton != buttons.ladyb)
        {
            selectedButton = buttons.ladyb;
        }
        else
        {
            selectedButton = buttons.none;
        }
        // TODO: follow mouse
    }

    //method to set net button on selected button
    public void Collect()
    {
        if(beetlPrefab.GetComponent<Insect>().collectCost > money)
        {
            return;
        }
        if(ladybPrefab.GetComponent<Insect>().collectCost > money)
        {
            return;
        }

        if(selectedButton != buttons.net)
        {
            selectedButton = buttons.net;
        }
        else
        {
            selectedButton = buttons.none;
        }
    }

    //method to open menu button
    public void OpenMenu()
    {
        menu.SetActive(true);
        PauseGame();
    }

    //method to open infomenu
    public void OpenInfoMenu()
    {
        infomenu.SetActive(true);
        audioSource.PlayOneShot(message);
        PauseGame();
    }

    //method to close infomenu button
    public void CloseInfoMenu()
    {
        infomenu.SetActive(false);
        ResumeGame();
    }

    //method to close menu button
    public void CloseMenu()
    {
        ResumeGame();
        menu.SetActive(false);
    }

    //method to disable clicks
    public void DisableClick()
    {
        canClick = false;
    }

    //method to enable clicks
    public void EnableClick()
    {
        canClick = true;
    }

    //method to start button on main menu scene
    public void BtnStart()
    {
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
        var gScore = source["global"]["score"] as UnityEngine.Localization.SmartFormat.PersistentVariables.IntVariable;
        gScore.Value = 0;
        SceneManager.LoadScene("Game");
    }

    //method to rank button on main menu scene
    public void BtnRank()
    {

    }

    //method to exit button on main menu scene
    public void BtnExit()
    {
        Application.Quit();
    }

    //method to return button on game over scene
    public void BackButton()
    {
        SceneManager.LoadScene("Start");
    }

    //coroutine for migration; Each 30 seconds occurs a new migration
    IEnumerator Migration(float time)
    {
        yield return new WaitForSeconds(time);
        Migrate();
        StartCoroutine(Migration(30));

    }

    // function to call goal if player idle for 30 seconds
    IEnumerator TimeMeta()
    {
        if ((timeToMeta >= timeToMetaLimit) && canMeta)
        { 
            if (level == 0)
            {
                level = 1;
            }
            canMeta = false;
            timeToMeta = 0;
            yield return Meta();
        }
        
        timeToMeta += 1;
        yield return new WaitForSeconds(1);
        //Debug.Log(timeToMeta);
        StartCoroutine(TimeMeta());
        
        
    }

    //coroutine to check a goal trigger. New goals are launched every 1000 points
    IEnumerator Meta()
    {
       
        // Get our GlobalVariablesSource
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
        
        // Get the specific meta global variables mPlant,mTime and mNumber
        var mPlant = source["global"]["mPlant"] as UnityEngine.Localization.SmartFormat.PersistentVariables.StringVariable;
        var mTime = source["global"]["mTime"] as UnityEngine.Localization.SmartFormat.PersistentVariables.IntVariable;
        var mNumber = source["global"]["mNumber"] as UnityEngine.Localization.SmartFormat.PersistentVariables.IntVariable;
        Debug.Log(mPlant);

    
        // random plant
        int plant = Random.Range(0, 4);
        
        

        //generate text of the meta
        plantName = prefabs[plant].tag;
        mPlant.Value = plantName;
        
        //verify localization to translate the variable 
        if (LocalizationSettings.SelectedLocale.LocaleName == "Portuguese (Brazil) (pt-BR)")
        {
            if (plantName == "kale")
            {
                mPlant.Value = "couves";
            }
            else if (plantName == "grass")
            {
                mPlant.Value = "gramas";
            }
            else if (plantName == "corn")
            {
                mPlant.Value = "milhos";
            }
            else if (plantName == "tomato")
            {
                mPlant.Value = "tomates";
            }
        }

        int mPlants = 0;
        
        //make the migration based on the insect's prey
        foreach (GameObject insect in plaguesPrefabs)
        {

            if (insect.GetComponent<Insect>().prey == plantName)
            {
                Migrate(System.Array.IndexOf(plaguesPrefabs, insect));
            }
        }

        //define total plant to the meta and put on global Variable
        mPlants = level * 3;
        mNumber.Value = mPlants;
        Debug.Log(mNumber);

        //set the table reference for text of meta
        localizedtext.SetReference("UItext", "meta");
        textMenu.text = localizedtext.GetLocalizedString();


        // meta variables
        timeToMeta = 0;
        int metaTimeLimit = level*20;
        int metaTime = metaTimeLimit;
        int actualplants = totalPlants[plant];
        //Debug.Log(actualplants);
        mTime.Value = metaTimeLimit;

        OpenInfoMenu();

        // loop of count metaTime
        for (int i = 0;i < metaTimeLimit; i++)
        {
        
            metaText.text = "time left: "+metaTime;
            metaTime -= 1;
            
            yield return new WaitForSeconds(1);
        }
        int harvestsMade = totalPlants[plant] - actualplants;

        //check result of meta
        Debug.Log(harvestsMade);
        if (harvestsMade >= mPlants)
        {
            localizedtext.SetReference("UItext", "archieveMeta");
            textMenu.text = localizedtext.GetLocalizedString(localizedtext, "archieveMeta");
            OpenInfoMenu();
            EarnMoney(250);
        }
        else
        {

            localizedtext.SetReference("UItext", "failMeta");
            textMenu.text = localizedtext.GetLocalizedString(localizedtext, "failMeta");
            OpenInfoMenu();
            SpendMoney(300);
        }
        canMeta = true;
        metaText.text = "";
    }

    //migrate method. Instantiate a random quantity of a random plague
    public void Migrate(int plague = 4)
    {
        //random plague
        if (plague == 4)
        {
            plague = Random.Range(0, 4);
        }
        

        for (int i = 0; i < Random.Range(3, 6); i++)
        {
            Vector3 place = new Vector3(Random.Range(-12, 12), Random.Range(-3, 3), 0);
            GameObject obj = Instantiate(plaguesPrefabs[plague], place, Quaternion.identity);
        }
    }

    //check if have more then 4 predators on game at same time
    public void CheckPenalty()
    {
        // Get our GlobalVariablesSource
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
        var gFine = source["global"]["gFine"] as UnityEngine.Localization.SmartFormat.PersistentVariables.IntVariable;

        GameObject[] beetles,ladyBugs;
        beetles = GameObject.FindGameObjectsWithTag("beetle");
        ladyBugs = GameObject.FindGameObjectsWithTag("ladybug");
        int p = beetles.Length/2 + ladyBugs.Length/2;
        if (p >= 5)
        {
            gFine.Value = p * 60; 
            SpendMoney(p*60);
           
            localizedtext.SetReference("UItext", "limitP");
            textMenu.text = localizedtext.GetLocalizedString(localizedtext,"limitP");
            
           


            timeToPenalty = 0;
            OpenInfoMenu();
            
        }
    }
    
    //method to pause the game
    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    //method to resume the game
    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    //method to mute de game
    public void MuteToogle(bool muted)
    {
        if (muted)
        {
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = 1;
        }
    }

    //method to quit scene Button
    public void BtnQuit()
    {
        SceneManager.LoadScene("GameOver");
    }
    
    //method to Open the MigrationInfo
    IEnumerator OpenMigrationInfo()
    {
        migrationInfo.SetActive(true);
        audioSource.PlayOneShot(message);
        yield return new WaitForSeconds(3);
        migrationInfo.SetActive(false);

    }
}
