using System;
using System.Collections;
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
    [SerializeField] float chaseRange = 5f;
    float distanceToTarget = Mathf.Infinity;
    [SerializeField] bool isProvoked = false;

    [SerializeField] float chaseTimer = 5f;
    [SerializeField] float chaseTimeRemaining;

    [Header("Audio And SFX")]
    AudioSource enemyAudioSource;
    [SerializeField] bool isDetectPlayerSFX = false;

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
        distanceToTarget = Vector3.Distance(transform.position, player.position);

        ChaseBehaviour();
        if (isProvoked && distanceToTarget < chaseRange)
        {
            ChasePlayer();
        }
        else if (isProvoked && distanceToTarget > chaseRange)
        {
            // ShootPlayer(); if in the future there is an archer zombie of some sort
            return;
        };

        ResetChaseTime();

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("The Enemy has hit player");

            chaseTimeRemaining = chaseTimer;
        }
    }

    private void ChaseBehaviour()
    {
        if (distanceToTarget <= chaseRange && !isProvoked)
        {
            isProvoked = true;
            Debug.Log("PLAYER SPOTTED AND CHASING");

        }
    }

    void ChasePlayer()
    {
        // giảm thời gian đuổi
        if (chaseTimeRemaining <= chaseTimer && chaseTimeRemaining > Mathf.Epsilon)
        {
            chaseTimeRemaining -= Time.deltaTime;
        }
        Mathf.Clamp(chaseTimeRemaining, 0, chaseTimer);

        // tăng tốc lao tới người chơi
        navMeshAgent.SetDestination(player.position);
        navMeshAgent.speed = navMeshAgent.acceleration;

        // nếu đang trong trạng thái calm chuyển sang isProvoked thì sẽ chạy âm thanh
        if (!isDetectPlayerSFX && !enemyAudioSource.isPlaying)
        {
            // phát âm thanh báo hiệu bắt đầu đuổi người chơi 
            //enemyAudioSource.PlayOneShot(detectPlayerSFX, detectPlayerVolume);

            Debug.Log("Agressive Audio Played");
            isDetectPlayerSFX = true;

        }

        // nếu hết chaseTimer và người chơi đã ra ngoài tầm chaseRange -> dừng đuổi
        if (chaseTimeRemaining <= Mathf.Epsilon && distanceToTarget > chaseRange)
        {
            StopChasing();

        }
        // nếu hết chaseTimer và người chơi vẫn trong tầm chaseRange -> tiếp tục đuổi thêm chaseTimer
        else if (chaseTimeRemaining <= Mathf.Epsilon && distanceToTarget < chaseRange)
        {
            chaseTimeRemaining = chaseTimer;
        }

    }

    void StopChasing()
    {
        isProvoked = false;

        // nếu đuổi hết chaseTimer mà vẫn chưa bắt đc ng chơi
        if (isDetectPlayerSFX && !enemyAudioSource.isPlaying)
        {
            // phát âm thanh thông báo đã bỏ cuộc
            //enemyAudioSource.PlayOneShot(giveupPlayerSFX, giveupVolume);
            Debug.Log("Give up Audio Played");

            isDetectPlayerSFX = false;
        }

        // giảm tốc độ của enemy về trạng thái bình thường
        navMeshAgent.speed = baseSpeed;

        ReturnToPost();

    }

    void ReturnToPost()
    {
        // quay trở lại guardPost
        navMeshAgent.SetDestination(guardPosition.position);

    }

    void ResetChaseTime()
    {
        // nếu đã về vị trí đứng gác và !isProvoked
        if (distanceToTarget < 1f && !isProvoked)
        {
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
