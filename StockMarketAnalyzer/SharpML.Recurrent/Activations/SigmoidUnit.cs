using System;

namespace SharpML.Recurrent.Activations
{
    [Serializable]
    public class SigmoidUnit : INonlinearity
    {
        private static long _serialVersionUid = 1L;
        private readonly long _id;

        private double alpha;
        public double Alpha
        { get { return alpha; } }

        public long Id
        {
            get { return _id; }
        }


        public SigmoidUnit()
        {
            _id = _serialVersionUid + 1;
            alpha = 1;
        }

        public SigmoidUnit(double alpha_) : this()
        {
            alpha = alpha_;
        }

        public double Forward(double x)
        {
            return 1 / (1 + Math.Exp(-1 * alpha * x));
        }

        public double Backward(double x)
        {
            double act = Forward(x);
            return alpha * act * (1 - act);
        }
    }
}
