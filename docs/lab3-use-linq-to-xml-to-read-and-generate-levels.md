# Use Linq to XML to read and generate levels

In [Part 2](lab2-create-a-car-user-control.md) we created some Cars to place on the board, in this post we will read the level information and place the Cars accordingly.

You can find a number of xml files in the levels folder with level information. Create a folder in your project called Levels and add the xml files to this folder.

Each level has it’s own LEVELX.XML file and a level is represented as a number of cars:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<CARS>
  <CAR COLOR="PURPLE" ROW="0" COL="0" LENGTH="2" ORIENTATION="HORIZONTAL"/>
  <CAR COLOR="RED" ROW="2" COL="1" LENGTH="2" ORIENTATION="HORIZONTAL"/>
  <CAR COLOR="ORANGE" ROW="4" COL="4" LENGTH="2" ORIENTATION="HORIZONTAL"/>
```

## 1. Read from the level xml file and draw the cars

To draw the cars on the level, we simply read the contents of the XML file using Linq to XML, and then walk through the data and create a new car for each element.

In order to use System.XML.Linq you need to add a reference to System.Xml.Linq.dll and add `using System.Xml.Linq;`

```csharp
private void AddCars(string levelName)
{
    XDocument levelDoc = XDocument.Load("Levels/" + levelName + ".xml");

    var cars = from car in levelDoc.Descendants("CAR")
               select new
               {
                   Orientation = car.Attribute("ORIENTATION").Value,
                   Length = Int32.Parse(car.Attribute("LENGTH").Value),
                   Row = Int32.Parse(car.Attribute("ROW").Value),
                   Column = Int32.Parse(car.Attribute("COL").Value),
                   Color = car.Attribute("COLOR").Value
               };

    foreach (var car in cars)
    {
        Orientation orientation;
        Color color = GetColorFromName(car.Color);
        if (car.Orientation == "HORIZONTAL")
            orientation = Orientation.Horizontal;
        else
            orientation = Orientation.Vertical;

        AddCar(orientation, car.Length, color, car.Row, car.Column);
    }
}
```

The GetColorFromName method is a helper method that translates the color string to an actual Color.

```csharp
private Color GetColorFromName(string color)
{
    switch (color)
    {
        case "WHITE":
            return Colors.White;
        case "BLACK":
            return Colors.Black;
        case "RED":
            return Colors.Red;
        case "BLUE":
            return Colors.Blue;
        case "YELLOW":
            return Colors.Yellow;
        case "GREEN":
            return Colors.Green;
        case "PURPLE":
            return Colors.Purple;
        case "ORANGE":
            return Colors.Orange;
        default:
            return Colors.Black;
    }
}
```

In order to keep track of which calls are used and not the page has a

```csharp
bool[,] Filled = new bool[6, 7];
```

member variable.

AddCar adds the cars to the canvas and marks the slots as occupied.

```csharp
private void AddCar(Orientation orientation, int length, Color color, int row, int col)
{
    Car c = new Car(orientation, length, color, row, col);
    Mark(row, col, length, orientation, true);
    CarCanvas.Children.Add(c);
    c.SetValue(Canvas.LeftProperty, col * 60.0);
    c.SetValue(Canvas.TopProperty, row * 60.0);
}

private void Mark(int row, int col, int length, Orientation orientation, bool filled)
{
    //mark positions as filled or unfilled
    if (orientation == Orientation.Horizontal)
        for (int i = 0; i < length; i++)
            Filled[row, col + i] = filled;
    else
        for (int i = 0; i < length; i++)
            Filled[row + i, col] = filled;
}
```

## 2. Reset the board after previous levels

The InitializeLevel method removes all cars from the board (created when displaying previous levels), resets Filled as well as moves (int)
and displays all the cars for this level.

```csharp
private void InitializeLevel(string levelName)
{
    moves = 0;
    tbMoves.Text = "0";


    //clear filled
    for (int i = 0; i < 6; i++)
        for (int j = 0; j < 7; j++)
            Filled[i, j] = false;

    //remove all the cars from previous rounds
    CarCanvas.Children.Clear();

    //add new cars and squares
    AddCars(levelName);
}
```

Call InitializeLevel(“Level1”) from the Page constructor, build and run the application.

## 3. Add code to show new levels when the user selects a level in the combo

In the XAML for the cboLevel combo box add the event handler for SelectionChanged, if you type SelectionChanged=”  and tab out it will generate it automatically in the code behind.

Add the following code to the event handler.

```csharp
if (cboLevel != null)
{
    string level = ((ComboBoxItem)cboLevel.Items[cboLevel.SelectedIndex]).Content.ToString();
    InitializeLevel(level);
    tbLevel.Text = level.Substring(5);
}
```

Now you can build and run the application to browse around among the levels. Unfortunately at this point this is a pretty useless application since you can’t really move the cars around.  That is what we’ll do in the next part.
