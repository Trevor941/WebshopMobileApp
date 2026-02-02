using CommunityToolkit.Mvvm.Input;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}