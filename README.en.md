# Running Performance

[Lire en français](./README.md)

## What it is for

A small C# (.NET 10) library for running:

- **My VMA and VO₂max**, estimated from a field test (Cooper, Léger-Bouchard, Vaussenat, half-Cooper, 3' + 9', 10 × 400 m);
- **The time I can target** over another distance, with Riegel's formula;
- **The paces of each zone**, as a percentage of my VMA (maximal aerobic speed).

It is extracted from the running calculators of my iOS app, Allure+. It has no external dependencies.

## Functions

| Function | Role |
|---|---|
| `FieldTests.*` | VMA or VO₂max from each field protocol, with the protocol's own formula. |
| `Acsm.Vo2MaxFromVmaKmh` / `VmaKmhFromVo2Max` | VMA ↔ VO₂max conversion, ACSM equation for flat running. |
| `RacePredictor.PredictSeconds` | Predicted time over another distance (Riegel). |
| `VmaZones.BoundsFor` | Speed and pace bounds of a zone, from VMA. |
| `VmaZones.ReferencePaces` | Reference paces by race (5 km, 10 km, half, marathon). |

## Usage

```csharp
using RunningPerformance;

// Cooper, 2,400 m in 12 minutes
var vo2 = FieldTests.CooperVo2Max(2_400);       // ≈ 42.4 ml/kg/min
var vma = Acsm.VmaKmhFromVo2Max(vo2);           // ≈ 11.66 km/h

// 10 km in 45 min: predicted half marathon
var half = RacePredictor.PredictSeconds(2_700, 10_000, 21_097.5);   // ≈ 5,958 s

// Zone 2 (65–75 % of VMA) with a VMA of 15 km/h
var z2 = VmaZones.BoundsFor(VmaZones.Standard[1], 15);
```

## Riegel's formula

`t₂ = t₁ × (d₂ / d₁)^1.06`. The exponent is a population average. It works best between 5 km and the half marathon. For a runner with low weekly mileage, it often overestimates the marathon; an individual exponent typically ranges from 1.04 to 1.10.

## Limits

- The Cooper and half-Cooper tests depend on motivation and on steady pacing.
- The half-Cooper (6 minutes) is an empirical approximation without a solid published basis.
- Zones are **percentages** of VMA, not threshold measurements.
- Riegel ignores elevation, heat, wind and race strategy.

## Tests

```bash
dotnet test
```
