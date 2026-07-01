using UnityEngine;

public class GoalCamera : MonoBehaviour
{
	[SerializeField] Transform target;

	[Header("スタート演出設定")]
	[SerializeField] Vector3 startFrontOffset = new(0f, 10f, -10f);
	[SerializeField] float startMoveDuration = 1.2f;

	[Header("通常時の追従設定")]
	[SerializeField] Vector3 followOffset = new(0f, 4.5f, -6f);
	[SerializeField] float followLerpSpeed = 8f;

	[Header("ゴール演出設定")]
	[SerializeField] Vector3 goalFrontOffset = new(0f, 2.5f, 3f);
	[SerializeField] float goalMoveDuration = 1.2f;

	bool isStartSequence = true;
	bool isGoalSequence;
	float startTimer;
	float goalTimer;
	Vector3 goalStartPosition;
	void Start()
	{
		if (target == null)
		{
			GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
			if (playerObj != null)
			{
				target = playerObj.transform;
                Debug.Log("Playerを設定");
			}
		}

		if (target != null)
		{
			transform.position = target.position + startFrontOffset;
		}
	}

	void LateUpdate()
	{
		if (target == null) return;

		if (isStartSequence)
		{
			startTimer += Time.deltaTime;
			float startT = Mathf.Clamp01(startTimer / Mathf.Max(0.01f, startMoveDuration));
			startT = Mathf.SmoothStep(0f, 1f, startT);

			Vector3 fromPos = target.position + startFrontOffset;
			Vector3 toPos = target.position + followOffset;
			transform.position = Vector3.Lerp(fromPos, toPos, startT);

			if (startT >= 1f)
			{
				isStartSequence = false;
				transform.position = toPos;
				Debug.Log("スタート演出終了");
			}
			return;
		}

		if (!isGoalSequence)
		{
			Vector3 desiredPos = target.position + followOffset;        // プレイヤーの位置にオフセットを加えた位置を計算
			transform.position = Vector3.Lerp(transform.position, desiredPos, followLerpSpeed * Time.deltaTime);
            transform.position = desiredPos; // 追従の補間を無効化して、カメラがプレイヤーの位置に直接追従するように変更

			return;
		}

		goalTimer += Time.deltaTime;
		float t = Mathf.Clamp01(goalTimer / Mathf.Max(0.01f, goalMoveDuration));
		t = Mathf.SmoothStep(0f, 1f, t);

		Vector3 desiredGoalPos = target.position + target.TransformDirection(goalFrontOffset);
		transform.position = Vector3.Lerp(goalStartPosition, desiredGoalPos, t);
		transform.LookAt(target.position);
	}

	public void StartGoalSequence(Transform player)
	{
		if (player == null) return;

		target = player;
		isGoalSequence = true;
		goalTimer = 0f;
		goalStartPosition = transform.position;
		Debug.Log("ゴール演出開始");
	}







}
