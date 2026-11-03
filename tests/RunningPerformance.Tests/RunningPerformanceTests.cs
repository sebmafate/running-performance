namespace RunningPerformance.Tests;

public class FieldTestsTests
{
    [Fact]
    public void Cooper_2400m_GivesDocumentedVo2AndVma()
    {
        // (2400 − 504,9) / 44,73 ≈ 42,368 ml/kg/min → VMA ≈ 11,66 km/h.
        // La doc iOS arrondit le VO₂max à 42,4 avant de convertir, et obtient 11,67.
        var vo2 = FieldTests.CooperVo2Max(2400);
        var vma = Acsm.VmaKmhFromVo2Max(vo2);

        Assert.Equal(42.368, vo2, precision: 3);
        Assert.Equal(11.66, vma, precision: 2);
    }

    [Fact]
    public void Cooper_BelowThreshold_ReturnsZero()
    {
        Assert.Equal(0, FieldTests.CooperVo2Max(500));
    }

    [Fact]
    public void Vaussenat_ReferenceCase_IsFifteenPointSixTwoFive()
    {
        // 15 km/h complété, 500 m dans le palier 16 (800 m) : 15 + 500 / 800
        Assert.Equal(15.625, FieldTests.VaussenatVma(15, 500), precision: 9);
    }

    [Fact]
    public void Vaussenat_DistanceClampedToNextStage()
    {
        // 9 999 m ne peut pas dépasser le palier suivant (800 m)
        Assert.Equal(16, FieldTests.VaussenatVma(15, 9_999), precision: 9);
    }

    [Fact]
    public void Vaussenat_NoStageCompleted_ReturnsZero()
    {
        Assert.Equal(0, FieldTests.VaussenatVma(0, 500));
    }

    [Fact]
    public void Leger_Stage8_IsTwelveKmhAndDocumentedVo2()
    {
        Assert.Equal(12.0, FieldTests.LegerStageSpeedKmh(8), precision: 9);
        Assert.Equal(12.0, FieldTests.LegerVma(8), precision: 9);
        // 5,857 × 12 − 19,458 ≈ 50,83
        Assert.Equal(50.83, FieldTests.LegerVo2Max(8), precision: 2);
    }

    [Fact]
    public void Leger_FirstStage_Is8Point5()
    {
        Assert.Equal(8.5, FieldTests.LegerStageSpeedKmh(1), precision: 9);
    }

    [Fact]
    public void HalfCooper_1200m_IsTwelveKmh()
    {
        Assert.Equal(12.0, FieldTests.HalfCooperVma(1200), precision: 9);
    }

    [Fact]
    public void ThreeNine_IsAverageOfTwoSpeeds()
    {
        Assert.Equal(16.5, FieldTests.ThreeNineVma(18, 15), precision: 9);
    }

    [Fact]
    public void TenByFourHundred_90SecondsIsSixteenKmh()
    {
        Assert.Equal(16.0, FieldTests.TenByFourHundredVma(90), precision: 9);
    }

    [Fact]
    public void NonPositiveInputs_ReturnZero()
    {
        Assert.Equal(0, FieldTests.HalfCooperVma(0));
        Assert.Equal(0, FieldTests.ThreeNineVma(0, 15));
        Assert.Equal(0, FieldTests.TenByFourHundredVma(0));
    }
}

public class AcsmTests
{
    [Fact]
    public void Vma15_GivesFiftyThreePointFiveVo2()
    {
        // 15 km/h = 250 m/min : 250 × 0,2 + 3,5 = 53,5
        Assert.Equal(53.5, Acsm.Vo2MaxFromVmaKmh(15), precision: 9);
    }

    [Fact]
    public void Vo2_53Point5_GivesFifteenKmh()
    {
        Assert.Equal(15.0, Acsm.VmaKmhFromVo2Max(53.5), precision: 9);
    }

    [Fact]
    public void RoundTrip_IsIdentity()
    {
        Assert.Equal(13.7, Acsm.VmaKmhFromVo2Max(Acsm.Vo2MaxFromVmaKmh(13.7)), precision: 9);
    }
}

public class RacePredictorTests
{
    [Fact]
    public void TenKmInFortyFive_PredictsHalfMarathonNearOneHourThirtyNine()
    {
        // 2 700 × (21 097,5 / 10 000)^1,06 ≈ 5 958 s ≈ 1 h 39 min 18 s
        var seconds = RacePredictor.PredictSeconds(2_700, 10_000, 21_097.5);

        Assert.InRange(seconds, 5_950, 5_965);
    }

    [Fact]
    public void ExponentOne_IsLinear()
    {
        Assert.Equal(5_400, RacePredictor.PredictSeconds(2_700, 10_000, 20_000, exponent: 1.0), precision: 6);
    }

    [Fact]
    public void SameDistance_ReturnsSameTime()
    {
        Assert.Equal(2_700, RacePredictor.PredictSeconds(2_700, 10_000, 10_000), precision: 9);
    }

    [Fact]
    public void Pace_IsSecondsPerKilometre()
    {
        Assert.Equal(270, RacePredictor.SecondsPerKm(2_700, 10_000), precision: 9);
    }

    [Fact]
    public void NonPositiveInput_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RacePredictor.PredictSeconds(0, 10_000, 21_097.5));
    }
}

public class VmaZonesTests
{
    [Fact]
    public void Z1_WithVma15_BoundsAreCorrect()
    {
        // 55–65 % de 15 km/h : 8,25 à 9,75 km/h
        var bounds = VmaZones.BoundsFor(VmaZones.Standard[0], 15);

        Assert.Equal(8.25, bounds.MinKmh, precision: 9);
        Assert.Equal(9.75, bounds.MaxKmh, precision: 9);
        Assert.Equal(3_600 / 9.75, bounds.FastSecondsPerKm, precision: 9);
        Assert.Equal(3_600 / 8.25, bounds.SlowSecondsPerKm, precision: 9);
    }

    [Fact]
    public void FastBound_HasLowerPaceValue()
    {
        var bounds = VmaZones.BoundsFor(VmaZones.Standard[4], 15);

        Assert.True(bounds.FastSecondsPerKm < bounds.SlowSecondsPerKm);
    }

    [Fact]
    public void FiveKmReferencePace_IsNinetyTwoToNinetySevenPercent()
    {
        var bounds = VmaZones.BoundsFor(VmaZones.ReferencePaces[6], 15);

        Assert.Equal("5 km", VmaZones.ReferencePaces[6].Name);
        Assert.Equal(13.8, bounds.MinKmh, precision: 9);
        Assert.Equal(14.55, bounds.MaxKmh, precision: 9);
    }

    [Fact]
    public void NonPositiveVma_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VmaZones.BoundsFor(VmaZones.Standard[0], 0));
    }
}
