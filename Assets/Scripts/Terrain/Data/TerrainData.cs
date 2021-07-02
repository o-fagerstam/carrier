using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdateableData
{
    public float uniformScale = 20f;
    
    public bool useFalloff;
    public bool useFlatShading;
    
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public float heightOffset;

    public float MinHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
    public float MaxHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
}
