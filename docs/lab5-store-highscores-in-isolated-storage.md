# Store high scores in isolated storage

In parts one through four we created a traffic jam game called Seattle Streets. The object of the game was to move cars around and get the red car to reach the exit in as few moves as possible. In this last part we are going to store away high scores for the levels in isolated storage, i.e. we are going to store away the least number of moves the user has been able to complete a level in.

I have choose the following format of the record list:

```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<RECORDS>
  <RECORD LEVEL="1" MOVES="15" />
  <RECORD LEVEL="2" MOVES="23" />
</RECORDS>
```

So what we will do in this part is to read the current record at the start of a level and display it in the Record TextBlock, and once the red car reaches the exit we will save the score if the number of moves is less than the record for that level.

Silverlight Isolated storage is basically just a folder on the hard drive where you can create files or folders up to a given size limit. In our case the file will be quite small so we don’t have to worry about reaching the limit, but if not you should always check that you haven’t reached the limit before saving something.

The path to your isolated storage differs from operating system to operating system but in Windows 2003 it will be a folder under

```cmd
C:\Documents and Settings\<username>\Local Settings\Application Data\Microsoft\Silverlight\
```

For this particular application my isolated storage folder was:

```cmd
C:\Documents and Settings\<username>\Local Settings\Application Data\Microsoft\Silverlight\is\ekrkqr54.wnr\tx5hm2dm.szm\1\s\5y2lu35ideijonura3bs1rinntito5zrrijuwcxv1cbmj5dgw0aaagga\f
```

So while I am developing I can go in here and look at the file to make sure things got written OK.

The isolated storage folders are per user, per application.

## 1. Create methods to read and write high score records

In order to be able to work with IsolatedStorage and do some IO operations, we need to add the following using statements:

```csharp
using System.IO.IsolatedStorage;
using System.IO;
Let’s start with getting data from the records.xml file

int GetHighScore(string level)
{
    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
    {
        XDocument doc;
        IsolatedStorageFileStream isoStream;

        //if the records file does not exist
        if (!isoStore.FileExists("records.xml"))
        {
            return -1;
        }
        else
        {
            //read the current data and add a new record or modify the old one
            isoStream = new IsolatedStorageFileStream("records.xml", FileMode.Open, FileAccess.Read, isoStore);
            doc = XDocument.Load(isoStream);
            isoStream.Close();

            var records = from record in doc.Descendants("RECORD")
                          where record.Attribute("LEVEL").Value == level
                          select new
                          {
                              Level = record.Attribute("LEVEL").Value,
                              Moves = record.Attribute("MOVES").Value
                          };

            if (records.Count() > 0)
            {
                return Int32.Parse(records.ElementAt(0).Moves);
            }
            else
            {
                return -1;
            }
        }
    }
}
```

IsolatedStorageFile.GetUserStoreForApplication() will get us a pointer to the isolated storage scope for this user.

We check if the records.xml file exists and if not we return –1 showing that no records have been recorded for this level (or any level for that matter).

If it does exist we can get an IsolatedStorageFileStream that we can use just like any stream and pass it to XDocument.Load for example to load up the XML and work on it with Linq to XML.

The next step here is to extract any RECORDS for this particular level, and if we have one we return the number of moves, else we just return –1 again.

Writing the records work very much in the same way:

```csharp
void WritePotentialHighScore(string level, int moves)
{
    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
    {
        XDocument doc;
        IsolatedStorageFileStream isoStream;

        //if the records file does not exist
        if (!isoStore.FileExists("records.xml"))
        {
            //create new document
            isoStream = new IsolatedStorageFileStream("records.xml", FileMode.Create, isoStore);
            doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                        new XElement("RECORDS",
                            new XElement("RECORD",
                                new XAttribute("LEVEL", level), new XAttribute("MOVES", moves))));
            doc.Save(isoStream);
            isoStream.Close();
        }
        else
        {
            //read the current data and add a new record or modify the old one
            isoStream = new IsolatedStorageFileStream("records.xml", FileMode.Open, FileAccess.Read, isoStore);
            doc = XDocument.Load(isoStream);
            isoStream.Close();

            var records = from record in doc.Descendants("RECORD")
                          where record.Attribute("LEVEL").Value == level
                          select new
                          {
                              Level = record.Attribute("LEVEL").Value,
                              Moves = record.Attribute("MOVES").Value
                          };
 

            if (records.Count() > 0)
            {
                //check to see what the value is and alter it
                if (Int32.Parse(records.ElementAt(0).Moves) > moves)
                {
                    foreach(XElement el in doc.Element("RECORDS").Elements("RECORD")){
                        if (el.Attribute("LEVEL").Value == level)
                        {
                            el.Attribute("MOVES").Value = moves.ToString();
                        }
                    }
                    isoStream = new IsolatedStorageFileStream("records.xml", FileMode.Create, FileAccess.Write, isoStore);
                    doc.Save(isoStream);
                    isoStream.Close();
                }
            }
            else
            {
                //otherwise add a new record
                doc.Element("RECORDS").Add(new XElement("RECORD",
                                new XAttribute("LEVEL", level), new XAttribute("MOVES", moves)));
                isoStream = new IsolatedStorageFileStream("records.xml", FileMode.Create, FileAccess.Write, isoStore);
                doc.Save(isoStream);
                isoStream.Close();
            }
        }
    }
}
```

If the file doesn’t exist we create it and generate the header etc. The tricky part is updating it, and I am not 100% sure that this is the best way to save the data but the way I have done it above is by reading the data, modifying it and then opening a new stream to write it back.

## 2. Display the high score at the start of a level

With the get and write methods already in place this becomes a pretty easy task. In InitializeLevel we can just add this code

```csharp
//get the current highscore
int highscore = GetHighScore(levelName.Substring(5));
if (highscore == -1)
    tbBest.Text = "-";
else
    tbBest.Text = highscore.ToString();
3. Store the high scores when the user finishes a level

Finally, the only part that is missing is storing the high scores when the user finishes a level.  

if (currentCar.Orientation == Orientation.Horizontal)
{
    double left = Canvas.GetLeft(currentCar);
    int col = PosToColOrRow(left);
    Canvas.SetLeft(currentCar, col * 60.0);
    currentCar.Column = col;
    if (currentCar.Column + currentCar.Length == 7)
    {
        MessageBox.Show("Congratulations");
        WritePotentialHighScore(tbLevel.Text, moves+1);
    }
}
```

Only horizontal cars can reach the exit, and the exit is reached when starting column + length = 7.  When this is done we display a message box and write the “potential” high score to isolated storage. WritePotentialHighScore takes care of checking if it should write it or not.

And with that, the game is finished! ENJOY!!!
