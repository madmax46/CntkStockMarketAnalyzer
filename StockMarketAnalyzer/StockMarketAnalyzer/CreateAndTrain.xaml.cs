using LiveCharts;
using LiveCharts.Wpf;
using StockMarketNetworkLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StockMarketAnalyzer
{
    /// <summary>
    /// Логика взаимодействия для CreateAndTrain.xaml
    /// </summary>
    public partial class CreateAndTrain : Window
    {
        Model model_;
        public CreateAndTrain()
        {
            InitializeComponent();
            model_ = new Model();
            model_.ModelLoaded += Model__ModelLoaded;
            model_.FileLoadedEvent += Model__FileLoadedEvent;
            model_.FileForecastLoadedEvent += Model__FileForecastLoadedEvent;

            model_.InsideErrorEvent += Model__InsideErrorEvent;

            model_.ModelCreatedEvent += Model__ModelCreatedEvent;

            model_.TrainingProcessEvent += Model__TrainingProcessEvent;
            model_.TrainigProcessEndEvent += Model__TrainigProcessEndEvent;

            model_.EvaluateEndEvent += Model__EvaluateEndEvent;
            model_.EvalRegresModelEndEvent += Model__EvalRegresModelEndEvent;
            UIOutputActivationFunc.DataContext = ConvertEnums.EnumOutputActivationFunc;
            UIActivationFunc.DataContext = ConvertEnums.EnumActivationFunc;
            UILearnerCombo.DataContext = ConvertEnums.EnumTrainerAlgorithms;
            UILossFuncVal.DataContext = ConvertEnums.EnumLossFunc;
            UIEvalFuncVal.DataContext = ConvertEnums.EnumEvalErrorFunc;
            UITaskType.DataContext = ConvertEnums.EnumValidationType;
            UINetType.DataContext = ConvertEnums.EnumNetType;
            UINormalizeMethod.DataContext = ConvertEnums.EnumNormalizationMethod;
            UINormalizeMethodForecast.DataContext = ConvertEnums.EnumNormalizationMethod;
        }


        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        #region model events
        private void Model__FileForecastLoadedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                UIDatagridLoadedDataForecast.DataContext = model_.AllLoadedForecastData.AsDataView();
                UiTextPathToFileForecast.Text = model_.PathToForecastData;
            });

        }

        private void Model__TrainingProcessEvent(TrainingStatInfo trainArgs, TrainingStatInfo testArgs)
        {
            Dispatcher.Invoke(() =>
            {
                UIProgressLearner.Value = trainArgs.ProgressInPercent;
                UIProggressTextBlock.Text = $"Обучение. Выполнено {trainArgs.ProgressInPercent}%;  {trainArgs.Epoch} эпоха обучения;  {trainArgs.Accuracy.ToString("F")}% точность; {trainArgs.ElapsedTime.TotalSeconds.ToString("F")} с. время выполнения";

                var seriesFinded = UILossChart.Series?.FirstOrDefault(r => r.Title == "Train");

                if (seriesFinded != null)
                {
                    seriesFinded.Values.Add(trainArgs.Loss);
                }

                var seriesFindedAcc = UIAccuracyChart.Series?.FirstOrDefault(r => r.Title == "Train");
                if (seriesFindedAcc != null)
                {
                    seriesFindedAcc.Values.Add(trainArgs.Accuracy);
                }

                var seriesFindedTestAcc = UIAccuracyChart.Series?.FirstOrDefault(r => r.Title == "Test");
                if (seriesFindedTestAcc != null)
                {
                    seriesFindedTestAcc.Values.Add(testArgs.Accuracy);
                }


            });
        }
        //private void Model__TestingProcessEvent(TrainingStatInfo args)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        UIProgressLearner.Value = args.ProgressInPercent;
        //        UIProggressTextBlock.Text = $"Тестирование. Выполнено {args.ProgressInPercent}%;  {args.Epoch} эпоха тестирования;  {args.Accuracy.ToString("F")}% точность; {args.ElapsedTime.TotalSeconds.ToString("F")} с. время выполнения";

        //        var seriesFinded = UILossChart.Series?.FirstOrDefault(r => r.Title == "Test");

        //        if (seriesFinded != null)
        //        {
        //            seriesFinded.Values.Add(args.Loss);
        //        }

        //        var seriesFindedAcc = UIAccuracyChart.Series?.FirstOrDefault(r => r.Title == "Test");
        //        if (seriesFindedAcc != null)
        //        {
        //            seriesFindedAcc.Values.Add(args.Accuracy);
        //        }

        //    });
        //}


        //private void Model__TestingProcessEndEvent(TrainingStatInfo args)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        UITestStatus.Text = $" {args.Accuracy.ToString("F")}% точность; {args.ElapsedTime.TotalSeconds.ToString("F")} с. время выполнения";
        //    });
        //    //  UITrainStatus
        //}
        private void Model__TrainigProcessEndEvent(TrainingStatInfo trainArgs, TrainingStatInfo testArgs)
        {
            Dispatcher.Invoke(() =>
            {
                UITrainStatus.Text = $" {trainArgs.Accuracy.ToString("F")}% точность; {trainArgs.ElapsedTime.TotalSeconds.ToString("F")} с. время";
                UITestStatus.Text = $" {testArgs.Accuracy.ToString("F")}% точность";
            });
        }


        private void Model__ModelCreatedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                UIStatusCreateNet.Text = "Сатус: сеть создана";
            });
        }

        private void Model__InsideErrorEvent(ErrorInfo info)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(string.Concat(info.exception, Environment.NewLine, info.AdditionalMsg), "Внимание!");
            });
        }

        private void Model__FileLoadedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                UiAllDataGrid.DataContext = model_.AllLoadedLearnData.AsDataView();
                UiTextPathToFile.Text = model_.PathToData;
            });
        }


        private void Model__EvaluateEndEvent(List<List<double>> result)
        {
            Dispatcher.Invoke(() =>
            {
                if (!result.Any())
                    return;
                UIDatagridResultOfAnalysis.AutoGenerateColumns = false;
                UIDatagridResultOfAnalysis.Columns.Clear();
                for (int i = 0; i < result[0].Count; i++)
                {
                    var col = new DataGridTextColumn();
                    col.Header = "Class " + i;
                    col.Binding = new Binding(string.Format("[{0}]", i));
                    UIDatagridResultOfAnalysis.Columns.Add(col);
                }
                UIDatagridResultOfAnalysis.DataContext = result;
            });
        }


        private void Model__EvalRegresModelEndEvent(List<double[]> changePrice)
        {
            Dispatcher.Invoke(() =>
            {
                UIRegressionResultChart.Series = new SeriesCollection()
                    {
                        new LineSeries
                        {
                            Title = "genuine",
                            Values = new ChartValues<double>(changePrice.Select(r=>r[0]).ToList()),
                            Fill = Brushes.Transparent
                            

                        },
                        new LineSeries
                        {
                            Title = "evaluate",
                            Values = new ChartValues<double>(changePrice.Select(r=>r[1]).ToList()),
                            Fill = Brushes.Transparent
                        }
                    };
            });
        }


        private void Model__ModelLoaded(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UIStatusCreateNet.Text = "Сатус: сеть загружена";
            });
        }



        #endregion


        #region button click events

        /// <summary>
        /// метод загрузки данных из файла в датагрид/таблицу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UiButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            string separator = UIColumtSplit.Text.Trim();
            bool headeTable = UICheckBoxHeader.IsChecked != null ? Convert.ToBoolean(UICheckBoxHeader.IsChecked) : false;
            model_.LoadData(separator, headeTable);
        }


        /// <summary>
        /// Метод обработки данных для кластеризации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIClasterization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NormalizationMethod normMethod = (NormalizationMethod)UINormalizeMethod.SelectedValue;
                int window = Convert.ToInt32(UIWindow.Text);
                int nextVal = Convert.ToInt32(UINextValue.Text);
                int oneColIndex = Convert.ToInt32(UIImportantPriceValue.Text);
                string allAnalysisValues = UIAllAnalysisValues.Text;
                int percentTrain = Convert.ToInt32(UIPercentTrain.Text);
                int percentTest = Convert.ToInt32(UIPercentTest.Text);
                int useColAnalysisData = UIUseDataColumns.SelectedIndex;
                double dopMultPriceChange = Convert.ToDouble(UIDopMultPriceChabge.Text);
                model_.CreateRegression(normMethod, window, nextVal, oneColIndex, allAnalysisValues, percentTrain, percentTest, useColAnalysisData, dopMultPriceChange);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }

        }

        private void UIClassification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NormalizationMethod normMethod = (NormalizationMethod)UINormalizeMethod.SelectedValue;
                int window = Convert.ToInt32(UIWindow.Text);
                int nextVal = Convert.ToInt32(UINextValue.Text);
                int oneColIndex = Convert.ToInt32(UIImportantPriceValue.Text);
                string allAnalysisValues = UIAllAnalysisValues.Text;
                int percentTrain = Convert.ToInt32(UIPercentTrain.Text);
                int percentTest = Convert.ToInt32(UIPercentTest.Text);
                int useColAnalysisData = UIUseDataColumns.SelectedIndex;
                model_.CreateClassification(normMethod, window, nextVal, oneColIndex, allAnalysisValues, percentTrain, percentTest, useColAnalysisData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }

        }



        private void UICreateNet_Click(object sender, RoutedEventArgs e)
        {
            //здесь сбор параметров сети происходит
            try
            {
                ClearStatus();
                InitGraphsLoss(10);
                InitGraphsAccuracy(10);
                UIRegressionResultChart.Series = new SeriesCollection();
                UIStatusCreateNet.Text = "Сатус: сеть создается";
                ANNType annType = (ANNType)UINetType.SelectedValue;
                int LSTMCellDim = Convert.ToInt32(UILSTMSellDimension.Text);
                int inputDim = Convert.ToInt32(UIInputNeuronCount.Text);
                int outputDim = Convert.ToInt32(UIOutpuNeuronCount.Text);
                string hiddenDim = UIHiddenLayersCount.Text;
                ActivationFunc activationFunc = (ActivationFunc)UIActivationFunc.SelectedValue;
                OutputActivationFunc outputActivationFunc = (OutputActivationFunc)UIOutputActivationFunc.SelectedValue;
                model_.CreateNet(annType, inputDim, outputDim, hiddenDim, activationFunc, outputActivationFunc, LSTMCellDim);
                UIProgressLearner.Maximum = 100;
                UIProgressLearner.Value = 0;
                UIProggressTextBlock.Text = string.Empty;
            }
            catch (Exception ex)
            {
                UIStatusCreateNet.Text = "Сатус: ошибка создания сети";
                MessageBox.Show(ex.ToString());
            }

        }
        private void UITrainButtonClick(object sender, RoutedEventArgs e)
        {
            //обучение сети)

            try
            {
                ClearStatus();
                TrainerAlgorithms trainerAlgorithm = (TrainerAlgorithms)UILearnerCombo.SelectedValue;
                string learningRate = UIlearnRateBox.Text;
                int epochCount = Convert.ToInt32(UIEpochCountBox.Text);
                int batchCount = Convert.ToInt32(UiBatchCount.Text);
                double learnerMomentum = Convert.ToDouble(UiMomentum.Text);
                LossFunc lossFunc = (LossFunc)UILossFuncVal.SelectedValue;
                EvalErrorFunc evalError = (EvalErrorFunc)UIEvalFuncVal.SelectedValue;
                ValidationType validationType = (ValidationType)UITaskType.SelectedValue;
                InitGraphsLoss(epochCount);
                InitGraphsAccuracy(epochCount);
                model_.TrainNet(trainerAlgorithm, learningRate, epochCount, batchCount, learnerMomentum, lossFunc, evalError, validationType);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void UITestNet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UITestStatus.Text = string.Empty;
                CLearAccurscyChart();
                model_.TestNet();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void UICancel_Click(object sender, RoutedEventArgs e)
        {
            model_.CancelTrainTest();
        }



        private void UIClearUniportantInfo_Click(object sender, RoutedEventArgs e)
        {
            model_.ClearWrongColumns();
        }

        private void UILeaveOnlyStockPrice_Click(object sender, RoutedEventArgs e)
        {
            model_.LeaveOnlyAnalysisData();
        }


        private void UiButtonLoadForecast_Click(object sender, RoutedEventArgs e)
        {
            string separator = UIColumtSplitForecast.Text.Trim();
            bool headeTable = UICheckBoxHeaderForecast.IsChecked != null ? Convert.ToBoolean(UICheckBoxHeaderForecast.IsChecked) : false;
            model_.LoadDataForecast(separator, headeTable);
        }

        private void UIEvaluateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NormalizationMethod normMethod = (NormalizationMethod)UINormalizeMethodForecast.SelectedValue;
                int window = Convert.ToInt32(UIWindowForecast.Text);
                string allAnalysisColumns = UIAnalysisDataForecast.Text;

                model_.EvaluateForecastData(allAnalysisColumns, window, normMethod);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void UIClearUniportantInfoForecast_Click(object sender, RoutedEventArgs e)
        {
            model_.ClearWrongColumnsForecastData();
        }
        private void UILeaveOnlyStockPriceForecast_Click(object sender, RoutedEventArgs e)
        {
            model_.LeaveOnlyAnalysisDataForecast();
        }



        private void DataGridColumnHeader_OnClick(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null)
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    UiAllDataGrid.SelectedCells.Clear();
                }

                foreach (var item in UiAllDataGrid.Items)
                {
                    if (UiAllDataGrid.SelectedCells.Contains(new DataGridCellInfo(item, columnHeader.Column)))
                        continue;

                    UiAllDataGrid.SelectedCells.Add(new DataGridCellInfo(item, columnHeader.Column));
                }
            }
        }

        private void UIEvaluateRegressionModel_Click(object sender, RoutedEventArgs e)
        {
            model_.EvalueateRegressionModel();
        }



        private void UISaveModelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            model_.SaveNeuralModel();
        }

        private void UILoadModelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            model_.LoadNeuralModel();
        }

        private void UIExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion


        #region additional privat methods

        void ClearStatus()
        {
            UITrainStatus.Text = string.Empty;
            UITestStatus.Text = string.Empty;
        }

        void InitGraphsLoss(int maxEpoch)
        {
            UILossChart.Series = new SeriesCollection()
                    {
                        new LineSeries
                        {
                            Title = "Train",
                            Values = new ChartValues<double>()

                        },
                        new LineSeries
                        {
                            Title = "Test",
                            Values = new ChartValues<double>()

                        }
                    };

            List<string> lables = new List<string>();
            for (int i = 1; i <= maxEpoch; i++)
            {
                lables.Add(i.ToString());

            }
            UILossChart.AxisX[0].Labels = lables;

        }

        void InitGraphsLoss(List<string> lables)
        {
            UILossChart.Series = new SeriesCollection()
                    {
                        new LineSeries
                        {
                            Title = "Train",
                            Values = new ChartValues<double>()

                        },
                        new LineSeries
                        {
                            Title = "Test",
                            Values = new ChartValues<double>()

                        }
                    };
            if (UILossChart.AxisX[0].Labels == null)
                UILossChart.AxisX[0].Labels = new List<string>();

            UILossChart.AxisX[0].Labels = lables;
        }

        void InitGraphsAccuracy(int maxEpoch)
        {
            UIAccuracyChart.Series = new SeriesCollection()
                    {
                        new LineSeries
                        {
                            Title = "Train",
                            Values = new ChartValues<double>()
                        },
                        new LineSeries
                        {
                            Title = "Test",
                            Values = new ChartValues<double>()
                        }
                    };

            List<string> lables = new List<string>();
            for (int i = 1; i <= maxEpoch; i++)
            {
                lables.Add(i.ToString());

            }
            UIAccuracyChart.AxisX[0].Labels = lables;
        }

        void CLearAccurscyChart()
        {
            var series1 = UILossChart.Series.FirstOrDefault(r => r.Title == "Test");
            if (series1 != null)
            {
                series1.Values.Clear();
            }
            var series2 = UIAccuracyChart.Series.FirstOrDefault(r => r.Title == "Test");
            if (series2 != null)
            {
                series2.Values.Clear();
            }
        }


        #endregion

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UIClearRegressChart_Click(object sender, RoutedEventArgs e)
        {
            UIRegressionResultChart.Series = new SeriesCollection();
        }

        private void UIEvaluateRegressionModel_Copy_Click(object sender, RoutedEventArgs e)
        {
            model_.EvalueateRegressionModelForecastData();
        }

        private void UICreateDataForTestMatFunction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NormalizationMethod normMethod = (NormalizationMethod)UINormalizeMethod.SelectedValue;
                int window = Convert.ToInt32(UIWindow.Text);
                int nextVal = Convert.ToInt32(UINextValue.Text);
                int oneColIndex = Convert.ToInt32(UIImportantPriceValue.Text);
                int percentTrain = Convert.ToInt32(UIPercentTrain.Text);
                int percentTest = Convert.ToInt32(UIPercentTest.Text);
                int useColAnalysisData = UIUseDataColumns.SelectedIndex;
                model_.CreateRegressionOnMathFunction(normMethod, window, nextVal, oneColIndex, percentTrain, percentTest, useColAnalysisData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }

        }
    }
}
