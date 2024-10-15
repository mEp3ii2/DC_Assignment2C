using System;
using System.Collections.Generic;
using System.ServiceModel;
using APIClasses;

namespace WebServer
{
    [ServiceContract]
    public interface IJobService
    {
        [OperationContract]
        List<int> GetAvailableJobIds();

        [OperationContract]
        Job GetJob(int jobId);

        [OperationContract]
        bool SubmitJobResult(JobResult result);
    }
}
