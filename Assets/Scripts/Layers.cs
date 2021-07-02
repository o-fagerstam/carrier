public enum Layers {
    Targetable = 3,
    Water = 4,
    Projectile = 6,
    BoatWheel = 7,
    Land = 8
}

public enum LayerMasks {
    Water = (1 << Layers.Water),
    Land = (1 << Layers.Land),
    ShipGunTarget = (1 << Layers.Land) + (1 << Layers.Targetable) + (1 << Layers.Water)
}