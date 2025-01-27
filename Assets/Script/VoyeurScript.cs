﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class VoyeurScript : MonoBehaviour {

    public float Speed;
	public float TurnSpeed;
	FOV2DEyes eyes;
	FOV2DVisionCone visionCone;
	Animator anim;
	bool targetAcquire = false;

	public bool TakingPicture = false;

	private Rigidbody _rigidBody;
    private SoundManager soundManager;


	// Use this for initialization
	void Start () {
		anim = GetComponentInChildren<Animator> ();
		_rigidBody = GetComponent<Rigidbody> ();
		eyes = GetComponentInChildren<FOV2DEyes>();
		visionCone = GetComponentInChildren<FOV2DVisionCone>();

		InvokeRepeating ("CheckVision", 0, 0.3f);
		//GameManager.getInstance().setPlayerPos(this.transform.position);
        soundManager = SoundManager.getInstance();
		GameManager.getInstance ().SetPlayer (this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		float forwardVel = Input.GetAxis ("Horizontal");
		float sideVel = Input.GetAxis ("Vertical");

		anim.SetBool ("move", forwardVel != 0 || sideVel != 0);
		anim.SetBool ("sneak", Input.GetKey (KeyCode.LeftShift));

		float rotation = (Input.GetKey (KeyCode.Q) ? -1 : 0) + (Input.GetKey (KeyCode.E) ? 1 : 0);
		Vector3 velocity = new Vector3 (forwardVel, 0, sideVel) * Speed;
		velocity = transform.TransformDirection (velocity);
		_rigidBody.velocity = velocity;

		//_rigidBody.transform.Translate(velocity * Speed * Time.deltaTime);
		_rigidBody.transform.Rotate (new Vector3(0, rotation,0), TurnSpeed * Time.deltaTime);

		if (targetAcquire && Input.GetMouseButtonDown (0)) {
			visionCone.status = FOV2DVisionCone.Status.Suspicious;
			anim.SetBool("camera", true);
			TakingPicture = true;
            soundManager.StartCameraSound();
		} else if (targetAcquire) {
			if(visionCone.status == FOV2DVisionCone.Status.Suspicious)
			{
				TakePicture();
			}
			anim.SetBool("camera", false);
			TakingPicture = false;
			visionCone.status = FOV2DVisionCone.Status.Alert;
		}else{
			anim.SetBool("camera", false);
			TakingPicture = false;
			visionCone.status = FOV2DVisionCone.Status.Idle;
		}
	}

    public void LateUpdate()
    {
        if (anim.GetNextAnimatorStateInfo(0).IsName("Walk"))
        {
            //soundManager.StartParentScream();
        }
    }

	void CheckVision(){
	 	targetAcquire = false;
		CalculatePoints (false);
	}

	void TakePicture()
	{
		CalculatePoints (true);
		GameManager.getInstance ().setPlayerPos (this.transform.position);
	}

	void CalculatePoints(bool isPhoto)
	{
		ArrayList targetHits = new ArrayList ();
		foreach (RaycastHit hit in eyes.hits) 
		{
			if (hit.transform && hit.transform.tag == "Girl" && !targetHits.Contains(hit.transform.gameObject.GetHashCode())) 
			{
				targetAcquire = true;
				targetHits.Add(hit.transform.gameObject.GetHashCode());
				int points = (int)(Mathf.Min(10, 20 * Mathf.Max(0f, 1 - hit.distance/eyes.fovMaxDistance)));
				if(isPhoto)
				{
					points *= 100;
				}

				GameState gs = GameManager.getInstance().getGameState();

				if(gs == GameState.Stealth)
					points *=2;
				else if(gs == GameState.Detected)
					points /=2;

				GameManager.getInstance().updatePoints(points);
			}
		}
	}
    void OnTriggerEnter(Collider other)
    {
        anim.SetBool("jump", other.tag == "Window");        
    }
	void endGame()
	{
		GameManager.getInstance ().Win ();
	}
}
