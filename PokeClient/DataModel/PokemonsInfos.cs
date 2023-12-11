using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeClient.DataModel;


public class PokemonsInfos
{
    public int count { get; set; }
    public object next { get; set; }
    public object previous { get; set; }
    public List<PokemonInfo> results { get; set; }
}

public class PokemonInfo
{
    public string name { get; set; }
    public string url { get; set; }
}

