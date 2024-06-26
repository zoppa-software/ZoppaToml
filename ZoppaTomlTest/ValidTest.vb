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

End Class
