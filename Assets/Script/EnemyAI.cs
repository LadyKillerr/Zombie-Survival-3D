using System;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


public class EnemyAI : MonoBehaviour
{
    [Header("Target Coordinates")]
    [SerializeField] Transform guardPosition;
    [SerializeField] Transform player;

    [Header("Chase Behaviour")]
    [SerializeField] bool hasHitPlayer = false;
    [SerializeField] float chaseRange = 5f;
    float distanceToTarget = Mathf.Infinity;
    [SerializeField] bool isTired = false;

    [SerializeField] float chaseTimer = 5f;
    [SerializeField] float chaseTimeRemaining;

    [Header("Audio And SFX")]
    AudioSource enemyAudioSource;
    [SerializeField] bool isDetectPlayer = false;

    [SerializeField] AudioClip detectPlayerSFX;
    [SerializeField][Range(0, 1)] float detectPlayerVolume = 0.5f;
    [SerializeField] AudioClip giveupPlayerSFX;
    [SerializeField][Range(0, 1)] float giveupVolume = 0.5f;


    // initialization variables
    NavMeshAgent navMeshAgent;
    float baseSpeed;




    void Start()
    {
        enemyAudioSource = GetComponent<AudioSource>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        baseSpeed = navMeshAgent.speed;

        navMeshAgent.transform.position = guardPosition.transform.position;

        chaseTimeRemaining = chaseTimer;
    }

    void Update()
    {
        CheckEnemyStatus();

        ChaseBehaviour();


    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            hasHitPlayer = true;
            Debug.Log("The Enemy has hit player");

            chaseTimeRemaining = chaseTimer;
        }
    }

    private void ChaseBehaviour()
    {
        // tính khoảng cách tới player
        distanceToTarget = Vector3.Distance(transform.position, player.position);

        if (distanceToTarget <= chaseRange && !isTired)
        {
            ChasePlayer();
            Debug.Log("PLAYER SPOTTED AND CHASING");

        }
    }



    void ChasePlayer()
    {
        isTired = false;
        // giảm thời gian đuổi
        if (chaseTimeRemaining <= chaseTimer && chaseTimeRemaining > Mathf.Epsilon)
        {
            chaseTimeRemaining -= Time.deltaTime;
        }
        Mathf.Clamp(chaseTimeRemaining, 0, chaseTimer);

        // tăng tốc lao tới người chơi
        navMeshAgent.SetDestination(player.position);
        navMeshAgent.speed = navMeshAgent.acceleration;

        // nếu đang trong trạng thái calm
        if (!isDetectPlayer && !enemyAudioSource.isPlaying)
        {
            // phát âm thanh báo hiệu bắt đầu đuổi người chơi 
            //enemyAudioSource.PlayOneShot(detectPlayerSFX, detectPlayerVolume);

            Debug.Log("Agressive Audio Played");
            isDetectPlayer = true;

        }

        // nếu đã bắt được người chơi khi đang đuổi thì reset chaseTimeRemaining để tiếp tục đuổi 
        if (hasHitPlayer)
        {
            chaseTimeRemaining = chaseTimer;
            hasHitPlayer = false;
        }

        // nếu quá chaseTimer mà vẫn chưa bắt được người chơi
        if (chaseTimeRemaining <= Mathf.Epsilon)
        {
            StopChasing();

        }

    }

    void StopChasing()
    {
        isTired = true;

        // nếu đuổi hết chaseTimer mà vẫn chưa bắt đc ng chơi
        if (isDetectPlayer && !enemyAudioSource.isPlaying)
        {
            // phát âm thanh thông báo đã bỏ cuộc
            //enemyAudioSource.PlayOneShot(giveupPlayerSFX, giveupVolume);
            Debug.Log("Give up Audio Played");

            isDetectPlayer = false;
        }

        // giảm tốc độ của enemy 
        navMeshAgent.speed = baseSpeed;

        ReturnToPost();

    }

    void ReturnToPost()
    {
        // quay trở lại guardPost
        navMeshAgent.SetDestination(guardPosition.position);

    }

    private void CheckEnemyStatus()
    {
        if (Vector3.Distance(transform.position, guardPosition.position) < 0.1f && isTired)
        {
            isTired = false;
            chaseTimeRemaining = chaseTimer;
        }
    }

    void OnDrawGizmosSelected()
    {
        //Display Path When Selected
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, chaseRange);

    }
}
