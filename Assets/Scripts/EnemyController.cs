using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int hp;
    public float speed;
    public GameObject spawnedEnemy;
    public GameObject[] allEnemies;
    private bool spawning;
    public HealthController healthBar;
    public GameObject[] dropList;
    public string[] enemyTypes = { "melee", "ranged" };
    public string type;
    private Rigidbody2D shot;
    public int damage;
    public float cooldown;
    public GameObject bullet;
    private Rigidbody2D rb;
    public GameObject spawnCircle;
    public int roundNumber;
    public GameObject closestEnemy;
    public GameObject secondClosest;
    public float distanceToCloseEnemy;
    public bool burning = false;
    public int burnDamage;
    public int baseHealth = 1;
    public int numEnemies = 1;
    public float dropRate;
    public float camSize;
    public CameraController cam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseHealth = GameObject.FindGameObjectWithTag("EController").GetComponent<EnemyController>().baseHealth;
        hp = Random.Range(baseHealth, baseHealth + 10);
        healthBar.SetMaxHP(hp);
        speed = Random.Range(0.5f, 1.5f);
        type = enemyTypes[Random.Range(0, enemyTypes.Length)];
        if (type == "ranged" && speed < 1.0f){
            speed = Random.Range(1.0f, 1.5f);
        }
        damage = 2;
        cooldown = speed;
        roundNumber = 1;
        distanceToCloseEnemy = Mathf.Infinity;
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        closestEnemy = null;
        dropRate = .3f;
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
    }

    void Update()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (allEnemies.Length == 0 && !spawning)
        {
            roundNumber += 1;
            //Debug.Log("round " + roundNumber.ToString() + " starting");
            //Debug.Log(roundNumber % 5);
            if (roundNumber % 5 == 0)
            {
                clearBullets();
                StartCoroutine(spawnBoss());
                baseHealth += 3;
                numEnemies += 1;
            }
            else
            {
                clearBullets();
                for (int i = 0; i <= Random.Range(numEnemies, numEnemies + 4); i++)
                {
                    StartCoroutine(spawn());
                }
            }
            spawning = true;
        }

        distanceToCloseEnemy = Mathf.Infinity;
        if (allEnemies.Length >= 2)
        {
            foreach (GameObject currEnemy in allEnemies)
            {
                float distanceToEnemy =
                    (currEnemy.transform.position - this.transform.position).sqrMagnitude;
                if (distanceToEnemy < distanceToCloseEnemy && distanceToEnemy != 0)
                {
                    distanceToCloseEnemy = distanceToEnemy;
                    closestEnemy = currEnemy;
                }
            }
        }
        else
        {
            closestEnemy = this.gameObject;
        }

        if (this.tag == "Enemy")
        {
            Move();
            if (hp <= 0)
            {
                Destroy(this.gameObject);                
                if (Random.Range(0f, 5.0f)/5.0f <= dropRate)
                {
                    Drop();
                }
            }
        }
        else 
        {
            camSize = cam.GetSize();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "player_bullet(Clone)")
        {
            BulletController hit =
                collision.gameObject.GetComponent<BulletController>();
            hp -= hit.getDamage();
            healthBar.SetHP(hp);
            if (hit.getBurn() > 0 && !burning)
            {
                burning = true;
                StartCoroutine(burn());
                burnDamage = hit.getBurn();                
            }
        }
        else if (collision.gameObject.tag == "Enemy") {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
            collision.collider.attachedRigidbody.velocity = Vector2.zero;
        }

    }

    void Move()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Vector3 target = players[0].transform.position;
        float moveSpeed = speed * Time.deltaTime;
        switch (type)
        {
            case "melee":
                transform.up = target - this.transform.position;
                rb.velocity = transform.up * speed;
                break;
            case "ranged":
                transform.up = target - transform.position;
                if (cooldown > 0)
                {
                    cooldown -= Time.deltaTime;
                }
                else
                {
                    shoot(target);
                }

                break;
        }

    }

    IEnumerator spawn()
    {
        Vector2 coords = new Vector2(Random.Range(-1*(camSize-1.3f), camSize-1.3f), Random.Range(-1*(camSize-0.7f), camSize-0.7f));
        Instantiate(spawnCircle, coords, Quaternion.identity);
        yield return new WaitForSeconds(1);
        Instantiate(spawnedEnemy, coords, Quaternion.identity);
        spawning = false;
    }

    IEnumerator spawnBoss()
    {
        hp = (baseHealth + 10) * 5;
        Vector2 coords = new Vector2(0f, 4f);
        Instantiate(spawnCircle, coords, Quaternion.identity);
        yield return new WaitForSeconds(1);
        Instantiate(spawnedEnemy, coords, Quaternion.identity);
        spawning = false;
        GameObject[] boss = GameObject.FindGameObjectsWithTag("Enemy");
        boss[0].GetComponent<EnemyController>().setHealth(hp);
    }

    IEnumerator burn()
    {
        while (burning)
        {
            hp -= burnDamage;
            healthBar.SetHP(hp);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Drop()
    {
        Instantiate(dropList[Random.Range(0, dropList.Length)], transform.position, Quaternion.identity);
    }

    void shoot(Vector2 target)
    {

        shot =
            Instantiate(bullet.GetComponent<Rigidbody2D>(),
            rb.position +
            new Vector2(transform.up.x * 0.75f, transform.up.y * 0.75f),
            rb.transform.rotation) as
            Rigidbody2D;
        shot.velocity = transform.up * 3;
        BulletController bull = shot.GetComponent<BulletController>();
        bull.setDamage(damage);
        cooldown = speed;
    }

    public void setHealth(int h)
    {
        hp = h;
        healthBar.SetMaxHP(hp);
    }

    void clearBullets()
    {
        Debug.Log("Attempting to clear...");
        GameObject[] remaining = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject obj in remaining)
        {
            if (obj.name == "enemy_bullet(Clone)")
            {
                Destroy(obj);
            }
        }
    }

    public GameObject getClosestEnemy()
    {
        return closestEnemy;
    }

}
