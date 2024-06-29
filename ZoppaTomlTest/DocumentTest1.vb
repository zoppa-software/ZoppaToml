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

        Assert.Equal(42, doc.GetByKeyNames("a.b.c.answer").GetValue(Of Integer)())

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

    <Fact>
    Sub Case05()
        Dim doc = TomlDocument.LoadFromFile("valid\spec-example-1.toml")

        'Assert.Equal(1, doc.Count)

        Assert.Equal("TOML Example", doc("title").GetValue(Of String)())

        Assert.Equal("Lance Uppercut", doc("owner")("name").GetValue(Of String)())
        Assert.Equal(New DateTimeOffset(1979, 5, 27, 7, 32, 0, New TimeSpan(-8, 0, 0)), doc("owner")("dob").GetValue(Of DateTimeOffset)())

        Assert.Equal("192.168.1.1", doc("database")("server").GetValue(Of String)())
        Assert.Equal(8001, doc("database")("ports")(0).GetValue(Of Integer)())
        Assert.Equal(8001, doc("database")("ports")(1).GetValue(Of Integer)())
        Assert.Equal(8002, doc("database")("ports")(2).GetValue(Of Integer)())
        Assert.Equal(5000, doc("database")("connection_max").GetValue(Of Integer)())
        Assert.Equal(True, doc("database")("enabled").GetValue(Of Boolean)())

        Assert.Equal("10.0.0.1", doc("servers")("alpha")("ip").GetValue(Of String)())
        Assert.Equal("eqdc10", doc("servers")("alpha")("dc").GetValue(Of String)())

        Assert.Equal("10.0.0.2", doc("servers")("beta")("ip").GetValue(Of String)())
        Assert.Equal("eqdc10", doc("servers")("beta")("dc").GetValue(Of String)())

        Assert.Equal("gamma", doc("clients")("data")(0)(0).GetValue(Of String)())
        Assert.Equal("delta", doc("clients")("data")(0)(1).GetValue(Of String)())
        Assert.Equal(1, doc("clients")("data")(1)(0).GetValue(Of Integer)())
        Assert.Equal(2, doc("clients")("data")(1)(1).GetValue(Of Integer)())
        Assert.Equal("alpha", doc("clients")("hosts")(0).GetValue(Of String)())
        Assert.Equal("omega", doc("clients")("hosts")(1).GetValue(Of String)())
    End Sub

    <Fact>
    Sub Case06()
        Dim data = New Byte() {&HFF, &HFF, &HFF, &HFF, &HFF, &HFF, &HFF, &HFF}
        Dim raw = New RawSource(data)
        Dim point = raw.GetPointer()
        Assert.Throws(Of InvalidOperationException)(
            Sub()
                Dim a = point.Current
            End Sub
        )
    End Sub

    <Fact>
    Sub Case07()
        Dim doc = TomlDocument.Load("integers = [ 1, 2, 3 ]")

        Assert.Equal(GetType(TomlArray), doc("integers").GetValueType)

        Assert.Equal(GetType(Long), doc("integers").GetValueType(0))
        Assert.Throws(Of IndexOutOfRangeException)(
            Sub()
                Dim a = doc("integers").GetValueType(3)
            End Sub
        )

        Assert.Throws(Of NotSupportedException)(
            Sub()
                Dim a = doc("integers")("noname")
            End Sub
        )

        Assert.Throws(Of IndexOutOfRangeException)(
            Sub()
                Dim a = doc("integers")(3).GetValue(Of Integer)()
            End Sub
        )

        Assert.Equal(GetType(TomlArray), doc("integers").Get.GetType())
        Assert.Equal(GetType(TomlArray), doc("integers").GetValue(Of TomlArray).GetType())

        Assert.Throws(Of IndexOutOfRangeException)(
            Sub()
                Dim a = doc("integers").GetValue(Of Integer)(3)
            End Sub
        )

        Assert.Equal("[ 1, 2, 3 ]", doc("integers").ToString())
    End Sub

End Class
