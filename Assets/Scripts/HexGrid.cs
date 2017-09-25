using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour {

	public Transform TilePrefab;
	public List<TileStateManager> Tiles;
	public GameObject TileUI;
	public Transform UIParent;
	public TileStateManager[,] grid;
	public List<TileStateManager> path;

	public float height = 2.0f;
	public float radius = 0.87f;
	public bool useAsInnerCircleRadius = true;
	public float UIDist = 0.1f;

	public Material[] GroundText;
	public Texture2D heightmap;
	public Vector3 size = new Vector3(50, 5, 50);

	private float offsetX, offsetY;

	void Start() {
		float unitLength = ( useAsInnerCircleRadius )? (radius / (Mathf.Sqrt(3)/2)) : radius;

		offsetX = unitLength * Mathf.Sqrt(3);
		offsetY = unitLength * 1.5f;

		grid = new TileStateManager[50, 50];

		for( int i = 0; i < 50; i++ ) {
			for( int j = 0; j < 50; j++ ) {

				Vector2 hexpos = HexOffset( i, j );
				Vector3 pos = new Vector3( hexpos.x, 0, hexpos.y );
				Transform hex = GameObject.Instantiate(TilePrefab, pos, Quaternion.identity, transform);
				hex.localPosition = pos;

				int x = Mathf.FloorToInt(pos.x / size.x * heightmap.width);
				int z = Mathf.FloorToInt(pos.z / size.z * heightmap.height);
				hex.localScale = new Vector3 (1, heightmap.GetPixel(x, z).grayscale * size.y, 1);
				bool walkable = (heightmap.GetPixel(x, z).g > 0f ? true : false); 
				hex.GetComponentInChildren<MeshRenderer> ().material = GroundText [0];
				hex.GetComponentInChildren<MeshRenderer> ().material.mainTextureOffset = new Vector2 (i * 0.04f, 0.04f * j);

				Vector3 UIPos = new Vector3 (pos.x, hex.localScale.y + UIDist, pos.z);
				GameObject UI = GameObject.Instantiate(TileUI, UIPos, Quaternion.identity, transform) as GameObject;
				UI.transform.localPosition = UIPos;
				Tiles.Add (hex.GetComponent<TileStateManager>());

				//bool walkable = true;
				hex.GetComponent<TileStateManager>().InitTile (walkable, pos, i, j, UI);
				grid [i, j] = hex.GetComponent<TileStateManager> ();
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

	public List<TileStateManager> GetNeighbours(TileStateManager tile) {
		List<TileStateManager> neighbours = new List<TileStateManager> ();

		Collider[] cols = Physics.OverlapSphere (tile.worldPosition, 2);

		foreach (Collider c in cols) {
			neighbours.Add (c.transform.parent.GetComponent<TileStateManager> ());
		}

		return neighbours;
	}
}

