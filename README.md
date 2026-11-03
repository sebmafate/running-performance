# Running Performance

[Read in English](./README.en.md)

## À quoi ça sert

Une petite bibliothèque C# (.NET 10) pour la course à pied :

- **Ma VMA et mon VO₂max**, estimés à partir d'un test de terrain (Cooper, Léger-Bouchard, Vaussenat, demi-Cooper, 3' + 9', 10 × 400 m) ;
- **Le temps que je peux viser** sur une autre distance, avec la loi de Riegel ;
- **Les allures de chaque zone**, en pourcentage de ma VMA.

Elle est tirée des calculateurs de course de mon application Allure+. Elle n'a aucune dépendance externe.

## Fonctions

| Fonction | Rôle |
|---|---|
| `FieldTests.*` | VMA ou VO₂max à partir de chaque protocole de terrain, avec la formule du protocole. |
| `Acsm.Vo2MaxFromVmaKmh` / `VmaKmhFromVo2Max` | Passage VMA ↔ VO₂max, équation ACSM pour la course à plat. |
| `RacePredictor.PredictSeconds` | Temps prédit sur une autre distance (loi de Riegel). |
| `VmaZones.BoundsFor` | Vitesses et allures d'une zone, à partir de la VMA. |
| `VmaZones.ReferencePaces` | Allures de référence par épreuve (5 km, 10 km, semi, marathon). |

## Comment l'utiliser

```csharp
using RunningPerformance;

// Cooper, 2 400 m en 12 minutes
var vo2 = FieldTests.CooperVo2Max(2_400);       // ≈ 42,4 ml/kg/min
var vma = Acsm.VmaKmhFromVo2Max(vo2);           // ≈ 11,66 km/h

// 10 km en 45 min : temps prédit au semi
var semi = RacePredictor.PredictSeconds(2_700, 10_000, 21_097.5);   // ≈ 5 958 s

// Zone 2 (65–75 % de VMA) avec une VMA de 15 km/h
var z2 = VmaZones.BoundsFor(VmaZones.Standard[1], 15);
```

## La loi de Riegel

`t₂ = t₁ × (d₂ / d₁)^1,06`. L'exposant est une moyenne de population. Il convient entre 5 km et le semi-marathon. Pour un coureur peu kilométré, il sur-estime souvent le marathon : un exposant individuel varie entre 1,04 et 1,10.

## Limites

- Le test de Cooper et le demi-Cooper sont sensibles à la motivation et à la régularité de l'allure.
- Le demi-Cooper (6 minutes) est une approximation empirique, sans base publiée solide.
- Les zones sont des **pourcentages** de la VMA, pas des mesures de seuil.
- Riegel ignore le relief, la chaleur, le vent et la stratégie de course.

## Tests

```bash
dotnet test
```
