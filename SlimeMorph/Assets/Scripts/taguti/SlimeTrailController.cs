using UnityEngine;

public class SlimeTrailController : MonoBehaviour
{
    [Header("移動時の跡エフェクト")]
    [SerializeField] private ParticleSystem slimeTrail;

    [Header("どれくらい動いたら移動中と判定するか")]
    [SerializeField] private float moveThreshold = 0.001f;

    [Header("止まった判定までの時間")]
    [SerializeField] private float stopDelay = 0.15f;

    [Header("ダメージ時のエフェクト（子オブジェクト）")]
    [SerializeField] private GameObject damageVFXObject;

    [Header("Inspectorでダメージエフェクトを試す")]
    [SerializeField] private bool damageEffectTest = false;

    private Vector3 lastPosition;
    private float stopTimer;

    private bool previousDamageEffectTest;

    private void Start()
    {
        lastPosition = transform.position;

        if (slimeTrail != null)
        {
            slimeTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (damageVFXObject != null)
        {
            damageVFXObject.SetActive(false);
        }

        previousDamageEffectTest = damageEffectTest;
    }

    private void Update()
    {
        CheckMoveTrail();
        CheckDamageEffectTest();
    }

    private void CheckMoveTrail()
    {
        if (slimeTrail == null) return;

        float distance = Vector3.Distance(transform.position, lastPosition);

        if (distance > moveThreshold)
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

        lastPosition = transform.position;
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
}