using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCam : MonoBehaviour {

	public Vector3 rotOffset = new Vector3 (0, 180, 0);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 v = Camera.main.transform.position;
		transform.LookAt (v);
		transform.Rotate(rotOffset);
	}
}
