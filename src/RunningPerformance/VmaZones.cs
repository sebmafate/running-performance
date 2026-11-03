namespace RunningPerformance;

/// <summary>Zone d'entraînement définie en pourcentage de la VMA.</summary>
/// <param name="Name">Nom de la zone.</param>
/// <param name="MinPercent">Borne basse, en % de la VMA.</param>
/// <param name="MaxPercent">Borne haute, en % de la VMA.</param>
public sealed record VmaZone(string Name, double MinPercent, double MaxPercent);

/// <summary>Vitesses et allures d'une zone, à partir de la VMA.</summary>
public sealed record ZoneBounds(double MinKmh, double MaxKmh, double FastSecondsPerKm, double SlowSecondsPerKm);

/// <summary>Modèle à cinq zones en % de la VMA, et allures de référence par épreuve.</summary>
public static class VmaZones
{
    public static readonly IReadOnlyList<VmaZone> Standard =
    [
        new("Z1 · Endurance fondamentale", 55, 65),
        new("Z2 · Seuil aérobie", 65, 75),
        new("Z3 · Seuil anaérobie", 75, 88),
        new("Z4 · Résistance", 88, 95),
        new("Z5 · VMA", 95, 105)
    ];

    /// <summary>Allures de référence par type d'effort, en % de la VMA.</summary>
    public static readonly IReadOnlyList<VmaZone> ReferencePaces =
    [
        new("Endurance fondamentale", 60, 65),
        new("Sous le seuil", 70, 80),
        new("Au seuil", 80, 90),
        new("Marathon", 75, 82),
        new("Semi-marathon", 82, 87),
        new("10 km", 87, 92),
        new("5 km", 92, 97)
    ];

    /// <summary>
    /// Bornes de vitesse et d'allure d'une zone. Une vitesse plus élevée donne une allure
    /// (s/km) plus faible : la borne rapide correspond au pourcentage le plus haut.
    /// </summary>
    public static ZoneBounds BoundsFor(VmaZone zone, double vmaKmh)
    {
        if (vmaKmh <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vmaKmh), "La VMA doit être strictement positive.");
        }

        var minKmh = vmaKmh * zone.MinPercent / 100.0;
        var maxKmh = vmaKmh * zone.MaxPercent / 100.0;
        return new ZoneBounds(
            MinKmh: minKmh,
            MaxKmh: maxKmh,
            FastSecondsPerKm: 3_600.0 / maxKmh,
            SlowSecondsPerKm: 3_600.0 / minKmh);
    }
}
