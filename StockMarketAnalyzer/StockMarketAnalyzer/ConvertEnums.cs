using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using StockMarketNetworkLib;

namespace StockMarketAnalyzer
{
    public class ConvertEnums
    {
        public static IEnumerable<ActivationFunc> EnumActivationFunc
        {
            get
            {
                return Enum.GetValues(typeof(ActivationFunc)).Cast<ActivationFunc>();
            }
        }

        public static IEnumerable<OutputActivationFunc> EnumOutputActivationFunc
        {
            get
            {
                return Enum.GetValues(typeof(OutputActivationFunc)).Cast<OutputActivationFunc>();
            }
        }

        public static IEnumerable<TrainerAlgorithms> EnumTrainerAlgorithms
        {
            get
            {
                return Enum.GetValues(typeof(TrainerAlgorithms)).Cast<TrainerAlgorithms>();
            }
        }

        public static IEnumerable<LossFunc> EnumLossFunc
        {
            get
            {
                return Enum.GetValues(typeof(LossFunc)).Cast<LossFunc>();
            }
        }

        public static IEnumerable<EvalErrorFunc> EnumEvalErrorFunc
        {
            get
            {
                return Enum.GetValues(typeof(EvalErrorFunc)).Cast<EvalErrorFunc>();
            }
        }
        public static IEnumerable<ValidationType> EnumValidationType
        {
            get
            {
                return Enum.GetValues(typeof(ValidationType)).Cast<ValidationType>();
            }
        }
        public static IEnumerable<ANNType> EnumNetType
        {
            get
            {
                return Enum.GetValues(typeof(ANNType)).Cast<ANNType>();
            }
        }
        public static IEnumerable<NormalizationMethod> EnumNormalizationMethod
        {
            get
            {
                return Enum.GetValues(typeof(NormalizationMethod)).Cast<NormalizationMethod>();
            }
        }
        


    }

}
