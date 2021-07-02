using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TextureData : UpdateableData {
    private float savedMinHeight;
    private float savedMaxHeight;
    public void ApplyToMaterial(Material material) {

        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;
        material.SetFloat("Vector1_MinHeight", minHeight);
        material.SetFloat("Vector1_MaxHeight", maxHeight);
    }
}
