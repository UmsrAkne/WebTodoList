namespace WebTodoApp.Models
{
    public interface IDBConnectionStrings
    {
        string UserName { get; }

        string PassWord { get; }

        string HostName { get; }

        int PortNumber { get; }

        string ServiceName { get; }
    }
}
