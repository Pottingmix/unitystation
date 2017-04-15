﻿using PlayGroup;
using NPC;
using UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Kill: NetworkBehaviour{

    public Sprite deadSprite;
    public GameObject meatPrefab;
    public GameObject corpsePrefab;
    public int amountSpawn = 1;

    private SpriteRenderer spriteRenderer;
    private RandomMove randomMove;
    private bool dead = false;
    private bool sliced = false;

    public override void OnStartClient() {
        spriteRenderer = GetComponent<SpriteRenderer>();
		randomMove = GetComponent<RandomMove>();
		base.OnStartClient();
    }

    void OnMouseDown() {
        if(UIManager.Hands.CurrentSlot.Item != null && PlayerManager.PlayerInReach(transform)) {
            if(UIManager.Hands.CurrentSlot.Item.GetComponent<ItemAttributes>().type == ItemType.Knife) {
                if(!dead) {
                    //Send death to all clients for pete
                    PlayerManager.LocalPlayerScript.playerNetworkActions.CmdKillNpc(this.gameObject);
                } else if(!sliced) {
                   //Spawn the new meat
                    PlayerManager.LocalPlayerScript.playerNetworkActions.CmdGibNpc(this.gameObject);
                    sliced = true;
                }
            }
        }
    }

	void OnCollisionEnter2D (Collision2D coll){
		if (isServer)
		{
			BulletBehaviour b = coll.gameObject.GetComponent<BulletBehaviour>();
			if (b != null)
			{
				if (!dead)
				{
					RpcDie();
				}
				else if (!sliced)
				{
					sliced = true;
					Gib();
				}
			}
		}
	}

    [ClientRpc]
    public void RpcDie() {
        dead = true;
        randomMove.enabled = false;
        spriteRenderer.sprite = deadSprite;
        SoundManager.Play("Bodyfall", 0.5f);
    }

    //server only
    public void Gib() {

            // Spawn Meat
            for(int i = 0; i < amountSpawn; i++) {
            GameObject meat = Instantiate(meatPrefab, transform.position, Quaternion.identity) as GameObject; 
            NetworkServer.Spawn(meat);
        }

            // Spawn Corpse
        GameObject corpse = Instantiate(corpsePrefab, transform.position, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(corpse);
        NetworkServer.Destroy(this.gameObject);
    }
        
}