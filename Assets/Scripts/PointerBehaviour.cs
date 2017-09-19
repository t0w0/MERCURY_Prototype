using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerBehaviour : MonoBehaviour {

	Ray ray;
	RaycastHit hit;

	public Texture2D cursorTexture;
	public Texture2D cursorTextureLeft;
	public Texture2D cursorTextureRight;
	public Texture2D cursorTextureUp;
	public Texture2D cursorTextureDown;
	private Vector2 hotSpot = Vector2.zero;
	private CursorMode cursorMode = CursorMode.ForceSoftware;

	public HexGrid hexGrid;

	public Material over;
	public Material unover;
	public Material inside;
	public Material outside;
	public Material ghostTown;
	public Material newTerritory;

	private GameObject overObject;

	public GameObject town;
	public GameObject district;
	public GameObject army;
	public GameObject selectHighlight;
	public GameObject inconstructibleEffect;
	public GameObject surtuile;
	public Transform surtuilesParent;

	private GameObject thisTown;
	public float objectRadius = 2; 
	public float maxHeight = 0.5f;

	public List<TileStateManager> Towns;
	public List<TileStateManager> Districts;
	public List<TileStateManager> Armies;
	public List<GameObject> GhostDistricts;
	public List<GameObject> Surtuiles;

	public Collider[] allOverlappingColliders;
	public List<Transform> allOverlappingTilesIn;
	public List<Transform> allOverlappingTilesOut;

	private Transform selectTile;

	private float pitch;
	private float pitchIncr = 0.1f;

	public bool settlingTown = false;
	public bool settlingDistrict = false;
	public bool spawningArmy = false;
	private bool townSettle = false;

	public bool AllUIShown = false;

	void Start () {
	
		Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);

	}

	void Update()
	{
		
		if (Input.GetKeyDown(KeyCode.Space)) {
			CenterOnSelect ();
		}

		else if (Input.GetAxis ("Mouse ScrollWheel") > 0 && pitch < 1) {
			cameraPitch (true);
		}
		else if (Input.GetAxis ("Mouse ScrollWheel") < 0 && pitch > 0) {
			cameraPitch (false);
		}
		else if (Input.GetKeyDown(KeyCode.X)) {
			ShowAllUI ();
		}

		else if (Input.GetMouseButton(0)) {
			
			if (settlingTown && overObject != null) {
				Construct (town);
			}

			else if (settlingDistrict && overObject != null) {
				if (overObject.transform.parent.GetComponent<TileStateManager> ().territory != TileTerritories.Inside)
					return;
				FilterTileFrom (TileTerritories.Empty, false);

				Construct (district);
				DeleteGhostDistrict ();

			}

			else if (spawningArmy && overObject != null) {
				overObject.transform.parent.GetComponent<TileStateManager> ().SpawnArmyOnTile (army, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z));
				Armies.Add (overObject.transform.parent.GetComponent<TileStateManager>());
				overObject.transform.parent.GetComponent<TileStateManager> ().ShowBuilding (true);
				Destroy (thisTown);
				constructionEffect (false);
				spawningArmy = false;
			}

			else if (!settlingTown && !settlingDistrict && !spawningArmy && overObject != null) {
				Select (overObject.transform);
			}
		}

		if (Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width/10) {
			Camera.main.transform.parent.position = new Vector3 ( Camera.main.transform.parent.position.x - 0.5f, Camera.main.transform.parent.position.y , Camera.main.transform.parent.position.z) ;
			Cursor.SetCursor(cursorTextureLeft, hotSpot, cursorMode);
		}
		else if (Input.mousePosition.x < Screen.width && Input.mousePosition.x > 9*Screen.width/10) {
			Camera.main.transform.parent.position = new Vector3 ( Camera.main.transform.parent.position.x + 0.5f, Camera.main.transform.parent.position.y , Camera.main.transform.parent.position.z) ;
			Cursor.SetCursor(cursorTextureRight, hotSpot, cursorMode);
		}
		if (Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height/10) {
			Camera.main.transform.parent.position = new Vector3 ( Camera.main.transform.parent.position.x, Camera.main.transform.parent.position.y , Camera.main.transform.parent.position.z - 0.5f) ;
			Cursor.SetCursor(cursorTextureDown, hotSpot, cursorMode);
		}
		else if (Input.mousePosition.y < Screen.height && Input.mousePosition.y > 9*Screen.height/10) {
			Camera.main.transform.parent.position = new Vector3 ( Camera.main.transform.parent.position.x, Camera.main.transform.parent.position.y , Camera.main.transform.parent.position.z  + 0.5f) ;
			Cursor.SetCursor(cursorTextureUp, hotSpot, cursorMode);
		}
		if (Input.mousePosition.x < 9 * Screen.width/10 && Input.mousePosition.x > Screen.width/10 && Input.mousePosition.y < 9*Screen.height/10 && Input.mousePosition.y > Screen.height/10)
			Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);


		ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit))
		{
			//Si l'objet sous le pointeur n'est pas une tuile ne rien faire
			if (hit.collider.gameObject.tag != "Case")
				return;

			//si une tuile à déja été survolée
			if (overObject != null) {
				
				//si c'est la même retour
				if (overObject == hit.collider.gameObject) {
					return;
				}
			} 

			//si aucune tuile n'a été survolée précédemment
			else {
				overObject = hit.collider.gameObject;
			}

			//si aucun bouton n'est pressé
			if (!settlingTown && !settlingDistrict && !spawningArmy) {

				//overObject.transform.parent.GetComponent<TileStateManager> ().ShowFeeds(false);
				//overObject.transform.parent.GetComponent<TileStateManager> ().ShowTile(true);


				ClearTilesHighlight ();
				
				overObject = hit.collider.gameObject;

				//overObject.GetComponent<MeshRenderer> ().material = over;

				//overObject.transform.parent.GetComponent<TileStateManager> ().ShowFeeds(true);
				//overObject.transform.parent.GetComponent<TileStateManager> ().ShowTile(true);

			}

			//si le bouton créer une ville à été préssé
			else if (settlingTown) {

				ClearTilesHighlight ();

				overObject = hit.collider.gameObject;

				OverlapTilesIn (overObject.transform, objectRadius);
				OverlapTilesOut (allOverlappingTilesIn, objectRadius);

				for (int i = 0; i < allOverlappingTilesOut.Count; i++) {
					//allOverlappingTilesOut [i].GetComponent<MeshRenderer> ().material = outside;
					//allOverlappingTilesOut [i].transform.parent.GetComponent<TileStateManager> ().ShowUI (true);
					allOverlappingTilesOut [i].transform.parent.GetComponent<TileStateManager> ().ShowTile (true);
					allOverlappingTilesOut [i].transform.parent.GetComponent<TileStateManager> ().tile.GetComponent<MeshRenderer> ().material.color = new Color (0.93f, 0.45f, 0.15f, 0.5f);
				}
				for (int i = 0; i < allOverlappingTilesIn.Count; i++) {
					//allOverlappingTilesIn [i].GetComponent<MeshRenderer> ().material = newTerritory;
					allOverlappingTilesIn [i].transform.parent.GetComponent<TileStateManager> ().ShowFeeds (true);
					allOverlappingTilesIn [i].transform.parent.GetComponent<TileStateManager> ().ShowTile (true);
					allOverlappingTilesIn [i].transform.parent.GetComponent<TileStateManager> ().tile.GetComponent<MeshRenderer> ().material.color = new Color(0.75f, 0.82f, 0.22f, 0.5f); ;
				}

				if (overObject != null) {
					if (thisTown == null) {
						thisTown = Instantiate (town, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z), Quaternion.identity);
						thisTown.GetComponentInChildren<MeshRenderer> ().material = ghostTown;
					}
					else
						thisTown.transform.position = new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z);
				}
			}
				
			else if (settlingDistrict && hit.collider.transform.parent.GetComponent<TileStateManager> ().territory == TileTerritories.Inside && hit.collider.transform.parent.GetComponent<TileStateManager> ().building == TileBuildings.Empty) {


				ClearTilesHighlight ();

				overObject = hit.collider.gameObject;

				OverlapTilesIn (hit.collider.transform, objectRadius);
				OverlapTilesOut (allOverlappingTilesIn, objectRadius);

				for (int i = 0; i < allOverlappingTilesOut.Count; i++) {
					if (allOverlappingTilesOut [i].transform.parent.GetComponent<TileStateManager> ().territory == TileTerritories.Empty && allOverlappingTilesOut [i] != overObject.transform) {
						//allOverlappingTilesOut [i].GetComponent<MeshRenderer> ().material = outside;
						//allOverlappingTilesOut [i].transform.parent.GetComponent<TileStateManager> ().ShowUI (true);
						allOverlappingTilesOut [i].transform.parent.GetComponent<TileStateManager> ().ShowFeeds (false);
						allOverlappingTilesOut [i].transform.parent.GetComponent<TileStateManager> ().ShowTile (true);
					}
				}
				for (int i = 0; i < allOverlappingTilesIn.Count; i++) {
					if (allOverlappingTilesIn [i].transform.parent.GetComponent<TileStateManager> ().territory == TileTerritories.Outside && allOverlappingTilesIn [i] != overObject.transform) {
						//allOverlappingTilesIn [i].GetComponent<MeshRenderer> ().material = newTerritory;
						allOverlappingTilesIn [i].transform.parent.GetComponent<TileStateManager> ().ShowFeeds (true);
						allOverlappingTilesIn [i].transform.parent.GetComponent<TileStateManager> ().ShowTile (true);
					}
				}
		
				if (overObject != null) {
					if (thisTown == null) {
						thisTown = Instantiate (district, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z), Quaternion.identity);
						thisTown.GetComponentInChildren<MeshRenderer> ().material = ghostTown;
					}
					else
						thisTown.transform.position = new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z);
				}

			}

			else if (spawningArmy && hit.collider.transform.parent.GetComponent<TileStateManager> ().building != TileBuildings.Town && hit.collider.transform.parent.GetComponent<TileStateManager> ().building != TileBuildings.District) {

				overObject = hit.collider.gameObject;

				if (overObject != null) {
					if (thisTown == null) {
						thisTown = Instantiate (army, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z), Quaternion.identity);
					}
					else
						thisTown.transform.position = new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z);
				}

			}
		}
	}

	public void SettlingMode () {
		constructionEffect (true);
		ShowBuildingsFrom (TileBuildings.Town, false);
		ShowBuildingsFrom (TileBuildings.District, false);

		if (townSettle) {
			FilterTileFrom (TileTerritories.Empty, true);
			settlingDistrict = true;
			ShowGhostDistrict (Towns[0].transform);
			return;
		}
		settlingTown = true;


	}

	public void SpawnMode () {
		constructionEffect (true);

		spawningArmy = true;
	}

	public void OverlapTilesIn (Transform overObject, float rad) {

		allOverlappingTilesIn.Clear ();

		allOverlappingColliders = Physics.OverlapSphere (overObject.transform.position, rad);

		for (int i = 0; i < allOverlappingColliders.Length; i++) {
			
			float heightDifference = Mathf.Abs (overObject.transform.parent.transform.localScale.y - allOverlappingColliders [i].transform.parent.transform.localScale.y);

			if (heightDifference < maxHeight && !allOverlappingTilesIn.Contains(allOverlappingColliders [i].transform)) {
				allOverlappingTilesIn.Add (allOverlappingColliders [i].transform);
			}
		}
	}

	public void OverlapTilesOut (List<Transform> overlap, float rad) {

		allOverlappingTilesOut.Clear ();

		for (int i = 0; i < allOverlappingTilesIn.Count; i++) {
			
			allOverlappingColliders = Physics.OverlapSphere (allOverlappingTilesIn[i].transform.position, rad);

			for (int j = 0 ; j < allOverlappingColliders.Length ; j++) {
				
				float heightDifference = Mathf.Abs (allOverlappingTilesIn[i].transform.parent.transform.localScale.y - allOverlappingColliders [j].transform.parent.transform.localScale.y);

				if (heightDifference < maxHeight && !allOverlappingTilesOut.Contains(allOverlappingColliders [j].transform) && allOverlappingColliders [j].transform.parent.GetComponent<TileStateManager>().territory == TileTerritories.Empty) {
					if (!allOverlappingTilesIn.Contains(allOverlappingColliders [j].transform)) 
						allOverlappingTilesOut.Add (allOverlappingColliders [j].transform);
				}
			}
		}
	}

	public void Select (Transform tr) {
		selectTile = tr;
		ClearTilesHighlight ();
		selectTile.parent.GetComponent<TileStateManager> ().ShowTile (true);
		selectTile.parent.GetComponent<TileStateManager> ().ShowFeeds (true);
		selectHighlight.transform.position = tr.position;
		if (selectTile.parent.GetComponent<TileStateManager> ().unit == TileUnits.Army) {
			SelectArmy ();
		}

	}

	public void CenterOnSelect () {
		Camera.main.transform.position = new Vector3 (selectTile.position.x, selectTile.position.y + 10, selectTile.position.z - 7.5f);
	}

	public void constructionEffect (bool state) {
		for (int i = 0; i < Towns.Count; i++) {
			Towns [i].ConstructionEffect (state);
		}
		for (int i = 0; i < Districts.Count; i++) {
			Districts [i].ConstructionEffect (state);
		}
	}

	public void SelectArmy() {
		ClearTilesHighlight ();
		OverlapTilesIn (selectTile, objectRadius);
		OverlapTilesOut (allOverlappingTilesIn, objectRadius);
		/*for (int i = 0; i < allOverlappingTilesOut.Count; i++) {
			if (allOverlappingTilesOut [i].transform.parent.GetComponent<TileStateManager>().territory != TileTerritories.Inside && allOverlappingTilesOut [i] != overObject.transform)
				allOverlappingTilesOut [i].GetComponent<MeshRenderer> ().material = outside;
		}
		for (int i = 0; i < allOverlappingTilesIn.Count; i++) {
			allOverlappingTilesIn [i].GetComponent<MeshRenderer> ().material = newTerritory;
		}*/
	}

	public void ClearTilesHighlight () {

		foreach (TileStateManager tile in hexGrid.Tiles) {
			
			if (tile.territory == TileTerritories.Empty && tile.transform.GetChild (0) != selectTile) {
				tile.ShowTile (false);
				tile.ShowFeeds (false);
				tile.tile.GetComponent<MeshRenderer> ().material.color = Color.white;
			}
			else if (tile.territory == TileTerritories.Outside) {
				tile.ShowTile (true);
				tile.ShowFeeds (false);
			}
			else if (tile.territory == TileTerritories.Inside) {
				tile.ShowTile (true);
				tile.ShowFeeds (true);
			}
		} 
	}

	public void cameraPitch (bool b) {
		if (b) pitch += pitchIncr;
		else pitch -= pitchIncr;
		Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Mathf.Lerp (5, 20, Mathf.InverseLerp (0, 1, pitch)), Camera.main.transform.position.z);
		Camera.main.transform.rotation = Quaternion.Euler (Mathf.Lerp (35, 60, Mathf.InverseLerp (0, 1, pitch)), 0, 0);
	}

	public void Construct (GameObject prefab) {

		ClearTilesHighlight ();

		overObject.transform.parent.GetComponent<TileStateManager> ().CreateDistrictOnTile (prefab, inconstructibleEffect, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z), inside);
		overObject.transform.parent.GetComponent<TileStateManager> ().ShowBuilding (false);

		if (settlingDistrict)
			Districts.Add (overObject.transform.parent.GetComponent<TileStateManager>());
		if (settlingTown)
			Towns.Add (overObject.transform.parent.GetComponent<TileStateManager>());
		
		Destroy (thisTown);
		constructionEffect (false);
		settlingDistrict = false;
		settlingTown = false;
		townSettle = true;

		foreach (Transform tile in GetAllTilesByBuilding(TileBuildings.District)) {
			tile.GetComponent<TileStateManager>().ShowBuilding (false);
		}
		ShowBuildingsFrom (TileBuildings.Town, true);
		ShowBuildingsFrom (TileBuildings.District, true);

		OverlapTilesIn (hit.collider.transform, objectRadius);
		OverlapTilesOut (allOverlappingTilesIn, objectRadius);

		for (int i = 0; i < allOverlappingTilesOut.Count; i++) {
			if (allOverlappingTilesOut [i].parent.GetComponent<TileStateManager> ().territory == TileTerritories.Empty)
				allOverlappingTilesOut [i].parent.GetComponent<TileStateManager> ().CreateOutsideContent(outside);
		}
		for (int i = 0; i < allOverlappingTilesIn.Count; i++) {
			if (allOverlappingTilesIn [i].parent.GetComponent<TileStateManager> ().territory == TileTerritories.Empty || allOverlappingTilesIn [i].parent.GetComponent<TileStateManager> ().territory == TileTerritories.Outside)
				allOverlappingTilesIn [i].parent.GetComponent<TileStateManager> ().CreateInsideContent(inside);
		}
	}

	public void ShowGhostDistrict (Transform town) {
		List<Transform> inside = GetAllTilesByTerritory (TileTerritories.Inside);

		for (int i = 0; i < inside.Count; i++) {
			if (inside [i].GetComponent<TileStateManager> ().building == TileBuildings.Empty) {
				
				GameObject d = Instantiate (district, new Vector3 (inside [i].GetChild (0).position.x, inside [i].localScale.y, inside [i].GetChild (0).position.z), Quaternion.identity);
				d.GetComponentInChildren<MeshRenderer> ().material = ghostTown;
				GhostDistricts.Add (d);
			}
		}
	}

	public void DeleteGhostDistrict () {
		for (int i = 0; i < GhostDistricts.Count; i++) {
			Destroy (GhostDistricts [i]);
		}
		GhostDistricts.Clear ();
	}

	public List<Transform> GetAllTilesByTerritory (TileTerritories content) {
		List<Transform> myl = new List<Transform>();
		for (int i = 0; i < hexGrid.Tiles.Count; i++) {
			if (hexGrid.Tiles [i].GetComponent<TileStateManager> ().territory == content) {
				myl.Add (hexGrid.Tiles [i].transform);
			}
		}
		return myl;
	}

	public List<Transform> GetAllTilesByBuilding (TileBuildings content) {
		List<Transform> myl = new List<Transform>();
		for (int i = 0; i < hexGrid.Tiles.Count; i++) {
			if (hexGrid.Tiles [i].GetComponent<TileStateManager> ().building == content) {
				myl.Add (hexGrid.Tiles [i].transform);
			}
		}
		return myl;
	}

	public void ShowAllUI () {
		AllUIShown = !AllUIShown;
		foreach (TileStateManager go in hexGrid.Tiles) {
			go.ShowFeeds(AllUIShown);
		}
		
	}

	public void FilterTileFrom (TileTerritories territory, bool b) {
		List<Transform>  tiles = GetAllTilesByTerritory (territory);

		foreach (Transform tr in tiles) {
			if (b) {
				tr.gameObject.layer = 8;
				tr.GetChild(0).gameObject.layer = 8;
			} else {
				tr.gameObject.layer = 0;
				tr.GetChild(0).gameObject.layer = 0;
			}
		}
	}

	public void ShowBuildingsFrom (TileBuildings building, bool b) {
		foreach (Transform tile in GetAllTilesByBuilding(building)) {
			tile.GetComponent<TileStateManager>().ShowBuilding (b);
		}
	}

}
