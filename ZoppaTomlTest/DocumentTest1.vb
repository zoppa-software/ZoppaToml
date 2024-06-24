Imports System
Imports Xunit
Imports ZoppaToml
Imports ZoppaToml.TomlLexical

Public Class DocumentTest1

    <Fact>
    Sub Case01()
        Dim doc = TomlDocument.LoadFromFile("valid\empty-file.toml")
        Assert.Equal(0, doc.Count)

        Assert.Throws(Of IO.FileNotFoundException)(
            Sub() TomlDocument.LoadFromFile("valid\xxxxx.toml")
        )
    End Sub

    <Fact>
    Sub Case02()
        Dim doc = TomlDocument.LoadFromFile("valid\example.toml")

        Assert.Equal(2, doc.Count)
        Assert.Equal(New DateTimeOffset(1987, 7, 5, 17, 45, 0, TimeSpan.Zero), doc("best-day-ever").GetValue(Of DateTimeOffset)())

        With doc("numtheory")
            Assert.Equal(False, .Items("boring").GetValue(Of Boolean)())
            Assert.Equal(6, .Items("perfection").GetValue(Of Integer)(0))
            Assert.Equal(28, .Items("perfection").GetValue(Of Integer)(1))
            Assert.Equal(496, .Items("perfection").GetValue(Of Integer)(2))
        End With
    End Sub

    <Fact>
    Sub Case03()
        Dim doc = TomlDocument.LoadFromFile("valid\implicit-and-explicit-after.toml")

        Assert.Equal(1, doc.Count)

        Assert.Equal(42, doc("a")("b")("c")("answer").GetValue(Of Integer)())
        Assert.Equal(43, doc("a")("better").GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub Case04()
        Dim doc = TomlDocument.LoadFromFile("valid\implicit-and-explicit-before.toml")

        Assert.Equal(1, doc.Count)

        Assert.Equal(42, doc("a")("b")("c")("answer").GetValue(Of Integer)())
        Assert.Equal(43, doc("a")("better").GetValue(Of Integer)())
    End Sub

End Class
