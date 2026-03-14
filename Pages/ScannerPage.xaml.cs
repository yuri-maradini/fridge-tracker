using System.Text.Json;
using ZXing.Net.Maui;

namespace calories_tracker.Pages;

public partial class ScannerPage : ContentPage
{
    private static readonly HttpClient _httpClient = new HttpClient();
    public ScannerPage()
    {
        InitializeComponent();

        // Configuriamo lo scanner per leggere i codici dei prodotti commerciali (EAN, UPC, ecc.)
        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.OneDimensional,
            AutoRotate = true,
            Multiple = false
        };
    }

    // Questo evento scatta automaticamente appena la fotocamera inquadra un codice
    private void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var primoCodice = e.Results?.FirstOrDefault();

        if (primoCodice != null)
        {
            // 1. Fermiamo lo scanner per non leggere lo stesso codice 100 volte in un secondo!
            barcodeReader.IsDetecting = false;

            // 2. Torniamo sul "thread principale" per aggiornare lo schermo
            Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlertAsync("Codice letto!", $"Cerco il codice {primoCodice.Value} online...", "OK");

                // Chiamiamo il nostro nuovo metodo per interrogare il database
                await CercaProdottoOnline(primoCodice.Value);

                // Finito tutto, chiudiamo la fotocamera
                await Navigation.PopModalAsync();
            });
        }
    }

    // --- NUOVO METODO PER IL DATABASE ---
    //private async Task CercaProdottoOnline(string codice)
    //{
    //    string url = $"https://world.openfoodfacts.org/api/v0/product/{codice}.json?fields=product_name,brands,status";

    //    try
    //    {
    //        // 1. Facciamo la richiesta al sito
    //        string rispostaJson = await _httpClient.GetStringAsync(url);

    //        // 2. Analizziamo il testo JSON ricevuto
    //        using JsonDocument doc = JsonDocument.Parse(rispostaJson);
    //        JsonElement root = doc.RootElement;

    //        // 3. OpenFoodFacts restituisce "status: 1" se trova il prodotto
    //        if (root.GetProperty("status").GetInt32() == 1)
    //        {
    //            JsonElement product = root.GetProperty("product");

    //            // Estraiamo il nome e la marca (con un controllo in caso manchino)
    //            string nome = product.TryGetProperty("product_name", out var n) ? n.GetString() : "Nome sconosciuto";
    //            string marca = product.TryGetProperty("brands", out var b) ? b.GetString() : "Marca sconosciuta";

    //            // Mostriamo il risultato finale!
    //            await DisplayAlertAsync("Prodotto Trovato! 🎉", $"Nome: {nome}\nMarca: {marca}", "Fantastico");
    //        }
    //        else
    //        {
    //            await DisplayAlertAsync("Ops!", "Prodotto non trovato nel database gratuito.", "Chiudi");
    //        }
    //        await Navigation.PopModalAsync();
    //    }
    //    catch (Exception e)
    //    {
    //        await DisplayAlertAsync("Errore di Rete", "Non sono riuscito a collegarmi a Internet.", "OK");
    //    }
    //}

    private async Task CercaProdottoOnline(string codice)
    {
        // Usiamo il server trial di UPCitemdb
        string url = $"https://api.upcitemdb.com/prod/trial/lookup?upc={codice}";

        try
        {
            // Spesso queste API richiedono un "User-Agent" per non scambiarci per bot maligni
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "AppScannerMaui/1.0");
            }

            string rispostaJson = await _httpClient.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(rispostaJson);
            JsonElement root = doc.RootElement;

            // UPCitemdb restituisce "OK" se la chiamata è andata a buon fine
            if (root.GetProperty("code").GetString() == "OK")
            {
                JsonElement items = root.GetProperty("items");

                // Controlliamo se la lista dei risultati non è vuota
                if (items.GetArrayLength() > 0)
                {
                    JsonElement prodotto = items[0];

                    string nome = prodotto.TryGetProperty("title", out var n) ? n.GetString() : "Nome sconosciuto";
                    string marca = prodotto.TryGetProperty("brand", out var b) ? b.GetString() : "Marca sconosciuta";

                    await DisplayAlertAsync("trovato", prodotto.ToString(), "Ok");
                    //await DisplayAlertAsync("Trovato! ⚡", $"Nome: {nome}\nMarca: {marca}", "Ottimo");
                }
                else
                {
                    await DisplayAlertAsync("Ops!", "Prodotto non trovato nel database UPCitemdb.", "Chiudi");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Errore", "Impossibile contattare il server.", "OK");
        }
    }


    // Metodo per il bottone di chiusura manuale
    private async void OnChiudiClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}