
namespace ChannelsExample.Worker
{
    public interface IFileImportWorker
    {
        Task Import(FileImportRequest request);
    }
}
