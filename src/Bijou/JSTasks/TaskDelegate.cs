namespace Bijou.JSTasks
{
    internal delegate int PushTask(AbstractJSTask task);

    internal delegate void CancelTask(int id);
}
