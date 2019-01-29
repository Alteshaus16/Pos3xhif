# Listen und ObservableCollection
![View Model Demo App Ui](ViewModelDemoApp2Ui.png)

In diesem Beispiel sollen alle Personen in einer Liste dargestellt werden. Beim Klicken auf einen
Eintrag der Liste werden die Daten geladen. Diese Features werden durch eine *ListBox* bereitgestellt.
Da Person ein komplexer Typ ist, muss �ber ein *DataTemplate* die Anzeige in der ListBox gesteuert werden.
Folgendes Beispiel zeigt *Firstname* und *Lastname* untereinander an:

```xml
<ListBox DockPanel.Dock="Left" ItemsSource="{Binding Persons}" SelectedItem="{Binding CurrentPerson}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <DockPanel Margin="5 5 5 5">
                <StackPanel>
                    <TextBlock Text="{Binding Firstname}" />
                    <TextBlock FontWeight="Bold" Text="{Binding Lastname}" />
                </StackPanel>
            </DockPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

Dabei ist *Persons* die Collection von Personen in *MainViewModel*, *CurrentPerson* ist das Property in
*MainViewModel*, in welches die Liste die aktuell ausgew�hlte Person hineinschreibt. Es muss nat�rlich
daher ein public set Property sein.

F�r die Darstellung von Listen gibt es in WPF mehrere Controls:
![List Types](ListTypes.png)
*<sup>Quelle: http://www.sws.bfh.ch/~amrhein/Skripten/Info2/, Kapitel 8: WPF Listen und Tabellen</sup>*

## Listen und das dazugeh�rige Property - zwei Ans�tze
Listen binden sich an eine Collection im ViewModel. Diese Collection muss nat�rlich bereitgestellt
werden. Das kann auf mehrere Arten passieren:

### Ansatz 1: Erstellen der Collection mittels LINQ Abfrage aus dem Model
Hier wird in *get* des Properties eine LINQ Abfrage geschrieben, die die Daten aus dem Model holt. Gegebenenfalls
muss mit *ToList()* die Ausf�hrung erzwungen werden, damit z. B. die Daten aus der Datenbank gelesen werden.

Werden nun die zugrundeliegenden Daten �ber die GUI ge�ndert (hinzuf�gen oder l�schen von Elementen), 
muss �ber *PropertyChanged()* die Liste neu eingelesen werden. Bei einer �nderung der Objekte selbst wird 
die �nderung sofort dargestellt, da es sich bei der Liste nur um Referenzen auf die Originalobjekte 
handelt. Dennoch muss folgendes beachtet werden:
- Der Aufruf von *PropertyChanged()* muss immer beim Hinzuf�gen oder L�schen erfolgen, um eine konsistente 
  Darstellung zu gew�hrleisten.
- *PropertyChanged()* liest die Liste zur G�nze neu ein. Bei einer langsamen Quelle (z. B. einem Webservice)
  kann hier eine Latenz f�r den Anwender entstehen, vor allem wenn sehr h�ufig Objekte manipuliert werden.

### Ansatz 2: Arbeiten �ber eine *ObservableCollection*
In diesem Ansatz wird im ViewModel eine eigene Liste definiert, die die Personenobjekte in unserem Beispiel
speichert. Bei einer normalen Liste m�ssten wir beim Hinzuf�gen oder L�schen folgende Schritte ausf�hren:
- Hinzuf�gen des Objektes in der Liste im Viewmodel
- Hinzuf�gen des Objektes im Model
- Aufruf von *PropertyChanged()* 

Mit einer *ObservableCollection* wird automatisch beim Hinzuf�gen oder L�schen von Elementen ein Event
(*CollectionChanged*) geworfen, auf das wir zentral reagieren k�nnen. Der Aufruf von *PropertyChanged()* 
entf�llt, da die ObservableCollection das Interface *INotifyPropertyChanged* implementiert.

F�gen wir nun eine Person durch die Logik in *GeneratePersonCommand* zur ObservableCollection im ViewModel
hinzu, wird zwar die GUI ohne unser Zutun aktualisiert, die Person wird aber nicht im Model gespeichert. 
Es entsteht folgende Situation:
![Untracked Objects](UntrackedObjects.png)

Das neue Objekt wird zwar  zur ObservableCollection hinzugef�gt, das Model wei� aber nichts davon. Es 
ist ein *"untracked object"*. Diesen Fall kann auf 2 Arten gel�st werden.
**Ansatz 1:** Im ViewModel wird der Event *CollectionChanged* abonniert. Im Eventhandler wird mit folgendem Code
  die Person im Model eingetragen oder gel�scht (je nach �nderung der Liste):
```c#
private void PersonObservable_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
{
    foreach (Person p in e.NewItems?.Cast<Person>() ?? Enumerable.Empty<Person>())
    {
        personDb.Person.Add(p);
    }
    foreach (Person p in e.OldItems?.Cast<Person>() ?? Enumerable.Empty<Person>())
    {
        personDb.Person.Remove(p);
    }
}
```

**Ansatz 2:** Wir leiten von *ObservableCollection&lt;T&gt;* eine eigene Klasse *SynchronizedObservable&lt;T&gt;*
ab. Diese Klasse wird im Beispiel verwendet und kann nun f�r alle Basiscollections, in die zur�ckgeschrieben
werden soll, verwendet werden.
```c#
public class SynchronizedObservable<T> : ObservableCollection<T>
{
    private readonly ICollection<T> sourceCollection;

    public SynchronizedObservable(ICollection<T> sourceCollection) : base(sourceCollection)
    {
        this.sourceCollection = sourceCollection;
        CollectionChanged += SynchronizedObservable_CollectionChanged;
    }

    private void SynchronizedObservable_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (T p in e.NewItems?.Cast<T>() ?? Enumerable.Empty<T>())
        {
            sourceCollection.Add(p);
        }
        foreach (T p in e.OldItems?.Cast<T>() ?? Enumerable.Empty<T>())
        {
            sourceCollection.Remove(p);
        }
    }
}
```



