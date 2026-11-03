namespace RunningPerformance;

/// <summary>
/// Estimation de la VMA (vitesse maximale aérobie, km/h) et du VO₂max (ml/kg/min) à partir de
/// tests de terrain. Chaque méthode suit son protocole publié, avec la formule de l'original.
/// </summary>
public static class FieldTests
{
    /// <summary>Léger-Bouchard : vitesse du premier palier, en km/h.</summary>
    public const double LegerFirstStageKmh = 8.5;

    /// <summary>Léger-Bouchard : incrément de vitesse par palier, en km/h.</summary>
    public const double LegerStageIncrementKmh = 0.5;

    /// <summary>
    /// Cooper (1968), 12 minutes : VO₂max = (distance − 504,9) / 44,73.
    /// Renvoie 0 si la distance est inférieure à 504,9 m (résultat sinon négatif).
    /// </summary>
    public static double CooperVo2Max(double distanceMeters) =>
        distanceMeters > 504.9 ? (distanceMeters - 504.9) / 44.73 : 0;

    /// <summary>
    /// Vaussenat (Brue) : paliers de 1 km/h toutes les 3 minutes, avec une distance de
    /// 50 m × vitesse (km/h) par palier.
    /// </summary>
    /// <param name="lastCompletedKmh">Vitesse (km/h) du dernier palier entièrement réussi.</param>
    /// <param name="distanceInNextMeters">Distance couverte dans le palier suivant, avant l'arrêt.
    /// 0 si le palier n'a pas été commencé.</param>
    /// <remarks>
    /// VMA = palier + distance / distance_du_palier_suivant, avec distance_du_palier_suivant =
    /// (palier + 1) × 50. Le dénominateur est donc celui du palier suivant : la distance du palier
    /// courant (750 m pour 15 km/h) est une erreur courante.
    /// Exemple : 15 km/h complété, 500 m dans le palier 16 : 15 + 500/800 = 15,625 km/h.
    /// </remarks>
    public static double VaussenatVma(int lastCompletedKmh, double distanceInNextMeters)
    {
        if (lastCompletedKmh < 1)
        {
            return 0;
        }

        var nextStageMeters = (lastCompletedKmh + 1) * 50.0;
        var clamped = Math.Clamp(distanceInNextMeters, 0, nextStageMeters);
        return lastCompletedKmh + clamped / nextStageMeters;
    }

    /// <summary>Léger-Bouchard : vitesse (km/h) du palier n, à partir de 1.</summary>
    public static double LegerStageSpeedKmh(int stage) =>
        LegerFirstStageKmh + (stage - 1) * LegerStageIncrementKmh;

    /// <summary>Léger-Bouchard : VMA = vitesse du dernier palier complété, ou 0 si aucun.</summary>
    public static double LegerVma(int lastCompletedStage) =>
        lastCompletedStage >= 1 ? LegerStageSpeedKmh(lastCompletedStage) : 0;

    /// <summary>Léger & Lambert (1982) : VO₂max = 5,857 × V − 19,458, où V est la vitesse du palier (km/h).</summary>
    public static double LegerVo2Max(int lastCompletedStage) =>
        lastCompletedStage >= 1 ? 5.857 * LegerStageSpeedKmh(lastCompletedStage) - 19.458 : 0;

    /// <summary>Demi-Cooper (6 minutes) : VMA (km/h) = distance (m) / 100. Estimation empirique.</summary>
    public static double HalfCooperVma(double distanceMeters) =>
        distanceMeters > 0 ? distanceMeters / 100.0 : 0;

    /// <summary>Test 3' + 9' : VMA = moyenne des vitesses (km/h) sur 3 minutes et sur 9 minutes.</summary>
    public static double ThreeNineVma(double speed3MinKmh, double speed9MinKmh) =>
        speed3MinKmh > 0 && speed9MinKmh > 0 ? (speed3MinKmh + speed9MinKmh) / 2.0 : 0;

    /// <summary>Test 10 × 400 m : VMA (km/h) = 1 440 / temps moyen par répétition (s).</summary>
    public static double TenByFourHundredVma(double averageSecondsPer400m) =>
        averageSecondsPer400m > 0 ? 1_440.0 / averageSecondsPer400m : 0;
}

/// <summary>Relation VMA ↔ VO₂max, équation ACSM pour la course à plat.</summary>
/// <remarks>
/// VO₂ (ml/kg/min) = 0,2 × vitesse (m/min) + 3,5. Le 0,2 est le coût en oxygène de la course,
/// le 3,5 le métabolisme de repos (1 MET).
/// </remarks>
public static class Acsm
{
    public static double Vo2MaxFromVmaKmh(double vmaKmh) =>
        vmaKmh > 0 ? vmaKmh * 1_000.0 / 60.0 * 0.2 + 3.5 : 0;

    public static double VmaKmhFromVo2Max(double vo2Max) =>
        vo2Max > 3.5 ? (vo2Max - 3.5) / 0.2 * 60.0 / 1_000.0 : 0;
}
