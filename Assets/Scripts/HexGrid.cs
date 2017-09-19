using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour {
	
	public Transform TilePrefab;
	public List<TileStateManager> Tiles;
	public GameObject TileUI;
	public Transform UIParent;

	public int x = 50;
	public int y = 50;

	public float height = 2.0f;
	public float radius = 0.87f;
	public bool useAsInnerCircleRadius = true;
	public float UIDist = 0.1f;

	public Material[] GroundText;

	private float offsetX, offsetY;

	void Start() {
		float unitLength = ( useAsInnerCircleRadius )? (radius / (Mathf.Sqrt(3)/2)) : radius;

		offsetX = unitLength * Mathf.Sqrt(3);
		offsetY = unitLength * 1.5f;

		for( int i = 0; i < x; i++ ) {
			for( int j = 0; j < y; j++ ) {
				Vector2 hexpos = HexOffset( i, j );
				Vector3 pos = new Vector3( hexpos.x, 0, hexpos.y );
				Transform hex = GameObject.Instantiate(TilePrefab, pos, Quaternion.identity, transform);
				hex.localPosition = pos;
				hex.localScale = new Vector3 (1, Random.Range(1f, Random.Range (1f, Random.Range (1f, height))), 1);
				hex.GetComponentInChildren<MeshRenderer> ().material = GroundText [Random.Range(0,Random.Range (0, GroundText.Length+1))];
				Vector3 UIPos = new Vector3 (pos.x, hex.localScale.y + UIDist, pos.z);
				GameObject UI = GameObject.Instantiate(TileUI, UIPos, Quaternion.identity, transform) as GameObject;
				UI.transform.localPosition = UIPos;
				Tiles.Add (hex.GetComponent<TileStateManager>());
				hex.GetComponent<TileStateManager>().InitTile (UI);
			}
		}
	}

	Vector2 HexOffset( int x, int y ) {
		Vector2 position = Vector2.zero;

		if( y % 2 == 0 ) {
			position.x = x * offsetX;
			position.y = y * offsetY;
		}
		else {
			position.x = ( x + 0.5f ) * offsetX;
			position.y = y * offsetY;
		}

		return position;
	}
}

