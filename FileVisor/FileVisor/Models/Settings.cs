namespace FileVisor.Models
{
    public class Settings
    {
        public enum SortColumn
        {
            Name, Type, DateCreated, DateModified, Size
        }

        public enum SortDirection
        {
            Ascending, Descending
        }

        public enum FileOperationType
        {
            None, Cut, Copy
        }

        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public bool StartMaximized { get; set; }
        public double TreeViewWidth { get; set; }
        public double NameColumnWidth { get; set; }
        public double TypeColumnWidth { get; set; }
        public double DateCreatedColumnWidth { get; set; }
        public double DateModifiedColumnWidth { get; set; }
        public double SizeColumnWidth { get; set; }
        public SortColumn SelectedSortColumn { get; set; }
        public SortDirection SelectedSortDirection { get; set; }
        public bool ShowFileExtensions { get; set; }
        public bool ShowHiddenElements { get; set; }
        public bool ShowSystemElements { get; set; }
        public bool OldSystemPropertiesView { get; set; }
        public static FileOperationType CurrentFileOperationType { get; set; }

        const double DEFAULT_WINDOW_WIDTH                   = 1280;
        const double DEFAULT_WINDOW_HEIGHT                  = 720;
        const bool DEFAULT_START_MAXIMIZED                  = false;
        const double DEFAULT_TREE_VIEW_WIDTH                = 260;
        const double DEFAULT_NAME_COLUMN_WIDTH              = 280;
        const double DEFAULT_TYPE_COLUMN_WIDTH              = 270;
        const double DEFAULT_DATE_CREATED_COLUMN_WIDTH      = 160;
        const double DEFAULT_DATE_MODIFIED_COLUMN_WIDTH     = 160;
        const double DEFAULT_SIZE_COLUMN_WIDTH              = 100;
        const SortColumn DEFAULT_SORT_COLUMN                = SortColumn.Name;
        const SortDirection DEFAULT_SORT_DIRECTION          = SortDirection.Ascending;
        const bool DEFAULT_SHOW_FILE_EXTENSIONS             = false;
        const bool DEFAULT_SHOW_HIDDEN_ELEMENTS             = false;
        const bool DEFAULT_SHOW_SYSTEM_ELEMENTS             = false;
        const bool DEFAULT_OLD_SYSTEM_PROPERTIES_VIEW       = false;
        const FileOperationType DEFAULT_FILE_OPERATION_TYPE = FileOperationType.None;

        public Settings()
        {
            WindowWidth              = DEFAULT_WINDOW_WIDTH;
            WindowHeight             = DEFAULT_WINDOW_HEIGHT;
            StartMaximized           = DEFAULT_START_MAXIMIZED;
            TreeViewWidth            = DEFAULT_TREE_VIEW_WIDTH;
            NameColumnWidth          = DEFAULT_NAME_COLUMN_WIDTH;
            TypeColumnWidth          = DEFAULT_TYPE_COLUMN_WIDTH;
            DateCreatedColumnWidth   = DEFAULT_DATE_CREATED_COLUMN_WIDTH;
            DateModifiedColumnWidth  = DEFAULT_DATE_MODIFIED_COLUMN_WIDTH;
            SizeColumnWidth          = DEFAULT_SIZE_COLUMN_WIDTH;
            SelectedSortColumn       = DEFAULT_SORT_COLUMN;
            SelectedSortDirection    = DEFAULT_SORT_DIRECTION;
            ShowFileExtensions       = DEFAULT_SHOW_FILE_EXTENSIONS;
            ShowHiddenElements       = DEFAULT_SHOW_HIDDEN_ELEMENTS;
            ShowSystemElements       = DEFAULT_SHOW_SYSTEM_ELEMENTS;
            OldSystemPropertiesView  = DEFAULT_OLD_SYSTEM_PROPERTIES_VIEW;
            CurrentFileOperationType = DEFAULT_FILE_OPERATION_TYPE;
        }

        public Settings(double windowWidth, double windowHeight, bool startMaximized, double treeViewWidth, double nameColumnWidth, double typeColumnWidth, double dateCreatedColumnWidth, double dateModifiedColumnWidth, double sizeColumnWidth, SortColumn sortColumn, SortDirection sortDirection, bool showFileExtensions, bool showHiddenElements, bool showSystemElements, bool oldSystemPropertiesView, FileOperationType fileOperationType)
        {
            WindowWidth              = windowWidth;
            WindowHeight             = windowHeight;
            StartMaximized           = startMaximized;
            TreeViewWidth            = treeViewWidth;
            NameColumnWidth          = nameColumnWidth;
            TypeColumnWidth          = typeColumnWidth;
            DateCreatedColumnWidth   = dateCreatedColumnWidth;
            DateModifiedColumnWidth  = dateModifiedColumnWidth;
            SizeColumnWidth          = sizeColumnWidth;
            SelectedSortColumn       = sortColumn;
            SelectedSortDirection    = sortDirection;
            ShowFileExtensions       = showFileExtensions;
            ShowHiddenElements       = showHiddenElements;
            ShowSystemElements       = showSystemElements;
            OldSystemPropertiesView  = oldSystemPropertiesView;
            CurrentFileOperationType = fileOperationType;
        }
    }
}
