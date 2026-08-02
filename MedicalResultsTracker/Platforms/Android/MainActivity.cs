using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MedicalResultsTracker.Services.Backup;
using Microsoft.Maui;

namespace MedicalResultsTracker
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        /// <summary>
        /// Системный выбор папки возвращает результат сюда — другого пути у него нет.
        /// Дальше его подхватывает <see cref="FolderPicker"/>.
        /// </summary>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            if (!FolderPicker.HandleResult(requestCode, resultCode, data))
            {
                base.OnActivityResult(requestCode, resultCode, data);
            }
        }
    }
}
