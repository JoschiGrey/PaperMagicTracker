using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PaperMagicTracker.Classes
{
    public enum TypeLists {
        Creature,
        Artifact,
        Enchantment,
        Land,
        Planeswalker,
        Spell,
        SuperTypes,
        PermanentTypes,
        SpellTypes
    }


    public static class CardTypes
    {

        public static async Task CreateAsync(ILogger logger, HttpClient client)
        {
            if(CardTypeDictionary is not null) return;

            var taskList = new List<Task<HttpResponseMessage>>();

            Parallel.ForEach(Enum.GetValues<TypeLists>(),  (superType, _) =>
                    {
                        string path = @$"sample-data\{superType.ToString().ToLower()}-types.json";

                        var task = client.GetAsync(path);

                        taskList.Add(task);

                        logger.LogTrace($"Send Call to {path}");
                    });


            var dictionary = new Dictionary<TypeLists, CardTypeList>();
            var completeList = new List<string>();

            CreateHardcodedLists();

            while (taskList.Count > 0)
            {
                var finishedTask = await Task.WhenAny(taskList);
                var response = await finishedTask;

                if (!response.IsSuccessStatusCode)
                {
                    taskList.Remove(finishedTask);
                    continue;
                }
                
                var typeList = await response.Content.ReadFromJsonAsync<CardTypeList>();

                Console.WriteLine($"Finished deserialization of {typeList.Uri}");

                var lowerCase = typeList.Uri.Segments.Last().Replace("-types", "");

                typeList.Name = char.ToUpper(lowerCase[0]) + lowerCase[1..];

                var listEnum = (TypeLists)Enum.Parse(typeof(TypeLists), typeList.Name);

                dictionary.Add(listEnum, typeList);
                completeList.AddRange(typeList.Data);

                taskList.Remove(finishedTask);
            }


            CardTypeDictionary = dictionary;
            AllCardTypes = completeList;

            void CreateHardcodedLists()
            {
                //The card types and Supertypes aren't supplied in a catalog. We just hard add them here

                List<string> superTypes = new List<string>()
                {
                    "Basic",
                    "Legendary",
                    "Ongoing",
                    "Snow",
                    "World",
                    "Host",
                    "Elite"
                };

                var superTypesList = new CardTypeList(TypeLists.SuperTypes.ToString(), superTypes);

                completeList.AddRange(superTypes);
                dictionary.Add(TypeLists.SuperTypes, superTypesList);

                List<string> permanentTypes = new List<string>()
                {
                    "Artifact",
                    "Creature",
                    "Enchantment",
                    "Land",
                    "Planeswalker"
                };

                var permanentTypesList = new CardTypeList(TypeLists.PermanentTypes.ToString(), permanentTypes);

                completeList.AddRange(permanentTypes);
                dictionary.Add(TypeLists.PermanentTypes, permanentTypesList);

                List<string> spellTypes = new List<string>()
                {
                    "Instant",
                    "Sorcery"
                };

                var spellTypesList = new CardTypeList(TypeLists.SpellTypes.ToString(), spellTypes);

                completeList.AddRange(spellTypes);
                dictionary.Add(TypeLists.SpellTypes, spellTypesList);
            }
        }

        public static IReadOnlyDictionary<TypeLists, CardTypeList>? CardTypeDictionary { get; private set; }

        public static IReadOnlyList<string>? AllCardTypes { get; private set; }
    }


    public class CardTypeList
    {
        public CardTypeList(string name, List<string> data)
        {
            Uri = new Uri("https://en.wikipedia.org/wiki/HTTP_404");
            Name = name;
            Data = data;
            TotalValues = data.Count;
        }

        [JsonConstructor]
        public CardTypeList(
            Uri uri,
            int totalValues,
            IReadOnlyList<string> data
        )
        {
            this.Uri = uri;
            this.TotalValues = totalValues;
            this.Data = data;
        }

        [JsonPropertyName("uri")]
        public Uri Uri { get; }

        [JsonPropertyName("total_values")]
        public int TotalValues { get; }

        [JsonPropertyName("data")]
        public IReadOnlyList<string> Data { get; }

        public string? Name { get; set; }
    }


}