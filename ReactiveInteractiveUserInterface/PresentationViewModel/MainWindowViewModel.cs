//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using TP.ConcurrentProgramming.Presentation.ViewModel;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
      Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
            StartCommand = new RelayCommand(param =>
            {
                if (int.TryParse(param?.ToString(), out int numberOfBalls))
                {
                    Start(numberOfBalls);
                }
            },
        param =>
        {
            if (int.TryParse(param?.ToString(), out int number))
            {
                return number >= 1 && number <= 20;
            }
            return false;
        });

        }

        #endregion ctor

        #region public API

        public void Start(int numberOfBalls)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      ModelLayer.Start(numberOfBalls);
      Observer.Dispose();
    }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

    #endregion public API
    public RelayCommand StartCommand { get; }

    //height width

    private double fieldWidth;
    public double FieldWidth
    {
        get => fieldWidth;
        set
        {
            fieldWidth = value;
            RaisePropertyChanged(nameof(FieldWidth));
            ModelLayer.UpdateFieldSize(fieldWidth, fieldHeight);
        }
    }

    private double fieldHeight;
    public double FieldHeight
    {
        get => fieldHeight;
        set
        {
            fieldHeight = value;
            RaisePropertyChanged(nameof(FieldHeight));
            ModelLayer.UpdateFieldSize(fieldWidth, fieldHeight);
        }
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          Balls.Clear();
          Observer.Dispose();
          ModelLayer.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        Disposed = true;
      }
    }

    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;

        #endregion private

        
    }
}