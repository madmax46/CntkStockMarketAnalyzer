using System.Collections.Generic;
using SharpML.Recurrent.Models;

namespace SharpML.Recurrent.Networks
{
    public interface ILayer 
    {
        Matrix ComputeOutput(Matrix input, Graph g);
        void ResetState();
        List<Matrix> GetParameters();
    }
}
