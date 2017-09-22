using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PointerBehaviour : MonoBehaviour {

	//MouseOver
	private Ray ray;
	private RaycastHit hit;

	//CursorTextures
	public Texture2D cursorTexture;
	public Texture2D cursorTextureLeft;
	public Texture2D cursorTextureRight;
	public Texture2D cursorTextureUp;
	public Texture2D cursorTextureDown;
	private Vector2 hotSpot = Vector2.zero;
	private CursorMode cursorMode = CursorMode.ForceSoftware;
	private Vector3 mousePos;

	//Dependencies
	public HexGrid hexGrid;

	//Materials
	public Material ghostTown;

	//Colors
	public Color selectionColor;
	public Color gridColor;
	public Color insideColor;
	public Color outsideColor;
	public Color inconstructColor;

	//Prefabs
	public GameObject town;
	public GameObject district;
	public GameObject army;
	public GameObject selectHighlight;
	public GameObject[] inconstructibleEffects;
	public GameObject surtuile;
	public Transform surtuilesParent;
	public GameObject townCartouche;

	//Parameters
	public float objectRadius = 2; 
	public float maxHeight = 0.5f;
	private float pitch; 					//Camera pitch
	private float pitchIncr = 0.05f;
	private float panIncr = 0.5f;

	private GameObject overObject;
	private GameObject buildingInstance;
	private Transform selectedTile;
	public GameObject terrain;

	public List<TileStateManager> Towns;
	public List<TileStateManager> Districts;
	public List<TileStateManager> Armies;
	public List<GameObject> GhostDistricts;
	public List<GameObject> Surtuiles;

	public Collider[] allOverlappingColliders;
	public List<TileStateManager> allOverlappingTilesIn;
	public List<TileStateManager> allOverlappingTilesOut;
	public List<TileStateManager> inconstructibleOverlappingTiles;

	//States
	public bool settlingTown = false;
	public bool settlingDistrict = false;
	public bool spawningArmy = false;
	private bool townSettle = false;
	private int effectIndex = 0;

	public bool AllFeedsShown = false;

	void Start () {
		Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
	}

	void Update()
	{
		//Inputs
		if (Input.GetKeyDown (KeyCode.Space))
			CenterOnSelect ();
		else if (Input.GetKeyDown (KeyCode.X))
			ShowAllFeeds ();
		else if (Input.GetKeyDown (KeyCode.C))
			SettlingMode ();
		else if (Input.GetAxis ("Mouse ScrollWheel") > 0 && pitch < 1)
			cameraPitch (true);
		else if (Input.GetAxis ("Mouse ScrollWheel") < 0 && pitch > 0)
			cameraPitch (false);
		else if (mousePos != Input.mousePosition)
			PanningCamera ();
		else if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit ();
		else if (Input.GetKeyDown (KeyCode.E)) {
			if (effectIndex < inconstructibleEffects.Length-1)
				effectIndex++;
			else
				effectIndex = 0;
			ChangeEffects ();
		}
		else if (Input.GetKeyDown (KeyCode.R)) {
			SceneManager.LoadScene (0);
		}
		else if (Input.GetKeyDown (KeyCode.A)) {
			SpawnMode();
		}

		//OnMouseClick
		else if (Input.GetMouseButton(0) && overObject != null) {

			TileStateManager overTile = overObject.transform.parent.GetComponent<TileStateManager> ();

			if (settlingTown) {
				Construct (town);
			}

			else if (settlingDistrict) {
				if (overTile.territory != TileTerritories.Inside)
					return;
				FilterTileFrom (TileTerritories.Empty, false);
				Construct (district);
				DeleteGhostDistrict ();
			}

			else if (spawningArmy) {
				overTile.SpawnArmyOnTile (army, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z));
				Armies.Add (overTile);
				//overTile.ShowBuilding (true);
				Destroy (buildingInstance);
				constructionEffect (false);
				spawningArmy = false;
			}

			else if (!settlingTown && !settlingDistrict && !spawningArmy) {
				Select (overObject.transform);
			}
		}
			
		//OnOver
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

				ClearTilesHighlight ();
				overObject = hit.collider.gameObject;
			}

			//si le bouton créer une ville à été préssé
			else if (settlingTown) {

				ClearTilesHighlight ();

				overObject = hit.collider.gameObject;

				OverlapTilesIn (overObject.transform, objectRadius);
				OverlapTilesOut (allOverlappingTilesIn, objectRadius);

				foreach (TileStateManager t in allOverlappingTilesOut) {
					t.ShowTile (true);
					t.tile.GetComponent<MeshRenderer> ().material.color = outsideColor;
				}
				foreach (TileStateManager t in allOverlappingTilesIn) {
					t.ShowFeeds (true);
					t.ShowTile (true);
					t.tile.GetComponent<MeshRenderer> ().material.color = insideColor;
				}
				foreach (TileStateManager t in inconstructibleOverlappingTiles) {
					t.ShowTile (true);
					t.tile.GetComponent<MeshRenderer> ().material.color = inconstructColor;
				} 

				if (overObject != null) {
					if (buildingInstance == null) {
						buildingInstance = Instantiate (town, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z), Quaternion.identity);
						buildingInstance.GetComponentInChildren<MeshRenderer> ().material = ghostTown;
					}
					else
						buildingInstance.transform.position = new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z);
				}
			}
				
			else if (settlingDistrict && hit.collider.transform.parent.GetComponent<TileStateManager> ().territory == TileTerritories.Inside && hit.collider.transform.parent.GetComponent<TileStateManager> ().building == TileBuildings.Empty) {

				ClearTilesHighlight ();

				overObject = hit.collider.gameObject;

				OverlapTilesIn (hit.collider.transform, objectRadius);
				OverlapTilesOut (allOverlappingTilesIn, objectRadius);

				for (int i = 0; i < allOverlappingTilesOut.Count; i++) {
					if (allOverlappingTilesOut [i].territory == TileTerritories.Empty && allOverlappingTilesOut [i] != overObject.transform) {
						allOverlappingTilesOut [i].ShowTile (true);
					}
				}
				for (int i = 0; i < allOverlappingTilesIn.Count; i++) {
					if (allOverlappingTilesIn [i].territory == TileTerritories.Outside && allOverlappingTilesIn [i] != overObject.transform) {
						allOverlappingTilesIn [i].ShowFeeds (true);
						allOverlappingTilesIn [i].ShowTile (true);
					}
				}
				foreach (TileStateManager t in allOverlappingTilesOut) {
					if (t.territory == TileTerritories.Empty && t != overObject.transform) {
						t.ShowTile (true);
						t.tile.GetComponent<MeshRenderer> ().material.color = outsideColor;
					}

				}
				foreach (TileStateManager t in allOverlappingTilesIn) {
					if (t.territory == TileTerritories.Outside && t != overObject.transform) {
						t.ShowFeeds (true);
						t.ShowTile (true);
						t.tile.GetComponent<MeshRenderer> ().material.color = insideColor;
					}
				}
				foreach (TileStateManager t in inconstructibleOverlappingTiles) {
					if (t.territory == TileTerritories.Outside && t != overObject.transform) {
						t.ShowTile (true);
						t.tile.GetComponent<MeshRenderer> ().material.color = inconstructColor;
					}
				} 
		
				if (overObject != null) {
					if (buildingInstance == null) {
						buildingInstance = Instantiate (district, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z), Quaternion.identity);
						buildingInstance.GetComponentInChildren<MeshRenderer> ().material = ghostTown;
					}
					else
						buildingInstance.transform.position = new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z);
				}
			}

			else if (spawningArmy && hit.collider.transform.parent.GetComponent<TileStateManager> ().building != TileBuildings.Town && hit.collider.transform.parent.GetComponent<TileStateManager> ().building != TileBuildings.District) {

				overObject = hit.collider.gameObject;

				if (overObject != null) {
					if (buildingInstance == null) {
						buildingInstance = Instantiate (army, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z), Quaternion.identity);
					}
					else
						buildingInstance.transform.position = new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z);
				}

			}
		}
	}

	public void ChangeEffects () {
		List<Transform> t = GetAllTilesByBuilding (TileBuildings.Town);
		List<Transform> d = GetAllTilesByBuilding (TileBuildings.District);
		foreach (Transform tr in t) {
			tr.GetComponent<TileStateManager>().ChangeInconstructibleEffect (inconstructibleEffects[effectIndex]);		
		}
		foreach (Transform tr in d) {
			tr.GetComponent<TileStateManager>().ChangeInconstructibleEffect (inconstructibleEffects[effectIndex]);		
		}
		
	}

	public void ClearTilesHighlight () {

		foreach (TileStateManager tile in hexGrid.Tiles) {
			
			if (tile.territory == TileTerritories.Empty && tile.transform.GetChild (0) != selectedTile) {
				tile.ShowTile (false);
				tile.ShowFeeds (false);
				tile.tile.GetComponent<MeshRenderer> ().material.color = selectionColor;
			}
			else if (tile.territory == TileTerritories.Outside) {
				tile.ShowTile (false);
				tile.ShowFeeds (false);
			}
			else if (tile.territory == TileTerritories.Inside) {
				tile.ShowTile (true);
				if (settlingDistrict || settlingTown) {
					if (tile.building != TileBuildings.Empty)
						tile.ShowFeeds (false);
					else
						tile.ShowFeeds (true);
				}

				else {
					tile.ShowFeeds (true);
				}
			}
		} 
	}

	public void Construct (GameObject prefab) {

		ClearTilesHighlight ();
		if (settlingTown)
			overObject.transform.parent.GetComponent<TileStateManager> ().CreateTownOnTile (prefab, inconstructibleEffects[effectIndex], townCartouche, new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z));
		else
			overObject.transform.parent.GetComponent<TileStateManager> ().CreateDistrictOnTile (prefab, inconstructibleEffects[effectIndex], new Vector3 (overObject.transform.position.x, overObject.transform.parent.transform.localScale.y, overObject.transform.position.z));
		overObject.transform.parent.GetComponent<TileStateManager> ().ShowBuilding (false);

		if (settlingDistrict)
			Districts.Add (overObject.transform.parent.GetComponent<TileStateManager>());
		if (settlingTown)
			Towns.Add (overObject.transform.parent.GetComponent<TileStateManager>());
		
		Destroy (buildingInstance);
		constructionEffect (false);
		settlingDistrict = false;
		settlingTown = false;
		townSettle = true;

		foreach (Transform tile in GetAllTilesByBuilding(TileBuildings.District)) {
			tile.GetComponent<TileStateManager>().ShowBuilding (false);
		}
		foreach (Transform tile in GetAllTilesByBuilding(TileBuildings.Town)) {
			tile.GetComponent<TileStateManager>().ShowBuilding (false);
		}
		ShowBuildingsFrom (TileBuildings.Town, true);
		ShowBuildingsFrom (TileBuildings.District, true);

		OverlapTilesIn (hit.collider.transform, objectRadius);
		OverlapTilesOut (allOverlappingTilesIn, objectRadius);

		for (int i = 0; i < allOverlappingTilesOut.Count; i++) {
			if (allOverlappingTilesOut [i].territory == TileTerritories.Empty)
				allOverlappingTilesOut [i].CreateOutsideContent(selectionColor);
		}
		for (int i = 0; i < allOverlappingTilesIn.Count; i++) {
			if (allOverlappingTilesIn [i].territory == TileTerritories.Empty || allOverlappingTilesIn [i].territory == TileTerritories.Outside)
				allOverlappingTilesIn [i].CreateInsideContent(selectionColor);
		}
	}

	public void ShowGhostDistrict (Transform town) {
		List<Transform> inside = GetAllTilesByTerritory (TileTerritories.Inside);

		for (int i = 0; i < inside.Count; i++) {
			if (inside [i].GetComponent<TileStateManager> ().building == TileBuildings.Empty) {
				GameObject d = Instantiate (district, new Vector3 (inside [i].GetChild (0).position.x, inside [i].localScale.y, inside [i].GetChild (0).position.z), Quaternion.identity);
				d.transform.localScale = new Vector3 (1, 0.5f, 1);
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

	public void FilterTileFrom (TileTerritories territory, bool b) {
		List<Transform>  tiles = GetAllTilesByTerritory (territory);

		foreach (Transform tr in tiles) {
			if (b) {
				tr.gameObject.layer = 8;
				tr.GetChild(0).gameObject.layer = 8;
				terrain.layer = 8;
			} else {
				tr.gameObject.layer = 0;
				tr.GetChild(0).gameObject.layer = 0;
				terrain.layer = 0;
			}
		}
	}

	public void ShowBuildingsFrom (TileBuildings building, bool b) {
		foreach (Transform tile in GetAllTilesByBuilding(building)) {
			tile.GetComponent<TileStateManager>().ShowBuilding (b);
		}
	}

	public void ShowAllFeeds () {
		AllFeedsShown = !AllFeedsShown;
		foreach (TileStateManager go in hexGrid.Tiles) {
			go.ShowFeeds(AllFeedsShown);
		}
	}

	public void SelectArmy() {
		ClearTilesHighlight ();
		OverlapTilesIn (selectedTile, objectRadius*2);
		OverlapTilesOut (allOverlappingTilesIn, objectRadius*2);
		for (int i = 0; i < allOverlappingTilesOut.Count; i++) {
			if (allOverlappingTilesOut [i].territory != TileTerritories.Inside && allOverlappingTilesOut [i] != overObject.transform) {
				allOverlappingTilesOut [i].tile.GetComponent<MeshRenderer> ().material.color = outsideColor;
				allOverlappingTilesOut [i].ShowTile(true);
			}
		}
		for (int i = 0; i < allOverlappingTilesIn.Count; i++) {
			allOverlappingTilesIn [i].tile.GetComponent<MeshRenderer> ().material.color = insideColor;
			allOverlappingTilesIn [i].ShowTile(true);
		}
	}

	public void constructionEffect (bool state) {
		for (int i = 0; i < Towns.Count; i++) {
			Towns [i].ConstructionEffect (state);
		}
		for (int i = 0; i < Districts.Count; i++) {
			Districts [i].ConstructionEffect (state);
		}
	}

	public void OverlapTilesIn (Transform overObject, float rad) {

		allOverlappingTilesIn.Clear ();
		inconstructibleOverlappingTiles.Clear ();
		allOverlappingColliders = Physics.OverlapSphere (overObject.transform.position, rad);

		for (int i = 0; i < allOverlappingColliders.Length; i++) {
			Debug.Log (allOverlappingColliders [i].transform.parent.transform);
			float heightDifference = Mathf.Abs (overObject.transform.parent.transform.localScale.y - allOverlappingColliders [i].transform.parent.transform.localScale.y);
			if (heightDifference < maxHeight && !allOverlappingTilesIn.Contains (allOverlappingColliders [i].transform.parent.GetComponent<TileStateManager> ())) {
				allOverlappingTilesIn.Add (allOverlappingColliders [i].transform.parent.GetComponent<TileStateManager> ());
			} else {
				inconstructibleOverlappingTiles.Add(allOverlappingColliders [i].transform.parent.GetComponent<TileStateManager> ());
			}
		}
		Debug.Log (inconstructibleOverlappingTiles.Count);
	}

	public void OverlapTilesOut (List<TileStateManager> overlap, float rad) {

		allOverlappingTilesOut.Clear ();

		for (int i = 0; i < allOverlappingTilesIn.Count; i++) {

			allOverlappingColliders = Physics.OverlapSphere (allOverlappingTilesIn[i].transform.position, rad);

			for (int j = 0 ; j < allOverlappingColliders.Length ; j++) {
				TileStateManager tileScript = allOverlappingColliders [j].transform.parent.GetComponent<TileStateManager> ();
				float heightDifference = Mathf.Abs (allOverlappingTilesIn[i].transform.localScale.y - tileScript.transform.localScale.y);
				if (heightDifference < maxHeight && !allOverlappingTilesOut.Contains (tileScript) && tileScript.territory == TileTerritories.Empty) {
					if (!allOverlappingTilesIn.Contains (tileScript))
						allOverlappingTilesOut.Add (tileScript);
				}
				else {
					if (!allOverlappingTilesIn.Contains (tileScript))
						inconstructibleOverlappingTiles.Add(allOverlappingColliders [i].transform.parent.GetComponent<TileStateManager> ());
				}
			}
		}
	}

	public void Select (Transform tr) {
		selectedTile = tr;
		ClearTilesHighlight ();
		selectedTile.parent.GetComponent<TileStateManager> ().ShowTile (true);
		selectedTile.parent.GetComponent<TileStateManager> ().ShowFeeds (true);
		selectHighlight.transform.position = tr.position;
		if (selectedTile.parent.GetComponent<TileStateManager> ().unit == TileUnits.Army) {
			SelectArmy ();
		}
	}

	public void CenterOnSelect () {
		Camera.main.transform.position = new Vector3 (selectedTile.position.x, selectedTile.position.y + 10, selectedTile.position.z - 7.5f);
	}

	public void SettlingMode () {
		constructionEffect (true);
		ShowBuildingsFrom (TileBuildings.Town, false);
		ShowBuildingsFrom (TileBuildings.District, false);

		if (townSettle) {
			FilterTileFrom (TileTerritories.Empty, true);
			settlingDistrict = true;
			ClearTilesHighlight ();
			ShowGhostDistrict (Towns[0].transform);
			return;
		}
		settlingTown = true;
		ClearTilesHighlight ();
	}

	public void SpawnMode () {
		constructionEffect (true);
		spawningArmy = true;
	}

	public void cameraPitch (bool b) {
		if (b) pitch += pitchIncr;
		else pitch -= pitchIncr;
		Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Mathf.Lerp (4, 30, Mathf.InverseLerp (0, 1, pitch)), Camera.main.transform.position.z);
		Camera.main.transform.rotation = Quaternion.Euler (Mathf.Lerp (40, 90, Mathf.InverseLerp (0, 1, pitch)), 0, 0);
	}

	public void PanningCamera () {
		
		mousePos = Input.mousePosition;
		Transform camTr = Camera.main.transform.parent;
		int w = Screen.width;
		int h = Screen.height;
		int b = w/50;

		if (mousePos.x > 0 && mousePos.x < b) {
			if (camTr.position.x > -20f)
				camTr.position = new Vector3 ( camTr.position.x - panIncr, camTr.position.y , camTr.position.z) ;
			Cursor.SetCursor(cursorTextureLeft, hotSpot, cursorMode);
			mousePos = Vector3.zero;
		}
		else if (mousePos.x < w && mousePos.x > w-b) {
			if (camTr.position.x < 20f)
				camTr.position = new Vector3 ( camTr.position.x + panIncr, camTr.position.y , camTr.position.z) ;
			Cursor.SetCursor(cursorTextureRight, hotSpot, cursorMode);
			mousePos = Vector3.zero;
		}
		if (mousePos.y > 0 && mousePos.y < b) {
			if (camTr.position.z > -36.5f)
				camTr.position = new Vector3 ( camTr.position.x, camTr.position.y , camTr.position.z - panIncr) ;
			Cursor.SetCursor(cursorTextureDown, hotSpot, cursorMode);
			mousePos = Vector3.zero;
		}
		else if (mousePos.y < w && mousePos.y > h-b) {
			if (camTr.position.z < 20f)
				camTr.position = new Vector3 ( camTr.position.x, camTr.position.y , camTr.position.z  + panIncr) ;
			Cursor.SetCursor(cursorTextureUp, hotSpot, cursorMode);
			mousePos = Vector3.zero;
		}
		else if (mousePos.x > b && mousePos.x < w-b && mousePos.y > b && mousePos.y < h-b)
			Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
	}

}
