namespace Bijou
{
    internal interface ILogger
    {
        void Info(string message);

        void Error(string message);

        void Warn(string message);
    }
}