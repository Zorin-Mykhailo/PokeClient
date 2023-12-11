using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PokeClient.DataModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace PokeClient;

public partial class MainWindow : Window
{
    ObservableCollection<Pokemon> Pokemons = [];
    PokeApiClient PokeApiCli = new ();

    public MainWindow()
    {
        InitializeComponent();
        PokeApiCli = new();
    }

    private void BtnLoadPokemons_Click(object sender, RoutedEventArgs e)
    {
        LoadPokemons();
    }


    private void LoadPokemons()
    {
        ProgressStatus.Value = 0;
        Pokemons = [];
        // 1. Отримуємо список імен покемонів
        PokemonsInfos pokemonsInfos = PokeApiCli.GetPokemonsInfo();
        ProgressStatus.Maximum = pokemonsInfos.results.Count;


        TablePokemons.ItemsSource = Pokemons;

        BackgroundWorker worker = new()
        {
            WorkerReportsProgress = true,
            
        };
        worker.DoWork += Worker_DoWork;
        worker.ProgressChanged += Worker_ProgressChanged;
        worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        worker.RunWorkerAsync(pokemonsInfos);
    }

    private void Worker_DoWork(object? sender, DoWorkEventArgs e)
    {
        PokemonsInfos pokemonsInfos = (e.Argument as PokemonsInfos)!;

        int chunkSize = 25;
        // 2. Розбиваємо список імем на пакети
        IEnumerable<string[]> chunks = pokemonsInfos.results.Select(e => e.name).Chunk(chunkSize);

        foreach(var chunk in chunks)
        {
            // 3. Перетворюємо пакети в задачі
            Task<Pokemon>[] getPokemonTasks = chunk.Select(pokeName => new Task<Pokemon>(() => PokeApiCli.GetPokemon(pokeName))).ToArray();
            // 4. Та запускаємо їх виконання
            foreach(var getPokemonTask in getPokemonTasks) getPokemonTask.Start();

            // 5. Чекаємо доки всі вони виконаються
            Task.WaitAll(getPokemonTasks);
            // 6. Отримуємо результат виконання (тобто покемона) по кожній із задач
            List<Pokemon> retreivedPokemons = getPokemonTasks.Select(e => e.Result).ToList();

            (sender as BackgroundWorker).ReportProgress(0, retreivedPokemons);
        }
        e.Result = 100;
    }

    private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        List<Pokemon> retreivedPokemons = (List<Pokemon>)e.UserState;
        if(e.UserState != null)
        {
            // 7. Завантажуємо в фінальну таблицю
            foreach(Pokemon pokemon in retreivedPokemons)
                Pokemons.Add(pokemon);
        }

        ProgressStatus.Value = Pokemons.Count;
    }

    private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        List<String> mismatches = new List<String>();
        MessageBox.Show($"Завантажено {Pokemons.Count} покемони");
    }    
}


public class PokeApiClient
{
    private HttpClient Client { get; set; }

    public PokeApiClient()
    {
        Client = new() { BaseAddress = new Uri(@"https://pokeapi.co/api/v2/") };
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private string GetAsJson(string query)
    {
        HttpResponseMessage response = Client.GetAsync(query).Result;
        response.EnsureSuccessStatusCode();
        string result = response.Content.ReadAsStringAsync().Result;
        return JToken.Parse(result).ToString(Formatting.Indented);
    }

    public PokemonsInfos GetPokemonsInfo()
    {
        string json = GetAsJson($"pokemon?limit=100000&offset=0");
        return JsonConvert.DeserializeObject<PokemonsInfos>(json)!;

    }

    public Pokemon GetPokemon(string pokemonName)
    {
        string json = GetAsJson($"pokemon/{pokemonName}");
        return JsonConvert.DeserializeObject<Pokemon>(json)!;
    }
}