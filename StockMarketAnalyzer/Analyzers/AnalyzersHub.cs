using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMarketAnalyzer.Analyzers
{
   public class AnalyzersHub
    {
        public readonly List<IMarketAnalyzer> AllAnalyzers = new List<IMarketAnalyzer>()
        {
            new Calculate1LCLeastSquaresMovingAverage(),
            new Calculate3HMA(),
            new Calculate4MovingAverageConvergenceDivergence(),
            new Calculate4PercentagePriceOscillator(),
            new CalculateAbsolutePriceOscillator(),
            new CalculateAbsoluteStrengthIndex(),
            new CalculateAbsoluteStrengthMTFIndicator(),
            new CalculateAcceleratorOscillator(),
            new CalculateAccumulationDistributionLine(),
            new CalculateAccumulativeSwingIndex(),
            new CalculateAdaptiveAutonomousRecursiveMovingAverage(),
            new CalculateAdaptiveAutonomousRecursiveTrailingStop(),
            new CalculateAdaptiveErgodicCandlestickOscillator(),
            new CalculateAdaptiveExponentialMovingAverage(),
            new CalculateAdaptiveLeastSquares(),
            new CalculateAdaptiveMovingAverage(),
            new CalculateAdaptivePriceZoneIndicator(),
            new CalculateAdaptiveRelativeStrengthIndex(),
            new CalculateAdaptiveStochastic(),
            new CalculateAhrensMovingAverage(),
            new CalculateAlligatorIndex(),
            new CalculateAlphaDecreasingExponentialMovingAverage(),
            new CalculateAnchoredMomentum(),
            new CalculateApirineSlowRelativeStrengthIndex(),
            new CalculateArnaudLegouxMovingAverage(),
            new CalculateAroonOscillator(),
            new CalculateAsymmetricalRelativeStrengthIndex(),
            new CalculateAtrFilteredExponentialMovingAverage(),
            new CalculateAutoDispersionBands(),
            new CalculateAutoFilter(),
            new CalculateAutoLine(),
            new CalculateAutoLineWithDrift(),
            new CalculateAutonomousRecursiveMovingAverage(),
            new CalculateAverageAbsoluteErrorNormalization(),
            new CalculateAverageDirectionalIndex(),
            new CalculateAverageMoneyFlowOscillator(),
            new CalculateAverageTrueRange(),
            new CalculateAverageTrueRangeChannel(),
            new CalculateAverageTrueRangeTrailingStops(),
            new CalculateAwesomeOscillator(),
            new CalculateBalanceOfPower(),
            new CalculateBayesianOscillator(),
            new CalculateBearPowerIndicator(),
            new CalculateBelkhayateTiming(),
            new CalculateBetterVolumeIndicator(),
            new CalculateBilateralStochasticOscillator(),
            new CalculateBollingerBands(),
            new CalculateBollingerBandsAvgTrueRange(),
            new CalculateBollingerBandsFibonacciRatios(),
            new CalculateBollingerBandsPercentB(),
            new CalculateBollingerBandsWidth(),
            new CalculateBollingerBandsWithAtrPct(),
            new CalculateBreakoutRelativeStrengthIndex(),
            new CalculateBryantAdaptiveMovingAverage(),
            new CalculateBuffAverage(),
            new CalculateBullPowerIndicator(),
            new CalculateCalmarRatio(),
            new CalculateCamarillaPivotPoints(),
            new CalculateCCTStochRSI(),
            new CalculateCenterOfLinearity(),
            new CalculateChaikinMoneyFlow(),
            new CalculateChaikinOscillator(),
            new CalculateChaikinVolatility(),
            new CalculateChandeCompositeMomentumIndex(),
            new CalculateChandeForecastOscillator(),
            new CalculateChandeIntradayMomentumIndex(),
            new CalculateChandeKrollRSquaredIndex(),
            new CalculateChandelierExit(),
            new CalculateChandeMomentumOscillator(),
            new CalculateChandeMomentumOscillatorAbsolute(),
            new CalculateChandeMomentumOscillatorAbsoluteAverage(),
            new CalculateChandeMomentumOscillatorAverageDisparityIndex(),
            new CalculateChandeMomentumOscillatorFilter(),
            new CalculateChandeQuickStick(),
            new CalculateChandeTrendScore(),
            new CalculateChandeVolatilityIndexDynamicAverageIndicator(),
            new CalculateChartmillValueIndicator(),
            new CalculateChoppinessIndex(),
            new CalculateChopZone(),
            new CalculateClosedFormDistanceVolatility(),
            new CalculateCommodityChannelIndex(),
            new CalculateCommoditySelectionIndex(),
            new CalculateCompoundRatioMovingAverage(),
            new CalculateConditionalAccumulator(),
            new CalculateConfluenceIndicator(),
            new CalculateConnorsRelativeStrengthIndex(),
            new CalculateConstanceBrownCompositeIndex(),
            new CalculateContractHighLow(),
            new CalculateCoppockCurve(),
            new CalculateCoralTrendIndicator(),
            new CalculateCorrectedMovingAverage(),
            new CalculateCubedWeightedMovingAverage(),
            new CalculateDailyAveragePriceDelta(),
            new CalculateDampedSineWaveWeightedFilter(),
            new CalculateDampingIndex(),
            new CalculateDecisionPointBreadthSwenlinTradingOscillator(),
            new CalculateDecisionPointPriceMomentumOscillator(),
            new CalculateDeltaMovingAverage(),
            new CalculateDema2Lines(),
            new CalculateDemandOscillator(),
            new CalculateDemarker(),
            new CalculateDemarkPivotPoints(),
            new CalculateDemarkPressureRatioV1(),
            new CalculateDemarkPressureRatioV2(),
            new CalculateDemarkRangeExpansionIndex(),
            new CalculateDemarkReversalPoints(),
            new CalculateDemarkSetupIndicator(),
            new CalculateDEnvelope(),
            new CalculateDerivativeOscillator(),
            new CalculateDetrendedPriceOscillator(),
            new CalculateDetrendedSyntheticPrice(),
            new CalculateDidiIndex(),
            new CalculateDiNapoliMovingAverageConvergenceDivergence(),
            new CalculateDiNapoliPercentagePriceOscillator(),
            new CalculateDiNapoliPreferredStochasticOscillator(),
            new CalculateDirectionalTrendIndex(),
            new CalculateDisparityIndex(),
            new CalculateDistanceWeightedMovingAverage(),
            new CalculateDMIStochastic(),
            new CalculateDominantCycleTunedRelativeStrengthIndex(),
            new CalculateDonchianChannels(),
            new CalculateDonchianChannelWidth(),
            new CalculateDoubleExponentialMovingAverage(),
            new CalculateDoubleExponentialSmoothing(),
            new CalculateDoubleSmoothedMomenta(),
            new CalculateDoubleSmoothedRelativeStrengthIndex(),
            new CalculateDoubleSmoothedStochastic(),
            new CalculateDoubleStochasticOscillator(),
        };

        public IMarketAnalyzer GetInstanceByType(string analyzerType)
        {
            return AllAnalyzers.FirstOrDefault(a => a.GetType().ToString() == analyzerType);
        }

        public Signal GetDecision(string analyzerType, List<TickerData> tickers)
        {
            var analyzer = GetInstanceByType(analyzerType);
            if (analyzer == null)
                return Signal.None;
            return analyzer.Analyze(tickers).Decision;
        }

        public AnalyzersHub()
        {
        }
    }
}
