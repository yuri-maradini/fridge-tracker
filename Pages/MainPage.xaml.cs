using calories_tracker.Models;
using calories_tracker.PageModels;

namespace calories_tracker.Pages
{
    public partial class MainPage : ContentPage // <-- "partial" è obbligatorio
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnBtnFotocameraClicked(object sender, EventArgs e)
        {
            // 1. Controlliamo lo stato dei permessi per la fotocamera
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                // 2. Se non abbiamo il permesso, lo chiediamo all'utente
                status = await Permissions.RequestAsync<Permissions.Camera>();

                if (status != PermissionStatus.Granted)
                {
                    // 3. Se l'utente rifiuta, mostriamo un avviso e ci fermiamo
                    await DisplayAlertAsync("Attenzione", "Senza la fotocamera non posso leggere i codici a barre!", "OK");
                    return;
                }
            }

            // 4. Se abbiamo i permessi, apriamo la pagina dello scanner!
            // (Questa pagina la creeremo nel prossimo step)
            await Navigation.PushModalAsync(new ScannerPage());
        }
    }
}