using ZoppaToml;

dynamic doc = TomlDocument.LoadFromFile("spec-example-1.toml");

// 
Console.WriteLine(doc.title.GetValue<string>());

dynamic owner = doc.owner;
Console.WriteLine(owner.name.GetValue<string>());
Console.WriteLine(owner.dob.GetValue<DateTimeOffset>());

dynamic database = doc.database;
Console.WriteLine(database.server.GetValue<string>());
foreach (TomlValue member in database.ports) {
    Console.WriteLine(member.GetValue<int>());
}
Console.WriteLine(database.connection_max.GetValue<int>());
Console.WriteLine(database.enabled.GetValue<bool>());

var servers = doc["servers"];
Console.WriteLine(servers["alpha"]["ip"].GetValue<string>());
Console.WriteLine(servers["alpha"]["dc"].GetValue<string>());
Console.WriteLine(servers["beta"]["ip"].GetValue<string>());
Console.WriteLine(servers["beta"]["dc"].GetValue<string>());

Console.WriteLine(doc["clients"]["data"][0][0].GetValue<string>());
Console.WriteLine(doc["clients"]["data"][0][1].GetValue<string>());
Console.WriteLine(doc["clients"]["data"][1][0].GetValue<int>());
Console.WriteLine(doc["clients"]["data"][1][1].GetValue<int>());

Console.WriteLine(doc["clients"]["hosts"][0].GetValue<string>());
Console.WriteLine(doc["clients"]["hosts"][1].GetValue<string>());
