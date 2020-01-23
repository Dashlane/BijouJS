namespace Bijou.Executor
{
    public class UWPChakraExecutorFactory : IJsExecutorFactory
    {
        public IJsExecutorHost CreateJsExecutorHost()
        {
            return new UWPChakraHostExecutor();
        }
    }
}
