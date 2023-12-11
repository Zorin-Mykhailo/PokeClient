namespace PokeClient.DataModel;

public class Pokemon
{
    public Ability[] abilities { get; set; }
    public int? base_experience { get; set; }
    public Form[] forms { get; set; }
    public Game_Indices[] game_indices { get; set; }
    public int height { get; set; }
    public Held_Items[] held_items { get; set; }
    public int id { get; set; }
    public bool is_default { get; set; }
    public string location_area_encounters { get; set; }
    public Move[] moves { get; set; }
    public string name { get; set; }
    public int order { get; set; }
    public object[] past_abilities { get; set; }
    public object[] past_types { get; set; }
    public Species species { get; set; }
    public Sprites sprites { get; set; }
    public Stat[] stats { get; set; }
    public Type[] types { get; set; }
    public int weight { get; set; }
}
