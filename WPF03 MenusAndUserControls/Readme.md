# User Controls und Men�s
Dieses Beispiel demonstriert, wie ein UserControl und ein Men� eingesetzt werden kann. Das UserControl ist in eine eigene Datei 
ausgelagerter XAML Code. Es kann ein eigenes ViewModel besitzen und ist wie ein getrenntes Fenster zu betrachten. 
Beim Verwenden von Men�punkten wird h�ufig zwischen verschiedenen User Controls umgeschulten. Durch den Einsatz eines 
Converters kann dies deklarativ in XAML erfolgen.

## Funktionsprinzip
In XAML wird mit *MenuItem* ein Men� definiert. Beim Klicken wird der Command ausgef�hrt, welcher in *MainViewModel*
den aktiven Men�punkt setzt. Mit *CommandParameter* wird als zus�tzlicher Parameter der (eindeutige) Namen
des Men�punktes mitgegeben, den wir angeklickt haben:

```xml
<MenuItem Header="_File">
    <MenuItem Header="Add Person" Command="{Binding ActivateMenuitem}" CommandParameter="Add" />
    <MenuItem Header="Edit Person" Command="{Binding ActivateMenuitem}" CommandParameter="Edit" />
    <Separator />
    <MenuItem x:Name="Exit" Header="Exit"  Click="Exit_Click"/>
</MenuItem>
```

Weiter unten im Content Bereich von *MainWindow* werden die einzelnen Controls, die hinter den Men�punkten
stehen, geladen. Da sie nat�rlich nicht alle gleichzeitig sichtbar sein sollen, wird die Visibility mit
einem Converter festgelegt. Der Converter bekommt einerseits als Value den in *MainViewModel* gesetzten aktiven
Men�punkt (*ActiveMenuitem:string*) und als *ConverterParameter* den Wert, bei dem es sichtbar sein soll:

```xml
<ContentControl Visibility="{Binding ActiveMenuitem, ConverterParameter=Add, Converter={StaticResource MenuVisibilityConverter}}">
    <ContentControl.Content>
        <local:AddPersonControl/>
    </ContentControl.Content>
</ContentControl>
<ContentControl Visibility="{Binding ActiveMenuitem, ConverterParameter=Edit, Converter={StaticResource MenuVisibilityConverter}}">
    <ContentControl.Content>
        <local:EditPersonControl/>
    </ContentControl.Content>
</ContentControl>  
```

Der Converter selbst pr�ft dann nur, ob die Werte �bereinstimmen. Wenn ja, wird Visible geliefert:

```c#
/// <summary>
/// Converter f�r die Sichtbarkeit der Controls.
/// </summary>
public class MenuVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Wandelt den value in eine Visibility Eigenschaft um.
    /// </summary>
    /// <param name="value">�ber das Binding aus XAML �bergebener Wert (das aktuelle Men�iten).</param>
    /// <param name="targetType"></param>
    /// <param name="parameter">�ber ConverterParameter aus XAML �bergebener wert, wann Visible geliefert werden soll.</param>
    /// <param name="culture"></param>
    /// <returns>true, wenn value = parameter. Sonst false.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string activeMenuitem = value?.ToString() ?? "";
        string targetMenuitem = parameter?.ToString() ?? "";
        return activeMenuitem == targetMenuitem ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```
