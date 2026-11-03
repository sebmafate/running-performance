namespace RunningPerformance;

/// <summary>Prédiction de temps de course par la loi de Riegel (1981).</summary>
/// <remarks>
/// t₂ = t₁ × (d₂ / d₁)^1,06. L'exposant supérieur à 1 traduit le ralentissement de l'allure
/// quand la distance augmente. La loi est fiable entre 5 km et le semi-marathon. Elle sur-estime
/// en général le marathon pour un coureur sans endurance spécifique, et ignore le relief, la
/// chaleur et le vent.
/// </remarks>
public static class RacePredictor
{
    /// <summary>Exposant de fatigue de la loi de Riegel.</summary>
    public const double DefaultExponent = 1.06;

    /// <summary>Temps (s) prédit sur la distance cible.</summary>
    /// <param name="referenceTimeS">Temps de référence (s).</param>
    /// <param name="referenceDistanceM">Distance de référence (m).</param>
    /// <param name="targetDistanceM">Distance cible (m).</param>
    /// <param name="exponent">Exposant de fatigue individuel, entre 1,04 et 1,10 en pratique.</param>
    public static double PredictSeconds(
        double referenceTimeS,
        double referenceDistanceM,
        double targetDistanceM,
        double exponent = DefaultExponent)
    {
        if (referenceTimeS <= 0 || referenceDistanceM <= 0 || targetDistanceM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(referenceTimeS), "Temps et distances doivent être positifs.");
        }

        return referenceTimeS * Math.Pow(targetDistanceM / referenceDistanceM, exponent);
    }

    /// <summary>Allure moyenne (s/km) pour un temps (s) sur une distance (m).</summary>
    public static double SecondsPerKm(double timeS, double distanceM) => timeS / (distanceM / 1_000.0);
}
