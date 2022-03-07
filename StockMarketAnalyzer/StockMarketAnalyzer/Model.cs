using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CNTK;
using StockMarketNetworkLib;
using StockMarketNetworkLib.Data;

namespace StockMarketAnalyzer
{

    public interface IModel
    {
        void LoadData(string splitColum, bool isFirstHeader);
        void LoadDataForecast(string splitColum, bool isFirstHeader);
        void CreateNet(ANNType annType, int inputDim, int outputDim, string hiddenDim, ActivationFunc activationFunc, OutputActivationFunc outputActivationFunc, int LSTMCellDim);

        void TrainNet(TrainerAlgorithms trainerAlgorithms, string learningRate, int epochCount, int batchCount, double learnerMomentum, LossFunc lossFunc, EvalErrorFunc evalError, ValidationType validationType);

        void TestNet();
        void CreateClassification(NormalizationMethod normMethod, int window, int nextVal, int oneColIndex, string allAnalysisValues, int percentTrain, int percentTest, int useColAnalysisData);

        void CreateRegression(NormalizationMethod normnMethod, int window, int nextVal, int oneColIndex, string allAnalysisValues, int percentTrain, int percentTest, int useColAnalysisData, double dopMult);

        void LeaveOnlyAnalysisData();
        void LeaveOnlyAnalysisDataForecast();
        void ClearWrongColumns();
        void ClearWrongColumnsForecastData();
        void CancelTrainTest();

        void SaveNeuralModel();
        void LoadNeuralModel();

        void EvalueateRegressionModel();
    }


    class Model : IModel
    {
        #region Events
        public delegate void TrainingProcess(TrainingStatInfo trainArgs, TrainingStatInfo testArgs);
        public event TrainingProcess TrainingProcessEvent;

        public delegate void TrainigProcessEnd(TrainingStatInfo trainArgs, TrainingStatInfo testArgs);
        public event TrainigProcessEnd TrainigProcessEndEvent;

        public delegate void FileLoaded();
        public event FileLoaded FileLoadedEvent;
        public event FileLoaded FileForecastLoadedEvent;


        public delegate void ModelCreated();
        public event ModelCreated ModelCreatedEvent;

        public delegate void EvaluateEnd(List<List<double>> result);
        public event EvaluateEnd EvaluateEndEvent;

        public delegate void EvalueateRegressionModelEnd(List<double[]> changePrice);
        public event EvalueateRegressionModelEnd EvalRegresModelEndEvent;



        public delegate void InsideError(ErrorInfo ex);
        public event InsideError InsideErrorEvent;


        public event EventHandler ModelLoaded;

        #endregion

        #region Public properties

        public LearningCNTKConfig learningCNTKConfig { get; set; }

        public DataTable AllLoadedLearnData { get; set; }
        public DataTable AllLoadedForecastData { get; set; }

        public List<DataItem<double>> TrainData { get; set; }

        public List<DataItem<double>> TestData { get; set; }
        public List<DataItem<double>> ValidationData { get; set; }

        public string PathToData { get; private set; }
        public string PathToForecastData { get; private set; }



        #endregion



        #region Private properties

        private Function model;
        private ANNType annType = ANNType.LSTM;
        private IEnumerable<DataItem<double>> allAnalysisData;
        private List<DataItem<double>> allForecastData;
        private DeviceDescriptor device;
        private TrainerCNTK trainerCNTK;
        private NetworkState networkState;
        public Model()
        {
            var f = DeviceDescriptor.AllDevices();
            device = DeviceDescriptor.CPUDevice;

            try
            {
                device = DeviceDescriptor.GPUDevice(0);
                MessageBox.Show($"NOOOT Warning {string.Join("/n", f.Select(r => r.Type.ToString()))}");
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Warning {string.Join("/n", f.Select(r => r.Type.ToString()))}");
            }

            learningCNTKConfig = new LearningCNTKConfig();
            AllLoadedLearnData = new DataTable();
            TestData = new List<DataItem<double>>();
            TrainData = new List<DataItem<double>>();
            ValidationData = new List<DataItem<double>>();
            networkState = NetworkState.NotCreated;
        }

        #endregion


        //public static void TestCreatingAndTraningFNN()
        //{

        //    device = DeviceDescriptor.CPUDevice;

        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    System.Data.DataTable data = new System.Data.DataTable();

        //    //   data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\SBER_170101_180513 (2).txt", true);   //SBER_170101_180513
        //    // data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\AAPL.csv", true,",");   //SBER_170101_180513

        //    //data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\SBER_170101_180513.txt", true);   //SBER_170101_180513

        //    //data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\SBER_170101_180515.txt", true);   //SBER_170101_180513
        //    data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\GDAX.BTC-USD_170620_180620.txt", true);   //SBER_170101_180513

        //    //


        //    //List<int> list = new List<int>() { 1, 2,4 };
        //    var dataTable = PrepareData.ClearWrongColumns(data);

        //    //var values = PrepareData.ColumnToArray(dataTable,5);

        //    //var values = PrepareData.ColumnToArray(dataTable, 4, CultureInfo.GetCultureInfo("en-En"));
        //    var values5col = PrepareData.ColumnToArray(dataTable, 5, CultureInfo.GetCultureInfo("en-En"));

        //    var values = PrepareData.ColumnsToListArray(dataTable, new int[] { 2, 3, 4, 5, 6 }, CultureInfo.GetCultureInfo("en-En"));


        //    var allDataOld = PrepareData.CreateDataForClassification(values5col, 30, 1);
        //    // allData = PrepareData.CreateDataForClassificationManyAxisInOneCol(values5col,values, 30, 1);

        //    //allData = PrepareData.CreateDataForRegressionPriceChange(values5col, 30, 1);
        //    allData = PrepareData.CreateDataForRegressionPriceChangeManyAxisInOneCol(values5col, values, 30, 1, false, NormalizationMethod.ZScore);

        //    //var Laaaast = PrepareData.CreateDataForClassification(values, 30, 0).Last();
        //    var Laaaast = allData.Last();

        //    Console.WriteLine($"Работа с данными закочена {stopwatch.Elapsed}");

        //    //var model = CNTKNetBuilder.GenerateFullyConnectedNet(new int[] { 150, 100,64, 2 }, ActivationFunc.Tanh, OutputActivationFunc.SoftMax, device);
        //    // var model = CNTKNetBuilder.GenerateFullyConnectedNet(new int[] { 30, 64, 16, 1 }, ActivationFunc.Sigmoid, OutputActivationFunc.None, device);

        //    model = CNTKNetBuilder.GenerateLSTMNet(new int[] { 150, 200, 100, 50, 1 }, 30, ActivationFunc.LeakyReLU, OutputActivationFunc.None, device); //успешная регрессия
        //                                                                                                                                                 //var model = CNTKNetBuilder.GenerateLSTMNet(new int[] { 30, 64, 1 }, 30, device, ActivationFunc.Tanh, OutputActivationFunc.None);

        //    // model = CNTKNetBuilder.GenerateLSTMNet(new int[] { 150, 200, 100, 50, 2 }, 30, device, ActivationFunc.Tanh, OutputActivationFunc.SoftMax);

        //    TrainerCNTK trainerCNTK = new TrainerCNTK(new LearningCNTKConfig());
        //    trainerCNTK.TrainingEndEvent += TrainerCNTK_TrainingEndEvent;
        //    model = trainerCNTK.Train(model, ANNType.LSTM, device, allData);
        //    Console.WriteLine($"Обучение закончено {stopwatch.Elapsed}");

        //    var result = CNTKHelper.EvaluateModel(model, device, Laaaast);

        //    var res = CNTKHelper.ValidateRegressionModel(model, device, allData, 0.01);


        //    //var imap = new Dictionary<Variable, Value>()
        //    //    {
        //    //        { model.Output, Value.Create(new NDShape(1,30), new List<double[]>(){Laaaast.Input },new List<bool>{true }, device ) }
        //    //    };

        //    //var omap = new Dictionary<Variable, Value>() { { model, null } };
        //    //model.Evaluate(imap, omap, device);
        //    //var o = omap[model].GetDenseData<double>(model);
        //    //var fddd = o.Select(l => l.IndexOf(l.Max())).ToList();
        //    //var res = o.Max();




        //    var last = allData.Last();

        //}





        #region work on neural net


        public void CreateNet(ANNType annType_, int inputDim, int outputDim, string hiddenDim, ActivationFunc activationFunc, OutputActivationFunc outputActivationFunc, int LSTMCellDim)
        {

            if (networkState == NetworkState.Train || networkState == NetworkState.Test)
            {
                burnErrorEvent(null, "Сеть обучается/тестируется");
                return;
            }

            Task.Factory.StartNew(() =>
            {

                try
                {
                    List<int> layersSizeList = new List<int>() { inputDim };
                    string[] hiddenSplit = hiddenDim.Split(';');
                    layersSizeList.AddRange(hiddenSplit.Select(r => Convert.ToInt32(r)).ToArray());
                    layersSizeList.Add(outputDim);
                    int[] layersSizes = layersSizeList.ToArray();
                    if (device == null)
                        throw new Exception("device is null");
                    if (annType_ == ANNType.FNN)
                        model = CNTKNetBuilder.GenerateFullyConnectedNet(layersSizes, activationFunc, outputActivationFunc, device);
                    else
                        model = CNTKNetBuilder.GenerateLSTMNet(layersSizes, LSTMCellDim, activationFunc, outputActivationFunc, device);
                    annType = annType_;
                    networkState = NetworkState.Created;
                    ModelCreatedEvent?.Invoke();
                }
                catch (Exception ex)
                {
                    burnErrorEvent(ex, string.Empty);
                }

            });


        }

        public void TrainNet(TrainerAlgorithms trainerAlgorithm, string learningRateText, int epochCount, int batchCount, double learnerMomentum, LossFunc lossFunc, EvalErrorFunc evalError, ValidationType validationType)
        {
            if (model == null)
            {
                burnErrorEvent(null, "Не создана сеть");
                return;
            }

            if (TrainData == null || !TrainData.Any())
            {
                burnErrorEvent(null, "Не создана обучающая выборка");
                return;
            }
            GC.Collect();
            if (networkState != NetworkState.Created)
            {
                if (networkState != NetworkState.Ready)
                {
                    burnErrorEvent(null, "Сеть обучается");
                    return;
                }
            }


            double learningRate = 0.001;

            Task task = new Task(async () =>
            {
                try
                {
                    var vp = new VectorPairSizeTDouble();
                    var alllearningRates = learningRateText.Split(';').Select(r => Convert.ToDouble(r)).ToArray();
                    int epochCountInOneLearnRate = epochCount / alllearningRates.Length;
                    for (uint i = 0; i < alllearningRates.Length - 1; i++)
                    {
                        vp.Add(new PairSizeTDouble((i + 1) * Convert.ToUInt32(epochCountInOneLearnRate), alllearningRates[i]));
                    }
                    vp.Add(new PairSizeTDouble(1, alllearningRates[alllearningRates.Length - 1]));


                    LearningCNTKConfig learningCNTKConfig_ = new LearningCNTKConfig(trainerAlgorithm, batchCount, lossFunc, evalError, learningRate, epochCount);
                    learningCNTKConfig_.VectorPairLearningRate = vp;
                    if (trainerCNTK == null)
                    {
                        TrainerCNTK trainerCNTK_ = new TrainerCNTK(learningCNTKConfig_);
                        trainerCNTK = trainerCNTK_;
                    }
                    else
                    {
                        TrainerCNTK trainerCNTK_ = new TrainerCNTK(learningCNTKConfig_);
                        trainerCNTK.UpdateTrainingStatisticEvetn -= TrainerCNTK_UpdateTrainingStatisticEvetn;
                        trainerCNTK.TrainingEndEvent -= TrainerCNTK_TrainingEndEvent1;
                        trainerCNTK.TrainingCanceledEvent -= TrainerCNTK_TrainingCanceledEvent;
                        trainerCNTK.TrainerInsideErrorEvent -= TrainerCNTK_TrainerInsideErrorEvent;
                        trainerCNTK = trainerCNTK_;
                    }
                    trainerCNTK.UpdateTrainingStatisticEvetn += TrainerCNTK_UpdateTrainingStatisticEvetn;
                    trainerCNTK.TrainingEndEvent += TrainerCNTK_TrainingEndEvent1;
                    trainerCNTK.TrainingCanceledEvent += TrainerCNTK_TrainingCanceledEvent;
                    trainerCNTK.TrainerInsideErrorEvent += TrainerCNTK_TrainerInsideErrorEvent;


                    networkState = NetworkState.Train;

                    await trainerCNTK.TrainAsync(model, annType, device, TrainData, TestData);

                    learningCNTKConfig = learningCNTKConfig_;
                    networkState = NetworkState.Ready;

                }
                catch (Exception ex)
                {
                    networkState = NetworkState.Ready;
                    burnErrorEvent(ex, string.Empty);
                }
            });
            task.Start();
        }

        private void TrainerCNTK_TrainerInsideErrorEvent(Exception ex)
        {
            burnErrorEvent(ex, "Ошибка в обучении");
        }

        public void TestNet()
        {
            //if (model == null)
            //{
            //    burnErrorEvent(null, "Не создана сеть");
            //    return;
            //}

            //if (trainerCNTK == null)
            //{
            //    burnErrorEvent(null, "Сеть не обучена!");
            //}

            //if (TestData == null || !TestData.Any())
            //{
            //    burnErrorEvent(null, "Не создана выборка для тестирования");
            //    return;
            //}

            //if (networkState != NetworkState.Ready)
            //{
            //    burnErrorEvent(null, "Сеть еще не обучена");
            //    return;
            //}


            //Task task = new Task(async () =>
            //{
            //    try
            //    {
            //        networkState = NetworkState.Test;
            //        trainerCNTK.UpdateTestingStatisticEvetn += TrainerCNTK_UpdateTestingStatisticEvetn;
            //        trainerCNTK.TestingEndEvent += TrainerCNTK_TestingEndEvent;

            //        //await trainerCNTK.TestAsync(model, annType, device, TestData);
            //        networkState = NetworkState.Ready;

            //    }
            //    catch (Exception ex)
            //    {
            //        networkState = NetworkState.Ready;
            //        burnErrorEvent(ex, string.Empty);
            //    }
            //});
            //task.Start();
        }


        public void CancelTrainTest()
        {
            if (trainerCNTK != null)
            {
                trainerCNTK.CancelProcces();
            }
        }


        public void EvaluateForecastData(string allColumns, int window, NormalizationMethod normMethod)
        {
            if (model == null)
            {
                burnErrorEvent(null, "Не загружена сеть");
                return;
            }
            if (AllLoadedForecastData == null || AllLoadedForecastData.Rows.Count == 0)
            {
                burnErrorEvent(null, "Не загружены данные для анализа");
                return;
            }

            Task task = new Task(() =>
            {
                try
                {
                    PrepareForecastData(allColumns, window, normMethod);
                    foreach (var oneItem in allForecastData)
                    {
                        var evalResult = CNTKHelper.EvaluateModel(model, device, oneItem);

                        oneItem.Output = evalResult[0].ToList();
                    }

                    var outputResults = allForecastData.Select(r => r.Output).ToList();

                    EvaluateEndEvent?.Invoke(outputResults);

                }
                catch (Exception ex)
                {
                    burnErrorEvent(ex, string.Empty);
                }
            });
            task.Start();


        }

        public void SaveNeuralModel()
        {
            if (model == null || networkState == NetworkState.Train || networkState == NetworkState.Test)
            {
                string errorMsg = "не создана";
                if (networkState == NetworkState.Train) errorMsg = "обучается";
                if (networkState == NetworkState.Test) errorMsg = "тестируется";

                burnErrorEvent(null, $"Невозможно сохранить сеть, т.к. она {errorMsg}");
                return;
            }


            SaveFileDialog saveFileDialog = new SaveFileDialog();
            var resultDialog = saveFileDialog.ShowDialog();
            if (resultDialog != DialogResult.OK)
                return;

            string path = saveFileDialog.FileName;
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                var res = CNTKHelper.SaveModel(model, path, "");
                if (res == false)
                {
                    burnErrorEvent(null, "Ошибка сохранения сети");
                }
            }
            catch (Exception ex)
            {
                burnErrorEvent(ex, "Ошибка сохранения сети");
            }

        }

        public void LoadNeuralModel()
        {

            if (networkState == NetworkState.Train || networkState == NetworkState.Test)
            {
                burnErrorEvent(null, "Сеть обучается/тестируется");
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            var resultDialog = openFileDialog.ShowDialog();
            if (resultDialog != DialogResult.OK)
                return;

            string path = openFileDialog.FileName;
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                var loadedModel = CNTKHelper.LoadModel(path, device);
                if (loadedModel.Item1 == true)
                {
                    model = loadedModel.Item2;
                    networkState = NetworkState.Ready;
                    ModelLoaded?.Invoke(this, new EventArgs());
                }
                else
                {
                    burnErrorEvent(null, "Ошибка загрузки сети");
                }
            }
            catch (Exception ex)
            {
                burnErrorEvent(ex, "Ошибка загрузки сети");
            }
        }


        public void EvalueateRegressionModel()
        {
            EvalRegressionModelOnData(allAnalysisData.ToList());

        }

        public void EvalueateRegressionModelForecastData()
        {
            EvalRegressionModelOnData(allForecastData);

        }


        private void EvalRegressionModelOnData(List<DataItem<double>> data_)
        {
            if (model == null)
            {
                burnErrorEvent(null, "Не загружена сеть");
                return;
            }

            if (networkState == NetworkState.Train || networkState == NetworkState.Test)
            {
                burnErrorEvent(null, "Сеть обучается/тестируется");
                return;
            }

            if (data_ == null || data_.Count == 0)
            {
                burnErrorEvent(null, "Не загружены данные для анализа");
                return;
            }

            Task task = new Task(() =>
            {
                try
                {
                    List<double[]> results = new List<double[]>();
                    foreach (var oneItem in data_)
                    {
                        var evalResult = CNTKHelper.EvaluateModel(model, device, oneItem);
                        double[] pair = new double[2];
                        pair[0] = oneItem.Output[0];
                        pair[1] = evalResult[0][0];
                        results.Add(pair);
                    }

                    EvalRegresModelEndEvent?.Invoke(results);
                }
                catch (Exception ex)
                {
                    burnErrorEvent(ex, string.Empty);
                }
            });
            task.Start();
        }


        #endregion


        #region learner events


        private void TrainerCNTK_UpdateTrainingStatisticEvetn(TrainingStatInfo trainArgs, TrainingStatInfo testArgs)
        {
            TrainingProcessEvent?.Invoke(trainArgs, testArgs);
        }

        private void TrainerCNTK_TrainingEndEvent1(TrainingStatInfo trainargs, TrainingStatInfo testargs)
        {
            TrainigProcessEndEvent?.Invoke(trainargs, testargs);
        }

        private void TrainerCNTK_TrainingCanceledEvent()
        {


        }


        //private void TrainerCNTK_TrainingEndEvent()
        //{
        //    var Laaaast = allAnalysisData.Last();

        //    var result = CNTKHelper.EvaluateModel(model, device, Laaaast);

        //    var res1 = CNTKHelper.ValidateModel(model, device, allAnalysisData);

        //    var res = CNTKHelper.ValidateRegressionModel(model, device, allAnalysisData, 0.01);
        //}

        #endregion


        #region work on data for analysis

        public void LoadData(string splitColum, bool isFirstHeader)
        {
            string path = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            var resultDialog = openFileDialog.ShowDialog();
            if (resultDialog != DialogResult.OK)
                return;

            Task task = new Task(() =>
            {
                path = openFileDialog.FileName;
                if (string.IsNullOrEmpty(path))
                    return;

                PathToData = path;
                AllLoadedLearnData = PrepareData.ParseDataToTableMyVers(path, isFirstHeader, splitColum[0]);
                //AllLoadedData = PrepareData.ClearWrongColumns(AllLoadedData);

                FileLoadedEvent?.Invoke();
            });
            task.Start();
        }
        public void LoadDataForecast(string splitColum, bool isFirstHeader)
        {
            string path = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            var resultDialog = openFileDialog.ShowDialog();
            if (resultDialog != DialogResult.OK)
                return;

            Task task = new Task(() =>
            {
                path = openFileDialog.FileName;
                if (string.IsNullOrEmpty(path))
                    return;

                PathToForecastData = path;
                AllLoadedForecastData = PrepareData.ParseDataToTableMyVers(path, isFirstHeader, splitColum[0]);
                //AllLoadedData = PrepareData.ClearWrongColumns(AllLoadedData);

                FileForecastLoadedEvent?.Invoke();
            });
            task.Start();
        }


        public void ClearWrongColumns()
        {
            Task task = new Task(() =>
            {
                AllLoadedLearnData = PrepareData.ClearWrongColumns(AllLoadedLearnData);

                FileLoadedEvent?.Invoke();
            });
            task.Start();
        }
        public void ClearWrongColumnsForecastData()
        {
            Task task = new Task(() =>
            {
                AllLoadedForecastData = PrepareData.ClearWrongColumns(AllLoadedForecastData);

                FileForecastLoadedEvent?.Invoke();
            });
            task.Start();
        }

        public void LeaveOnlyAnalysisData()
        {
            Task task = new Task(() =>
            {
                AllLoadedLearnData = PrepareData.LeaveOnlyAnalysisData(AllLoadedLearnData);

                FileLoadedEvent?.Invoke();
            });
            task.Start();
        }
        public void LeaveOnlyAnalysisDataForecast()
        {
            Task task = new Task(() =>
            {
                AllLoadedForecastData = PrepareData.LeaveOnlyAnalysisData(AllLoadedForecastData);

                FileForecastLoadedEvent?.Invoke();
            });
            task.Start();
        }


        public void CreateClassification(NormalizationMethod normMethod, int window, int nextVal, int oneColIndex, string allAnalysisValues, int percentTrain, int percentTest, int useColAnalysisData)
        {
            Task task = new Task(() =>
            {
                try
                {
                    if (percentTrain + percentTest != 100)
                    {
                        burnErrorEvent(null, "percentTrain + percentTest должно быть равно 100");
                        return;
                    }

                    List<DataItem<double>> allDataForAnalysis = new List<DataItem<double>>();

                    double[] oneColumnValues = PrepareData.ColumnToArray(AllLoadedLearnData, oneColIndex);

                    if (useColAnalysisData == 0)
                    {
                        allDataForAnalysis = PrepareData.CreateDataForClassification(oneColumnValues, window, nextVal, normMethod);
                    }
                    else
                    {
                        string[] hiddenSplit = allAnalysisValues.Split(';');
                        int[] allAnalysisColIndex = hiddenSplit.Select(r => Convert.ToInt32(r)).ToArray();
                        var allColumnsValues = PrepareData.ColumnsToListArray(AllLoadedLearnData, allAnalysisColIndex);
                        allDataForAnalysis = PrepareData.CreateDataForClassificationManyAxisInOneCol(oneColumnValues, allColumnsValues, window, nextVal, normMethod);
                    }

                    var splitetData = PrepareData.SplitDataOnTestTrainValid(allDataForAnalysis, percentTrain, percentTest, 0, false);
                    TrainData = splitetData.Item1;
                    TestData = splitetData.Item2;
                    allAnalysisData = allDataForAnalysis;
                }
                catch (Exception ex)
                {
                    burnErrorEvent(ex, string.Empty);
                }
            });
            task.Start();
        }

        public void CreateRegression(NormalizationMethod normMethod, int window, int nextVal, int oneColIndex, string allAnalysisValues, int percentTrain, int percentTest, int useColAnalysisData, double dopMult = 1)
        {
            Task task = new Task(() =>
            {
                try
                {
                    if (percentTrain + percentTest != 100)
                        throw new Exception("percentTrain + percentTest должно быть равно 100");

                    IEnumerable<DataItem<double>> allDataForAnalysis = new List<DataItem<double>>();

                    double[] oneColumnValues = PrepareData.ColumnToArray(AllLoadedLearnData, oneColIndex);

                    if (useColAnalysisData == 0)
                    {
                        allDataForAnalysis = PrepareData.CreateDataForRegressionPriceChange(oneColumnValues, window, nextVal, dopMult, normalizationMethod: normMethod).ToList();
                    }
                    else
                    {
                        string[] hiddenSplit = allAnalysisValues.Split(';');
                        int[] allAnalysisColIndex = hiddenSplit.Select(r => Convert.ToInt32(r)).ToArray();
                        var allColumnsValues = PrepareData.ColumnsToListArray(AllLoadedLearnData, allAnalysisColIndex);
                        allDataForAnalysis = PrepareData.CreateDataForRegressionPriceChangeManyAxisInOneCol(oneColumnValues, allColumnsValues, window, nextVal, false, dopMult, normalizationMethod: normMethod);
                    }

                    var splitetData = PrepareData.SplitDataOnTestTrainValid(allDataForAnalysis.ToList(), percentTrain, percentTest, 0, false);
                    TrainData = splitetData.Item1;
                    TestData = splitetData.Item2;
                    allAnalysisData = allDataForAnalysis;
                }
                catch (Exception ex)
                {
                    burnErrorEvent(ex, string.Empty);
                }
            });
            task.Start();

        }


        public void CreateRegressionOnMathFunction(NormalizationMethod normMethod, int window, int nextVal, int oneColIndex, int percentTrain, int percentTest, int useColAnalysisData)
        {
            Task task = new Task(() =>
            {
                try
                {
                    if (percentTrain + percentTest != 100)
                        throw new Exception("percentTrain + percentTest должно быть равно 100");

                    List<DataItem<double>> allDataForAnalysis = new List<DataItem<double>>();

                    double[] oneColumnValues = PrepareData.ColumnToArray(AllLoadedLearnData, oneColIndex);

                    //if (useColAnalysisData == 0)
                    //{
                    allDataForAnalysis = PrepareData.CreateDataForRegression(oneColumnValues, window, nextVal, normalizationMethod: normMethod);
                    //}
                    //else
                    //{
                    //    string[] hiddenSplit = allAnalysisValues.Split(';');
                    //    int[] allAnalysisColIndex = hiddenSplit.Select(r => Convert.ToInt32(r)).ToArray();
                    //    var allColumnsValues = PrepareData.ColumnsToListArray(AllLoadedLearnData, allAnalysisColIndex);
                    //    allDataForAnalysis = PrepareData.CreateDataForRegressionPriceChangeManyAxisInOneCol(oneColumnValues, allColumnsValues, window, nextVal, false, dopMult, normalizationMethod: normMethod);
                    //}

                    var splitetData = PrepareData.SplitDataOnTestTrainValid(allDataForAnalysis, percentTrain, percentTest, 0, false);
                    TrainData = splitetData.Item1;
                    TestData = splitetData.Item2;
                    allAnalysisData = allDataForAnalysis;
                }
                catch (Exception ex)
                {
                    burnErrorEvent(ex, string.Empty);
                }
            });
            task.Start();

        }



        private void PrepareForecastData(string allColumns, int window, NormalizationMethod normMethod)
        {
            List<DataItem<double>> allDataForForecast = new List<DataItem<double>>();

            string[] hiddenSplit = allColumns.Split(';');
            int[] allAnalysisColIndex = hiddenSplit.Select(r => Convert.ToInt32(r)).ToArray();
            var allColumnsValues = PrepareData.ColumnsToListArray(AllLoadedForecastData, allAnalysisColIndex);

            allDataForForecast = PrepareData.CreateDataByWindow(allColumnsValues, window, false, normMethod);

            allForecastData = allDataForForecast;
        }

        #endregion



        #region burn events
        private void burnErrorEvent(Exception ex, string addMsg)
        {
            InsideErrorEvent?.Invoke(new ErrorInfo(ex, addMsg));
        }


        #endregion
    }
}
