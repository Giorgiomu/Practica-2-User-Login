Imports System.Windows
Imports System.Windows.Controls
Imports System.IO
Imports System.Windows.Media.Imaging

Class MainWindow

    Public Sub New()
        InitializeComponent()
        AddHandler Me.Loaded, AddressOf MainWindow_Loaded
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs)
        ' Intentar cargar imágenes desde varias ubicaciones si no aparecen en tiempo de ejecución
        Dim mapping = New Dictionary(Of String, String) From {
            {"logoImage", "Blizzard.png"},
            {"jamelLogo", "jamel.png"},
            {"userImage", "user.png"},
            {"img1", "game1.jpg"},
            {"img2", "game2.jpg"},
            {"img3", "game3.jpg"},
            {"img4", "game4.jpg"},
            {"img5", "game5.png"},
            {"img6", "game6.jpg"},
            {"img7", "game7.jpg"},
            {"img8", "game8.jpg"},
            {"img9", "game9.jpg"},
            {"img10", "game10.jpg"},
            {"img11", "game11.jpg"},
            {"img12", "game12.jpg"}
        }

        For Each kvp In mapping
            Dim elementName = kvp.Key
            Dim fileName = kvp.Value
            Dim imgControl = TryCast(Me.FindName(elementName), Image)
            If imgControl IsNot Nothing Then
                If imgControl.Source Is Nothing Then
                    Dim bmp = TryLoadBitmap(fileName)
                    If bmp IsNot Nothing Then
                        imgControl.Source = bmp
                    End If
                End If
            End If
        Next
    End Sub

    Private Function TryLoadBitmap(fileName As String) As BitmapImage
        ' Intentar ruta directa en el directorio del workspace (ruta conocida del usuario)
        Try
            Dim workspaceImages = Path.Combine("C:\\Users\\Jorge\\source\\repos\\Practica 1", "Images", fileName)
            If File.Exists(workspaceImages) Then
                Dim bi As New BitmapImage()
                bi.BeginInit()
                bi.UriSource = New Uri(workspaceImages, UriKind.Absolute)
                bi.CacheOption = BitmapCacheOption.OnLoad
                bi.EndInit()
                bi.Freeze()
                Return bi
            End If
        Catch
        End Try

        ' Intentar URIs del ensamblado
        Dim asmUri = "/Practica 1;component/Images/" & fileName
        Try
            Dim bi As New BitmapImage(New Uri(asmUri, UriKind.Relative))
            Return bi
        Catch
        End Try

        Dim packUri = "pack://application:,,,/Images/" & fileName
        Try
            Dim bi As New BitmapImage(New Uri(packUri, UriKind.Absolute))
            Return bi
        Catch
        End Try

        ' Intentar en la carpeta de salida (bin)
        Dim baseDir = AppDomain.CurrentDomain.BaseDirectory
        Dim candidate = Path.Combine(baseDir, "Images", fileName)
        If File.Exists(candidate) Then
            Try
                Dim bi As New BitmapImage()
                bi.BeginInit()
                bi.UriSource = New Uri(candidate, UriKind.Absolute)
                bi.CacheOption = BitmapCacheOption.OnLoad
                bi.EndInit()
                bi.Freeze()
                Return bi
            Catch
            End Try
        End If

        ' Subir directorios buscando la carpeta Images (proyecto u otras ubicaciones)
        Dim dirInfo As DirectoryInfo = New DirectoryInfo(baseDir)
        For i As Integer = 0 To 6
            If dirInfo Is Nothing Then Exit For
            candidate = Path.Combine(dirInfo.FullName, "Images", fileName)
            If File.Exists(candidate) Then
                Try
                    Dim bi As New BitmapImage()
                    bi.BeginInit()
                    bi.UriSource = New Uri(candidate, UriKind.Absolute)
                    bi.CacheOption = BitmapCacheOption.OnLoad
                    bi.EndInit()
                    bi.Freeze()
                    Return bi
                Catch
                End Try
            End If
            dirInfo = dirInfo.Parent
        Next

        Return Nothing
    End Function

    Private Sub AddToCart_Click(sender As Object, e As RoutedEventArgs)
        Dim btn = TryCast(sender, Button)
        If btn IsNot Nothing Then
            Dim itemName = If(btn.Tag, "elemento")
            MessageBox.Show(String.Format("{0} añadido al carrito.", itemName), "Carrito", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Public Sub SetWelcome(usuario As String)
        Try
            If WelcomeText IsNot Nothing Then
                WelcomeText.Text = "Bienvenido, " & usuario
            End If
            If SidebarWelcomeText IsNot Nothing Then
                SidebarWelcomeText.Text = "Bienvenido, " & usuario
            End If
        Catch
        End Try
    End Sub

    Private Sub LogoutButton_Click(sender As Object, e As RoutedEventArgs)
        ' Cerrar MainWindow y volver al LoginWindow
        Dim lw = New LoginWindow()
        lw.Show()
        Me.Close()
    End Sub

    Private Sub ExitAppButton_Click(sender As Object, e As RoutedEventArgs)
        ' TC06: cerrar la aplicación
        Application.Current.Shutdown()
    End Sub

End Class
