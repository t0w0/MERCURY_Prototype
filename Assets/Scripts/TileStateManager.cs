using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileStateManager : MonoBehaviour {

	public GameObject UI;
	public GameObject buildingModel;
	public GameObject armyModel;
	public GameObject effectModel;
	public GameObject tile;
	public GameObject feeds;
	public Transform myTransform;
	public GameObject cartouche;

	public int food = 0;
	public int science = 0;
	public int industry = 0;
	public int dust = 0;

	public TileBuildings building = TileBuildings.Empty;
	public TileUnits unit = TileUnits.Empty;
	public TileTerritories territory = TileTerritories.Empty;

	public bool feedsShown = false;
	public bool tileShown = false;

	public void Start () {
		myTransform = transform;
	}

	public void InitTile (GameObject ui) {
		food = Random.Range (0, 7);
		science = Random.Range (0, 7);
		industry = Random.Range (0, 7);
		dust = Random.Range (0, 7);
		UI = ui;
		feeds = UI.transform.GetChild (1).gameObject;
		tile = UI.transform.GetChild (0).gameObject;
		ActualiseFeeds ();

		ShowFeeds (false);
		ShowTile (false);
	}
	
	public void CreateTownOnTile (GameObject prefab, GameObject prefabEffect, GameObject townCartouchePrefab, Vector3 pos) {
		building = TileBuildings.Town;
		buildingModel = Instantiate (prefab, pos, Quaternion.identity);
		ShowFeeds(true);
		ShowTile (true);
		effectModel = Instantiate (prefabEffect, pos, Quaternion.identity, UI.transform);
		cartouche = Instantiate (townCartouchePrefab, pos, Quaternion.identity);
	}

	public void CreateDistrictOnTile (GameObject prefab, GameObject prefabEffect, Vector3 pos) {
		building = TileBuildings.District;
		buildingModel = Instantiate (prefab, pos, Quaternion.identity);
		ShowFeeds (true);
		ShowTile (true);
		effectModel = Instantiate (prefabEffect, pos, Quaternion.identity, UI.transform);
	}

	public void DestroyTileContent () {
		building = TileBuildings.Empty;
	}

	public void CreateInsideContent (Color c) {
		territory = TileTerritories.Inside;
		tile.GetComponent<MeshRenderer> ().material.color = c;
	}

	public void CreateOutsideContent (Color c) {
		territory = TileTerritories.Outside;
		tile.GetComponent<MeshRenderer> ().material.color = c;
	}

	public void SpawnArmyOnTile (GameObject prefab, Vector3 pos) {
		unit = TileUnits.Army;
		armyModel = Instantiate (prefab, pos, Quaternion.identity);
	}

	public void ConstructionEffect (bool state) {
		if (effectModel != null) {
			effectModel.SetActive (state);
		}
	}

	public void ActualiseFeeds () {
	
		UI.transform.GetChild (1).GetChild (0).GetComponent<Text> ().text = food.ToString();
		UI.transform.GetChild (1).GetChild (1).GetComponent<Text> ().text = science.ToString();
		UI.transform.GetChild (1).GetChild (2).GetComponent<Text> ().text = industry.ToString();
		UI.transform.GetChild (1).GetChild (3).GetComponent<Text> ().text = dust.ToString();

	}

	public void ShowFeeds (bool state) {
		feeds.SetActive (state);
	}
	public void ShowTile (bool state) {
		tile.SetActive (state);
	}
	public void ShowBuilding (bool state) {
		buildingModel.SetActive (state);
	}
	public void ChangeInconstructibleEffect (GameObject prefab) {

		Vector3 pos = new Vector3 (transform.position.x, transform.localScale.y, transform.position.z);
		Destroy (effectModel);
		effectModel = GameObject.Instantiate (prefab, pos, Quaternion.identity, UI.transform); 
	}
}
