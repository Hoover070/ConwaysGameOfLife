using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace HooverGOL122022
{
    public partial class Form1 : Form
    {
        // The universe array
        bool[,] universe = new bool[10, 10];
        bool[,] scratchpad = new bool[10, 10];

        // Drawing colors - changing cell colors
        Color gridColor;
        Color cellColor;
        Color x10GridColor;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

        //changeable count // default is false, which is finite. Once the option is clicked in the menu it changes. 
        public bool toroidal = false;
        //public bool GetToroidal
        //{
        //   get{ return toroidal; }
        //   set { toroidal = value; }
        //}
        //bool for the number in the center of the screen
        bool neighborCount = true;

        //bool to turn the grid on or off
        bool gridOn = true;

        //int for the random seed
        int seed = 1405029;

        bool HudVisible = true;

        


        public Form1()
        {
            InitializeComponent();

            // Setup the timer
            timer.Interval = 100; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer running

            //loading in settings for the color
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            gridColor = Properties.Settings.Default.GridColor;
            x10GridColor = Properties.Settings.Default.x10GridColor;
            cellColor = Properties.Settings.Default.CellColor;

            //loading in settings for the grid
            universe = ResizeUniverse<bool>(universe,Properties.Settings.Default.UniverseY, Properties.Settings.Default.UniverseX);
            scratchpad = ResizeUniverse<bool>(scratchpad,Properties.Settings.Default.UniverseY, Properties.Settings.Default.UniverseX);
            toroidal = Properties.Settings.Default.Finite;
            gridOn= Properties.Settings.Default.GridOn;
            timer.Interval = Properties.Settings.Default.Miliseconds;

            //loading random seed
            seed = Properties.Settings.Default.Seed;

        }

        // Calculate the next generation of cells
        private void NextGeneration() //to boldly go where no one has gone before
        {
            

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    ////clear the scratch pad
                    //ClearScratchpad();
                    int count;
                    //get the neighbor count 
                    if(toroidal == true)
                    {
                         count = CountNeighborsToroidal(x, y); 
                    }
                    else 
                         count = CountNeighborsFinite(x, y);

                    ////apply the rules and decide if cell should live or die in the next generation                    
                    ////rule 1
                    // Living cells with less than 2 living neighbors die in the next generation.
                    if (universe[x, y] == true && count < 2)
                    {
                        scratchpad[x, y] = false;

                    }
                    //rule 2
                    // Living cells with more than 3 living neighbors die in the next generation.
                    else if (universe[x, y] == true && count > 3)
                    {
                        scratchpad[x, y] = false;

                    }
                    //rule 3
                    //Living cells with 2 or 3 living neighbors live in the next generation.
                    else if (universe[x, y] == true && count == 2 || count == 3)
                    {
                        scratchpad[x, y] = true;

                    }
                    //rule 4
                    //Dead cells with exactly 3 living neighbors live in the next generation.
                    else if (universe[x, y] == false && count == 3)
                    {
                        scratchpad[x, y] = true;
                    }
                    else //the catchall for the remaining stationary cells
                    {
                        scratchpad[x, y] = universe[x, y];
                    }
                } 
            }
            //copy from the scratch pad to universe
            bool[,] temp = universe;
            universe = scratchpad;
            scratchpad = temp;

            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations : " + generations.ToString();

            //update stats strip Aivecell count
            toolStripStatusLabelLivingCells.Text = "Living Cells : " + CountAlive();

            //update states strip Seed
            toolStripStatusLabelSeed.Text = "Seed : " + seed.ToString();

            //update Stats strip Interval(miliseconds between geneartions)
            toolStripStatusLabelInterval.Text = "Interval : " + timer.Interval.ToString();

            //place invalidate
            graphicsPanel1.Invalidate();
        }


        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels // you need to do this math as float, so we have to figure out how to make the division be done as float as well
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            float cellWidth = (float)graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            float cellHeight = (float)graphicsPanel1.ClientSize.Height / universe.GetLength(1);
            // x10 cell width
            float x10CellWidth = (float)graphicsPanel1.ClientSize.Width / universe.GetLength(0)*10;
            // x10 cell height
            float x10CellHeight = (float)graphicsPanel1.ClientSize.Height / universe.GetLength(1)*10;
            if (gridOn == true)
            {
                // A Pen for drawing the grid lines (color, width)
                Pen gridPen = new Pen(gridColor, 1);
                Pen x10GridPen = new Pen(x10GridColor, 3);
                // A Brush for filling living cells interiors (color)
                Brush cellBrush = new SolidBrush(cellColor);
                //font for the neighborsCount
                Font font = new Font("Arial", 10f);
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                //for the x10 grid
                for (int y = 0; y < universe.GetLength(1) / 10; y++)
                {
                    // Iterate through the universe in the x, left to right
                    for (int x = 0; x < universe.GetLength(0) / 10; x++)
                    {
                        // A rectangle to represent each cell in pixels
                        //RectangleF - This is the float rectangle 
                        RectangleF cellRectx10 = RectangleF.Empty;
                        cellRectx10.X = x * x10CellWidth;
                        cellRectx10.Y = y * x10CellHeight;
                        cellRectx10.Width = x10CellWidth;
                        cellRectx10.Height = x10CellHeight;

                       

                        // Outline the cell with a pen
                        e.Graphics.DrawRectangle(x10GridPen, cellRectx10.X, cellRectx10.Y, cellRectx10.Width, cellRectx10.Height);
                    }
                }

                // Iterate through the universe in the y, top to bottom
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    // Iterate through the universe in the x, left to right
                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                      
                        



                        // A rectangle to represent each cell in pixels
                        //RectangleF - This is the float rectangle 
                        RectangleF cellRect = RectangleF.Empty;
                        cellRect.X = x * cellWidth;
                        cellRect.Y = y * cellHeight;
                        cellRect.Width = cellWidth;
                        cellRect.Height = cellHeight;

                        // Fill the cell with a brush if alive
                        if (universe[x, y] == true)
                        {

                            e.Graphics.FillRectangle(cellBrush, cellRect);
                        }
                        // Outline the cell with a pen
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

                        //activates the show neighbor count, and change the color of the neighbor count on wether or not the cell will live next generation
                        if (neighborCount == true)
                        {
                            //initiate neighbors count
                            int neighbors = CountNeighborsFinite(x, y);
                            //if 0 do not show anything 
                            if (neighbors == 0)
                            {
                                continue;
                            }
                            //show red because next generataion it will die next generation
                            else if (universe[x, y] == true && neighbors < 2)
                            {
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Red, cellRect, stringFormat);
                            }
                            // //show red because next generataion it will die next generation
                            else if (universe[x, y] == true && neighbors > 3)
                            {
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Red, cellRect, stringFormat);
                            }
                            //show red because next generataion it will live next generation
                            else if (universe[x, y] == true && neighbors == 2 || neighbors == 3)
                            {
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Green, cellRect, stringFormat);
                            }
                            //show red because next generataion it will live next generation
                            else if (universe[x, y] == false && neighbors == 3)
                            {
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Green, cellRect, stringFormat);
                            }
                            //nothing will happen to the cell next geneartion
                            else
                            {
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Black, cellRect, stringFormat);
                            }

                        }
                    }
                }


                //Draw Hud
                if(HudVisible == true)
                {
                    Font font1 = new Font("Arial", 15f);

                    StringFormat stringFormatHud = new StringFormat();
                    stringFormatHud.Alignment = StringAlignment.Near;
                    stringFormatHud.LineAlignment = StringAlignment.Far;

                    string boundry = "Finite";
                    if(toroidal == true)
                    {
                        boundry = "Toroidal";
                    }


                    Rectangle rect = graphicsPanel1.ClientRectangle;
                    string hud = $"Generations: {generations}\nCell Count: {CountAlive()}\nBoundry Type: {boundry}\nUniverse Size: Width: {universe.GetLength(1)}\tHeight: {universe.GetLength(0)}";

                    e.Graphics.DrawString(hud, font1, Brushes.Black, rect, stringFormatHud);
                }

                // Cleaning up pens and brushes
                gridPen.Dispose();
                cellBrush.Dispose();
            }
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                //once converted to floats above, convert this math to float as well
                // Calculate the width and height of each cell in pixels
                int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X /  CELL WIDTH
                int x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = e.Y / cellHeight;



                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                //never place invalidate inside of paint
                graphicsPanel1.Invalidate();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)//close the program
        {
            Close();

        }

        private void toolStripButton1_Click(object sender, EventArgs e)//start the generations
        {
            timer.Enabled = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)//pause the generartions
        {
            timer.Enabled = false;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)//next generation
        {
            NextGeneration();
        }

        //private void Form1_Load(object sender, EventArgs e) //dont mess with this in this class
        //{

        //}

        private int CountNeighborsFinite(int x, int y)
        {
            //initiate the variables used
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);

            //nested for loop for the number of rows
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                //nested forloop for the columns
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    // if xOffset and yOffset are both equal to 0 then continue
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    // if xCheck is less than 0 then continue
                    else if (xCheck < 0 )
                    {
                        continue;
                    }
                    // if yCheck is less than 0 then continue
                    else if (yCheck < 0 )
                    {
                        continue;
                    }
                    // if xCheck is greater than or equal too xLen then continue
                    else if (xCheck >= xLen)
                    {
                        continue;
                    }
                    // if yCheck is greater than or equal too yLen then continue
                    else if (yCheck >= yLen)
                    {
                        continue;
                    }
                    //default if nothing else is correct
                    else if (universe[xCheck, yCheck] == true) { count++; }
                }
            }
            return count;
        }

        private int CountNeighborsToroidal(int x, int y) //does not currently work
        {
            //initiate the variables used
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    // if xOffset and yOffset are both equal to 0 then continue
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    // if xCheck is less than 0 then set to xLen - 1
                    else if (xCheck < 0)
                    {
                        xLen = -1;
                    }
                    // if yCheck is less than 0 then set to yLen - 1
                    else if (yCheck < 0)
                    {
                        yLen = -1;
                    }
                    // if xCheck is greater than or equal too xLen then set to 0
                    else if (xCheck >= xLen)
                    {
                        xLen = 0;
                    }
                    // if yCheck is greater than or equal too yLen then set to 0
                    else if (yCheck >= yLen)
                    {
                        yLen = 0;
                    }

                    //default if nothing else is correct
                    else if (universe[xCheck, yCheck] == true) { count++; }
                }
            }
            return count;
        }

        public void ClearScratchpad()
        {
            for (int y = 0; y < scratchpad.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < scratchpad.GetLength(0); x++)
                {
                    scratchpad[x, y] = false;
                }
            }           
        }
        public void Clear()
        {
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    scratchpad[x,y] = false;
                    universe[x,y] = false;
                }
            }
            generations -= 1;
            //NextGeneration();
            graphicsPanel1.Invalidate();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)//clears the board but does not start a new game
        {
            Clear();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)//new game
        {
            Clear();
            generations = -1;         
            timer.Enabled = false;
            NextGeneration();
            graphicsPanel1.Invalidate();


        }
        private void RandomGameTime()
        {
           
            // Cycle cells using rand to set live/dead cells
            Random rand = new Random();
           

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {

                    // Random Board
                    if (rand.Next(0,2) == 0)
                    { 
                        universe[x, y] = true;
                    }
                    else
                        universe[x, y] = false;


                    

                }
            }
            graphicsPanel1.Invalidate();

        }
        private void RandomGameSeed()
        {

            // Cycle cells using rand to set live/dead cells
           Random randSeed = new Random(seed);

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {

                    // Random Board
                    if (randSeed.Next(0,2)==0)
                    {
                        universe[x, y] = true;
                    }
                    else
                    {
                        universe[x, y] = false;
                    }


                }
            }
            graphicsPanel1.Invalidate();
        }

        private void torodialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //updates the torodial bool to true 
            toroidal= true;

        }

        private void finiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //updates the torodial bool to default false
            toroidal= false;
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabelGenerations_Click(object sender, EventArgs e)
        {

        }

        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            dlg.Color = graphicsPanel1.BackColor;


            if (DialogResult.OK == dlg.ShowDialog())
            {
                graphicsPanel1.BackColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }

        }

        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ColorDialog dlg = new ColorDialog();

            //dlg.ShowDialog();

        }

        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            dlg.Color = gridColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                gridColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridX10ColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            dlg.Color = x10GridColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                x10GridColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void cellColorToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            dlg.Color = cellColor;

            

            if(DialogResult.OK == dlg.ShowDialog())
            { 
                cellColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int width = (int)universe.GetLength(0);
            int height = (int)universe.GetLength(1);

            Options dlg = new Options();

            dlg.TimerMiliseconds = timer.Interval;
            dlg.WidthUniverse = width;
            dlg.HeightUniverse = height;



            //to make a tool window
            //dlg.Show(); needs an update button to send the information back
            if (DialogResult.OK == dlg.ShowDialog())
            {
                timer.Interval = (int)dlg.TimerMiliseconds;
                universe = ResizeUniverse<bool>(universe, dlg.WidthUniverse, dlg.HeightUniverse) ;
                scratchpad = ResizeUniverse<bool>(scratchpad, dlg.WidthUniverse, dlg.HeightUniverse);
                graphicsPanel1.Invalidate();

            }

        }

        //the resize array function
        //takes in a bool array and then creates a new array with the values provided
        //then it copies the original array to the resize array and rturns the resized array
        bool[,] ResizeUniverse<T>(bool[,] original,  int rows, int columns)
        {
            //instatiate resizearray
            var resizeUniverse = new bool[rows, columns];

            for (int y = 0; y <= 0; y++)
            {
                for (int x = 1; x <= 1; x++)
                {

                     resizeUniverse[y, x] = original[y, x];


                }
            }
            return resizeUniverse;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //updating the settings for the color properties
            Properties.Settings.Default.BackColor = graphicsPanel1.BackColor;
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.x10GridColor = x10GridColor;
            Properties.Settings.Default.CellColor = cellColor;

            //updating the settings for the grid
            for (int y = 0; y < 1; y++)
            {
                for (int x = 0; x < 1; x++)
                {

                    Properties.Settings.Default.UniverseX = universe.GetLength(1);
                    Properties.Settings.Default.UniverseY = universe.GetLength(0);


                }
            }
            
            Properties.Settings.Default.Finite = toroidal;
            Properties.Settings.Default.GridOn = gridOn;
            Properties.Settings.Default.Miliseconds = timer.Interval;
            Properties.Settings.Default.Seed = seed;




            //saving the updates
            Properties.Settings.Default.Save();

        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e) //loads the default settings
        {
            Properties.Settings.Default.Reset();
            //loading in settings for the color
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            gridColor = Properties.Settings.Default.GridColor;
            x10GridColor = Properties.Settings.Default.x10GridColor;
            cellColor = Properties.Settings.Default.CellColor;

            //loading in settings for the grid
            universe = ResizeUniverse<bool>(universe, Properties.Settings.Default.UniverseY, Properties.Settings.Default.UniverseX);
            scratchpad = ResizeUniverse<bool>(scratchpad, Properties.Settings.Default.UniverseY, Properties.Settings.Default.UniverseX);
            toroidal = Properties.Settings.Default.Finite;
            gridOn = Properties.Settings.Default.GridOn;
            timer.Interval = Properties.Settings.Default.Miliseconds;
            graphicsPanel1.Invalidate();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e) //loads the last saved settings
        {
            Properties.Settings.Default.Reload();
            //loading in settings for the color
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            gridColor = Properties.Settings.Default.GridColor;
            x10GridColor = Properties.Settings.Default.x10GridColor;
            cellColor = Properties.Settings.Default.CellColor;

            //loading in settings for the grid
            universe = ResizeUniverse<bool>(universe, Properties.Settings.Default.UniverseY, Properties.Settings.Default.UniverseX);
            scratchpad = ResizeUniverse<bool>(scratchpad, Properties.Settings.Default.UniverseY, Properties.Settings.Default.UniverseX);
            toroidal = Properties.Settings.Default.Finite;
            gridOn = Properties.Settings.Default.GridOn;
            timer.Interval = Properties.Settings.Default.Miliseconds;
            graphicsPanel1.Invalidate();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            toroidal = true;
            
        }

        private void fromSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomSeed dlg = new RandomSeed();
           

            dlg.Seed = seed;



            if (DialogResult.OK == dlg.ShowDialog())
            {
                seed = dlg.Seed;
                RandomGameSeed();
            }
        }

        private void fromCurrentSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            seed = Properties.Settings.Default.Seed;
            RandomGameSeed();
        }

        private void fromTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomGameTime();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files |*.*|Cells|*.cells";
            dlg.FilterIndex = 2;
            dlg.DefaultExt = "cells";
            DateTime date = DateTime.Today;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                        // Write any comments you want to include first.
                        // Prefix all comment strings with an exclamation point.
                        // Use WriteLine to write the strings to the file. 
                        // It appends a CRLF for you.
                        writer.WriteLine("!saved;");

                        // Iterate through the universe one row at a time.
                        for (int y = 0; y < universe.GetLength(1); y++)
                        {
                            // Create a string to represent the current row.
                            String currentRow = string.Empty;
                            // Iterate through the current row one cell at a time.
                            for (int x = 0; x < universe.GetLength(0); x++)
                            {
                                // If the universe[x,y] is alive then append 'O' (capital O)
                                // to the row string.
                                if (universe[x, y] == true)
                                {

                                    currentRow += 'O';

                                }

                                // Else if the universe[x,y] is dead then append '.' (period)
                                // to the row string.
                                else
                                {
                                    currentRow += '.';

                                }
                            }

                            // Once the current row has been read through and the 
                            // string constructed then write it to the file using WriteLine.
                            writer.WriteLine(currentRow);

                        }

                        // After all rows and columns have been written then close the file.
                        writer.Close(); 
                    }
        } //need to change this to save to a file if one is open otherwise ask for filename

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files |*.*|Cells|*.cells";
            dlg.FilterIndex = 2;
            dlg.DefaultExt = "cells";
            DateTime date = DateTime.Today;

            if (DialogResult.OK == dlg.ShowDialog())
            {

                StreamWriter writer = new StreamWriter(dlg.FileName);

                // Write any comments you want to include first.
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.
                writer.WriteLine("!saved;");

                // Iterate through the universe one row at a time.
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    // Create a string to represent the current row.
                    String currentRow = string.Empty;
                    // Iterate through the current row one cell at a time.
                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        // If the universe[x,y] is alive then append 'O' (capital O)
                        // to the row string.
                        if (universe[x, y] == true)
                        {

                            currentRow += 'O';

                        }

                        // Else if the universe[x,y] is dead then append '.' (period)
                        // to the row string.
                        else
                        {
                            currentRow += '.';

                        }
                    }

                    // Once the current row has been read through and the 
                    // string constructed then write it to the file using WriteLine.
                    writer.WriteLine(currentRow);

                }

                // After all rows and columns have been written then close the file.
                writer.Close();
            }



        } //this asked for the name of the file to save to eachtime

        private void openToolStripMenuItem_Click(object sender, EventArgs e) //open a file
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                // Create a couple variables to calculate the width and height
                // of the data in the file.
                int maxWidth = 0;
                int maxHeight = 0;
                int yPos = 0;

                // Iterate through the file once to get its size.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then it is a comment
                    // and should be ignored.
                    if (row.StartsWith("!"))
                    {
                        continue;
                    }


                    // If the row is not a comment then it is a row of cells.
                    // Increment the maxHeight variable for each row read.
                    else
                    maxHeight++;


                    // Get the length of the current row string
                    // and adjust the maxWidth variable if necessary.
                    if(maxWidth < row.Count())
                    {
                        maxWidth = row.Count();
                    }
                   

                }

                // Resize the current universe and scratchPad
                // to the width and height of the file calculated above.
                universe = new bool[maxWidth, maxHeight];
                scratchpad = new bool[maxWidth, maxHeight];

                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then
                    // it is a comment and should be ignored.
                    if (row.StartsWith("!"))
                    {
                        continue;
                    }
                    // If the row is not a comment then 
                    // it is a row of cells and needs to be iterated through.
                    for (int xPos = 0; xPos < row.Length; xPos++)
                    {
                        // If row[xPos] is a 'O' (capital O) then
                        // set the corresponding cell in the universe to alive.
                        if (row[xPos]=='O')
                        {
                            universe[xPos, yPos] = true;
                        }
                        // If row[xPos] is a '.' (period) then
                        // set the corresponding cell in the universe to dead.
                        else if (row[xPos] == '.')
                        {
                            universe[ xPos, yPos] = false;
                        }
                        
                    }
                    yPos++;
                }
               
                
                // Close the file.
                reader.Close();

                // Update status strip generations
                toolStripStatusLabelGenerations.Text = "Generations : " + generations.ToString();

                //update stats strip Aivecell count
                toolStripStatusLabelLivingCells.Text = "Living Cells : " + CountAlive();

                //update states strip Seed
                toolStripStatusLabelSeed.Text = "Seed : " + seed.ToString();

                //update Stats strip Interval(miliseconds between geneartions)
                toolStripStatusLabelInterval.Text = "Geneartion Interval : " + timer.Interval.ToString();


                graphicsPanel1.Invalidate();
            }
        }

        public int CountAlive()
        {
            int count = 0;  
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (universe[x,y] == true)
                    {
                        count++;
                    }
                }
            }


            return count;
        }

        private void hUDToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
           if(HudVisible ==false)
            {
                //make the hud visible
                HudVisible = true;
                //checks the hud option
                ToolStripMenuItemHud.Checked = true;


            }
            else
            {
                //makes the hud invisible
                HudVisible = false;
                //unchecks the hud option
                ToolStripMenuItemHud.Checked = false;
            }
            graphicsPanel1.Invalidate();

        }

        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (neighborCount == false)
            {
                //make the hud visible
                neighborCount = true;
                //checks the hud option
                ToolStripMenuItemNeighborCount.Checked = true;


            }
            else
            {
                //makes the hud invisible
                neighborCount = false;
                //unchecks the hud option
                ToolStripMenuItemNeighborCount.Checked = false;
            }
            graphicsPanel1.Invalidate();



        }

        private void gridOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridOn == false)
            {
                //make the hud visible
                gridOn = true;
                //checks the hud option
                ToolStripMenuItemGridOn.Checked = true;


            }
            else
            {
                //makes the hud invisible
                gridOn = false;
                //unchecks the hud option
                ToolStripMenuItemGridOn.Checked = false;
            }
            graphicsPanel1.Invalidate();


        }
    }
}

