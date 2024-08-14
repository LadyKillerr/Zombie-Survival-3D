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
    [SerializeField] float attackingDistance = 1.5f;
    float distanceToTarget = Mathf.Infinity;
    [SerializeField] bool isProvoked = false;
    [SerializeField] bool isHitting = false;
    [SerializeField] float hitDelay = 1.5f;

    [SerializeField] float chaseTimer = 5f;
    [SerializeField] float chaseTimeRemaining;

    [Header("Audio And SFX")]
    AudioSource enemyAudioSource;


    [SerializeField] AudioClip provokedSFX;
    [SerializeField][Range(0, 1)] float provokedSFXVol = 0.5f;
    [SerializeField] AudioClip giveUpEngagingSFX;
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

        ToggleIsProvoked();
        ChaseTimeCountdown();

    }

    void ChaseTimeCountdown()
    {
        // giảm thời gian đuổi
        if (isProvoked && chaseTimeRemaining > Mathf.Epsilon)
        {
            chaseTimeRemaining -= Time.deltaTime;
            Mathf.Clamp(chaseTimeRemaining, 0, chaseTimer);
        }
        // nếu đã về vị trí đứng gác và !isProvoked -> reset chaseTimeRemaining để đuổi lượt mới
        else if (Vector3.Distance(transform.position, guardPosition.position) < 1f && !isProvoked)
        {
            chaseTimeRemaining = chaseTimer;
        }
    }

    void ToggleIsProvoked()
    {
        if (distanceToTarget <= chaseRange && !isProvoked)
        {
            isProvoked = true;
            Debug.Log("PLAYER SPOTTED AND CHASING");

            // nếu đang trong trạng thái calm chuyển sang isProvoked thì sẽ chạy âm thanh provokedSFX
            if (enemyAudioSource != null && !enemyAudioSource.isPlaying)
            {
                // phát âm thanh báo hiệu bắt đầu đuổi player 
                //enemyAudioSource.PlayOneShot(provokedSFX, provokedSFXVol);

                Debug.Log("Agressive Audio Played");


            }
        }
        else if (isProvoked)
        {
            EngagePlayer();
        }


    }

    void EngagePlayer()
    {
        if (distanceToTarget > attackingDistance)
        {
            ChasePlayer();
        }
        else if (distanceToTarget <= attackingDistance)
        {
            AttackPlayer();
        }

    }

    private void ChasePlayer()
    {
        // tăng tốc lao tới player
        navMeshAgent.SetDestination(player.position);
        navMeshAgent.speed = navMeshAgent.acceleration;



        // nếu hết chaseTimer và player đã ra ngoài tầm chaseRange -> dừng đuổi
        if (chaseTimeRemaining <= Mathf.Epsilon && distanceToTarget > chaseRange)
        {
            Debug.Log("Stopped Chasing the player");
            StopChasing();

        }
        // nếu hết chaseTimer và player vẫn trong tầm chaseRange -> tiếp tục đuổi = reset chaseTimer
        else if (chaseTimeRemaining <= Mathf.Epsilon && distanceToTarget < chaseRange)
        {
            chaseTimeRemaining = chaseTimer;
        }
    }

    private void AttackPlayer()
    {
        if (!isHitting)
        {
            // attack player
            Debug.Log(name + " Hit " + player.name);

            // continue pursuit 
            chaseTimeRemaining = chaseTimer;

            isHitting = true;
            StartCoroutine(ResetIsHitting(hitDelay));
        }
    }

    IEnumerator ResetIsHitting(float hitDelay)
    {
        yield return new WaitForSeconds(hitDelay);

        isHitting = false;
    }

    void StopChasing()
    {
        isProvoked = false;

        // nếu đuổi hết chaseTimer mà vẫn chưa bắt đc ng chơi
        if (enemyAudioSource != null && !enemyAudioSource.isPlaying)
        {
            // phát âm thanh thông báo đã bỏ cuộc
            //enemyAudioSource.PlayOneShot(giveUpEngagingSFX, giveupVolume);
            Debug.Log("Give up Audio Played");
        }

        // giảm tốc độ của enemy về trạng thái bình thường
        navMeshAgent.speed = baseSpeed;

        ReturnToPost();

    }

    void ReturnToPost()
    {
        // quay trở lại guardPost
        navMeshAgent.SetDestination(guardPosition.position);

        Debug.Log("Let enemy return to guard post");
    }

    void OnDrawGizmosSelected()
    {
        //Display Path When Selected
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, chaseRange);

    }
}
