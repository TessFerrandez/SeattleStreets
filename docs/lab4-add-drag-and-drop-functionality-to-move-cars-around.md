# Add Drag and Drop functionality to move the cars around

In [part 3](lab3-use-linq-to-xml-to-read-and-generate-levels.md) we finished the level generation, now we are getting ready to move the cars around a little.

## 1. Add EventHandlers for MouseLeftButtonDown, MouseMove and MouseLeftButtonUp on the cars

Add the following event handlers for mouse events in the AddCar method

```csharp
c.MouseLeftButtonDown += new MouseButtonEventHandler(OnCarLeftButtonDown);
c.MouseMove += new MouseEventHandler(OnCarMove);
c.MouseLeftButtonUp += new MouseButtonEventHandler(OnCarLeftButtonUp);
```

## 2. Record mouse movement

In OnCarLeftButtonDown, record the fact that the user has clicked the mouse over a car, and record the current position (relative to the CarCanvas)

```csharp
bool isMouseCaptured = false;
Point lastPoint;

void OnCarLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    isMouseCaptured = true;
    lastPoint = e.GetPosition(CarCanvas);
    Car currentCar = sender as Car;
}
```

The next step is to keep moving the car along with the mouse.  If the car orientation is horizontal we will change only it’s left position, and if the car is vertical we will change its top position.

```csharp
void OnCarMove(object sender, MouseEventArgs e)
{
    if (isMouseCaptured)
    {
        Car currentCar = sender as Car;
        Point currentPoint = e.GetPosition(CarCanvas);

        if (currentCar.Orientation == Orientation.Horizontal)
        {
            double left = Canvas.GetLeft(currentCar) + currentPoint.X - lastPoint.X;
            Canvas.SetLeft(currentCar, left);
            lastPoint = currentPoint;
        }
        else
        {
            double top = Canvas.GetTop(currentCar) + currentPoint.Y - lastPoint.Y;
            Canvas.SetTop(currentCar, top);
            lastPoint = currentPoint;
        }
    }
}
```

Finally if the user releases the button we stop moving

```csharp
void OnCarLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    isMouseCaptured = false;
}
```

If you build and run the application at this point you will notice that a lot of strange things will happen.

The car will only move while your mouse pointer is inside it so it will act very jerky if you move your mouse very fast. If you don’t release the mouse button while the mouse pointer is over the car the car will go hey wire, and last but not least you can move the car way outside of the canvas area.

## 3. Fix mouse problems

To fix the problems with not capturing the mouse if we move outside of the car area we can add a call to currentCar.CaptureMouse() in OnCarLeftButtonDown, and a call to release the mouse capture in OnCarLeftButtonUp()

```csharp
Car currentCar = sender as Car;
currentCar.ReleaseMouseCapture();
```

Now the cars move as they should, but still you can move the cars way outside the CarCanvas and over other cars.

## 4. Create drag and drop boundaries

What we would ideally want is for the user not to be able to drag the car over another car or outside of the CarCanvas. To do this we create a few helper functions that can calculate what the minimum and maximum x and y coordinates would be for a given car.

I am not going to go into any extreme detail on this, basically we just check for our current row/column and look at how far left/right/up/down we can go before we crash into another car.

```csharp
private double GetMinX(int row, int column)
{
    //find the earliest col that is not filled
    column--;
    while (column >= 0 && Filled[row, column] == false)
        column--;
    column++;
    return column * 60.0;
}

private double GetMinY(int row, int column)
{
    //find the earliest col that is not filled
    row--;
    while (row >= 0 && Filled[row, column] == false)
        row--;
    row++;
    return row * 60.0;
}

private double GetMaxX(int row, int column, int length)
{
    //find the latest col that is not filled
    //special case for row 2 since it has 7 columns

    if (row == 2)
    {
        column = column + length;
        while (column <= 6 && Filled[row, column] == false)
            column++;
        column = column - length;
        return column * 60.0;
    }
    else
    {
        column = column + length;
        while (column <= 5 && Filled[row, column] == false)
            column++;
        column = column - length;
        return column * 60.0;
    }
}

private double GetMaxY(int row, int column, int length)
{
    row = row + length;
    while (row <= 5 && Filled[row, column] == false)
        row++;
    row = row - length;
    return row * 60.0;
}
```

In OnCarMove we can now add this code to check for the boundaries

```csharp
double left = Canvas.GetLeft(currentCar) + currentPoint.X - lastPoint.X;
double minX = GetMinX(currentCar.Row, currentCar.Column);
double maxX = GetMaxX(currentCar.Row, currentCar.Column, currentCar.Length);
if (left <= minX) left = minX;
if (left >= maxX) left = maxX;
Canvas.SetLeft(currentCar, left);
lastPoint = currentPoint;
```

And similar code for top in the Vertical case

Finally in the OnCarLeftButtonUp we need to add some code to set the row and colum of the car change which positions are currently filled. While we are at it we can also update the number of moves when we are done moving the car.

```csharp
//unmark previous position
Mark(currentCar.Row, currentCar.Column, currentCar.Length, currentCar.Orientation, false);

if (currentCar.Orientation == Orientation.Horizontal)
{
    double left = Canvas.GetLeft(currentCar);
    int col = PosToColOrRow(left);
    Canvas.SetLeft(currentCar, col * 60.0);
    currentCar.Column = col;
}
else
{
    double top = Canvas.GetTop(currentCar);
    int row = PosToColOrRow(top);
    Canvas.SetTop(currentCar, row * 60.0);
    currentCar.Row = row;
}

//mark new position
Mark(currentCar.Row, currentCar.Column, currentCar.Length, currentCar.Orientation, true);
moves++;
tbMoves.Text = moves.ToString();
```

The PosToColOrRow method simply translates a position to a distinct column/row number

```csharp
private int PosToColOrRow(double position)
{
    return (int)(position / 60);
}
```

Now you can build and run the application and play the game.

In the last part we will put a finishing touch on it by recording high scores in Isolated Storage.
