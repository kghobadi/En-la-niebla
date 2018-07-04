using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deactivate : MonoBehaviour {

    Transform _player;
    public float deactivationDistance;

	void Start () {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
		if(Vector3.Distance(_player.position, transform.position) > deactivationDistance)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
	}
}
