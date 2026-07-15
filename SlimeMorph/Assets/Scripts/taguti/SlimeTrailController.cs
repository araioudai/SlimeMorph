using UnityEngine;

public class SlimeTrailController : MonoBehaviour
{
    [Header("移動時の跡エフェクト")]
    [SerializeField] private GameObject slimeTrailPrefab;

    [Header("Trailの位置調整")]
    [SerializeField] private Vector3 trailOffset = new Vector3(0f, 0.02f, -0.3f);

    [Header("どれくらいの速度で移動中と判定するか")]
    [SerializeField] private float moveSpeedThreshold = 0.001f;

    [Header("どれくらい位置が変わったら移動中と判定するか")]
    [SerializeField] private float moveDistanceThreshold = 0.00001f;

    [Header("止まった判定までの時間")]
    [SerializeField] private float stopDelay = 0.2f;

    [Header("Inspectorで移動Trailを強制再生テスト")]
    [SerializeField] private bool trailEffectTest = false;

    [Header("坂に沿わせる設定")]
    [SerializeField] private bool alignToGround = true;
    [SerializeField] private float groundRayHeight = 2f;
    [SerializeField] private float groundRayDistance = 5f;
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float rotationSmooth = 15f;

    [Header("傾きが逆の場合の補正")]
    [SerializeField] private Vector3 trailRotationOffset = Vector3.zero;

    [Header("ダメージ時のエフェクト（子オブジェクト）")]
    [SerializeField] private GameObject damageVFXObject;

    [Header("Inspectorでダメージエフェクトを試す")]
    [SerializeField] private bool damageEffectTest = false;

    [Header("ダメージエフェクトの位置調整")]
    [SerializeField] private Vector3 damageVFXOffset = new Vector3(0f, 0.5f, 0f);

    [Header("死亡時のエフェクト（子オブジェクト）")]
    [SerializeField] private GameObject deathVFXObject;

    [Header("Inspectorで死亡エフェクトを試す")]
    [SerializeField] private bool deathEffectTest = false;

    [Header("死亡エフェクトの位置調整")]
    [SerializeField] private Vector3 deathVFXOffset = new Vector3(0f, 0.5f, 0f);

    private Rigidbody rb;
    private float stopTimer;

    private GameObject slimeTrailObject;
    private ParticleSystem[] slimeTrailParticles;

    private Vector3 lastPosition;

    private bool previousTrailEffectTest;
    private bool previousDamageEffectTest;
    private bool previousDeathEffectTest;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("SlimeTrailController: 親にも自分にもRigidbodyが見つかりません。");
        }

        lastPosition = GetMoveTargetPosition();

        CreateSlimeTrail();

        previousTrailEffectTest = trailEffectTest;
        previousDamageEffectTest = damageEffectTest;
        previousDeathEffectTest = deathEffectTest;

        StopSlimeTrail(true);

        if (damageVFXObject != null)
        {
            damageVFXObject.SetActive(false);
        }

        if (deathVFXObject != null)
        {
            deathVFXObject.SetActive(false);
        }
    }

    private void Update()
    {
        FollowTrailPosition();
        AlignTrailToGround();

        CheckMoveTrail();
        CheckTrailEffectTest();
        CheckDamageEffectTest();
        CheckDeathEffectTest();
    }

    private Vector3 GetMoveTargetPosition()
    {
        if (rb != null)
        {
            return rb.transform.position;
        }

        return transform.position;
    }

    private void CreateSlimeTrail()
    {
        if (slimeTrailPrefab == null)
        {
            Debug.LogWarning("SlimeTrailPrefab が設定されていません。");
            return;
        }

        slimeTrailObject = Instantiate(
            slimeTrailPrefab,
            GetMoveTargetPosition() + trailOffset,
            Quaternion.identity
        );

        slimeTrailObject.transform.localScale = Vector3.one;
        slimeTrailObject.SetActive(true);

        slimeTrailParticles = slimeTrailObject.GetComponentsInChildren<ParticleSystem>(true);

        Debug.Log("SlimeTrail生成完了: " + slimeTrailObject.name);
        Debug.Log("取得したParticleSystem数: " + slimeTrailParticles.Length);
    }

    private void FollowTrailPosition()
    {
        if (slimeTrailObject == null) return;

        Vector3 basePosition = GetMoveTargetPosition();

        Vector3 rayStart = basePosition + Vector3.up * groundRayHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayDistance, groundLayer))
        {
            slimeTrailObject.transform.position = hit.point + trailOffset;
        }
        else
        {
            slimeTrailObject.transform.position = basePosition + trailOffset;
        }
    }

    private void AlignTrailToGround()
    {
        if (!alignToGround) return;
        if (slimeTrailObject == null) return;

        Vector3 rayStart =
            GetMoveTargetPosition() + Vector3.up * groundRayHeight;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            groundRayDistance,
            groundLayer))
        {
            /*
             * 平面の上方向はそのままにして、
             * 坂の傾きだけ反対方向に補正する。
             */
            Vector3 correctedNormal = new Vector3(
                -hit.normal.x,
                 hit.normal.y,
                -hit.normal.z
            ).normalized;

            Vector3 moveDirection = transform.forward;

            if (rb != null)
            {
                Vector3 velocity = rb.linearVelocity;

                if (velocity.sqrMagnitude > 0.0001f)
                {
                    moveDirection = velocity.normalized;
                }
            }

            // 補正後の坂に沿う進行方向
            Vector3 projectedForward =
                Vector3.ProjectOnPlane(moveDirection, correctedNormal).normalized;

            if (projectedForward.sqrMagnitude < 0.0001f)
            {
                projectedForward = Vector3.ProjectOnPlane(
                    transform.forward,
                    correctedNormal
                ).normalized;
            }

            Quaternion targetRotation =
                Quaternion.LookRotation(projectedForward, correctedNormal)
                * Quaternion.Euler(trailRotationOffset);

            slimeTrailObject.transform.rotation = Quaternion.Slerp(
                slimeTrailObject.transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSmooth
            );
        }
    }

    private void CheckMoveTrail()
    {
        if (slimeTrailParticles == null || slimeTrailParticles.Length == 0) return;

        Vector3 currentPosition = GetMoveTargetPosition();

        // XZ方向だけで移動判定
        Vector2 currentXZ = new Vector2(currentPosition.x, currentPosition.z);
        Vector2 lastXZ = new Vector2(lastPosition.x, lastPosition.z);

        float distance = Vector2.Distance(currentXZ, lastXZ);

        bool isMovingByPosition = distance > moveDistanceThreshold;

        bool isMovingByVelocity = false;

        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;

            isMovingByVelocity = velocity.magnitude > moveSpeedThreshold;
        }

        bool isMoving = isMovingByPosition || isMovingByVelocity;

        if (isMoving)
        {
            stopTimer = 0f;
            PlaySlimeTrail();
        }
        else
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= stopDelay)
            {
                StopSlimeTrail(false);
            }
        }

        lastPosition = currentPosition;
    }

    private void CheckTrailEffectTest()
    {
        if (trailEffectTest == previousTrailEffectTest) return;

        previousTrailEffectTest = trailEffectTest;

        if (trailEffectTest)
        {
            PlaySlimeTrail();
        }
        else
        {
            StopSlimeTrail(true);
        }
    }

    private void PlaySlimeTrail()
    {
        if (slimeTrailObject != null)
        {
            slimeTrailObject.SetActive(true);
        }

        if (slimeTrailParticles == null) return;

        foreach (ParticleSystem ps in slimeTrailParticles)
        {
            if (ps == null) continue;

            ps.gameObject.SetActive(true);

            var emission = ps.emission;
            emission.enabled = true;

            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }

            if (!ps.isPlaying)
            {
                ps.Play(true);
            }
        }
    }

    private void StopSlimeTrail(bool clear)
    {
        if (slimeTrailParticles == null) return;

        foreach (ParticleSystem ps in slimeTrailParticles)
        {
            if (ps == null) continue;

            if (clear)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
        damageVFXObject.transform.localPosition = damageVFXOffset;

        ParticleSystem[] particles = damageVFXObject.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.gameObject.SetActive(true);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }
    }

    private void StopDamageVFX()
    {
        if (damageVFXObject == null) return;

        ParticleSystem[] particles = damageVFXObject.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        damageVFXObject.SetActive(false);
    }

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
        deathVFXObject.transform.localPosition = deathVFXOffset;

        ParticleSystem[] particles = deathVFXObject.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.gameObject.SetActive(true);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }
    }

    private void StopDeathVFX()
    {
        if (deathVFXObject == null) return;

        ParticleSystem[] particles = deathVFXObject.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        deathVFXObject.SetActive(false);
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