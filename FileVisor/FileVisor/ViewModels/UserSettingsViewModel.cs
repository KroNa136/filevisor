using SortColumn = FileVisor.Models.Settings.SortColumn;
using SortDirection = FileVisor.Models.Settings.SortDirection;

using FileVisor.Models;
using System.Collections.ObjectModel;

namespace FileVisor.ViewModels
{
    internal class UserSettingsViewModel
    {
        public static ObservableCollection<SortColumnEntity> SortColumns { get; } = new ObservableCollection<SortColumnEntity>()
        {
            new SortColumnEntity()
            {
                ID = SortColumn.Name,
                Name = "Название"
            },
            new SortColumnEntity()
            {
                ID = SortColumn.Type,
                Name = "Тип"
            },
            new SortColumnEntity()
            {
                ID = SortColumn.DateCreated,
                Name = "Дата создания"
            },
            new SortColumnEntity()
            {
                ID = SortColumn.DateModified,
                Name = "Дата изменения"
            },
            new SortColumnEntity()
            {
                ID = SortColumn.Size,
                Name = "Размер"
            }
        };

        public static ObservableCollection<SortDirectionEntity> SortDirections { get; } = new ObservableCollection<SortDirectionEntity>()
        {
            new SortDirectionEntity()
            {
                ID = SortDirection.Ascending,
                Name = "По возрастанию"
            },
            new SortDirectionEntity()
            {
                ID = SortDirection.Descending,
                Name = "По убыванию"
            }
        };

        public SortColumn SelectedSortColumnID { get; set; }
        public SortDirection SelectedSortDirectionID { get; set; }
        public bool ShowFileExtensions { get; set; }
        public bool ShowHiddenElements { get; set; }
        public bool ShowSystemElements { get; set; }
        public bool OldSystemPropertiesView { get; set; }

        public UserSettingsViewModel(Settings settings)
        {
            SelectedSortColumnID = settings.SelectedSortColumn;
            SelectedSortDirectionID = settings.SelectedSortDirection;
            ShowFileExtensions = settings.ShowFileExtensions;
            ShowHiddenElements = settings.ShowHiddenElements;
            ShowSystemElements = settings.ShowSystemElements;
            OldSystemPropertiesView = settings.OldSystemPropertiesView;
        }
    }
}
