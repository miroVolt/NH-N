using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GenericEvent), typeof(Animator), typeof(SpriteRenderer))]
public class GenericAnimationEvent : MonoBehaviour {
	public bool endAnimationSequence;
	
	public float speed = 8.0f;
	public float arriveRadius = 0.1f;

	public bool hasOrientation;
	public string[] names;
	public Vector2[] positions;

	[HideInInspector]
	public bool isPlaying;

	GameController gameController;
	Animator animator;

	int animationIndex;
	Vector2 wantedPosition;
	Vector2 deltaPosition;
	int orientationIndex;
	string lastAnimationName;

	void Start () {
		gameObject.name = gameObject.name + "-genericAnimation";
		 
		gameController = GameController.gameController;
		animator = GetComponent<Animator>();

		if (names.Length == 0) {
			return;
		}
		for (int i=0; i<names.Length; i++) {
			if (names[i] == "") {
				names[i] = names[i-1];
			}
		}
	}

	void Update () {
		if (!isPlaying) {
			return;
		} else if (deltaPosition.magnitude < arriveRadius) {
			if (animationIndex < positions.Length - 1) {
				animationIndex ++;

				wantedPosition = (Vector2)transform.position + positions[animationIndex] * GameController.gameScale;
				deltaPosition = wantedPosition - (Vector2)transform.position;
				GetOrientationIndex();
			} else {
				isPlaying = false;
				animationIndex = 0;
				if (endAnimationSequence) {
					Debug.Log(gameObject.name + " - end animation sequence");
					gameController.gameState = GameController.stateSearch;
				}
				gameObject.SendMessage("EventCallBack", SendMessageOptions.RequireReceiver);
				return;
			}
		}

		deltaPosition = wantedPosition - (Vector2)transform.position;

		// Play animation
		string animationName = names[animationIndex];
		if (hasOrientation) {
			animationName += PlayerAnimatorController.s + PlayerAnimatorController.orientationNames[orientationIndex];
		}
		PlayAnimation(animationName);

		transform.position = Vector3.MoveTowards(
			transform.position,
			new Vector3(wantedPosition.x, wantedPosition.y, transform.position.z),
			Time.deltaTime * speed);
	}

	public void OnEvent () {
		Debug.Log(gameObject.name + " - get generic animation event");
		gameController.gameState = GameController.stateAnimation;

		isPlaying = true;
		animationIndex = 0;

		wantedPosition = (Vector2)transform.position + positions[animationIndex] * GameController.gameScale;
		deltaPosition = wantedPosition - (Vector2)transform.position;
		GetOrientationIndex();
	}

	public int GetOrientationIndex () {
		if (deltaPosition.magnitude < arriveRadius) {
			return orientationIndex;
		}

		if (Mathf.Abs(deltaPosition.x) > Mathf.Abs(deltaPosition.y)) {
			if (deltaPosition.x > 0) {
				orientationIndex = PlayerAnimatorController.orientRight;
			} else {
				orientationIndex = PlayerAnimatorController.orientLeft;
			}
		} else if (Mathf.Abs(deltaPosition.x) < Mathf.Abs(deltaPosition.y)) {
			if (deltaPosition.y > 0) {
				orientationIndex = PlayerAnimatorController.orientBack;
			} else {
				orientationIndex = PlayerAnimatorController.orientFront;
			}
		}


		return orientationIndex;
	}


	public void PlayAnimation (string animationName) {
		if (animationName == lastAnimationName) {
			return;
		}
		
		lastAnimationName = animationName;
		
		AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (!animatorStateInfo.IsName(animationName)) {
			animator.Play(animationName, 0);
		}
	}

	public void OnDrawGizmosSelected () {
		Gizmos.color = Color.gray;
		for (int x=-8; x<8; x++) {
			Gizmos.DrawRay(
				transform.position + new Vector3(x * GameController.gameScale, -8, 0),
				Vector3.up * 8 * GameController.gameScale);
		}
		for (int y=-8; y<8; y++) {
			Gizmos.DrawRay(
				transform.position + new Vector3(-8, y * GameController.gameScale, 0),
				Vector3.right * 8 * GameController.gameScale);
		}
		
		Gizmos.color = Color.red;
		Vector2 position = Vector2.zero;
		for (int i=0; i<positions.Length; i++) {
			position += positions[i];
			Gizmos.DrawWireSphere((Vector2)transform.position + position * GameController.gameScale, 0.2f);
		}
	}
}
