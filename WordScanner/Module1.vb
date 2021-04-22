Imports System.IO
Imports System.Threading
Imports System.Timers
Imports Newtonsoft.Json.Linq
Module Module1
    Dim DebugMode As Integer = 0
    Dim Fails As Integer = 0
    Dim EnglishSearch As Boolean = False
    Sub Main()
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.White
        Console.InputEncoding = System.Text.Encoding.Unicode 'make sure the console can display japanese characters
        Console.OutputEncoding = System.Text.Encoding.Unicode
        EnglishSearch = False

        Console.Write("Search for: ")
        Dim SearchWord As String = Console.ReadLine.ToLower.Trim.Replace(" ", "")
        If SearchWord.Contains("e") = True Or SearchWord.Contains("a") = True Then
            EnglishSearch = True
        End If
        If SearchWord = "/files" Then
            Dim appDir As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
            Process.Start(appDir)
            Main()
        End If
        DebugMode = 0
        If SearchWord.Contains("d1") Then
            SearchWord = SearchWord.Replace("d1", "")
            Console.WriteLine(" - Will show information of where a result is found")
            Console.WriteLine(" - Shows the whole object of a result")
            DebugMode = 1
        End If
        If SearchWord.Contains("d2") Then
            SearchWord = SearchWord.Replace("d2", "")
            Console.WriteLine(" - Will show where results are found")
            Console.WriteLine(" - Won't stop search no matter how many fails")
            DebugMode = 2
        End If
        If SearchWord.Contains("d3") Then
            SearchWord = SearchWord.Replace("d3", "")
            Console.WriteLine(" - Will show first character for every item")
            DebugMode = 3
        End If

        Console.WriteLine()
        Console.WriteLine("Available dictionaries:")

        If SearchWord.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("#", "").Replace("""", "").Replace("&", "").Replace("0", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "").Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "") = "" Then
            Main()
        End If

        Dim DicDirs As New List(Of String)()
        For Each Dir As String In Directory.GetDirectories("Dictionary")
            DicDirs.Add(Dir.Replace("Dictionary\", ""))
            Console.WriteLine(DicDirs.Count & ". " & Dir.Replace("Dictionary\", ""))
        Next

        Console.WriteLine()
        Console.Write("Use dictionary(s): ")
        Dim DicChoices() As String
        If SearchWord.Contains("!") = False Then
            DicChoices = Console.ReadLine.Replace(" ", "").Split(",")
            Select Case DicChoices(0)
                Case "!"
                    DicChoices = {"4", "6"}
                Case "!!"
                    DicChoices = {"4", "5", "6"}
            End Select
        ElseIf SearchWord.Contains("!") = True Then
            SearchWord = SearchWord.Replace("!", "")
            DicChoices = {4, 6}
            Console.WriteLine("4, 6")
        ElseIf SearchWord.Contains("!!") = True Then
            SearchWord = SearchWord.Replace("!!", "")
            DicChoices = {4, 5, 6}
            Console.WriteLine("4, 5, 6")
        End If


        If DicChoices(0) = "back" Then
            Main()
        End If

        Dim Dictionary As String = "三省堂　スーパー大辞林"
        Fails = 0

        Console.Write("Searching in: ")
        For I = 0 To DicChoices.Length - 1
            Try
                If DicChoices(I) > DicDirs.Count Then
                    DicChoices(I) = -1
                    Continue For
                End If
            Catch ex As Exception
                Continue For
            End Try

            If I = DicChoices.Length - 1 And DicChoices.Length > 1 Then
                Console.Write("and ")
            End If
            Try
                Console.BackgroundColor = ConsoleColor.White
                Console.ForegroundColor = ConsoleColor.Black
                Console.Write(DicDirs.Item(DicChoices(I) - 1))
                Console.BackgroundColor = ConsoleColor.Black
                Console.ForegroundColor = ConsoleColor.White
            Catch ex As Exception
                Console.Write("error")
            End Try
            If I < DicChoices.Length - 1 Then
                Console.Write(", ")
            End If
        Next
        Console.WriteLine()
        Console.WriteLine()

        For I = 0 To DicChoices.Length - 1
            Dim DicIndex As Integer
            Try
                DicIndex = CInt(DicChoices(I) - 1)
            Catch ex As Exception
                Continue For
            End Try

            If IsNumeric(DicChoices(I)) Then
                Try
                    Dictionary = DicDirs.Item(DicIndex)
                    If DebugMode <> 3 Then
                        SearchJSON(SearchWord, Dictionary, "")
                    ElseIf DebugMode = 3 Then
                        SearchJSON(SearchWord, Dictionary, "contains")
                    End If
                Catch ex As Exception
                    If DebugMode <> 0 Then
                        Console.ForegroundColor = ConsoleColor.Red
                        Console.WriteLine(ex.Message)
                        Console.ForegroundColor = ConsoleColor.White
                    End If
                End Try
            End If
        Next

        Console.WriteLine("Done!")
        Console.ReadLine()
        Main()
    End Sub
    Function SearchJSON(ByVal SearchWord, ByVal Dictionary, ByVal DebugParameters)
        Dim Stopwatch As New Stopwatch()
        Stopwatch.Start()

        If Fails = 2 And DebugMode <> 2 Then
            Console.WriteLine("Word not found")
            Console.ReadLine()
            Main()
        End If

        Dim JRead As JArray
        Dim JsonRead As String
        Dim Results As Integer = 0
        Dim BackSave As Integer = 0
        Dim FirstFound As Boolean = False
        For DicFile = 1 To 100
            If My.Computer.Keyboard.AltKeyDown = True Then
                Threading.Thread.Sleep(100)
                If My.Computer.Keyboard.AltKeyDown = True Then
                    Threading.Thread.Sleep(100)
                    If My.Computer.Keyboard.AltKeyDown = True Then
                        Main()
                    End If
                End If
            End If
            If FirstFound = True Then
                DicFile = 100
                Continue For
            End If
            'Console.WriteLine("-- " & DicFile & " --")
            Try
                JsonRead = My.Computer.FileSystem.ReadAllText("Dictionary\" & Dictionary & "\term_bank_" & DicFile & ".json") 'retrieves the raw text of the Json file
            Catch ex As Exception
                DicFile = 100
                Continue For
            End Try

            Try
                JRead = JArray.Parse(JsonRead) 'tokenises the raw text
            Catch ex As Exception
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine(ex.Message)
                Console.WriteLine("Reason: Parse fail")
                Console.WriteLine("DicFile: " & DicFile)
                Console.WriteLine("Filepath: ..\bin\Debug\jmdict_english\term_bank_" & DicFile & ".json")
                Console.ForegroundColor = ConsoleColor.White
                Console.ReadLine()
                Main()
            End Try
            Dim JItem As JArray
            For ItemIndex = 0 To JRead.Count - 1
                JItem = JRead.Item(ItemIndex)
                If JItem.Item(1) = "" Then
                    JItem.Item(1) = JItem.Item(0)
                End If

                'Console.WriteLine("File{0} - Index{1}", DicFile, ItemIndex)
                'Console.WriteLine(JItem.Item(0))

                If DebugMode = 3 Then
                    Console.WriteLine("{0} ({1})", Strings.Left(JItem.Item(1), 1), ItemIndex)
                End If
                Dim Matched As Boolean = False
                'if it matches a field OR debug allows to contain string and contains
                If (JItem.Item(0) = SearchWord Or JItem.Item(1) = SearchWord) Or (DebugParameters.contains("contains") And (CStr(JItem.Item(0)).Contains(SearchWord) = True Or CStr(JItem.Item(1)).Contains(SearchWord) = True)) Then 'if it's a match to the word you're searching for. It won't be a match if the item is 2 or more characters bigger than what you searched for
                    Matched = True
                ElseIf EnglishSearch = True Then
                    Console.WriteLine(JItem.Item(5).Count - 1)
                    For I = 0 To JItem.Item(5).Count - 1
                        If CStr(JItem.Item(5).Item(I)).Contains(SearchWord) = True Then
                            Matched = True
                        End If
                    Next

                End If
                If Matched = True Then
                    If FirstFound = False And DebugMode <> 1 Then
                        Console.BackgroundColor = ConsoleColor.White
                        Console.ForegroundColor = ConsoleColor.Black
                        Console.WriteLine("{0}:", Dictionary)
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.White
                    End If
                    FirstFound = True
                    If DebugMode <> 1 Then
                        If JItem.Item(1) <> "" Then
                            Console.Write(JItem.Item(0)) 'display the word (0) and then but the pronunciation (1) in brackets
                            Console.Write("(")
                            Console.Write(JItem.Item(1))
                            Console.WriteLine(")")
                        Else
                            Console.WriteLine(JItem.Item(0)) 'if there is no pronunciation, no brackets are needed
                        End If
                        Dim KanjiWord As String = CStr(JItem.Item(0))
                        Console.WriteLine(CStr(JItem.Item(5).First).Replace("―", "{" & KanjiWord & "}")) 'then display the meaning
                        Console.WriteLine()
                    ElseIf DebugMode = 1 Then
                        Console.WriteLine("Dictionary {3}: Result no.{0} of Json{1}, Index {2}) ", Results + 1, DicFile, ItemIndex, Dictionary)
                        Console.WriteLine(JItem.ToString)
                        Console.WriteLine()
                    End If

                    Results += 1 'this variable increase if a match is found
                End If
            Next
        Next

        Stopwatch.Stop()

        If Results = 0 Then
            Fails += 1
            Console.ForegroundColor = ConsoleColor.DarkYellow
            Console.WriteLine("No results in {0}", Dictionary)
            Console.ForegroundColor = ConsoleColor.White
        End If

        If DebugMode <> 0 Then
            Console.WriteLine("Found {0} items in {3} containing {1} in {2} seconds", Results, SearchWord, CStr(Stopwatch.ElapsedMilliseconds / 1000), Dictionary) 'shows how many matches there are
        Else
            Console.WriteLine("---------------------------------------------")
        End If

        Return ("")
    End Function

End Module
