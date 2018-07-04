using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerPos : MonoBehaviour {
    Transform _player;

	void Start () {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = _player.position;
	}
}
