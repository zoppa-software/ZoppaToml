Imports System
Imports System.Runtime.Intrinsics.X86
Imports Xunit
Imports ZoppaToml
Imports ZoppaToml.TomlLexical

Public Class ValidTest

    <Fact>
    Sub Array1Test()
        Dim doc = TomlDocument.LoadFromFile("valid\array\array.toml")

        With doc("ints")
            Assert.Equal(1, .GetValue(Of Integer)(0))
            Assert.Equal(2, .GetValue(Of Integer)(1))
            Assert.Equal(3, .GetValue(Of Integer)(2))
        End With

        Assert.Equal(1.1, doc("floats")(0).GetValue(Of Single))
        Assert.Equal(2.1, doc("floats")(1).GetValue(Of Single))
        Assert.Equal(3.1, doc("floats")(2).GetValue(Of Single))

        Assert.Equal("a", doc("strings")(0).GetValue(Of String))
        Assert.Equal("b", doc("strings")(1).GetValue(Of String))
        Assert.Equal("c", doc("strings")(2).GetValue(Of String))

        Assert.Equal(2, doc("comments").Length)
        Assert.Equal(1, doc("comments")(0).GetValue(Of Integer))
        Assert.Equal(2, doc("comments")(1).GetValue(Of Integer))
    End Sub

    <Fact>
    Sub Array2Test()
        Dim doc = TomlDocument.LoadFromFile("valid\array\array-subtables.toml")

        Assert.Equal(2, doc("arr").Length)
        Assert.Equal(1, doc("arr")(0)("subtab")("val").GetValue(Of Integer))
        Assert.Equal(2, doc("arr")(1)("subtab")("val").GetValue(Of Integer))
    End Sub

    <Fact>
    Sub Array3Test()
        Dim doc = TomlDocument.LoadFromFile("valid\array\bool.toml")

        Assert.Equal(True, doc("a")(0).GetValue(Of Boolean))
        Assert.Equal(False, doc("a")(1).GetValue(Of Boolean))
    End Sub

    <Fact>
    Sub Array4Test()
        Dim doc = TomlDocument.LoadFromFile("valid\array\empty.toml")

        Assert.Equal(0, doc("thevoid")(0)(0)(0)(0).Length)
    End Sub

    <Fact>
    Sub Array5Test()
        Dim doc = TomlDocument.LoadFromFile("valid\array\hetergeneous.toml")

        Assert.Equal(1, doc("mixed")(0)(0).GetValue(Of Integer)())
        Assert.Equal(2, doc("mixed")(0)(1).GetValue(Of Integer)())
        Assert.Equal("a", doc("mixed")(1)(0).GetValue(Of String)())
        Assert.Equal("b", doc("mixed")(1)(1).GetValue(Of String)())
        Assert.Equal(1.1, doc("mixed")(2)(0).GetValue(Of Double)())
        Assert.Equal(2.1, doc("mixed")(2)(1).GetValue(Of Double)())
    End Sub

    <Fact>
    Sub Array6Test()
        Dim doc = TomlDocument.LoadFromFile("valid\array\mixed-int-array.toml")

        Assert.Equal(1, doc("arrays-and-ints")(0).GetValue(Of Integer)())
        Assert.Equal("Arrays are not integers.", doc("arrays-and-ints")(1)(0).GetValue(Of String)())
    End Sub

    <Fact>
    Sub BoolTest()
        Dim doc = TomlDocument.LoadFromFile("valid\bool\bool.toml")

        Assert.Equal(True, doc("t").GetValue(Of Boolean)())
        Assert.Equal(False, doc("f").GetValue(Of Boolean)())
    End Sub

    <Fact>
    Sub CommentTest1()
        Dim doc = TomlDocument.LoadFromFile("valid\comment\after-literal-no-ws.toml")

        Assert.Equal(Double.PositiveInfinity, doc("inf").GetValue(Of Double)())
        Assert.Equal(Double.NaN, doc("nan").GetValue(Of Double)())
        Assert.Equal(True, doc("true").GetValue(Of Boolean)())
        Assert.Equal(False, doc("false").GetValue(Of Boolean)())
    End Sub

    <Fact>
    Sub CommentTest2()
        Dim doc = TomlDocument.LoadFromFile("valid\comment\at-eof.toml")

        Assert.Equal("value", doc("key").GetValue(Of String)())
    End Sub

    <Fact>
    Sub CommentTest3()
        Dim doc = TomlDocument.LoadFromFile("valid\comment\at-eof2.toml")

        Assert.Equal("value", doc("key").GetValue(Of String)())
    End Sub

    <Fact>
    Sub CommentTest4()
        Dim doc = TomlDocument.LoadFromFile("valid\comment\everywhere.toml")

        Assert.Equal(42, doc("group")("answer").GetValue(Of Integer)())
        Assert.Equal(42, doc("group")("more")(0).GetValue(Of Integer)())
        Assert.Equal(42, doc("group")("more")(1).GetValue(Of Integer)())
        Assert.Equal(New DateTimeOffset(1979, 5, 27, 7, 32, 12, New TimeSpan(-7, 0, 0)), doc("group")("dt").GetValue(Of DateTimeOffset)())
        Assert.Equal(New DateTime(1979, 5, 27), doc("group")("d").GetValue(Of Date)())
    End Sub

    <Fact>
    Sub CommentTest5()
        Dim doc = TomlDocument.LoadFromFile("valid\comment\noeol.toml")

        Assert.Equal(0, doc.Count)
    End Sub

    <Fact>
    Sub CommentTest6()
        Dim doc = TomlDocument.LoadFromFile("valid\comment\tricky.toml")

        Assert.Equal("11", doc("section")("one").GetValue(Of String)())
        Assert.Equal("22#", doc("section")("two").GetValue(Of String)())
        Assert.Equal("#", doc("section")("three").GetValue(Of String)())
        Assert.Equal($"# no comment{vbLf}# nor this{vbLf}#also not comment", doc("section")("four").GetValue(Of String)())
        Assert.Equal(5.5, doc("section")("five").GetValue(Of Double)())
        Assert.Equal(6, doc("section")("six").GetValue(Of Integer)())
        Assert.Equal("eight", doc("section")("8").GetValue(Of String)())
        Assert.Equal(1000.0, doc("section")("ten").GetValue(Of Double)())
        Assert.True(Math.Abs(11.1 - doc("section")("eleven").GetValue(Of Double)()) < 0.00001)

        Assert.Equal("hash bang", doc("hash#tag")("#!").GetValue(Of String)())
        Assert.Equal("#", doc("hash#tag")("arr3")(0).GetValue(Of String)())
        Assert.Equal("#", doc("hash#tag")("arr3")(1).GetValue(Of String)())
        Assert.Equal("###", doc("hash#tag")("arr3")(2).GetValue(Of String)())
        Assert.Equal(1, doc("hash#tag")("arr4")(0).GetValue(Of Integer)())
        Assert.Equal(2, doc("hash#tag")("arr4")(1).GetValue(Of Integer)())
        Assert.Equal(3, doc("hash#tag")("arr4")(2).GetValue(Of Integer)())
        Assert.Equal(4, doc("hash#tag")("arr4")(3).GetValue(Of Integer)())
        Assert.Equal("#", doc("hash#tag")("arr5")(0)(0)(0)(0)(0).GetValue(Of String)())
        Assert.Equal("}#", doc("hash#tag")("tbl1")("#").GetValue(Of String)())
    End Sub

    <Fact>
    Sub DatetimeTest1()
        Dim doc = TomlDocument.LoadFromFile("valid\datetime\datetime.toml")

        Assert.Equal(New DateTimeOffset(1987, 7, 5, 17, 45, 0, TimeSpan.Zero), doc("space").GetValue(Of DateTimeOffset)())
        Assert.Equal(New DateTimeOffset(1987, 7, 5, 17, 45, 0, TimeSpan.Zero), doc("lower").GetValue(Of DateTimeOffset)())
    End Sub

    <Fact>
    Sub DatetimeTest2()
        Dim doc = TomlDocument.LoadFromFile("valid\datetime\edge.toml")

        Assert.Equal(New DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero), doc("first-offset").GetValue(Of DateTimeOffset)())
        Assert.Equal(New DateTime(1, 1, 1, 0, 0, 0), doc("first-local").GetValue(Of DateTime)())
        Assert.Equal(New DateTime(1, 1, 1, 0, 0, 0), doc("first-date").GetValue(Of DateTime)())
        Assert.Equal(New DateTimeOffset(9999, 12, 31, 23, 59, 59, TimeSpan.Zero), doc("last-offset").GetValue(Of DateTimeOffset)())
        Assert.Equal(New DateTime(9999, 12, 31, 23, 59, 59), doc("last-local").GetValue(Of DateTime)())
        Assert.Equal(New DateTime(9999, 12, 31, 0, 0, 0), doc("last-date").GetValue(Of DateTime)())
    End Sub

    <Fact>
    Sub DatetimeTest3()
        Dim doc = TomlDocument.LoadFromFile("valid\datetime\leap-year.toml")

        Assert.Equal(New DateTimeOffset(2000, 2, 29, 15, 15, 15, TimeSpan.Zero), doc("2000-datetime").GetValue(Of DateTimeOffset)())
        Assert.Equal(New DateTime(2000, 2, 29, 15, 15, 15), doc("2000-datetime-local").GetValue(Of DateTime)())
        Assert.Equal(New DateTime(2000, 2, 29, 0, 0, 0), doc("2000-date").GetValue(Of DateTime)())
        Assert.Equal(New DateTimeOffset(2024, 2, 29, 15, 15, 15, TimeSpan.Zero), doc("2024-datetime").GetValue(Of DateTimeOffset)())
        Assert.Equal(New DateTime(2024, 2, 29, 15, 15, 15), doc("2024-datetime-local").GetValue(Of DateTime)())
        Assert.Equal(New DateTime(2024, 2, 29, 0, 0, 0), doc("2024-date").GetValue(Of DateTime)())
    End Sub

    <Fact>
    Sub DatetimeTest4()
        Dim doc = TomlDocument.LoadFromFile("valid\datetime\local.toml")

        Assert.Equal(New DateTime(1987, 7, 5, 17, 45, 0), doc("local").GetValue(Of DateTime)())
        Assert.Equal(New DateTime(1977, 12, 21, 10, 32, 0, 555), doc("milli").GetValue(Of DateTime)())
        Assert.Equal(New DateTime(1987, 7, 5, 17, 45, 0), doc("space").GetValue(Of DateTime)())
    End Sub

End Class
