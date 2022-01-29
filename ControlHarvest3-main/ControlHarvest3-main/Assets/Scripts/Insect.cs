using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Insect : MonoBehaviour, AgentFactory
{


    // generate ID to Micelio
    private string id_agent = Agent.GenerateAgentID();


    private Rigidbody2D body;
    private float delta;
    private float energy;
    private SpriteRenderer spriteRenderer;

    public int cost, collectCost, nutritionalValue;
    public float energyMax;

    //sound attributes
    public AudioClip dying;
    public AudioClip bite;
    public AudioClip reproduce;
    public AudioSource audioSource;

    // Movement attributes
    public float moveMaxAngle, moveMaxSpeed, moveFrequency;
    public float actualSpeed;
    public int forageTax;
    private bool canWalk = true;

    // Praying attributes
    public string prey;
    public float preyDelay;
    private bool canPrey = true;

    // Reproduction attributes
    public float reproduceDelay, reproduceRate;
    private bool canReproduce = false;


    // Start is called before the first frame update
    void Start()
    {
        //start settings for SpriteRender
        spriteRenderer = GetComponent<SpriteRenderer>();
        //start settings for rigidbody
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        delta = 0f;
        //start movement
        actualSpeed = moveMaxSpeed;
        body.velocity = new Vector2(moveMaxSpeed, 0);
        //call change direction method
        ChangeDirection();
        //start energy
        energy = energyMax;

        //start canReproduce
        StartCoroutine(Reproducestart());

        //Energy coroutine
        StartCoroutine(Energyloss(1));

    }


    // Update is called once per frame
    void Update()
    {
        //check change direction
        delta += Time.deltaTime;
        if (delta >= moveFrequency)
        {
            ChangeDirection();

            delta = 0f;
        }
        // Face the movement direction
        transform.up = body.velocity.normalized;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cone of view trigger
        if (other.gameObject.layer == 11 || other.gameObject.layer == 9)
        {
            View(other);
        }
        // Default object trigger
        else
        {
            // Collision with prey tag
            string[] preys = prey.Split(',');
            foreach (string p in prey.Split(','))
            {
                //if it collides with an object that is a prey
                if (other.gameObject.CompareTag(p) && canPrey)
                {
                    canPrey = false;
                    StartCoroutine(preytrue());
                    Prey(other);
                }
            }
            // Collision with others of same tag
            if (other.CompareTag(gameObject.tag))
            {
                //check if reproduction will occur
                if (Random.Range(0, 100) < reproduceRate && canReproduce && energy >= 30 && other.GetComponent<Insect>().energy >= 30)
                {
                    //change reproduce flag
                    canReproduce = false;
                    Debug.Log("reproduce");
                    //call reproduce coroutine
                    StartCoroutine(Reproduce());
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == 11 || other.gameObject.layer == 9)
        {
            View(other);
        }
    }
    //view method
    private void View(Collider2D other)
    {
        //Debug.Log("I see you");
        string[] preys = prey.Split(',');
        foreach (string p in prey.Split(','))
        {
            //if it collides with an object that is a prey, canPrey and is inside forageTax
            if (other.gameObject.CompareTag(p) && canPrey && Random.Range(0, 100) < forageTax)
            {
                Debug.Log("prey");
                //Choose Prey Angle to tilt
                //Vector2 dir = other.transform.position - transform.position;
                //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                delta = 0f;
                // Rotate the sprite direction
                //float sin = Mathf.Sin(angle);
                //float cos = Mathf.Cos(angle);

                //Vector2 old = body.velocity.normalized;
                //Vector2 move = new Vector2(cos * old.x - sin * old.y, sin * old.x + cos * old.y);
                this.transform.LookAt(other.transform);
                // Change the movement direction
                //body.velocity = move * actualSpeed;

            }
        }
        // TODO: go towards food
        // Rotate the sprite direction
        //Vector3 dir = other.transform.position - transform.position;
        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //float sin = Mathf.Sin(angle);
        //float cos = Mathf.Cos(angle);
        //Vector2 old = body.velocity.normalized;
        //Vector2 move = new Vector2(cos * old.x - sin * old.y, sin * old.x + cos * old.y);

        // Change the movement direction
        //body.velocity = move * moveMaxSpeed;

    }

    //prey method
    private void Prey(Collider2D other)
    {
        //stop the object and take nutritionalValue 
        body.velocity *= 0.01f;
        if (other.GetComponent<Insect>() == true)
        {

            other.gameObject.GetComponent<Insect>().canWalk = false;
            other.gameObject.GetComponent<Insect>().actualSpeed *= 0.01f;
            energy += other.GetComponent<Insect>().nutritionalValue;
        }
        else
        {
            energy += other.GetComponent<Plant>().nutritionalValue;
        }
        delta = moveFrequency - preyDelay;
        audioSource.PlayOneShot(bite);
        //cap energy to energyMax
        if (energy > energyMax)
        {
            energy = energyMax;
        }
        //send prey log
        
        Activity preylog = new Activity("prey", System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
        preylog.SetPosition(this.transform.position.x, this.transform.position.y);
        preylog.AddAgent(this, "predator");
        GameManager.micelio.SendActivity(preylog);


        //destroy preyed object
        Destroy(other.gameObject, 1);
    }

    //reproduction method
    IEnumerator Reproduce()
    {
 
        // stop the object
        body.velocity *= 0.01f;
        delta = moveFrequency - reproduceDelay;
        //loose energy
        energy -= 25;
        audioSource.PlayOneShot(reproduce);
        //instantiate a new object on this object position
        Vector3 place = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);
        GameObject obj = Instantiate(gameObject, place, Quaternion.identity);

        //reproduction log
        /*Activity reproducelog = new Activity("Reproduce", System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
        reproducelog.SetPosition(this.transform.position.x, this.transform.position.y);
        reproducelog.AddAgent(this, "Reproductor");
        micelio.SendActivity(reproducelog);*/

        yield return new WaitForSeconds(reproduceDelay);
        //can reproduce again after a delay
        canReproduce = true;

    }

    //change direction method
    private void ChangeDirection()
    {
        // Choose a random angle to tilt
        float angle = Random.Range(-moveMaxAngle * Mathf.Deg2Rad, moveMaxAngle * Mathf.Deg2Rad);

        // Rotate the sprite direction
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);
        Vector2 old = body.velocity.normalized;
        Vector2 move = new Vector2(cos * old.x - sin * old.y, sin * old.x + cos * old.y);

        // Change the movement direction
        body.velocity = move * actualSpeed;
    }

    //energy loss method; loses 1 energy every second, speed reduce based on energy
    IEnumerator Energyloss(float time)
    {
        yield return new WaitForSeconds(time);
        energy -= 1;
        //Debug.Log("energy = "+energy);

        //change speed and color based on energy
        changeInsect();

        //destroy object if energy is 0 or less
        if (energy <= 0)
        {
            audioSource.PlayOneShot(dying);

            Destroy(gameObject, 2.0f);
        }
        else
        {
            StartCoroutine(Energyloss(1));
        }


    }
    IEnumerator Reproducestart()
    {
        yield return new WaitForSeconds(reproduceDelay);
        canReproduce = true;
    }
    //change speed and color of the Insect based on energy percentual
    private void changeInsect()
    {
        //verify if something is not impeding the movement
        if (canWalk)
        {
            float percentEnergy;
            percentEnergy = (energy / energyMax) * 100.0f;
            actualSpeed = (moveMaxSpeed / 100.0f) * percentEnergy;

            float amount = 1 - (percentEnergy / 100);
            spriteRenderer.material.SetFloat("_GrayscaleAmount", amount);
        }

    }
    //wait for not prey 2 at same time
    IEnumerator preytrue()
    {
        yield return new WaitForSeconds(3);
        canPrey = true;
    }

    //generate agent information
    public Agent GetAgent()
    {
        Agent a = new Agent(id_agent, this.name, this.GetType().Name);

        //a.AddProperty("munição", municao);
        //a.AddProperty("pontos de vida", hp);
        //a.AddProperty("patente", patente);
        return a;
    }
}