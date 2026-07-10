using UnityEngine;

public class SlimeTrailController : MonoBehaviour
{
    [Header("移動時の跡エフェクト")]
    [SerializeField] private ParticleSystem slimeTrail;

    [Header("どれくらい動いたら移動中と判定するか")]
    [SerializeField] private float moveSpeedThreshold = 0.001f;

    [Header("止まった判定までの時間")]
    [SerializeField] private float stopDelay = 0.15f;

    [Header("ダメージ時のエフェクト（子オブジェクト）")]
    [SerializeField] private GameObject damageVFXObject;

    [Header("Inspectorでダメージエフェクトを試す")]
    [SerializeField] private bool damageEffectTest = false;

    [Header("死亡時のエフェクト（子オブジェクト）")]
    [SerializeField] private GameObject deathVFXObject;

    [Header("Inspectorでダメージエフェクトを試す")]
    [SerializeField] private bool deathEffectTest = false;

    private Rigidbody rb;
    private float stopTimer;

    private bool previousDamageEffectTest;
    private bool previousDeathEffectTest;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("SlimeTrailController: Rigidbodyが見つかりません。PlayerにRigidbodyを付けてください。");
        }

        if (slimeTrail != null)
        {
            slimeTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (damageVFXObject != null)
        {
            damageVFXObject.SetActive(false);
        }

        if (deathVFXObject != null)
        {
            deathVFXObject.SetActive(false);
        }

        previousDamageEffectTest = damageEffectTest;
        previousDeathEffectTest = deathEffectTest;
    }

    private void Update()
    {
        CheckMoveTrail();
        CheckDamageEffectTest();
        CheckDeathEffectTest();
    }

    private void CheckMoveTrail()
    {
        if (slimeTrail == null) return;
        if (rb == null) return;

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        bool isMoving = horizontalVelocity.magnitude > moveSpeedThreshold;

        if (isMoving)
        {
            stopTimer = 0f;

            if (!slimeTrail.isPlaying)
            {
                slimeTrail.Play();
            }
        }
        else
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= stopDelay)
            {
                if (slimeTrail.isPlaying)
                {
                    slimeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
    }

    private void CheckDamageEffectTest()
    {
        if (damageEffectTest == previousDamageEffectTest) return;

        previousDamageEffectTest = damageEffectTest;

        if (damageEffectTest)
        {
            PlayDamageVFX();
        }
        else
        {
            StopDamageVFX();
        }
    }

    private void PlayDamageVFX()
    {
        if (damageVFXObject == null) return;

        damageVFXObject.SetActive(true);

        ParticleSystem[] particles = damageVFXObject.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }
    }

    private void StopDamageVFX()
    {
        if (damageVFXObject == null) return;

        ParticleSystem[] particles = damageVFXObject.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        damageVFXObject.SetActive(false);
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            PlayDamageVFX();
        }
    }
    */

    private void CheckDeathEffectTest()
    {
        if (deathEffectTest == previousDeathEffectTest) return;

        previousDeathEffectTest = deathEffectTest;

        if (deathEffectTest)
        {
            PlayDeathVFX();
        }
        else
        {
            StopDeathVFX();
        }
    }

    private void PlayDeathVFX()
    {
        if (deathVFXObject == null) return;

        deathVFXObject.SetActive(true);

        ParticleSystem[] particles = deathVFXObject.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }
    }

    private void StopDeathVFX()
    {
        if (deathVFXObject == null) return;

        ParticleSystem[] particles = deathVFXObject.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        deathVFXObject.SetActive(false);
    }
}