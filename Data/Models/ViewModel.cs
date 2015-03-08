using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Data.Models
{
  public class ViewModel : INotifyPropertyChanged
  {
    private string _cloudServiceName;
    private string _deploymentName;
    private bool _isConfigMode;
    private string _roleName;
    private string _subscriptionId;
    private string _token;

    public ViewModel(string subId, string cloudServiceName, string deployName, string roleName, string token)
    {
      _subscriptionId = subId;
      _cloudServiceName = cloudServiceName;
      _deploymentName = deployName;
      _roleName = roleName;
      _token = token;
    }

    public ViewModel()
    {
    }

    private bool _hasValues
    {
      get
      {
        return !string.IsNullOrWhiteSpace(_subscriptionId) && !string.IsNullOrWhiteSpace(_cloudServiceName) &&
               !string.IsNullOrWhiteSpace(_deploymentName) && !string.IsNullOrWhiteSpace(_roleName);
      }
    }

    public bool IsConfigMode
    {
      get { return _isConfigMode; }
      set { SetField(ref _isConfigMode, value); }
    }

    public string Token
    {
      get { return _token; }
      set
      {
        SetField(ref _token, value);
        OnPropertyChanged(string.Empty);
      }
    }

    public string SubscriptionId
    {
      get { return _subscriptionId; }
      set
      {
        SetField(ref _subscriptionId, value);
        OnPropertyChanged(string.Empty);
      }
    }

    public string CloudServiceName
    {
      get { return _cloudServiceName; }
      set
      {
        SetField(ref _cloudServiceName, value);
        OnPropertyChanged(string.Empty);
      }
    }

    public string DeploymentName
    {
      get { return _deploymentName; }
      set
      {
        SetField(ref _deploymentName, value);
        OnPropertyChanged(string.Empty);
      }
    }

    public string RoleName
    {
      get { return _roleName; }
      set
      {
        SetField(ref _roleName, value);
        OnPropertyChanged(string.Empty);
      }
    }

    public bool HasValues
    {
      get { return _hasValues; }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetField<T>(ref T field, T value,
      [CallerMemberName] string propertyName = null)
    {
      if (EqualityComparer<T>.Default.Equals(field, value)) return false;
      field = value;
      OnPropertyChanged(propertyName);
      return true;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}