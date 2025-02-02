﻿using UnityEngine;
using UnityEditor;
using System.Collections;

public class TileEditor : MonoBehaviour {
	// Sprite sheet to use
	public Texture2D spriteSheet;
	// Tile size in units
	public float tileSize = 2.0f;
	// Sorting layer
	public int sortingLayerID = 1;
	// Map name
	public string mapName;
	// Map size in ones
	public Vector2 mapSize = new Vector2(8, 8);
	// Icon size in pixels
	public int iconSize = 64;
	// Connected button GUI style
	public GUISkin guiSkin;
	// Has Collider
	public bool hasCollider;
	
	// Generated tiles
	[HideInInspector]
	public Sprite[] tiles;
	// Generated tile textures for previewing
	[HideInInspector]
	public Texture2D[] tileTextures;
	// Mouse position on map
	[HideInInspector]
	public Vector3 mousePositionOnMap;
	// Real mouse position on map
	[HideInInspector]
	public Vector3 realMousePositionOnMap;
	// Select to generate tiles
	[HideInInspector]
	public bool generateTiles;
	// Tile's ID's references
	[HideInInspector]
	public int[] tileTextureIDtoTileNo;
	// Selected tile's ID
	[HideInInspector]
	public int tileTextureID;
	
	
	public void OnDrawGizmosSelected () {
		realMousePositionOnMap.x = mousePositionOnMap.x * tileSize + transform.position.x + tileSize * 0.5f;
		realMousePositionOnMap.y = mousePositionOnMap.y * tileSize + transform.position.y + tileSize * 0.5f;
		realMousePositionOnMap.z = transform.position.z;
		
		Gizmos.color = Color.gray;
		for (int x=1; x<mapSize.x; x++) {
			Gizmos.DrawRay(
				transform.position + new Vector3(x * tileSize, 0, 0),
				Vector3.up * mapSize.y * tileSize);
		}
		for (int y=1; y<mapSize.y; y++) {
			Gizmos.DrawRay(
				transform.position + new Vector3(0, y * tileSize, 0),
				Vector3.right * mapSize.x * tileSize);
		}
		
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(
			realMousePositionOnMap,
			new Vector3(tileSize, tileSize, 0.1f)
			);
	}
	
	public void xGenerateTiles () {
		// Import sprite sheet
		string spriteSheetPath = AssetDatabase.GetAssetPath(spriteSheet);
		TextureImporter spriteSheetAsset = (TextureImporter)AssetImporter.GetAtPath(spriteSheetPath);
		// Set to readable
		if (!spriteSheetAsset.isReadable) {
			spriteSheetAsset.isReadable = true;
			AssetDatabase.ImportAsset (spriteSheetPath);
		}
		// Load sprites
		Object[] spriteSheetObjects = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
		
		tiles = new Sprite[spriteSheetObjects.Length];
		for (int i=1; i<spriteSheetObjects.Length; i++) {
			tiles[i] = (Sprite)spriteSheetObjects[i];
		}
		
		GenerateTileTextures();
		
		generateTiles = false;
	}
	
	public void xGenerateTileTextures () {
		tileTextures = new Texture2D[tiles.Length];
		
		for (int i=1; i<tileTextures.Length; i++) {
			Rect rect = tiles[i].rect;
			tileTextures[i] = new Texture2D(iconSize, iconSize);
			
			for (int y=0; y<iconSize; y++) {
				for (int x=0; x<iconSize; x++) {
					Color refColor = spriteSheet.GetPixel(
						(int)((float)x / iconSize * rect.width + rect.x),
						(int)((float)y / iconSize * rect.height + rect.y));
					tileTextures[i].SetPixel(x, y, refColor);
				}
			}
			
			tileTextures[i].Apply();
		}
	}
	
	public void GenerateTiles () {
		if (mapName == "") {
			mapName = gameObject.name;
		}

		// Import sprite sheet
		string spriteSheetPath = AssetDatabase.GetAssetPath(spriteSheet);
		TextureImporter spriteSheetAsset = (TextureImporter)AssetImporter.GetAtPath(spriteSheetPath);
		// Set to readable
		if (!spriteSheetAsset.isReadable) {
			spriteSheetAsset.isReadable = true;
			AssetDatabase.ImportAsset (spriteSheetPath);
		}
		// Load sprites
		Object[] spriteSheetObjects = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
		
		tiles = new Sprite[spriteSheetObjects.Length];
		for (int i=1; i<spriteSheetObjects.Length; i++) {
			tiles[i] = (Sprite)spriteSheetObjects[i];
		}
		
		GenerateTileTextures();
		
		generateTiles = false;
	}
	
	public void GenerateTileTextures () {
		tileTextures = new Texture2D[20 * 6 * 8];
		tileTextureIDtoTileNo = new int[tileTextures.Length];
		
		int g = 0;
		for (int i=0; i<2; i++) {
			int iAnchorY = i * 256;
			
			for (int j=0; j<5; j++) {
				int jAnchorX = j * 96;
				
				for (int k=0; k<16; k++) {
					int kAnchorY = 240 - k * 16;
					
					bool hasTile = false;
					for (int l=0; l<6; l++) {
						int lAnchorX = l * 16;
						
						int tileNo = FindTileAtPosition(jAnchorX + lAnchorX, iAnchorY + kAnchorY);
						if (tileNo > 0) {
							
							Rect rect = tiles[tileNo].rect;
							tileTextures[g * 6 + l] = new Texture2D(iconSize, iconSize);
							
							for (int y=0; y<iconSize; y++) {
								for (int x=0; x<iconSize; x++) {
									Color refColor = spriteSheet.GetPixel(
										(int)((float)x / iconSize * rect.width + rect.x),
										(int)((float)y / iconSize * rect.height + rect.y));
									tileTextures[g * 6 + l].SetPixel(x, y, refColor);
								}
							}
							tileTextures[g * 6 + l].Apply();
							
							tileTextureIDtoTileNo[g * 6 + l] = tileNo;
							
							hasTile = true;
						}
					}
					if (hasTile) {
						g++;
					}
				}
			}
		}
	}
	
	public int FindTileAtPosition (float x, float y) {
		for (int i=1; i<tiles.Length; i++) {
			Rect rect = tiles[i].rect;
			Vector2 tilePositions = new Vector2(rect.x, rect.y);
			if ((int)tilePositions.x == (int)x && (int)tilePositions.y == (int)y) {
				return i;
			}
		}
		return 0;
	}
}
