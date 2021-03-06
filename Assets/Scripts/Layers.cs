public enum Layers {
    Targetable = 3,
    Water = 4,
    Projectile = 6,
    BoatWheel = 7,
    Land = 8,
    Selectable = 9
}

public enum LayerMasks {
    Water = (1 << Layers.Water),
    Land = (1 << Layers.Land),
    Terrain = (1 << Layers.Land) + (1 << Layers.Water),
    Targetable = (1 << Layers.Targetable),
    ShipGunTarget = (1 << Layers.Land) + (1 << Layers.Targetable) + (1 << Layers.Water),
    Selectable = (1 << Layers.Selectable)
}