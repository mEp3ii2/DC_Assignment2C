using System;
using System.ComponentModel;

[Serializable]
public class Job : INotifyPropertyChanged
{
    private string status;
    private string result;

    public int JobId { get; set; }
    public string JobName { get; set; }
    public string Base64Code { get; set; }
    public string Hash { get; set; }

    public string Status
    {
        get { return status; }
        set
        {
            if (status != value)
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }
    }

    public string Result
    {
        get { return result; }
        set
        {
            if (result != value)
            {
                result = value;
                OnPropertyChanged("Result");
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
