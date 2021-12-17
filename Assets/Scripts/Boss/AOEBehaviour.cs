using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEBehaviour : AttackBehaviour
{
    public bool selected = false;
    public static PlayerController player;
    // boss will only start charging up for attack when player is within this distance
    public float maxChargingDistance = 4;
    public float radius = 8;
    public float damage = 10f;
    public float fullAudioLength = 3.0f;
    public float maxVolume = 0.2f;
    public bool attacked = false;
    public AudioSource source;
    public EnemyHealth healthAndNav;
    public BossBehaviour boss;
    private Collider2D col;

    void Awake()
    {
        player = player == null ? GameObject.Find("Player").GetComponent<PlayerController>() : player;
        healthAndNav = GetComponent<EnemyHealth>();
        boss = GetComponent<BossBehaviour>();
    }

    override public bool attack() {
        if (Vector2.Distance(transform.position, player.transform.position) < maxChargingDistance) {
            StartCoroutine(attackSeq());
            return true;
        }
        return false;
    }

    public override string getAttackTag()
    {
        return "AOE";
    }

    public void inflictDmg() {
        // start animation
        // Debug.Log("aoe unleashed");
        boss.animator.SetTrigger("Shockwave");
        if (Vector2.Distance(transform.position, player.transform.position) <= radius) {
            col.enabled = false;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position);
            // Debug.Log("hit " + hit.collider.gameObject.name);
            // player can hide behind wall
            if (hit.collider.gameObject.GetComponent<PlayerController>() != null)
                player.deductHealth(damage);
            col.enabled = true;
        }
    }

    // gradual increase in volume
    IEnumerator AudioQueue() {
        while (true) {
            // Debug.Log("Vol " + source.volume + " " + maxVolume);
            yield return new WaitForSeconds(0.1f);
            if (source.volume < maxVolume) {
                source.volume += 0.02f;
            }
        }
    }

    IEnumerator StopMovePrepareAttack() {
        yield return new WaitForSeconds(fullAudioLength * 0.8f);
        healthAndNav.inAttackSeq = true;
        yield return null;
    }

    IEnumerator attackSeq() {
        StartCoroutine(StopMovePrepareAttack());
        yield return new WaitForSeconds(fullAudioLength * (1 - player.hear / 100));
        source.Play();
        IEnumerator audioQueue = AudioQueue();
        StartCoroutine(audioQueue);
        yield return new WaitForSeconds(player.hear / 100 * fullAudioLength);
        StopCoroutine(audioQueue);
        inflictDmg();
        source.Stop();
        source.volume = 0;
        boss.attackCompleted = true;
        yield return new WaitForSeconds(0.5f);
        healthAndNav.inAttackSeq = false;
        yield return null;
    }
}
