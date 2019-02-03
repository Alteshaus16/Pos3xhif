# Observable Collections
![View Model Demo App Ui](ViewModelDemoApp2Ui.png)

Dieses Beispiel ist eine Erg�nzung zum Beispiel 3 (Listen). In diesem Ansatz wird im ViewModel eine 
**eigene unabh�bngige Liste** definiert, die die Personenobjekte speichert. Im vorigen Beispiel mussten 
wir beim Hinzuf�gen oder L�schen folgende Schritte ausf�hren:
- Hinzuf�gen bzw. L�schen des Personenobjektes im Model.
- Aufruf von *PropertyChanged()*, welches die gesamte Liste neu l�dt.

Der letzte Punkt - das erneute Laden der Liste - kann bei Webservices sehr zeitintensiv sein. Schlie�lich werden
dann die Daten �ber das Internet neu geladen. Da wir jetzt im ViewModel eine eigene Liste f�hren, k�nnen wir
unseren Ablauf beim Hinzuf�gen oder L�schen von Personenobjekten �ndern:
- Hinzuf�gen bzw. L�schen des Personenobjektes im ViewModel.
- Synchronisation mit dem Model im Hintergrund.

F�r die Liste im ViewModel verwenden wir eine *ObservableCollection*. Sie feuert beim Hinzuf�gen oder 
L�schen von Elementen das Event *CollectionChanged*, auf das wir zentral reagieren k�nnen. Der Aufruf 
von *PropertyChanged()* entf�llt, da die ObservableCollection das Interface *INotifyPropertyChanged* 
implementiert.

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

**Was ist besser? Der vorige Ansatz mit der "normalen" Liste oder die ObservableCollection?**
Diese Frage kann nicht pauschal beantwortet werden. Wenn das Model die Daten schnell bereitstellen kann,
wie es z. B. bei einem OR Mapper mit Proxy der Fall ist, bietet der Zugang aus Beispiel 3 sicher eine
einfachere M�glichkeit der Anzeige. Wenn eine Synchronisation mit dem Model ausprogrammiert werden muss,
dann ist unsere Klasse *SynchronizedObservable<T>* ein geeigneter Ort, diesen Code unterzubringen.

