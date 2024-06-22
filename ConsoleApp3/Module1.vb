Imports zoppaToml

Module Module1

    Sub Main()
        Dim doc = TomlDocument.LoadFromFile("example.toml")

        Dim v1 = doc("best-day-ever").GetValue(Of Date)()
        Dim v2 = doc("numtheory")("boring").GetValue(Of Boolean)
        Dim va1 = doc("numtheory")("perfection").GetValue(Of Long)(0)
        Dim va2 = doc("numtheory")("perfection").GetValue(Of Long)(1)
        Dim va3 = doc("numtheory")("perfection").GetValue(Of Long)(2)
        Dim a As Integer = 0
    End Sub

End Module
