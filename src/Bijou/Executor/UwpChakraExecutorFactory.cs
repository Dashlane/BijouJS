namespace Bijou.Executor
{
    public class UWPChakraExecutorFactory
    {
        public UWPChakraHostExecutor CreateJsExecutorHost()
        {
            return new UWPChakraHostExecutor();
        }
    }
}