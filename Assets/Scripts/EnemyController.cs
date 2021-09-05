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

    public string[] enemyTypes = {"ranged"};

    public string type;

    private Rigidbody2D shot;

    public int damage;

    public float cooldown;

    public GameObject bullet;

    private Rigidbody2D rb;

    public GameObject spawnCircle;

    public int roundNumber;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = Random.Range(1, 11);
        healthBar.SetMaxHP (hp);
        speed = Random.Range(0.5f, 1.5f);
        type = enemyTypes[Random.Range(0,enemyTypes.Length)];
        damage = 2;
        cooldown = speed;
        roundNumber = 1;
    }

    void Update()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (allEnemies.Length == 0 && !spawning)
        {
            roundNumber += 1;
            Debug.Log("round " + roundNumber.ToString() + " starting");
            Debug.Log(roundNumber % 5);
            if (roundNumber % 5 == 0)
            {
                clearBullets();
                StartCoroutine(spawnBoss());
            }
            else
            {
                clearBullets();
                for (int i = 0; i <= Random.Range(1, 5); i++)
                {                 
                    StartCoroutine(spawn());
                }
            }          
            spawning = true;
        }

        if (this.tag == "Enemy")
        {
            Move();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "player_bullet(Clone)")
        {
            BulletController hit =
                collision.gameObject.GetComponent<BulletController>();

            Destroy(collision.collider.gameObject);
            hp -= hit.getDamage();
            healthBar.SetHP (hp);
        }
        if (hp <= 0)
        {
            Destroy(this.gameObject);
            Drop(Random.Range(0f, 5.0f));
        }
    }

    void Move()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Vector2 target = players[0].transform.position;
        float moveSpeed = speed * Time.deltaTime;
        switch (type){
            case "melee":
                transform.position =
                    Vector2.MoveTowards(transform.position, target, moveSpeed);
                break;
            case "ranged":
                transform.up = target - new Vector2(transform.position.x, transform.position.y);
                if (cooldown > 0)
                {
                    cooldown -= Time.deltaTime;
                }
                else {
                    shoot(target);
                }
                
            break;
        }
        
    }

    IEnumerator spawn()
    {
        Vector2 coords = new Vector2(Random.Range(-3.7f, 3.7f), Random.Range(-4.3f, 4.3f));
        Instantiate(spawnCircle, coords, Quaternion.identity);
        yield return new WaitForSeconds(1);
        Instantiate(spawnedEnemy, coords, Quaternion.identity);
        spawning = false;         
    }

    IEnumerator spawnBoss()
    {
        hp = 50;
        Vector2 coords = new Vector2(0f, 4f);
        Instantiate(spawnCircle, coords, Quaternion.identity);
        yield return new WaitForSeconds(1);
        Instantiate(spawnedEnemy, coords, Quaternion.identity);
        spawning = false;
        GameObject[] boss = GameObject.FindGameObjectsWithTag("Enemy");
        boss[0].GetComponent<EnemyController>().setHealth(hp);      
    }

    void Drop(float dropNum)
    {
        switch (dropNum)
        {
            case float n when n <= 0.25f:
                Instantiate(dropList[0],
                transform.position,
                Quaternion.identity);
                break;
            case float n when n <= 0.5f:
                Instantiate(dropList[1],
                transform.position,
                Quaternion.identity);
                break;
            case float n when n <= 0.75f:
                Instantiate(dropList[2],
                transform.position,
                Quaternion.identity);
                break;
            case float n when n <= 1f:
                Instantiate(dropList[3],
                transform.position,
                Quaternion.identity);
                break;
        }
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
        bull.setDamage (damage);
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
        foreach (GameObject obj in remaining) {
            if (obj.name == "enemy_bullet(Clone)"){
                Destroy(obj);
            }
        }
    }

}
