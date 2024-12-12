using UnityEngine;

public class CustomBullet : MonoBehaviour
{
    [Header("Assignables")]
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemies;

    [Header("Bullet stats")]
    public float bounciness;
    public bool useGravity;

    [Header("Damage")]
    public int bulletDamage;
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;

    [Header("Lifetime")]
    public int maxCollisions;
    public float maxLifetime;
    public bool explodeOnTouch = true;

    int collisions;
    PhysicMaterial physics_mat;
    [HideInInspector] public GameObject shooter;

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        //When to explode:
        if (collisions > maxCollisions) Explode();

        //Count down lifetime
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) Explode();
    }

    private void Explode()
    {
        //Instantiate explosion
        if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);

        //Check for enemies
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);

        //Remove host from the array
        enemies = System.Array.FindAll(enemies, enemy => enemy.gameObject != InfectAbility.host);

        for (int i = 0; i < enemies.Length; i++)
        {
            //Get component of enemy and call TakeDamage
            enemies[i].GetComponent<EnemyAI>().TakeDamage(explosionDamage);

            //Add explosion force (if enemy has a rigidbody)
            if (enemies[i].GetComponentInParent<Rigidbody>())
                enemies[i].GetComponentInParent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
        }

        //Add a little delay, just to make sure everything works fine
        Invoke("Delay", 0.05f);

        //Add bullet back into the object pool
        //ObjectPooler.Instance.ReturnToPool("Bullet", gameObject);
    }
    private void Delay()
    {
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        //Debug the collision event
        Debug.Log("Collision with: " + collision.gameObject.name);

        //Count up collisions
        collisions++;

        //Deal base damage if bullet hits an enemy directly
        if (collision.gameObject.layer == LayerMask.NameToLayer("Host") && collision.gameObject != InfectAbility.host)
        {
            var enemyScript = collision.gameObject.GetComponent<EnemyAI>();
            if (enemyScript != null)
            {
                enemyScript.AttackShooter(shooter);
                if(collision.gameObject != enemyScript.enemyTarget)
                {
                    enemyScript.TakeDamage(bulletDamage);
                }
            }
        }

        //Deal base damage if bullet hits the player directly
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var parasiteHPScript = collision.gameObject.GetComponent<ParasiteHP>();
            if (parasiteHPScript != null)
            {
                parasiteHPScript.TakeDamage(bulletDamage);
            }
        }

        //Explode if bullet hits an enemy directly and explodeOnTouch is activated
        if (explodeOnTouch && (collision.gameObject.layer == LayerMask.NameToLayer("Host") || collision.gameObject.layer == LayerMask.NameToLayer("Player")))
        {
            Explode();
        }
    } 
    private void Setup()
    {
        //Create a new Physics material
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        //Assign material to collider
        GetComponent<SphereCollider>().material = physics_mat;
        rb.GetComponent<Rigidbody>();

        //Set gravity
        rb.useGravity = useGravity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
