using calories_tracker.Models;
using CommunityToolkit.Mvvm.Input;

namespace calories_tracker.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}