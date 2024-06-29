# ZoppaToml
シンプルなTomlファイルの読み込みライブラリです。

以下のようにTomlファイルを読み込みます。  
  
``` csharp
using ZoppaToml;

dynamic doc = TomlDocument.LoadFromFile("spec-example-1.toml");

// dynamicオブジェクトを使って取得できます
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

// インデクサを使って取得できます
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
```
  
Tomlファイルの内容は以下のとおりです。  
``` toml
# This is a TOML document. Boom.

title = "TOML Example"

[owner]
name = "Lance Uppercut"
dob = 1979-05-27T07:32:00-08:00 # First class dates? Why not?

[database]
server = "192.168.1.1"
ports = [ 8001, 8001, 8002 ]
connection_max = 5000
enabled = true

[servers]

  # You can indent as you please. Tabs or spaces. TOML don't care.
  [servers.alpha]
  ip = "10.0.0.1"
  dc = "eqdc10"

  [servers.beta]
  ip = "10.0.0.2"
  dc = "eqdc10"

[clients]
data = [ ["gamma", "delta"], [1, 2] ]

# Line breaks are OK when inside arrays
hosts = [
  "alpha",
  "omega"
]

```

## 作成情報
* 造田　崇（zoppa software）
* ミウラ第1システムカンパニー 
* takashi.zouta@kkmiuta.jp

## ライセンス
[apache 2.0](https://www.apache.org/licenses/LICENSE-2.0.html)
