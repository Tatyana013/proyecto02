using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;




public class PlayerController : MonoBehaviour
{

    [Header("Movimiento del Jugador")]
    public float movementVelocity = 5.0f; // Velocidad de movimiento del jugador
    public float rotationVelocity = 200.0f; // Velocidad de rotación del jugador
    private Animator anim; // Controlador de animación
    public float x, y; // Variables para capturar la entrada del movimiento

    [Header("Estado de Vida del Jugador")]
    public float currentHealth = 100f; // Vida actual del jugador
    public Slider sliderHealth; // Barra de salud en la UI

    [Header("Salto del Jugador")]
    public Rigidbody rb; // Rigidbody del jugador
    public float jumpForce = 8f; // Fuerza de salto
    public bool canJump; // Indica si el jugador puede saltar

    [Header("Ataque del Jugador")]
    public bool OnlyAvanze; // Indica si el jugador solo avanza al atacar
    public float punchImpulse = 10f; // Impulso de ataque
    public bool playerMove = true; 
    public bool isPunchingRight = false; // Si el jugador está atacando a la derecha
    public bool withWeapon = false; // Si el jugador está armado
    public float timeToDeactivateBool = 3f; // Tiempo para desactivar el estado de ataque
    private float timer = 0f; // Contador de tiempo para el ataque
    public float attackDistance = 0.2f; // Distancia de ataque

    [Header("Colisionadores y Enemigos")]
    public GameObject handCollider; // Colisionadores en las manos
    public GameObject[] enemies; // Lista de enemigos en la escena

    [Header("Armas del Jugador")]
    public GameObject[] weaponsGfx; // Gráficos de las armas
    private float currentDamage; // Daño actual del arma
    private Weapon.WeaponType currentWeaponType; // Tipo de arma actual
    private Weapon defaultFistWeapon; // Arma por defecto (puño)

    private EnemyPadre ep;
    void Start()
    {
        sliderHealth.maxValue = 100;
        sliderHealth.value = currentHealth;


        // Inicializa los objetos de enemigo y colisionadores de manos
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        //handCollider = GameObject.FindGameObjectWithTag("HandCollider");
        anim = GetComponent<Animator>(); // Asigna el componente Animator
        canJump = false; // Inicializa el salto como falso
        isPunchingRight = false;

        handCollider.SetActive(false);

        // Define el arma predeterminada (puño) y configura sus propiedades
        defaultFistWeapon = new Weapon();
        defaultFistWeapon.damage = 10;
        defaultFistWeapon.type = Weapon.WeaponType.Type_Fist;
        SetNewWeapon(defaultFistWeapon);

       
        
       
        
    }

    void Update()
    {
        HandleMovement(); // Maneja el movimiento del jugador

        if (Input.GetKeyDown(KeyCode.R))
        {
            SetNewWeapon(defaultFistWeapon); // Cambia al arma predeterminada (puño) con la tecla R.
           
        }

        // Verifica el temporizador de ataque para desactivar el collider
        if (isPunchingRight)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                DisableWeaponCollider(); // Desactiva el collider cuando el tiempo se agota
            }
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            TakeDamagePlayer(10);
            UpdateHealthSliderPlayer();
            Debug.Log("Se ha detectado colisión con bullet, vida actual: " + currentHealth);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // Detecta colisión con un enemigo y le aplica daño
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyPadre collidedEnemy = other.gameObject.GetComponent<EnemyPadre>();

            if (collidedEnemy != null)
            {
                if (isPunchingRight && weaponsGfx[(int)currentWeaponType].activeSelf)
                {
                    collidedEnemy.TakeDamageEnemy(currentDamage); // Aplica daño al enemigo.

                     Debug.Log("deberia de estar quitando : " + currentDamage);
                }


                
            }
        }
    }

    public void SetNewWeapon(Weapon newWeapon)
    {
        // Establece un nuevo arma con su daño y tipo
        currentDamage = newWeapon.damage;
        currentWeaponType = newWeapon.type;

        // Desactiva todos los gráficos de las armas y solo activa el actual
        for (int i = 0; i < weaponsGfx.Length; i++)
        {
            weaponsGfx[i].SetActive(false);
        }

        weaponsGfx[(int)currentWeaponType].SetActive(true);
        // weaponsGfx[(int)currentWeaponType].GetComponent<BoxCollider>().enabled = false;
    }

    void HandleMovement()
    {
        playerMove = true;
        //// Detecta si el jugador está atacando con la tecla E
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    IsPuching();
        //}

        if (Input.GetMouseButtonDown(0))
        {
            IsPuching();

            anim.SetBool("isPunchingRight", true);

            // Debug.Log("Se esta detectando IsPunching"); 
        }
      

        if (playerMove) // Si playerMove es verdadero se puede mover.
        {

           

            x = Input.GetAxis("Horizontal"); // Captura el movimiento horizontal
            y = Input.GetAxis("Vertical"); // Captura el movimiento vertical

            anim.SetFloat("VelX", x); // Configura la animación con el movimiento en X
            anim.SetFloat("VelY", y); // Configura la animación con el movimiento en Y

            // Rotación y movimiento
            transform.Rotate(0, x * Time.deltaTime * rotationVelocity, 0);
            transform.Translate(0, 0, y * Time.deltaTime * movementVelocity);

            // Debug.Log("Se esta leyendo la función de If(PlayerMove");
        }
        else
        {
            anim.SetBool("Idle", true); // Si está atacando, mantiene la animación en "Idle"
        }

        if (OnlyAvanze)
        {
            rb.velocity = transform.forward * punchImpulse; // Aplica impulso al avanzar
        }

        HandleJump(); // Llama al manejo de salto
    }

    void HandleJump()
    {
        if ( !isPunchingRight) // Si el jugador no está atacando
        {
            if (canJump) // Si puede saltar
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    playerMove = false; 
                    anim.SetBool("Jump", true); // Activa la animación de salto
                    rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse); // Aplica la fuerza de salto
                }

                anim.SetBool("TouchGround", true); // Indica que el jugador está en el suelo
            }
            else
            {
                PlayerFall(); // Llama a la función que maneja la caída
            }
        }
    }

    public void PlayerFall()
    {
        anim.SetBool("TouchGround", false); // Indica que el jugador no está en el suelo
        anim.SetBool("Jump", false); // Desactiva la animación de salto
    }

    public void IsPuching()
    {
        isPunchingRight = true;
        Debug.Log("Inicio de ataque, estado de isPunchingRight: " + isPunchingRight);

        // Activa el arma y su collider al iniciar el ataque
        weaponsGfx[(int)currentWeaponType].SetActive(true);
        weaponsGfx[(int)currentWeaponType].GetComponent<BoxCollider>().enabled = true;

        timer = timeToDeactivateBool; // Establece el temporizador con el tiempo especificado para el ataque

        Debug.Log("Estado de " + weaponsGfx[(int)currentWeaponType].name + ": " + weaponsGfx[(int)currentWeaponType].activeSelf);

    }

    public void TakeDamagePlayer(float damage)
    {
        Debug.Log(currentHealth);

        currentHealth -= damage; // Reducir la vida actual del jugador



        if (currentHealth <= 0)  // Verificar si la vida actual es menor o igual a 0
        {
            Die(); // Llama al método de muerte
        }


    }


    public void UpdateHealthSliderPlayer()
    {
        if (sliderHealth != null)
        {
            sliderHealth.value = currentHealth; // Actualiza el valor del slider de vida para reflejar la vida actual.
        }
    }

    public void NoPunch()
    {
        playerMove = true;
        isPunchingRight = false;
        OnlyAvanze = false;

        anim.SetBool("Idle", true); // Cambia la animación a Idle
        Debug.Log("Se ha leído NoPunch");
    }

    public void OnlyAdvanze()
    {
        OnlyAvanze = true; // Activa el avance solo al atacar
    }

    public void NoAdvance()
    {
        OnlyAvanze = false; // Desactiva el avance solo al atacar
    }

    void Die()
    {
        Debug.Log("El Player ha muerto"); // Muestra un mensaje en la consola
        gameObject.SetActive(false); // Desactiva el jugador
    }

    // Método que desactiva el booleano cuando termina la animación
    public void OnPunchAnimationEnd()
    {
        // Desactivar el booleano para volver a la animación principal
        anim.SetBool("isPunchingRight", false);
    }

    void DisableWeaponCollider()
    {
        isPunchingRight = false;
        weaponsGfx[(int)currentWeaponType].GetComponent<BoxCollider>().enabled = false;
        Debug.Log("Fin de ataque, estado de isPunchingRight: " + isPunchingRight);
    }
}

    

