namespace MauiTrackTestSP;

public partial class App : Application
{

    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}

    protected override void OnStart()
    {
        // Handle when your app starts
        Console.WriteLine("PCL OnStart");
    }

    protected override void OnSleep()
    {
        // Handle when your app sleeps
        Console.WriteLine("PCL OnSleep");
    }

    protected override void OnResume()
    {
        // Handle when your app resumes
        Console.WriteLine("PCL OnResume");
    }

}
