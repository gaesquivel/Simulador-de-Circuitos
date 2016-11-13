using CircuitMVVMBase.Commands;
using CircuitMVVMBase.Properties;
using Common;
using Gat.Controls;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace CircuitMVVMBase.MVVM
{
    public abstract class DrawableVMBase: ViewModelBase, IDescribible
    {
        private static object _selectedobject;


        protected Timeline animation;
        protected Storyboard myStoryboard;
        public ViewModelBase AnimationTarget { get; set; }

        public string ShortDescription { get; set; }

        [Browsable(false)]
        public static ObservableCollection<object> MainObjects { get; protected set; }

        [Browsable(false)]
        public virtual object SelectedObject
        {
            get { return _selectedobject; }
            set
            {
                RaisePropertyChanged(value, ref _selectedobject, true);
            }
        }

        static RecentFileList recents;
        public RecentFileList RecentFiles {
            get { return recents; }
            set {
                if (value == null)
                    throw new NullReferenceException();
                if (value != recents)
                    value.MenuClick += (s, e) => OpenFile(e.Filepath);
                recents = value;
            } 
        }


        //const string NoFile = "No file";
        ////const string NewFile = "New nameless file";

        //public static DependencyProperty FilepathProperty =
        //    DependencyProperty.Register(
        //    "Filepath",
        //    typeof(string),
        //    typeof(DrawableVMBase),
        //    new PropertyMetadata(NoFile));
        //public string Filepath
        //{
        //    get { return (string)GetValue(FilepathProperty); }
        //    set { SetValue(FilepathProperty, value); }
        //}


        RelayCommand simulatecmd;

        RelayCommand drawselectedcmd, openfilecmd, newfilecmd,
                    savecmd, saveascmd, animatecmd, runanimationcmd,
                    pauseanimationcmd, exportcmd, copytocmd, savebitmapcmd,
                    helpcmd, aboutcmd;

        #region Commands


        public RelayCommand NewFileCommand
        {
            get
            {
                return newfilecmd ?? (newfilecmd = new RelayCommand((Action<object>)newfile));
            }
        }

        protected virtual void newfile(object obj)
        {
            throw new NotImplementedException();
        }

        public RelayCommand AboutCommand
        {
            get
            {
                return aboutcmd ?? (aboutcmd = new RelayCommand((Action<object>)About));
            }
        }

        private void About(object obj)
        {
            //AboutControlViewModel model = new AboutControlViewModel();
            //Gat.Controls.About ab = new Gat.Controls.About();
            //ab.vi
            AboutControlView about = new AboutControlView();
            AboutControlViewModel vm = (AboutControlViewModel)about.FindResource("ViewModel");

            // setting several properties here
            vm.ApplicationLogo = new BitmapImage(new Uri("pack://application:,,,/Images/planoCsmall.png"));
            vm.Publisher = "Gabriel Esquivel";
            vm.Title = "Complex Plain 3D Viewer";
            vm.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            vm.AdditionalNotes = "Created by Gabriel Esquivel... gaesquivel99@gmail.com";
            //Assembly.
            // ...

            vm.Window.Content = about;
            vm.Window.Show();
        }

        public RelayCommand CopyToCommand
        {
            get
            {
                if (copytocmd == null)
                    return copytocmd = new RelayCommand((Action<object>)CopyTo);
                return copytocmd;
            }
        }

        public virtual void CopyTo(object obj)
        {
            throw new NotImplementedException();
        }

        public RelayCommand SaveBitmapCommand
        {
            get
            {
                if (savebitmapcmd == null)
                    return savebitmapcmd = new RelayCommand((Action<object>)SaveBitmap);
                return savebitmapcmd;
            }
        }

        public virtual void SaveBitmap(object obj)
        {
            throw new NotImplementedException();
        }

        public RelayCommand ExportCommand
        {
            get
            {
                if (exportcmd == null)
                    return exportcmd = new RelayCommand((Action<object>)Export);
                return exportcmd;
            }
        }
       

        public /*AsyncDelegateCommand<Task>*/RelayCommand SimulateCommand
        {
            get
            {
                if (simulatecmd == null)
                    return simulatecmd = new RelayCommand((Action<object>)Simulate);
                return simulatecmd;
            }
        }

        public RelayCommand DrawSelectedCommand
        {
            get
            {
                if (drawselectedcmd == null)
                    return drawselectedcmd = new RelayCommand((Action<object>)Redraw);
                return drawselectedcmd;
            }
        }

        public RelayCommand OpenFileCommand
        {
            get
            {
                if (openfilecmd == null)
                    return openfilecmd = new RelayCommand((Action<object>)OpenFile);
                return openfilecmd;
            }
        }

        public RelayCommand AnimateCommand
        {
            get
            {
                if (animatecmd == null)
                    return animatecmd = new RelayCommand(Animate, (x) => {
                        return animation != null && SelectedObject is ViewModelBase;
                    });
                return animatecmd;
            }
        }

        public RelayCommand PauseAnimationCommand
        {
            get
            {
                if (pauseanimationcmd== null)
                    return pauseanimationcmd = new RelayCommand(PauseAnimation,
                                        CanExecutePauseAnimation);
                return pauseanimationcmd;
            }
        }

        public RelayCommand RunAnimationCommand
        {
            get
            {
                if (runanimationcmd == null)
                    return runanimationcmd = new RelayCommand(RunAnimation, CanExecuteRunAnimation);
                return runanimationcmd;
            }
        }

        public RelayCommand SaveFileCommand
        {
            get
            {
                if (savecmd == null)
                    return savecmd = new RelayCommand((Action<object>)SaveFile);
                return savecmd;
            }
        }
        public RelayCommand SaveAsFileCommand
        {
            get
            {
                if (saveascmd == null)
                    return saveascmd = new RelayCommand((Action<object>)SaveAsFile);
                return saveascmd;
            }
        }

        

        #endregion


        //protected virtual bool AnimateCanExecute(object obj)
        //{
        //    return animation != null && SelectedObject is ViewModelBase;
        //}

        protected virtual bool CanExecuteRunAnimation(object obj)
        {
            return animation != null && AnimationTarget != null;
        }

        protected virtual void RunAnimation(object obj)
        {
            myStoryboard.Begin();
        }

        protected virtual bool CanExecutePauseAnimation(object obj)
        {
            return animation != null && AnimationTarget != null;
        }

        private void PauseAnimation(object obj)
        {
            myStoryboard.Pause();
        }


        public DrawableVMBase():base()
        {
            if (MainObjects == null)
                MainObjects = new ObservableCollection<object>();
            if (RecentFiles == null)
                RecentFiles = new RecentFileList();
            myStoryboard = new Storyboard();
            RecentFiles.MenuClick += (s, e) => OpenFile(e.Filepath);
        }

        protected virtual void Animate(object obj)
        {
            throw new NotImplementedException();
        }

        //private void animationchanged(object sender, EventArgs e)
        //{
        //   ;
        //}
        protected abstract void SaveAsFile(object obj);

        protected abstract void SaveFile(object obj);

        protected virtual void OpenFile(object obj)
        {
            if (obj is string)
            {
                string Filename = obj as string;
                Simulate(Filename);
            }
            else
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetEntryAssembly().Location); ;
                dlg.Filter = "Circuit Net List files (*.net)|*.net|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    Simulate(dlg.FileName);
                }
                RecentFiles.InsertFile(dlg.FileName);
           }
            
        }

        protected abstract void Redraw(object obj);

        public abstract void Simulate(object obj);

        public abstract void Export(object obj);

    }
}
