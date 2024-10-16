using System;
using System.Collections.Generic;
using System.ServiceModel;
using APIClasses;

namespace APIClasses
{
    [ServiceContract]
    public interface IJobService
    {
        [OperationContract]
        Job GetJob(); // Get an available job
        
        [OperationContract]
        void SubmitSolution(int jobId, string result); // Submit a solution for a specific job
    }
}
