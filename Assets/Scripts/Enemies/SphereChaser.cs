﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SphereChaser : MonoBehaviour
{
    NavMeshAgent agent;
    GameObject James;
    PlayerController pc;
    AudioSource sphereExplosion;

    // Start is called before the first frame update
    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        James = GameObject.FindGameObjectWithTag("James");
        sphereExplosion = GetComponent<AudioSource>();
        if(James != null)
            pc = James.GetComponent<PlayerController>();
        StartCoroutine(ControlAI());
    }

    private void OnCollisionEnter(Collision collision)
    {
        sphereExplosion.Play();
        if(collision.gameObject.layer == LayerMask.NameToLayer("Good"))
        {
            GameObject deathParticles = ObjectPooler.sharedInstance.GetPooledObject("DeathParticles");
            if(deathParticles != null)
            {
                deathParticles.transform.position = gameObject.transform.position;
                deathParticles.transform.rotation = gameObject.transform.rotation;
                deathParticles.SetActive(true);
            }
            gameObject.SetActive(false); 

            foreach(GameObject bullet in pc.bullets)
            {
                if(collision.gameObject.GetInstanceID() == bullet.GetInstanceID() - 4)
                {
                    if(LaserIndicator.instance.onLaserRestored != null)
                        LaserIndicator.instance.onLaserRestored.Invoke();

                    bullet.SetActive(false);
                    pc.bullets.Remove(bullet);

                    break;
                }
            }

            GameObject enemy = ObjectPooler.sharedInstance.GetPooledObject("SphereChaser");
            if(enemy != null)
            {
                enemy.transform.position = RandomNavmeshLocation(20);
                enemy.transform.rotation = gameObject.transform.rotation;
                enemy.SetActive(true);
            }
        }

        if(collision.gameObject.layer == LayerMask.NameToLayer("James"))
        {
            collision.gameObject.SetActive(false);
            if(pc.onJamesDeath != null)
                pc.onJamesDeath.Invoke();
        }
    }


    public IEnumerator ControlAI()
    {
        while(gameObject.activeSelf)
        {
            yield return StartCoroutine(CheckForPlayer());
        }
        yield return null;
    }

    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if(NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    public IEnumerator MoveAI(Vector3 destination)
    {
        agent.SetDestination(destination);
        yield return null;
    }

    public IEnumerator CheckForPlayer()
    {
        yield return StartCoroutine(MoveAI(James.transform.position));
        yield return null;
    }
}
