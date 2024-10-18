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
        Job GetJob(); 
        
        [OperationContract]
        void SubmitSolution(int jobId, string result); 


        [OperationContract]
        void AddJob(Job job);

        [OperationContract]
        void confirmJob(Job job,bool success);

        [OperationContract]
        CurrentStatus GetCurrentStatus();
    }
}
