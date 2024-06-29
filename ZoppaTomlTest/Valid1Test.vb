Imports System
Imports System.Dynamic
Imports System.Runtime.Intrinsics.X86
Imports Xunit
Imports ZoppaToml
Imports ZoppaToml.TomlLexical

Public Class Valid1Test

    <Fact>
    Sub FloatTest1()
        Dim doc = TomlDocument.LoadFromFile("valid\float\exponent.toml")

        Assert.Equal(300.0, doc("lower").GetValue(Of Double)())
        Assert.Equal(300.0, doc("upper").GetValue(Of Double)())
        Assert.Equal(0.03, doc("neg").GetValue(Of Double)())
        Assert.Equal(300.0, doc("pos").GetValue(Of Double)())
        Assert.Equal(3.0, doc("zero").GetValue(Of Double)())
        Assert.Equal(310.0, doc("pointlower").GetValue(Of Double)())
        Assert.Equal(310.0, doc("pointupper").GetValue(Of Double)())
        Assert.Equal(-0.1, doc("minustenth").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest2()
        Dim doc = TomlDocument.LoadFromFile("valid\float\float.toml")

        Assert.Equal(3.14, doc("pi").GetValue(Of Double)())
        Assert.Equal(+3.14, doc("pospi").GetValue(Of Double)())
        Assert.Equal(-3.14, doc("negpi").GetValue(Of Double)())
        Assert.Equal(0.123, doc("zero-intpart").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest3()
        Dim doc = TomlDocument.LoadFromFile("valid\float\inf-and-nan.toml")

        Assert.Equal(Double.NaN, doc("nan").GetValue(Of Double)())
        Assert.Equal(-Double.NaN, doc("nan_neg").GetValue(Of Double)())
        Assert.Equal(Double.NaN, doc("nan_plus").GetValue(Of Double)())
        Assert.Equal(Double.PositiveInfinity, doc("infinity").GetValue(Of Double)())
        Assert.Equal(Double.NegativeInfinity, doc("infinity_neg").GetValue(Of Double)())
        Assert.Equal(Double.PositiveInfinity, doc("infinity_plus").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest4()
        Dim doc = TomlDocument.LoadFromFile("valid\float\long.toml")

        Assert.Equal(3.1415926535897931, doc("longpi").GetValue(Of Double)())
        Assert.Equal(-3.1415926535897931, doc("neglongpi").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest5()
        Dim doc = TomlDocument.LoadFromFile("valid\float\max-int.toml")

        Assert.Equal(9.00719925474099E+15, doc("max_float").GetValue(Of Double)())
        Assert.Equal(-9.00719925474099E+15, doc("min_float").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest6()
        Dim doc = TomlDocument.LoadFromFile("valid\float\underscore.toml")

        Assert.Equal(3141.5927, doc("before").GetValue(Of Double)())
        Assert.Equal(3141.5927, doc("after").GetValue(Of Double)())
        Assert.Equal(300000000000000.0, doc("exponent").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest7()
        Dim doc = TomlDocument.LoadFromFile("valid\float\zero.toml")

        Assert.Equal(0.0, doc("zero").GetValue(Of Double)())
        Assert.Equal(0.0, doc("signed-pos").GetValue(Of Double)())
        Assert.Equal(-0.0, doc("signed-neg").GetValue(Of Double)())
        Assert.Equal(0E0, doc("exponent").GetValue(Of Double)())
        Assert.Equal(0E00, doc("exponent-two-0").GetValue(Of Double)())
        Assert.Equal(+0E0, doc("exponent-signed-pos").GetValue(Of Double)())
        Assert.Equal(-0E0, doc("exponent-signed-neg").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub InlineTest1()
        Dim doc = TomlDocument.LoadFromFile("valid\inline-table\bool.toml")

        Assert.Equal(True, doc("a")("a").GetValue(Of Boolean)())
        Assert.Equal(False, doc("a")("b").GetValue(Of Boolean)())
    End Sub

    <Fact>
    Sub InlineTest2()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\empty.toml")

        Assert.Equal(0, doc.empty1.Length)
        Assert.Equal(0, doc.empty2.Length)
        Assert.Equal(1, doc.empty_in_array(0).not_empty.GetValue(Of Integer))
        Assert.Equal(0, doc.empty_in_array(1).Length)
        Assert.Equal(0, doc.empty_in_array2(0).Length)
        Assert.Equal(1, doc.empty_in_array2(1).not_empty.GetValue(Of Integer))
        Assert.Equal(0, doc.many_empty(0).Length)
        Assert.Equal(0, doc.many_empty(1).Length)
        Assert.Equal(0, doc.many_empty(2).Length)
        Assert.Equal(0, doc.nested_empty.empty.Length)
        Assert.Equal(0, doc.with_cmt.Length)
    End Sub

    <Fact>
    Sub InlineTest3()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\end-in-bool.toml")

        Assert.Equal(">3.6", doc.black.python.GetValue(Of String)())
        Assert.Equal(">=18.9b0", doc.black.version.GetValue(Of String)())
        Assert.Equal(True, doc.black.allow_prereleases.GetValue(Of Boolean)())
    End Sub

    <Fact>
    Sub InlineTest4()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\inline-table.toml")

        Assert.Equal("Tom", doc.name.first.GetValue(Of String)())
        Assert.Equal("Preston-Werner", doc.name.last.GetValue(Of String)())
        Assert.Equal(1, doc.point.x.GetValue(Of Integer)())
        Assert.Equal(2, doc.point.y.GetValue(Of Integer)())
        Assert.Equal(1, doc.simple.a.GetValue(Of Integer)())
        Assert.Equal(1, doc("str-key").a.GetValue(Of Integer)())
        Dim res As New ArrayList()
        For Each item In doc("table-array")
            res.Add(item)
        Next
        Assert.Equal(1, res(0).a.GetValue(Of Integer)())
        Assert.Equal(2, res(1).b.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest5()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\key-dotted-1.toml")

        Assert.Equal(1, doc.a.a.b.GetValue(Of Integer)())
        Assert.Equal(1, doc.b.a.b.GetValue(Of Integer)())
        Assert.Equal(1, doc.c.a.b.GetValue(Of Integer)())
        Assert.Equal(1, doc.d.a.b.GetValue(Of Integer)())
        Assert.Equal(1, doc.e.a.b.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest6()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\key-dotted-2.toml")

        Assert.Equal(1, doc.many.dots.here.dot.dot.dot.a.b.c.GetValue(Of Integer)())
        Assert.Equal(2, doc.many.dots.here.dot.dot.dot.a.b.d.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest7()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\key-dotted-3.toml")

        Assert.Equal(1, doc.tbl.a.b.c.d.e.GetValue(Of Integer)())
        Assert.Equal(1, doc.tbl.x.a.b.c.d.e.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest8()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\key-dotted-4.toml")

        Assert.Equal(1, doc.arr(0).t.a.b.GetValue(Of Integer)())
        Assert.Equal(1, doc.arr(0).T.a.b.GetValue(Of Integer)())
        Assert.Equal(2, doc.arr(1).t.a.b.GetValue(Of Integer)())
        Assert.Equal(2, doc.arr(1).T.a.b.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest9()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\key-dotted-5.toml")

        Assert.Equal(1, doc("arr-1")(0).a.b.GetValue(Of Integer)())
        Assert.Equal("str", doc("arr-2")(0).GetValue(Of String)())
        Assert.Equal(1, doc("arr-2")(1).a.b.GetValue(Of Integer)())
        Assert.Equal(1, doc("arr-3")(0).a.b.GetValue(Of Integer)())
        Assert.Equal(2, doc("arr-3")(1).a.b.GetValue(Of Integer)())
        Assert.Equal("str", doc("arr-4")(0).GetValue(Of String)())
        Assert.Equal(1, doc("arr-4")(1).a.b.GetValue(Of Integer)())
        Assert.Equal(2, doc("arr-4")(2).a.b.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest10()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\key-dotted-6.toml")

        Assert.Equal(1, doc.top.dot.dot(0).dot.dot.dot.GetValue(Of Integer)())
        Assert.Equal(2, doc.top.dot.dot(1).dot.dot.dot.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest11()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\key-dotted-7.toml")

        Assert.Equal(1, doc.arr(0).a.b(0).c.d.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest12()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\multiline.toml")

        Assert.Equal(1, doc.tbl_multiline.a.GetValue(Of Integer)())
        Assert.Equal($"multiline{vbLf}", doc.tbl_multiline.b.GetValue(Of String)())
        Assert.Equal($"and yet{vbLf}another line", doc.tbl_multiline.c.GetValue(Of String)())
        Assert.Equal(4, doc.tbl_multiline.d.GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub InlineTest13()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\nest.toml")

        Assert.Equal(0, doc.tbl_tbl_empty.tbl_0.Length())
        Assert.Equal(1, doc.tbl_tbl_val.tbl_1.one.GetValue(Of Integer))
        Assert.Equal(1, doc.tbl_arr_tbl.arr_tbl(0).one.GetValue(Of Integer))
        Assert.Equal(1, doc.arr_tbl_tbl(0).tbl.one.GetValue(Of Integer))

        Assert.Equal(0, doc.arr_arr_tbl_empty(0)(0).Length())
        Assert.Equal(1, doc.arr_arr_tbl_val(0)(0).one.GetValue(Of Integer))
        Assert.Equal(1, doc.arr_arr_tbls(0)(0).one.GetValue(Of Integer))
        Assert.Equal(2, doc.arr_arr_tbls(0)(1).two.GetValue(Of Integer))
    End Sub

    <Fact>
    Sub InlineTest14()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\newline.toml")

        Assert.Equal(1, doc("trailing-comma-1").c.GetValue(Of Integer))

        Assert.Equal(1, doc("trailing-comma-2").c.GetValue(Of Integer))

        Assert.Equal("world", doc("tbl-1").hello.GetValue(Of String))
        Assert.Equal(2, doc("tbl-1")("1").GetValue(Of Integer))
        Assert.Equal(1, doc("tbl-1").arr(0).GetValue(Of Integer))
        Assert.Equal(2, doc("tbl-1").arr(1).GetValue(Of Integer))
        Assert.Equal(3, doc("tbl-1").arr(2).GetValue(Of Integer))
        Assert.Equal(1, doc("tbl-1").tbl.k.GetValue(Of Integer))

        Assert.Equal($"	Hello{vbLf}	", doc("tbl-2").k.GetValue(Of String))
    End Sub

    <Fact>
    Sub InlineTest15()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\inline-table\spaces.toml")

        Assert.Equal("4", doc("clap-1").version.GetValue(Of String)())
        Assert.Equal("derive", doc("clap-1").features(0).GetValue(Of String)())
        Assert.Equal("cargo", doc("clap-1").features(1).GetValue(Of String)())

        Assert.Equal("4", doc("clap-2").version.GetValue(Of String)())
        Assert.Equal("derive", doc("clap-2").features(0).GetValue(Of String)())
        Assert.Equal("cargo", doc("clap-2").features(1).GetValue(Of String)())

        Assert.Equal("x", doc("clap-2").nest.a.GetValue(Of String)())
        Assert.Equal(1.5, doc("clap-2").nest.b(0).GetValue(Of Double)())
        Assert.Equal(9, doc("clap-2").nest.b(1).GetValue(Of Double)())
    End Sub

End Class
