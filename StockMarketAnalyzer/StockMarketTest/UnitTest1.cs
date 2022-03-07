using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockMarketNetworkLib.Data;
using System.Data;
using System.Linq;
using System.Diagnostics;
using StockMarketNetworkLib;
using System.Collections.Generic;
using CNTK;

namespace StockMarketTest
{
    [TestClass]
    public class UnitTest1
    {
        
        public static void TestCreatingAndTraningFNN()
        {
            var device = DeviceDescriptor.CPUDevice;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            System.Data.DataTable data = new System.Data.DataTable();

            //   data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\SBER_170101_180513 (2).txt", true);   //SBER_170101_180513
            // data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\AAPL.csv", true,",");   //SBER_170101_180513

            //data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\SBER_170101_180513.txt", true);   //SBER_170101_180513

            data = PrepareData.ParseDataToTable(@"C:\Users\madmax\Desktop\SBER_170101_180515.txt", true);   //SBER_170101_180513



            //List<int> list = new List<int>() { 1, 2,4 };
            var dataTable = PrepareData.ClearWrongColumns(data);

            //var values = PrepareData.ColumnToArray(dataTable,5);

            //var values = PrepareData.ColumnToArray(dataTable, 4, CultureInfo.GetCultureInfo("en-En"));
            var values = PrepareData.ColumnToArray(dataTable, 5, CultureInfo.GetCultureInfo("en-En"));

            // var allData =   PrepareData.CreateDataForClassification(values,24*30,1); 
            var allData = PrepareData.CreateDataForClassification(values, 2, 1);
            var Laaaast = PrepareData.CreateDataForClassification(values, 2, 0).Last();

            Console.WriteLine($"Работа с данными закочена {stopwatch.Elapsed}");

            var model = CNTKNetBuilder.GenerateFullyConnectedNet(new int[] { 30, 20, 2 }, ActivationFunc.Sigmoid, OutputActivationFunc.Softmax, device);

            TrainerCNTK trainerCNTK = new TrainerCNTK(new StockMarketNetworkLib.LearningCNTKConfig());
            trainerCNTK.Train(model, ANNType.FNN,device,allData, allData);
            Console.WriteLine($"Обучение закончено {stopwatch.Elapsed}");



            Variable features = CNTKLib.InputVariable(new int[] { 1, 30 }, DataType.Double);
            Variable label = Variable.InputVariable(new int[] { 1,2 }, DataType.Double);

            var imap = new Dictionary<Variable, Value>()
                {
                    { features, Value.Create(new int []{ 1,30}, new List<List<double>>(){Laaaast.Input },new List<bool>{true }, device ) }
                };

            var omap = new Dictionary<Variable, Value>() { { model, null } };
            model.Evaluate(imap, omap, device);
            var o = omap[model].GetDenseData<double>(model);
            var fddd = o.Select(l => l.IndexOf(l.Max())).ToList();
            var res = o.Max();





            var last = allData.Last();
            //var last = allData[20];
            //var result = mLPNetwork.ComputeOutput(last.Input);

            //var result2 = mLPNetwork.ComputeOutput(Laaaast.Input);
            //var result3 = mLPNetwork.ComputeOutput(allData[allData.Count - 3].Input);

        }


        public static void Main()
        {
            TestCreatingAndTraningFNN();
        }
    }
}
