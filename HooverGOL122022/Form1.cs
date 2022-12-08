﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        Color gridColor = Color.Black;
        Color cellColor = Color.Gray;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

        //changeable count // default is false, which is finite. Once the option is clicked in the menu it changes. 
       bool toroidal = false;

        //bool for the number in the center of the screen
        bool neighborCount = true;

        public Form1()
        {
            InitializeComponent();

            // Setup the timer
            timer.Interval = 100; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer running
        }

        // Calculate the next generation of cells
        private void NextGeneration() //to boldly go where no one has gone before
        {
            //clear the scratchPad
            ClearScratchpad();

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
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();

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

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);
            Pen x10GridPen = new Pen(gridColor, 3);
            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);
            //font for the neighborsCount
            Font font = new Font("Arial", 10f);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

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
                        else if (universe[x,y] == true && neighbors < 2)
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
            //for the x10 grid
            for (int y = 0; y < universe.GetLength(1)/10; y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0)/10; x++)
                {
                    // A rectangle to represent each cell in pixels
                    //RectangleF - This is the float rectangle 
                    RectangleF cellRect = RectangleF.Empty;
                    cellRect.X = x * x10CellWidth;
                    cellRect.Y = y * x10CellHeight;
                    cellRect.Width = x10CellWidth;
                    cellRect.Height = x10CellHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {

                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    




                    // Outline the cell with a pen
                    e.Graphics.DrawRectangle(x10GridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                }
            }

            




            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
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

        private int CountNeighborsToroidal(int x, int y)
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

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
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
        private void RandomGame()
        {
           
            // Cycle cells using rand to set live/dead cells
            var rand = new Random();
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // Random Board
                    if (rand.Next(1, 101) < 75)
                        universe[x, y] = false;
                    else
                        universe[x, y] = true;
                }
            }
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
    }
}

