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

	public int food = 0;
	public int science = 0;
	public int industry = 0;
	public int dust = 0;

	public TileBuildings building = TileBuildings.Empty;
	public TileUnits unit = TileUnits.Empty;
	public TileTerritories territory = TileTerritories.Empty;

	public bool feedsShown = false;
	public bool tileShown = false;

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
	
	public void CreateTownOnTile (GameObject prefab, GameObject prefabEffect, Vector3 pos, Material inside) {
		building = TileBuildings.Town;
		buildingModel = Instantiate (prefab, pos, Quaternion.identity);
		//transform.GetComponentInChildren<MeshRenderer> ().material = inside;
		ShowFeeds(true);
		ShowTile (true);
		buildingModel = Instantiate (prefab, pos, Quaternion.identity);
		effectModel = Instantiate (prefabEffect, pos, Quaternion.identity);
	}

	public void CreateDistrictOnTile (GameObject prefab, GameObject prefabEffect, Vector3 pos, Material inside) {
		building = TileBuildings.District;
		buildingModel = Instantiate (prefab, pos, Quaternion.identity);
		ShowFeeds (true);
		ShowTile (true);
		//transform.GetComponentInChildren<MeshRenderer> ().material = inside;
		effectModel = Instantiate (prefabEffect, pos, Quaternion.identity);
	}

	public void DestroyTileContent () {
		building = TileBuildings.Empty;
	}

	public void CreateInsideContent (Material inside) {
		territory = TileTerritories.Inside;
		tile.GetComponent<MeshRenderer> ().material.color = new Color(0.75f, 0.82f, 0.22f, 0.5f);    
		//transform.GetComponentInChildren<MeshRenderer> ().material = inside;
	}

	public void CreateOutsideContent (Material outside) {
		territory = TileTerritories.Outside;
		tile.GetComponent<MeshRenderer> ().material.color = new Color (0.93f, 0.45f, 0.15f, 0.5f);
		//transform.GetComponentInChildren<MeshRenderer> ().material = outside;
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
}
