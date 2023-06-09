using MauiTrackTestSP.Services;

namespace MauiTrackTestSP;

public partial class MainPage : ContentPage
{
    private DateTime mdStart;

    public MainPage()
	{
		InitializeComponent();

        btnGetCurrentLocation.Clicked += btnGetCurrentLocation_Clicked;
        btnStartTracking.Clicked += btnStartTracking_Clicked;
        btnStopTracking.Clicked += btnStopTracking_Clicked;
        btnGeocode.Clicked += btnGeocode_Clicked;
        btnReverseGeocode.Clicked += btnReverseGeocode_Clicked;

        LocationManager.ReportInterval = TimeSpan.FromSeconds(15);
        LocationManager.GpsDataArrived += LocationManager_GpsDataArrived;
    }

    private async void btnGetCurrentLocation_Clicked(object sender, EventArgs e)
    {
        lblCurrentLocation.Text = "";
        var loGeoPosition = await LocationManager.GetCurrentLocationAsync();
        if (loGeoPosition != null)
        {
            await DisplayAlert("test", loGeoPosition.ToString(), "OK");
            lblCurrentLocation.Text = loGeoPosition.ToString();
        }
    }

    private async void btnStartTracking_Clicked(object sender, EventArgs e)
    {
        mdStart = DateTime.Now;
        await LocationManager.StartListeningAsync();
        lblStartedTracking.Text = mdStart.ToString();
    }

    private void btnStopTracking_Clicked(object sender, EventArgs e)
    {
        LocationManager.StopListening();
    }

    private void LocationManager_GpsDataArrived(GpsDataArrivedEventArgs e)
    {
        lblTrackedLocation.Text = e.Data.ToString();
        lblTrackedTime.Text = (mdStart - DateTime.Now).ToString();

        Console.WriteLine("Logging track event: " + DateTime.Now.ToString());
    }

    private async void btnGeocode_Clicked(object sender, EventArgs e)
    {
        var loCoordinate = await LocationManager.GeocodeAsync(txtAddress.Text);
        lblGeocodeResult.Text = $"{loCoordinate.Latitude}, {loCoordinate.Longitude}";
    }

    private async void btnReverseGeocode_Clicked(object sender, EventArgs e)
    {
        var loCoordinate = new Coordinate(double.Parse(txtLatitude.Text), double.Parse(txtLongitude.Text));
        var lcAddress = await LocationManager.ReverseGeocodeAsync(loCoordinate);
        lblReverseGeocodeResult.Text = lcAddress;
    }


}

