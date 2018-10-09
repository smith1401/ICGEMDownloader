using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Nito.AsyncEx;

namespace ICGEMDownloader
{
    class MainViewModel : ObservableObject, IAsyncInitialization
    {
        private readonly IcgemScraper scraper;
        private ICollectionView staticModelView;
        private int progress;
        private string progressInfo;
        private int _downloadModelCount;
        private StaticModel currentModel;
        private ObservableCollection<StaticModel> staticModelCollection;
        private bool downloadComplete;
        private readonly Dictionary<string, object> filterDict;

        public bool DownloadComplete
        {
            get => downloadComplete;
            set
            {
                downloadComplete = value;
                RaisePropertyChanged();
            }
        }

        public int DownloadModelCount
        {
            get => _downloadModelCount;
            set { _downloadModelCount = value; RaisePropertyChanged(); }
        }

        public string ProgressInfo
        {
            get => progressInfo;
            set { progressInfo = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<StaticModel> StaticModelCollection
        {
            get => staticModelCollection;
            set { staticModelCollection = value; RaisePropertyChanged(); }
        }

        public ICollectionView StaticModelView
        {
            get => staticModelView;
            set
            {
                staticModelView = value;
                RaisePropertyChanged();
            }
        }

        public StaticModel CurrentModel
        {
            get => currentModel;
            set
            {
                currentModel = value;
                RaisePropertyChanged();
            }
        }

        public INotifyTaskCompletion InitializationNotifier { get; }
        public Task Initialization => InitializationNotifier.Task;      

        public object LoadingView => new LoadingDialogView();

        public ICommand UpdateFilterCommand => new RelayCommand<object[]>(UpdateFilter);
        public ICommand SelectAllCommand => new RelayCommand<bool>(SelectAll);

        private void SelectAll(bool check)
        {
            foreach (StaticModel model in StaticModelView)
            {
                model.ShouldBeDownloaded = check;
            }
        }

        public AwaitableDelegateCommand DownloadCommand => new AwaitableDelegateCommand(Download);

        private async Task Download()
        {
            foreach (StaticModel model in StaticModelView)
            {
                model.Downloaded = false;
            }

            var modelsToBeDownloaded =
                StaticModelView.SourceCollection.Cast<StaticModel>().Where(sm => sm.ShouldBeDownloaded).ToList();

            DownloadModelCount = modelsToBeDownloaded.Count;
            Progress = 0;
            DownloadComplete = false;
            ProgressInfo = $"({Progress}/{DownloadModelCount})";

            // Create a block with an asynchronous action
            var block = new ActionBlock<StaticModel>(
                async _ =>
                {
                    CurrentModel = _;
                    await _.Download();

                    Progress++;
                    ProgressInfo = $"({Progress}/{DownloadModelCount})";
                },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 10000, // Cap the item count
                    CancellationToken = CancellationToken.None, // Enable cancellation
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 2, // Parallelize on all cores
                });

            // Add items to the block and asynchronously wait if BoundedCapacity is reached
            foreach (var model in modelsToBeDownloaded)
            {
                await block.SendAsync(model);
            }

            block.Complete();
            await block.Completion;
            DownloadComplete = block.Completion.IsCompleted;

            CurrentModel = null;         
        }

        public int Progress
        {
            get => progress;
            set { progress = value; RaisePropertyChanged(); }
        }

        private void UpdateFilter(object[] obj)
        {
            var key = obj[0].ToString();
            var value = obj[1];

            if (!filterDict.ContainsKey(key))
            {
                filterDict.Add(key, value);
            }
            else
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    filterDict.Remove(key);
                }
                else
                {
                    filterDict[key] = value;
                }
                
            }

            StaticModelView.Refresh();
        }

        public MainViewModel()
        {
            scraper = new IcgemScraper();
            filterDict = new Dictionary<string, object>();
            InitializationNotifier = NotifyTaskCompletion.Create(Init);     
        }

        private async Task Init()
        {
            await Task.Run(() => scraper.StartAsync());
            //StaticModelCollection = new ObservableCollection<StaticModel>(scraper.StaticModels);
            StaticModelView = CollectionViewSource.GetDefaultView(scraper.StaticModels);
            StaticModelView.Filter = OnFilterModels;
        }

        private bool OnFilterModels(object obj)
        {
            if (!(obj is StaticModel model)) return false;

            return filterDict.All(kvp =>
            {
                switch (kvp.Key)
                {
                    case "Name":
                        return model.Name.ToLower().Contains(kvp.Value.ToString());
                    case "Year":
                        return model.Year == Convert.ToInt32(kvp.Value);
                    case "Degree":
                        return model.Degree == Convert.ToInt32(kvp.Value);
                    case "Data":
                        return model.Data.ToLower().Contains(kvp.Value.ToString());
                    case "Download":
                        return model.ShouldBeDownloaded == Convert.ToBoolean(kvp.Value);
                    default:
                        return false;

                }
            });
        }
    }
}
