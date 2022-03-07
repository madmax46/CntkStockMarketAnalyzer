using CNTK;
using StockMarketNetworkLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketTest
{

    class class1
    {
        #region OOOOOLD
        private void CNTKNet(List<DataItem<double>> dataItems)
        {
            //foreach(var oneItem in dataItems)
            //{
            //    for(int i = 0; i <oneItem.Input.Length; i++)
            //    {
            //        oneItem.Input[i] = Math.Round(oneItem.Input[i],5);
            //    }
            //}


            var device = DeviceDescriptor.CPUDevice;

            var res = GetData();
            int inputDim = 2;
            int outputDim = 2;
            int hiddenDim = 64;
            // SizeTVector vector = new SizeTVector
            //var inputShape = new NDShape(1, inputDim);
            //var outputShape = new NDShape(1, outputDim);

            //Variable features = CNTKLib.InputVariable(new int[] { 1, 2 }, DataType.Double);
            //Variable label = Variable.InputVariable(outputShape, DataType.Double);

            Variable features = CNTKLib.InputVariable(new int[] { 2, 1 }, DataType.Double);
            Variable label = Variable.InputVariable(new int[] { 2, 1 }, DataType.Double);

            var W = new Parameter(new int[] { hiddenDim, 2 }, DataType.Double, CNTKLib.GlorotUniformInitializer(), device, "w1");
            var b = new Parameter(new int[] { hiddenDim }, DataType.Double, 1, device, "b1");
            var z1 = CNTKLib.Sigmoid((CNTKLib.Times(W, features) + b));
            //var z1 = CNTKLib.Times(W, features);



            var W2 = new Parameter(new int[] { 2, hiddenDim }, DataType.Double, CNTKLib.GlorotUniformInitializer(), device, "w2");
            var b2 = new Parameter(new int[] { 2 }, DataType.Double, 1, device, "b2");
            var model = CNTKLib.Softmax(CNTKLib.Times(W2, z1) + b2);
            string ffaf = model.AsString();

            var loss = CNTKLib.BinaryCrossEntropy(model, label);
            var evalError = CNTKLib.ClassificationError(model, label);

            CNTK.TrainingParameterScheduleDouble learningRatePerSample = new TrainingParameterScheduleDouble(0.1);

            IList<Learner> parameterLearner = new List<Learner>() { Learner.SGDLearner(model.Parameters(), learningRatePerSample) };

            var trainer = Trainer.CreateTrainer(model, loss, evalError, parameterLearner);

            int minbatchSize = 100;
            int numMinibatchesToTrain = 1000;



            //CNTKDictionary nTKDictionary = new CNTKDictionary()
            //{

            //}

            //  MinibatchSourceConfig minibatchSourceConfig = new MinibatchSourceConfig(transforms);

            int numEpochs = 10;
            StringBuilder stringBuilder = new StringBuilder();
            while (numEpochs > 0)
            {
                //var minibatchData = minibatchSource.GetNextMinibatch(minibatchSize, device);

                string s = string.Empty;
                foreach (var oneItem in res.Item1)
                //foreach (var oneItem in dataItems)
                {
                    Value fea = Value.CreateBatch<double>(new NDShape(1, 2), oneItem.Input, device);
                    Value fea2 = Value.CreateBatch<double>(new int[] { 2, 1 }, oneItem.Output, device);
                    string sad = fea2.AsString();
                    string ddd = fea.AsString();


                    var arguments = new Dictionary<Variable, Value>()
                //var arguments = new Dictionary<Variable, MinibatchData>()
                {
                    { features, fea },
                    { label, fea2 }
                };
                    foreach (var item in model.Arguments)
                    {
                        string s12 = item.Name + " " + item.AsString();
                    }

                    string s1223 = (fea.AsString());
                    string s12322 = (fea2.AsString());

                    trainer.TrainMinibatch(arguments, device);
                    //s = PrintTrainingProgress(trainer, 0, 1);
                }
                // stringBuilder.AppendLine(s);
                //z.Evaluate()

                //TestHelper.PrintTrainingProgress(trainer, miniBatchCount++, outputFrequencyInMinibatches);

                // Because minibatchSource is created with MinibatchSource.InfinitelyRepeat, 
                // batching will not end. Each time minibatchSource completes an sweep (epoch),
                // the last minibatch data will be marked as end of a sweep. We use this flag
                // to count number of epochs.
                //if (TestHelper.MiniBatchDataIsSweepEnd(minibatchData.Values))
                //{
                numEpochs--;
                //}
            }

            //textBox1.Text = stringBuilder.ToString();

            //Test(z, features, label/*, inputShape, outputShape*/, device);
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    //var res = mLPNetwork.ComputeOutput(new double[] { Convert.ToDouble(i) / 100, Convert.ToDouble(j) / 100 });
                    var imap = new Dictionary<Variable, Value>()
                    { { features, Value.Create<double>(new int []{ 2, 1},new List<List<double>> {  new List<double>() { Convert.ToDouble(i) / 100, Convert.ToDouble(j) / 100 } },new List<bool>{true }, device ) }
                    };
                    //foreach (var oneData in dataItems)
                    //{

                    //    var imap = new Dictionary<Variable, Value>()
                    //    {
                    //        { features, Value.Create<double>(new int []{ 2, 1},new List<List<double>> {  oneData.Input.ToList() },new List<bool>{true }, device ) }
                    //    };

                    var omap = new Dictionary<Variable, Value>() { { model, null } };
                    model.Evaluate(imap, omap, device);
                    var o = omap[model].GetDenseData<double>(model);
                    var fddd = o.Select(l => l.IndexOf(l.Max())).ToList();
                    var res1 = o.Max();
                    stringBuilder.AppendLine($"{i}  {j} => {o[0][0]} {o[0][1]} ");

                    //stringBuilder.AppendLine($"{oneData.Output[0]}  {oneData.Output[1]} => {o[0][0]} {o[0][1]} ");

                }

            }


            string ada = stringBuilder.ToString();



        }


        private Tuple<List<DataItem<double>>, List<DataItem<double>>> GetData()
        {

            List<DataItem<double>> traindata = new List<DataItem<double>>();
            List<DataItem<double>> points = new List<DataItem<double>>();

            Random random = new Random();
            for (int i = 0; i < 150; i++)
            {
                double poin1x = random.Next(0, 50);
                double poin1y = random.Next(0, 50);
                double poin2x = random.Next(50, 100);
                double poin2y = random.Next(50, 100);
                var dataItem1 = new DataItem<double>(new double[] { poin1x / 100, poin1y / 100 }, new double[] { 1, 0 });
                var dataItem2 = new DataItem<double>(new double[] { poin2x / 100, poin2y / 100 }, new double[] { 0, 1 });
                var dataItem3 = new DataItem<double>(new double[] { poin1x, poin1y }, new double[] { 1, 0 });
                var dataItem4 = new DataItem<double>(new double[] { poin2x, poin2y }, new double[] { 0, 1 });

                traindata.Add(dataItem1);
                traindata.Add(dataItem2);
                points.Add(dataItem3);
                points.Add(dataItem4);
            }


            Tuple<List<DataItem<double>>, List<DataItem<double>>> tuple = new Tuple<List<DataItem<double>>, List<DataItem<double>>>(traindata, points);
            return tuple;
        }
        #endregion
    }
}
