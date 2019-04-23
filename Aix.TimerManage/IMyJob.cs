using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.TimerManage
{
    public interface IMyJob
    {
        Task<MyJobResult> Execute(MyJobContext context);
    }


    public class MyJobResult
    {
        public bool IsSuccess { get; set; }
    }


    public class MyJobContext
    {
        public MyJobContext(CancellationToken cancellationToken)
        {
            Token = cancellationToken;
        }
        public CancellationToken Token { get; }

        public bool IsShutdownRequested => Token.IsCancellationRequested;
    }
}
