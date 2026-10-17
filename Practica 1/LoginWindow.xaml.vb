Imports System.IO
Imports System.Text.Json
Imports System.Collections.Generic

Class LoginWindow

    Private users(,) As String = New String(2, 1) {{"admin", "pass123"}, {"usuario", "1234"}, {"test", "test"}}
    Private usersList As List(Of User) = Nothing
    Private failedAttempts As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private lockoutUntil As New Dictionary(Of String, DateTime)(StringComparer.OrdinalIgnoreCase)

    Public Sub New()
        InitializeComponent()
        AddHandler Me.Loaded, AddressOf LoginWindow_Loaded
    End Sub

    Private Sub LoginWindow_Loaded(sender As Object, e As RoutedEventArgs)
        LoadUsersFromJson()
    End Sub

    Private Sub LoginButton_Click(sender As Object, e As RoutedEventArgs)
        Dim user = UsernameBox.Text.Trim()
        Dim pwd = PasswordBox.Password
        MessageText.Visibility = Visibility.Collapsed

        If String.IsNullOrEmpty(user) Or String.IsNullOrEmpty(pwd) Then
            ' TC04: campos vacíos -> mostrar MessageBox
            MessageBox.Show("Introduzca datos", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim auth = Authenticate(user, pwd)
        Select Case auth
            Case AuthResult.Success
                ' Login correcto: abrir MainWindow y pasar nombre
                Dim mw = New MainWindow()
                mw.SetWelcome(user)
                mw.Show()
                Me.Close()
            Case AuthResult.Locked
                ' TC05: bloqueo temporal tras 3 intentos
                MessageBox.Show("Se ha bloqueado temporalmente al usuario", "Bloqueado", MessageBoxButton.OK, MessageBoxImage.Warning)
            Case AuthResult.WrongPassword
                ' TC02: contraseña incorrecta
                MessageBox.Show("La contraseña introducida no es correcta", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Case AuthResult.UserNotFound
                ' TC03: usuario no existe
                MessageBox.Show("El usuario introducido no existe", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Select
    End Sub

    Private Sub ExitButton_Click(sender As Object, e As RoutedEventArgs)
        Application.Current.Shutdown()
    End Sub

    Private Function ValidateCredentials(user As String, pwd As String) As Boolean
        If usersList IsNot Nothing Then
            For Each u In usersList
                If String.Equals(u.Username, user, StringComparison.OrdinalIgnoreCase) AndAlso u.Password = pwd Then
                    Return True
                End If
            Next
            Return False
        End If

        For i As Integer = 0 To users.GetLength(0) - 1
            If users(i, 0).Equals(user, StringComparison.OrdinalIgnoreCase) AndAlso users(i, 1) = pwd Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Enum AuthResult
        Success = 0
        WrongPassword = 1
        UserNotFound = 2
        Locked = 3
    End Enum

    Private Function Authenticate(user As String, pwd As String) As AuthResult
        ' Primero buscar en usersList (JSON)
        Dim key = user.ToLowerInvariant()
        If usersList IsNot Nothing Then
            For Each u In usersList
                If String.Equals(u.Username, user, StringComparison.OrdinalIgnoreCase) Then
                    ' Comprobar bloqueo
                    If lockoutUntil.ContainsKey(key) AndAlso DateTime.Now < lockoutUntil(key) Then
                        Return AuthResult.Locked
                    End If

                    If u.Password = pwd Then
                        ' Reiniciar contador tras éxito
                        If failedAttempts.ContainsKey(key) Then failedAttempts(key) = 0
                        If lockoutUntil.ContainsKey(key) Then lockoutUntil.Remove(key)
                        Return AuthResult.Success
                    Else
                        ' Contabilizar intento fallido
                        If Not failedAttempts.ContainsKey(key) Then failedAttempts(key) = 0
                        failedAttempts(key) += 1
                        If failedAttempts(key) >= 3 Then
                            lockoutUntil(key) = DateTime.Now.AddSeconds(30)
                            failedAttempts(key) = 0
                            Return AuthResult.Locked
                        End If
                        Return AuthResult.WrongPassword
                    End If
                End If
            Next
            Return AuthResult.UserNotFound
        End If

        ' Fallback al arreglo en memoria
        For i As Integer = 0 To users.GetLength(0) - 1
            If users(i, 0).Equals(user, StringComparison.OrdinalIgnoreCase) Then
                If lockoutUntil.ContainsKey(key) AndAlso DateTime.Now < lockoutUntil(key) Then
                    Return AuthResult.Locked
                End If

                If users(i, 1) = pwd Then
                    If failedAttempts.ContainsKey(key) Then failedAttempts(key) = 0
                    If lockoutUntil.ContainsKey(key) Then lockoutUntil.Remove(key)
                    Return AuthResult.Success
                Else
                    If Not failedAttempts.ContainsKey(key) Then failedAttempts(key) = 0
                    failedAttempts(key) += 1
                    If failedAttempts(key) >= 3 Then
                        lockoutUntil(key) = DateTime.Now.AddSeconds(30)
                        failedAttempts(key) = 0
                        Return AuthResult.Locked
                    End If
                    Return AuthResult.WrongPassword
                End If
            End If
        Next
        Return AuthResult.UserNotFound
    End Function

    Private Sub ShowMessage(msg As String)
        MessageText.Text = msg
        MessageText.Visibility = Visibility.Visible
    End Sub

    Private Sub LoadUsersFromJson()
        Try
            ' Buscar Users.json en la carpeta de salida o en el workspace
            Dim baseDir = AppDomain.CurrentDomain.BaseDirectory
            Dim paths As New List(Of String) From {
                Path.Combine(baseDir, "Users.json"),
                Path.Combine(baseDir, "..\..\..\Users.json"),
                Path.Combine("C:\\Users\\Jorge\\source\\repos\\Practica 1", "Users.json")
            }

            Dim found As String = Nothing
            For Each p In paths
                If File.Exists(p) Then
                    found = p
                    Exit For
                End If
            Next

            If found Is Nothing Then Return

            Dim json = File.ReadAllText(found)
            Dim list = JsonSerializer.Deserialize(Of List(Of User))(json)
            If list IsNot Nothing Then
                usersList = list
            End If
        Catch
            ' Ignorar y usar valores por defecto
        End Try
    End Sub

    Private Class User
        Public Property Username As String
        Public Property Password As String
    End Class

End Class
