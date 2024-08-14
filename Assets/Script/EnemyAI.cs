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
    [SerializeField] float stoppingDistance = 1.5f;
    float distanceToTarget = Mathf.Infinity;
    [SerializeField] bool isProvoked = false;

    [SerializeField] float chaseTimer = 5f;
    [SerializeField] float chaseTimeRemaining;

    [Header("Audio And SFX")]
    AudioSource enemyAudioSource;
    [SerializeField] bool isDetectPlayerSFX = false;

    [SerializeField] AudioClip provokedSFX;
    [SerializeField][Range(0, 1)] float detectPlayerVolume = 0.5f;
    [SerializeField] AudioClip giveUpChasingSFX;
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

            // giảm thời gian đuổi
            if (chaseTimeRemaining <= chaseTimer && chaseTimeRemaining > Mathf.Epsilon)
            {
                chaseTimeRemaining -= Time.deltaTime;
            }
            Mathf.Clamp(chaseTimeRemaining, 0, chaseTimer);
        }
        else if (isProvoked && distanceToTarget > chaseRange)
        {
            // ShootPlayer(); if in the future there is an archer zombie of some sort
            
        };
    }

    private void ChaseBehaviour()
    {
        if (distanceToTarget <= chaseRange && !isProvoked)
        {
            isProvoked = true;
            Debug.Log("PLAYER SPOTTED AND CHASING");

        }
        
        // nếu đã về vị trí đứng gác và !isProvoked -> reset chaseTimeRemaining để đuổi lượt mới
        if (distanceToTarget < 1f && !isProvoked)
        {
            chaseTimeRemaining = chaseTimer;
        }

    }

    void ChasePlayer()
    {
        // tăng tốc lao tới player
        navMeshAgent.SetDestination(player.position);
        navMeshAgent.speed = navMeshAgent.acceleration;

        // nếu đang trong trạng thái calm chuyển sang isProvoked thì sẽ chạy âm thanh provokedSFX
        if (!isDetectPlayerSFX && !enemyAudioSource.isPlaying)
        {
            // phát âm thanh báo hiệu bắt đầu đuổi player 
            //enemyAudioSource.PlayOneShot(provokedSFX, detectPlayerVolume);

            Debug.Log("Agressive Audio Played");
            isDetectPlayerSFX = true;

        }

        // nếu hết chaseTimer và player đã ra ngoài tầm chaseRange -> dừng đuổi
        if (chaseTimeRemaining <= Mathf.Epsilon && distanceToTarget > chaseRange)
        {
            StopChasing();

        }
        // nếu hết chaseTimer và player vẫn trong tầm chaseRange -> tiếp tục đuổi = reset chaseTimer
        else if (chaseTimeRemaining <= Mathf.Epsilon && distanceToTarget < chaseRange)
        {
            chaseTimeRemaining = chaseTimer;
        }

        if (distanceToTarget < 2f)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        // attack player
        Debug.Log(name + " Hit " + player.name);

        // continue pursuit 
        chaseTimeRemaining = chaseTimer;
    }

    void StopChasing()
    {
        isProvoked = false;

        // nếu đuổi hết chaseTimer mà vẫn chưa bắt đc ng chơi
        if (isDetectPlayerSFX && !enemyAudioSource.isPlaying)
        {
            // phát âm thanh thông báo đã bỏ cuộc
            //enemyAudioSource.PlayOneShot(giveUpChasingSFX, giveupVolume);
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

    void OnDrawGizmosSelected()
    {
        //Display Path When Selected
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, chaseRange);

    }
}
