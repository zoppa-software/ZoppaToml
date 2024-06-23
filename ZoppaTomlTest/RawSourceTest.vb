Imports System
Imports Xunit
Imports ZoppaToml
Imports ZoppaToml.TomlLexical

Public Class RawSourceTest

    <Fact>
    Sub Case01()
        Dim raw As New RawSource("1234567890")

        Dim ptr = raw.GetPointer()
        Assert.Equal("12345...", ptr.TakeChar(5))

        ptr.Skip(8)
        Assert.Equal("90", ptr.TakeChar(5))
    End Sub

    <Fact>
    Sub Case02()
        Dim query =
"# この行は全てコメントです。
key = ""value""  # 行末までコメントです。
another = ""# これはコメントではありません"""
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(7, ans.Count)
        Assert.Equal("# この行は全てコメントです。", ans(0).ToString())
        Assert.Equal("key = ""value""", ans(2).ToString())
        Assert.Equal("another = ""# これはコメントではありません""", ans(6).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal("value", doc("key").GetValue(Of String)())
        Assert.Equal("# これはコメントではありません", doc("another").GetValue(Of String)())

        Dim raw2 As New RawSource("key = # 無効")
        Assert.Throws(Of TomlSyntaxException)(Sub() raw2.Lexical())

        Dim raw3 As New RawSource("first = ""Tom"" last = ""Preston-Werner"" # 無効")
        Assert.Throws(Of TomlSyntaxException)(Sub() raw3.Lexical())
    End Sub

    <Fact>
    Sub Case03()
        Dim raw As New RawSource(
"key = ""value"""
)

        Dim ans = raw.Lexical()
        Assert.Equal(1, ans.Count)
        Assert.Equal("key = ""value""", ans(0).ToString())
    End Sub

    <Fact>
    Sub Case04()
        Dim raw As New RawSource(
"key = ""value""
bare_key = ""value""
bare-key = ""value""
1234 = ""value"""
)

        Dim ans = raw.Lexical()
        Assert.Equal(7, ans.Count)
        Assert.Equal("key = ""value""", ans(0).ToString())
        Assert.Equal("bare_key = ""value""", ans(2).ToString())
        Assert.Equal("bare-key = ""value""", ans(4).ToString())
        Assert.Equal("1234 = ""value""", ans(6).ToString())
    End Sub

    <Fact>
    Sub Case05()
        Dim raw As New RawSource(
"""127.0.0.1"" = ""value""
""character encoding"" = ""value""
""ʎǝʞ"" = ""value""
'key2' = ""value""
'quoted ""value""' = ""value"""
)

        Dim ans = raw.Lexical()
        Assert.Equal(9, ans.Count)
        Assert.Equal("""127.0.0.1"" = ""value""", ans(0).ToString())
        Assert.Equal("""character encoding"" = ""value""", ans(2).ToString())
        Assert.Equal("""ʎǝʞ"" = ""value""", ans(4).ToString())
        Assert.Equal("'key2' = ""value""", ans(6).ToString())
        Assert.Equal("'quoted ""value""' = ""value""", ans(8).ToString())

        Dim raw2 As New RawSource("= ""no key name""")
        Assert.Throws(Of TomlSyntaxException)(Sub() raw2.Lexical())
    End Sub

    <Fact>
    Sub Case06()
        Dim query = 
"name = ""Orange""
physical.color = ""orange""
physical.shape = ""round""
site.""google.com"" = true"

        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(7, ans.Count)
        Assert.Equal("name = ""Orange""", ans(0).ToString())
        Assert.Equal("physical.color = ""orange""", ans(2).ToString())
        Assert.Equal("physical.shape = ""round""", ans(4).ToString())
        Assert.Equal("site.""google.com"" = true", ans(6).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal("Orange", doc("name").GetValue(Of String)())
        Assert.Equal("orange", doc("physical")("color").GetValue(Of String)())
        Assert.Equal("round", doc("physical")("shape").GetValue(Of String)())
        Assert.Equal(True, doc("site")("google.com").GetValue(Of Boolean)())
    End Sub

    <Fact>
    Sub Case07()
        Dim query =
"fruit.name = ""banana""       # これがベストプラクティスです
fruit. color = ""yellow""     # fruit.color と同じです
fruit . flavor = ""banana""   # fruit.flavor と同じです
"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(12, ans.Count)
        Assert.Equal("fruit.name = ""banana""", ans(0).ToString())
        Assert.Equal("fruit. color = ""yellow""", ans(4).ToString())
        Assert.Equal("fruit . flavor = ""banana""", ans(8).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal("banana", doc("fruit")("name").GetValue(Of String)())
        Assert.Equal("yellow", doc("fruit")("color").GetValue(Of String)())
        Assert.Equal("banana", doc("fruit")("flavor").GetValue(Of String)())
    End Sub

    <Fact>
    Sub Case08()
        Dim query = "str = ""I'm a string. \""You can quote me\"". Name\tJos\u00E9\r\nLocation\tSF."""
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(1, ans.Count)
        Assert.Equal("str = ""I'm a string. \""You can quote me\"". Name\tJos\u00E9\r\nLocation\tSF.""", ans(0).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal("I'm a string. ""You can quote me"". Name	José
Location	SF.", doc("str").GetValue(Of String)())
    End Sub

    <Fact>
    Sub Case09()
        Dim query = "str1 = """"""
Roses are red
Violets are blue"""""""
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(1, ans.Count)
        Assert.Equal("str1 = """"""
Roses are red
Violets are blue""""""", ans(0).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal("Roses are red
Violets are blue", doc("str1").GetValue(Of String)())

        Dim query2 = "
str2 = """"""The quick brown \


  fox jumps over \
    the lazy dog."""""""
        Dim doc2 = TomlDocument.Load(query2)
        Assert.Equal("The quick brown fox jumps over the lazy dog.", doc2("str2").GetValue(Of String)())

        Dim query3 = "
str3 = """"""\
       The quick brown \
       fox jumps over \
       the lazy dog.\
       """""""
        Dim doc3 = TomlDocument.Load(query3)
        Assert.Equal("The quick brown fox jumps over the lazy dog.", doc3("str3").GetValue(Of String)())
    End Sub

    <Fact>
    Sub Case10()
        Dim raw As New RawSource(
"str4 = """"""引用符2つ: """"。簡単ですね。""""""
# str5 = """"""引用符3つ: """"""。""""""  # 無効
str5 = """"""引用符3つ: """"\""。""""""
str6 = """"""引用符15個: """"\""""""\""""""\""""""\""""""\""。""""""

# ""This,"" she said, ""is just a pointless statement.""
str7 = """"""""This,"" she said, ""is just a pointless statement.""""""""")

        Dim ans = raw.Lexical()
        Assert.Equal(12, ans.Count)
        Assert.Equal("str4 = """"""引用符2つ: """"。簡単ですね。""""""", ans(0).ToString())
        Assert.Equal("# str5 = """"""引用符3つ: """"""。""""""  # 無効", ans(2).ToString())
        Assert.Equal("str5 = """"""引用符3つ: """"\""。""""""", ans(4).ToString())
        Assert.Equal("str6 = """"""引用符15個: """"\""""""\""""""\""""""\""""""\""。""""""", ans(6).ToString())
        Assert.Equal("str7 = """"""""This,"" she said, ""is just a pointless statement.""""""""", ans(11).ToString())
    End Sub

    <Fact>
    Sub Case11()
        Dim query = "# 表示されている文字列、そのものが得られます。
winpath  = 'C:\Users\nodejs\templates'
winpath2 = '\\ServerX\admin$\system32\'
quoted   = 'Tom ""Dubs"" Preston-Werner'
regex    = '<\i\c*\s*>'"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(9, ans.Count)
        Assert.Equal("# 表示されている文字列、そのものが得られます。", ans(0).ToString())
        Assert.Equal("winpath  = 'C:\Users\nodejs\templates'", ans(2).ToString())
        Assert.Equal("winpath2 = '\\ServerX\admin$\system32\'", ans(4).ToString())
        Assert.Equal("quoted   = 'Tom ""Dubs"" Preston-Werner'", ans(6).ToString())
        Assert.Equal("regex    = '<\i\c*\s*>'", ans(8).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal("C:\Users\nodejs\templates", doc("winpath").GetValue(Of String)())
        Assert.Equal("\\ServerX\admin$\system32\", doc("winpath2").GetValue(Of String)())
        Assert.Equal("Tom ""Dubs"" Preston-Werner", doc("quoted").GetValue(Of String)())
        Assert.Equal("<\i\c*\s*>", doc("regex").GetValue(Of String)())
    End Sub

    <Fact>
    Sub Case12()
        Dim query =
"regex2 = '''I [dw]on't need \d{2} apples'''
lines  = '''
生の文字列では、
最初の改行は取り除かれます。
   その他の空白は、
   保持されます。
'''"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(3, ans.Count)
        Assert.Equal("regex2 = '''I [dw]on't need \d{2} apples'''", ans(0).ToString())
        Assert.Equal("lines  = '''
生の文字列では、
最初の改行は取り除かれます。
   その他の空白は、
   保持されます。
'''", ans(2).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal("I [dw]on't need \d{2} apples", doc("regex2").GetValue(Of String)())
        Assert.Equal("生の文字列では、
最初の改行は取り除かれます。
   その他の空白は、
   保持されます。
", doc("lines").GetValue(Of String)())
    End Sub

    <Fact>
    Sub Case13()
        Dim raw As New RawSource(
"quot15 = '''Here are fifteen quotation marks: """"""""""""""""""""""""""""""'''

# apos15 = '''Here are fifteen apostrophes: ''''''''''''''''''  # 無効
apos15 = ""Here are fifteen apostrophes: '''''''''''''''""

# 'That,' she said, 'is still pointless.'
str = ''''That,' she said, 'is still pointless.''''")

        Dim ans = raw.Lexical()
        Assert.Equal(11, ans.Count)
        Assert.Equal("quot15 = '''Here are fifteen quotation marks: """"""""""""""""""""""""""""""'''", ans(0).ToString())
        Assert.Equal("apos15 = ""Here are fifteen apostrophes: '''''''''''''''""", ans(5).ToString())
        Assert.Equal("str = ''''That,' she said, 'is still pointless.''''", ans(10).ToString())
    End Sub

    <Fact>
    Sub Case14()
        Dim raw As New RawSource(
"apos15 = '''Here are fifteen apostrophes: ''''''''''''''''''")
        Assert.Throws(Of TomlSyntaxException)(Sub() raw.Lexical())
    End Sub

    <Fact>
    Sub Case15()
        Dim query = "int1 = +99
int2 = 42
int3 = 0
int4 = -17"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(7, ans.Count)
        Assert.Equal("int1 = +99", ans(0).ToString())
        Assert.Equal("int2 = 42", ans(2).ToString())
        Assert.Equal("int3 = 0", ans(4).ToString())
        Assert.Equal("int4 = -17", ans(6).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(99, doc("int1").GetValue(Of Integer)())
        Assert.Equal(42, doc("int2").GetValue(Of Integer)())
        Assert.Equal(0, doc("int3").GetValue(Of Integer)())
        Assert.Equal(-17, doc("int4").GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub Case16()
        Dim query =
"int5 = 1_000
int6 = 5_349_221
int7 = 53_49_221  # インド式命数法
int8 = 1_2_3_4_5  # 有効ですが推奨されません"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(11, ans.Count)
        Assert.Equal("int5 = 1_000", ans(0).ToString())
        Assert.Equal("int6 = 5_349_221", ans(2).ToString())
        Assert.Equal("int7 = 53_49_221", ans(4).ToString())
        Assert.Equal("int8 = 1_2_3_4_5", ans(8).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(1000, doc("int5").GetValue(Of Integer)())
        Assert.Equal(5349221, doc("int6").GetValue(Of Integer)())
        Assert.Equal(5349221, doc("int7").GetValue(Of Integer)())
        Assert.Equal(12345, doc("int8").GetValue(Of Integer)())
    End Sub

    <Fact>
    Sub Case17()
        Dim query = "# 接頭辞 `0x` が付いた 16 進数
hex1 = 0xDEADBEEF
hex2 = 0xdeadbeef
hex3 = 0xdead_beef

# 接頭辞 `0o` が付いた 8 進数
oct1 = 0o01234567
oct2 = 0o755 # Unix ファイルのパーミッションに便利

# 接頭辞 `0b` が付いた 2 進数
bin1 = 0b11010110"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(21, ans.Count)
        Assert.Equal("hex1 = 0xDEADBEEF", ans(2).ToString())
        Assert.Equal("hex2 = 0xdeadbeef", ans(4).ToString())
        Assert.Equal("hex3 = 0xdead_beef", ans(6).ToString())
        Assert.Equal("oct1 = 0o01234567", ans(11).ToString())
        Assert.Equal("oct2 = 0o755", ans(13).ToString())
        Assert.Equal("bin1 = 0b11010110", ans(20).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(3735928559, doc("hex1").GetValue(Of Long)())
        Assert.Equal(3735928559, doc("hex2").GetValue(Of Long)())
        Assert.Equal(3735928559, doc("hex3").GetValue(Of Long)())
        Assert.Equal(342391, doc("oct1").GetValue(Of Long)())
        Assert.Equal(493, doc("oct2").GetValue(Of Long)())
        Assert.Equal(214, doc("bin1").GetValue(Of Long)())
    End Sub

    <Fact>
    Sub Case18()
        Dim query =
"# 小数部
flt1 = +1.0
flt2 = 3.1415
flt3 = -0.01

# 指数部
flt4 = 5e+22
flt5 = 1e06
flt6 = -2E-2

# 少数部と指数部の両方
flt7 = 6.626e-34"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(21, ans.Count)
        Assert.Equal("flt1 = +1.0", ans(2).ToString())
        Assert.Equal("flt2 = 3.1415", ans(4).ToString())
        Assert.Equal("flt3 = -0.01", ans(6).ToString())
        Assert.Equal("flt4 = 5e+22", ans(11).ToString())
        Assert.Equal("flt5 = 1e06", ans(13).ToString())
        Assert.Equal("flt6 = -2E-2", ans(15).ToString())
        Assert.Equal("flt7 = 6.626e-34", ans(20).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(1.0, doc("flt1").GetValue(Of Double)())
        Assert.Equal(3.1415, doc("flt2").GetValue(Of Double)())
        Assert.Equal(-0.01, doc("flt3").GetValue(Of Double)())
        Assert.Equal(5.0E+22, doc("flt4").GetValue(Of Double)())
        Assert.Equal(1000000.0, doc("flt5").GetValue(Of Double)())
        Assert.Equal(-0.02, doc("flt6").GetValue(Of Double)())
        Assert.Equal(6.626E-34, doc("flt7").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub Case19()
        Dim raw As New RawSource(
"flt8 = 224_617.445_991_228")

        Dim ans = raw.Lexical()
        Assert.Equal(1, ans.Count)
        Assert.Equal("flt8 = 224_617.445_991_228", ans(0).ToString())
    End Sub

    <Fact>
    Sub Case20()
        Dim query =
"# 無限大
sf1 = inf  # 正の無限大
sf2 = +inf # 正の無限大
sf3 = -inf # 負の無限大

# 非数 (NaN)
sf4 = nan  # 実際の sNaN/qNaN エンコーディングは実装に依存します
sf5 = +nan # `nan` と等価
sf6 = -nan # 有効で、実際のエンコーディングは実装に依存します"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(28, ans.Count)
        Assert.Equal("sf1 = inf", ans(2).ToString())
        Assert.Equal("sf2 = +inf", ans(6).ToString())
        Assert.Equal("sf3 = -inf", ans(10).ToString())
        Assert.Equal("sf4 = nan", ans(17).ToString())
        Assert.Equal("sf5 = +nan", ans(21).ToString())
        Assert.Equal("sf6 = -nan", ans(25).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(Double.PositiveInfinity, doc("sf1").GetValue(Of Double)())
        Assert.Equal(Double.PositiveInfinity, doc("sf2").GetValue(Of Double)())
        Assert.Equal(Double.NegativeInfinity, doc("sf3").GetValue(Of Double)())
        Assert.Equal(Double.NaN, doc("sf4").GetValue(Of Double)())
        Assert.Equal(Double.NaN, doc("sf5").GetValue(Of Double)())
        Assert.Equal(-Double.NaN, doc("sf6").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub Case21()
        Dim query =
"bool1 = true
bool2 = false"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(3, ans.Count)
        Assert.Equal("bool1 = true", ans(0).ToString())
        Assert.Equal("bool2 = false", ans(2).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(True, doc("bool1").GetValue(Of Boolean)())
        Assert.Equal(False, doc("bool2").GetValue(Of Boolean)())
    End Sub

    <Fact>
    Sub Case22()
        Dim query =
"odt1 = 1979-05-27T07:32:00Z
odt2 = 1979-05-27T00:32:00-07:00
odt3 = 1979-05-27T00:32:00.999999-07:00"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(5, ans.Count)
        Assert.Equal("odt1 = 1979-05-27T07:32:00Z", ans(0).ToString())
        Assert.Equal("odt2 = 1979-05-27T00:32:00-07:00", ans(2).ToString())
        Assert.Equal("odt3 = 1979-05-27T00:32:00.999999-07:00", ans(4).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(New DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero), doc("odt1").GetValue(Of DateTimeOffset)())
        Assert.Equal(New DateTimeOffset(1979, 5, 27, 0, 32, 0, New TimeSpan(-7, 0, 0)), doc("odt2").GetValue(Of DateTimeOffset)())
        Assert.Equal(New DateTimeOffset(1979, 5, 27, 0, 32, 0, 999, New TimeSpan(-7, 0, 0)), doc("odt3").GetValue(Of DateTimeOffset)())
    End Sub

    <Fact>
    Sub Case23()
        Dim query = "odt4 = 1979-05-27 07:32:00Z"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(1, ans.Count)
        Assert.Equal("odt4 = 1979-05-27 07:32:00Z", ans(0).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(New DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero), doc("odt4").GetValue(Of DateTimeOffset)())
    End Sub

    <Fact>
    Sub Case24()
        Dim query =
"lt1 = 07:32:00
lt2 = 00:32:00.999999"
        Dim raw As New RawSource(query)

        Dim ans = raw.Lexical()
        Assert.Equal(3, ans.Count)
        Assert.Equal("lt1 = 07:32:00", ans(0).ToString())
        Assert.Equal("lt2 = 00:32:00.999999", ans(2).ToString())

        Dim doc = TomlDocument.Load(query)
        Assert.Equal(New TimeSpan(7, 32, 0), doc("lt1").GetValue(Of TimeSpan)())
        Assert.Equal(New TimeSpan(0, 0, 32, 0, 999), doc("lt2").GetValue(Of TimeSpan)())
    End Sub

    <Fact>
    Sub Case25()
        Dim raw As New RawSource(
"ldt1 = 1979-05-27T07:32:00
ldt2 = 1979-05-27T00:32:00.999999")

        Dim ans = raw.Lexical()
        Assert.Equal(3, ans.Count)
        Assert.Equal("ldt1 = 1979-05-27T07:32:00", ans(0).ToString())
        Assert.Equal("ldt2 = 1979-05-27T00:32:00.999999", ans(2).ToString())
    End Sub

    <Fact>
    Sub Case28()
        Dim raw As New RawSource(
"integers = [ 1, 2, 3 ]
colors = [ ""red"", ""yellow"", ""green"" ]
nested_arrays_of_ints = [ [ 1, 2 ], [3, 4, 5] ]
nested_mixed_array = [ [ 1, 2 ], [""a"", ""b"", ""c""] ]
string_array = [ ""all"", 'strings', """"""are the same"""""", '''type''' ]

# 異なるデータ型の値を混在させることができます
numbers = [ 0.1, 0.2, 0.5, 1, 2, 5 ]
contributors = [
  ""Foo Bar <foo@example.com>"",
  { name = ""Baz Qux"", email = ""bazqux@example.com"", url = ""https://example.com/bazqux"" }
]")

        Dim ans = raw.Lexical()
        Assert.Equal(16, ans.Count)
        Assert.Equal("integers = [ 1, 2, 3 ]", ans(0).ToString())
        Assert.Equal("colors = [ ""red"", ""yellow"", ""green"" ]", ans(2).ToString())
        Assert.Equal("nested_arrays_of_ints = [ [ 1, 2 ], [3, 4, 5] ]", ans(4).ToString())
        Assert.Equal("nested_mixed_array = [ [ 1, 2 ], [""a"", ""b"", ""c""] ]", ans(6).ToString())
        Assert.Equal("string_array = [ ""all"", 'strings', """"""are the same"""""", '''type''' ]", ans(8).ToString())
        Assert.Equal("numbers = [ 0.1, 0.2, 0.5, 1, 2, 5 ]", ans(13).ToString())
        Assert.Equal("contributors = [
  ""Foo Bar <foo@example.com>"",
  { name = ""Baz Qux"", email = ""bazqux@example.com"", url = ""https://example.com/bazqux"" }
]", ans(15).ToString())
    End Sub

    <Fact>
    Sub Case29()
        Dim raw As New RawSource(
"integers2 = [
  1, 2, 3
]

integers3 = [
  1,
  2, # 最後の行末の `,` は有効です
]")

        Dim ans = raw.Lexical()
        Assert.Equal(4, ans.Count)
        Assert.Equal("integers2 = [
  1, 2, 3
]", ans(0).ToString())
        Assert.Equal("integers3 = [
  1,
  2, # 最後の行末の `,` は有効です
]", ans(3).ToString())
    End Sub

    <Fact>
    Sub Case30()
        Dim raw As New RawSource(
"name = { first = ""Tom"", last = ""Preston-Werner"" }
point = { x = 1, y = 2 }
animal = { type.name = ""pug"" }")

        Dim ans = raw.Lexical()
        Assert.Equal(5, ans.Count)
        Assert.Equal("name = { first = ""Tom"", last = ""Preston-Werner"" }", ans(0).ToString())
        Assert.Equal("point = { x = 1, y = 2 }", ans(2).ToString())
        Assert.Equal("animal = { type.name = ""pug"" }", ans(4).ToString())
    End Sub

    <Fact>
    Sub Case31()
        Dim query =
"[[products]]
name = ""Hammer""
sku = 738594937

[[products]]  # 配列内の空のテーブル

[[products]]
name = ""Nail""
sku = 284758393

color = ""gray"""

        Dim doc = TomlDocument.Load(query)

        Dim tbl0 = doc("products")(0)
        Assert.Equal(2, tbl0.Length)
        Assert.Equal("Hammer", tbl0("name").GetValue(Of String)())
        Assert.Equal(738594937, tbl0("sku").GetValue(Of Long)())

        Dim tbl1 = doc("products")(1)
        Assert.Equal(0, tbl1.Length)

        Dim tbl2 = doc("products")(2)
        Assert.Equal(3, tbl2.Length)
        Assert.Equal("Nail", tbl2("name").GetValue(Of String)())
        Assert.Equal(284758393, tbl2("sku").GetValue(Of Long)())
        Assert.Equal("gray", tbl2("color").GetValue(Of String)())
    End Sub

End Class
